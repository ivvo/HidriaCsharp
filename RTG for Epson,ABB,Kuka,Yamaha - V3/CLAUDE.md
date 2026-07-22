# CLAUDE.md

Napotki za Claude Code (claude.ai/code) pri delu s to kodo.

## Kaj je to

WPF namizno orodje, ki generira kodo za robotske celice (Epson RC+, KUKA Hella, ABB Hidria, ABB
Simulacija, Yamaha RCX, Kawasaki AS) iz vmesnika, kjer uporabnik definira "programe" (robote) in
"postaje". Ima tudi funkcijo, ki že generiran projekt uvozi nazaj v model in vanj kirurško doda nove
postaje brez polnega regeneriranja.

Ta drevo ("V3") je razšla različica iste osnove kot referenčni projekt `Template Generator For
Robots` – ima dodaten Kawasaki, drugačen Yamaha generator, bogatejši HMI in simulacijski način.
Import/Update ter popravki so bili preneseni naknadno.

## Gradnja

Aktivna rešitev je `TemplateGenerator.sln` (`TemplateGenerator.Core` + `TemplateGenerator.Wpf` +
`TemplateGenerator.Tests`). `GenerateExcelFile/` in `GenerateExcelIO/` v korenu sta mrtvi (star
COM-Interop predhodnik), nista v rešitvi – ne popravljaj ju brez izrecne zahteve.

```
dotnet build TemplateGenerator.sln
```

Če je VS odprt in aplikacija teče, je `TemplateGenerator.Wpf.dll` zaklenjen (`MSB3027`) – zgradi
samo `TemplateGenerator.Core` (tam je skoraj vsa logika in se ne zaklene).

`dotnet test TemplateGenerator.sln` požene teste (glej spodaj).

## Arhitektura

### Postavitev

- **`TemplateGenerator.Core`** (`netstandard2.0`) – vsa logika: pogon predlog, modeli, MvvmCross
  `ShellViewModel`, Importer/Updater razredi, Excel IO. Brez UI odvisnosti.
- **`TemplateGenerator.Wpf`** (`net5.0-windows`, `UseWPF`+`UseWindowsForms`) – tanka WPF lupina.
  MvvmCross konvencija razreši `ShellViewModel` → `ShellView.xaml`.
- **`TemplateGenerator.Tests`** (`net8.0`, xUnit) – testi Import/Update + Excel.

### Pogon predlog (enosmerno: model → datoteke)

`TemplateGenerator.Core\Classes\Template.cs` je velik `internal static` razred, ena regija na
proizvajalca. Statični konstruktor prebere vse `*.txt` skelete iz `./Templates/<Proizvajalec>/…`
**relativno na delovni direktorij procesa**. Predloge so hranjene v `TemplateGenerator.Wpf\Templates\`
(v virski kontroli) in se ob gradnji kopirajo poleg izhodnega `.exe` (`CopyToOutputDirectory`), pa
tudi poleg testnega `.dll` (isti `Content Include` v testnem `.csproj`). Če delovni direktorij ne
vsebuje `Templates\`, `Template.FormatsLoaded` ostane `false` in `LoadError` dobi sporočilo; oba
vstopna glagola v `ShellViewModel` (`GenerateProject`/`UpdateProject`) to preverita in javita napako.

`ShellViewModel.GenerateProject(path)` se veji po `SelectedTemplate` in zapiše datoteke. Epson,
Yamaha in Kawasaki se zapišejo v časovno označeno podmapo; KUKA in ABB neposredno v izbrano mapo.

### Import & Update (dodajanje postaje v obstoječ projekt)

`ShellViewModel.ImportProject(path)` samodejno zazna proizvajalca iz oblike mape
(`*_Motion.mod` → ABB Hidria, `R1\Program\` → KUKA Hella, `*.prg` → Epson, `*.all` → Yamaha,
`*.as` → Kawasaki) in kliče ustrezni `*ProjectImporter`. `UpdateProject(path)` si zapomni
proizvajalca (`_importedVendor`) in kliče ustrezni `*ProjectUpdater`; pred urejanjem naredi varnostno
kopijo mape (`BackupFolder`). `_stationBaseline` hrani število postaj ob uvozu, da Updater ve, katere
so nove (dodane na konec).

Dva pristopa k posodobitvi:
- **Kirurško vstavljanje** (KUKA Hella, ABB Hidria): najdi blok → vstavi vejo/vrstico. Ohrani ročne
  popravke IN naučene točke. Deluje, ker sta KUKA/ABB generatorja v obeh drevesih bajt-identična.
- **Re-generacija + prekrivanje naučenih točk po imenu** (Epson, Yamaha, Kawasaki): celoten projekt
  se re-generira iz modela (enako kot Generate), naučene koordinate/komentarji pa se po IMENU točke
  prekrijejo nazaj. Uporabljeno tam, kjer je generator zapleten in/ali razšli od referenčnega
  (Epson: `inStation`, `gripper`/`testAll`, `For i/For j` homing, `pAboveStation`; Yamaha: redek
  seznam točk z vrzeljami, varni točki; Kawasaki: iskanje po imenu). Posledica: koda je vedno
  pravilna, naučene točke ohranjene, ročni popravki KODE pa ob Update niso ohranjeni.

Po proizvajalcih:
- **KUKA Hella / ABB Hidria** (kirurško): prenesena iz referenčnega projekta; delujeta, ker sta
  generatorja bajt-identična. Popravljen resničen hrošč: KUKA `$config.dat` je nove `SIGNAL` naslove
  lepil na obstoječo vrstico – zdaj se vstavljajo za koncem cele vrstice (regresijski test to varuje).
- **Epson** (re-generacija + prekrivanje): `EpsonProjectUpdater` re-generira vse datoteke iz modela in
  prekrije naučene `.pts` koordinate po `sLabel`. Slog "simple" je v V3 privzet; stanje "simulation"
  se zazna iz uvožene `.prg` (edina razlika, ki jo prinese: `Or 1 = 1` v homingu).
- **Yamaha / Kawasaki** (re-generacija + prekrivanje): glej spodaj (Yamaha) oz. `KawasakiProjectUpdater`
  (točke se prekrijejo po imenu `p<Postaja>`/`#pHome`; Kawasaki išče najbližjo točko po imenu).

### Yamaha Import/Update (re-generacija + prekrivanje naučenih točk)

V3-jev Yamaha generator je zapleten, a popolnoma **determinističen** iz modela postaj: en sam
`BackupFile.all` (koda + točke + IO), **redek** seznam točk z namernimi vrzeljami, varni točki
`pSafeL`/`pSafeR`, `pAboveStation`/`p<Postaja>Final`, comma-list homing (`CASE 3,4`), valeča IO
shema. Kirurško vstavljanje po odsekih bi bilo krhko, zato `YamahaProjectUpdater`:

1. celoten robotov del **RE-GENERIRA** iz modela (z že dodanimi postajami) – enako zaporedje kot
   `GenerateProject` (`Template.GetYamaha*`), zato je koda vedno pravilna in popolna;
2. **naučene koordinate/komentarje ohrani** tako, da jih po **IMENU točke** (`pHome`, `pStation1`,
   `pSafeL`, …) prekrije nazaj v na novo generirano datoteko (`[PNT]`/`[PCM]`).

Posledica: naučene točke se ohranijo (vključno z varnimi/spremenljivimi, ki niso v modelu), rezultat
je bajt-identičen svežemu Generate celotnega seznama – a morebitni **ročni popravki v `.all` kodi se
NE ohranijo** (za razliko od kirurške poti pri Epson/KUKA/ABB). `YamahaProjectImporter` prebere
seznam postaj iz glave (`Ime = indeks`), "free" iz `O_<Postaja>_FREE`, `Positions` iz imen točk
`p<Postaja><N>`, koordinate/komentar iz `[PNT]`/`[PCM]`. Podprt je en robot na projekt (MAIN odsek
upošteva le prvega).

### Dve pravili, ki ju je treba ohraniti pri vsakem `*ProjectUpdater`

1. **Nikoli v celoti ne prepiši datoteke z zunanje naučenimi podatki** (Epson `.pts`, KUKA
   `*_motion.dat`, ABB `Global.mod` robtargets, Yamaha `[PNT]`). Te se generirajo kot ničle; prave
   vrednosti nastanejo šele ob učenju na robotu. Kirurške poti (KUKA/ABB) jih ohranijo z vstavljanjem;
   Yamaha jih ohrani s prekrivanjem po imenu.
2. **"Origin completeness"**: vsaka "pojdi na postajo" funkcija ima notranje stikalo "od kod
   prihajam", ki mora našteti VSE postaje kot možen izvor – sicer robot ob premiku STRAN od nove
   postaje pade v napako. Kirurški Updaterji imajo za to poseben korak; Yamaha to dobi samodejno z
   re-generacijo.

### Excel IO tabela (`GenerateExcellIO.cs`)

`GenerateExcelIO.GenerateIO(...)` iz predloge (`IO_template_*.xlsx` / `templateIO.xlsx`) naredi
`IOTable.xlsx` (list `Sheet1`) in vpisuje celice. **Popravek**: `UpdateCellText` je poleg
`InlineString` puščal nastavljen tudi `CellValue` (`<v>`), kar je Excel javljal kot "nečitljiva
vsebina / popravljeno" – zdaj se `CellValue` postavi na `null`. Byte.bit naslovi (npr. `"3.0"`) gredo
skozi `UpdateCellText` (ostanejo besedilo), ne skozi `UpdateCellNumber` (ki bi jih skrčil na `3`).
Prava cela števila (indeks postaje, št. pozicij) ostanejo `Number` (locale-varno). Regresijski test
preverja, da nobena `InlineString` celica nima še `<v>` in da je `"3.0"` shranjen kot besedilo.

## Testi

`TemplateGenerator.Tests` (xUnit). `TestHelpers`:
- `ModuleInitializer` nastavi `TGR_SUPPRESS_EXPLORER=1`, da testi ne odpirajo Raziskovalca
  (`ShellViewModel.OpenFolder` to upošteva).
- `BuildAndGenerate` požene `ShellViewModel` kot UI (privzeti program "robot" + postaje).
- `ResolveProjectDir` najde časovno označeno podmapo (Epson/Yamaha/Kawasaki).
- `IsSubsequenceOfLines` preveri, da se ob Update nobena obstoječa vrstica ne izgubi.

Ključni testi: KUKA/ABB/Yamaha `Import_RoundTrips…`, `Update_AppendsNewStation…`,
`Update_NewStation_BecomesOrigin…`, ohranjanje naučenih točk; **Yamaha
`Update_ByteIdenticalToFullGenerate…`** (najmočnejši oracle); KUKA `Update_ConfigDat_SignalsAreOn
SeparateLines` (regresija za lepljenje SIGNAL); routing `ImportProject_AutoDetectsCorrectVendor` +
`Epson_Import_Works_But_Update_IsGated`; Excel `GeneratedIOTable_HasNoInlineStringCellWithStaleValue`.

## Modeli

`ProgramModel` (en robot) ima `ObservableCollection<StationModel>`. `StationModel`:
`RobotStationName`, izpeljani `RobotStationNameToUpper`, `StationFreeEnabled`, `Pallet`, `Positions`
(privzeto 1), koordinate `Xcord/Ycord/Zcord/R1cord/R2cord/R3cord` in `RobotStationComment` (V3-jev
Yamaha/Kawasaki generator bere te koordinate; KUKA/ABB/Epson jih pišejo kot ničle).

## Kaj NE spreminjati brez dogovora

- Generatorjev (`Template.cs`) ne "poravnavaj" z referenčno različico – drevesi sta se namenoma
  razšli in robotski programi drugih projektov so odvisni od obstoječega izhoda.
- Pri re-generacijskih Updaterjih (Epson/Yamaha/Kawasaki): če se `ShellViewModel.GenerateProject`
  zaporedje pisanja spremeni, ga uskladi tudi v pripadajočem `*ProjectUpdater.Regenerate...` (sicer
  Update ne bo več bajt-identičen svežemu Generate – to ujamejo testi).
