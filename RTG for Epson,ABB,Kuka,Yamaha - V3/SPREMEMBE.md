# Spremembe na projektu V3 — podroben opis

Ta dokument opisuje vse spremembe, narejene na projektu
`RTG for Epson,ABB,Kuka,Yamaha - V3` v tej iteraciji: dodano branje obstoječih projektov in
dodajanje postaj (Import/Update) za **vse** proizvajalce, popravek generiranja Excel IO tabele,
prenos predlog v virsko kontrolo, avtomatizirani testi in dokumentacija.

Čista kopija projekta pred spremembami ostaja v `RTG ... V3 (1).zip`; funkcija Update pred vsakim
pisanjem naredi še varnostno kopijo projektne mape (`<mapa>_backup_<datum_ura>`).

---

## 1. Nova funkcionalnost: branje projekta (Import) in dodajanje postaj (Update)

Cilj: obstoječ, s tem orodjem že generiran projekt prebrati nazaj v model in vanj **dodati nove
postaje**, ne da bi se izgubile **naučene koordinate** točk (in, kjer je to izvedljivo, ročni
popravki kode).

### 1.1 Uporabniški vmesnik

- **`TemplateGenerator.Wpf/Views/ShellView.xaml`** — dodana gumba **Import Project** in
  **Update Project** (nova vrstica na dnu mreže; obstoječa postavitev s slikami in tabelo ostane
  nedotaknjena).
- **`TemplateGenerator.Wpf/Views/ShellView.xaml.cs`** — dodana ročnika `ImportProjectClick` in
  `UpdateProjectClick` (mapo izbereš prek `FolderBrowserDialog`, enako kot pri Generate).

### 1.2 Krmilni model — `TemplateGenerator.Core/ViewModels/ShellViewModel.cs`

- Dodani polji:
  - `Dictionary<ProgramModel,int> _stationBaseline` — koliko postaj je imel program ob uvozu (da
    Update ve, katere na koncu seznama so nove).
  - `string _importedVendor` — kateri proizvajalec je bil uvožen (za pravilen dispatch pri Update).
- **`ImportProject(path)`** — samodejno zazna proizvajalca iz oblike izbrane mape in pokliče ustrezni
  bralnik:
  - `*_Motion.mod` → ABB Hidria
  - `R1\Program\` → KUKA Hella
  - `*.prg` → Epson Hidria
  - `*.all` → Yamaha
  - `*.as` → Kawasaki
- **`UpdateProject(path)`** — izračuna nove postaje (`Skip(baseline)`), naredi varnostno kopijo mape,
  in pokliče ustrezni posodabljalnik glede na `_importedVendor`.
- **`OpenFolder(path)`** — nov pomožni ovoj okoli `Process.Start("explorer.exe", …)`, ki odpiranje
  preskoči, kadar je nastavljena okoljska spremenljivka `TGR_SUPPRESS_EXPLORER=1` (da avtomatizirani
  testi ne odpirajo oken Raziskovalca), in tiho požre morebitno napako zagona. Vse dosedanje
  `Process.Start("explorer.exe", path)` klice (Generate za vse proizvajalce) sem zamenjal s tem
  ovojem.
- **`FormatsLoaded` zaščita** — na začetku `GenerateProject` in `UpdateProject`: če se predloge niso
  naložile, se prikaže jasno sporočilo (z `Template.LoadError`) in nič se ne zapiše.

### 1.3 Novi razredi za Import/Update — `TemplateGenerator.Core/Classes/`

Za vsakega proizvajalca po en bralnik (`*ProjectImporter`) in en posodabljalnik (`*ProjectUpdater`):

- `EpsonProjectImporter.cs`, `EpsonProjectUpdater.cs`
- `KukaHellaProjectImporter.cs`, `KukaHellaProjectUpdater.cs`
- `AbbHidriaProjectImporter.cs`, `AbbHidriaProjectUpdater.cs`
- `YamahaProjectImporter.cs`, `YamahaProjectUpdater.cs`
- `KawasakiProjectImporter.cs`, `KawasakiProjectUpdater.cs`

Vsak bralnik ob nepričakovani obliki vrže svojo tipizirano izjemo (`*ImportException`) z razumljivim
slovenskim sporočilom, namesto da bi tiho ugibal.

### 1.4 Dva pristopa k posodabljanju (in zakaj)

Posodabljalniki uporabljajo dva pristopa, izbrana glede na to, kako zapleten oz. razhajajoč je
generator posameznega proizvajalca:

**A) Kirurško vstavljanje — KUKA Hella, ABB Hidria**
- V obstoječe datoteke se vstavi samo koda za novo postajo (najdi blok → vstavi vejo/vrstico);
  preostala koda in naučene točke ostanejo nedotaknjene.
- Uporabljeno pri KUKA in ABB, ker sta ta dva generatorja v V3 **bajt-identična** referenčni
  različici (torej so vzorci za kirurško vstavljanje zanesljivi).
- **Prednost:** ohrani tudi ročne popravke kode.

**B) Re-generacija + prekrivanje naučenih točk po imenu — Epson, Yamaha, Kawasaki**
- Ob Update se celoten robotov del projekta **re-generira iz modela** (z že dodano postajo), enako
  kot pri Generate — torej kodo ustvari kar generator sam in je vedno pravilna in popolna.
- Nato se **naučene koordinate/komentarji ohranijo** tako, da se po **imenu točke** prekrijejo nazaj
  v na novo generirano datoteko.
- Uporabljeno tam, kjer je generator bodisi strukturno razhajajoč od referenčnega (Epson), bodisi
  preveč zapleten za varno kirurško vstavljanje (Yamaha: redek seznam točk z vrzeljami, varni točki;
  Kawasaki).
- **Prednost:** izhod je zajamčeno "na način V3" in bajt-identičen svežemu Generate.
- **Omejitev:** morebitni **ročni popravki v generirani kodi** se ob Update **ne ohranijo**; naučene
  točke pa se ohranijo (to je glavni namen).

### 1.5 Podpora po proizvajalcih

| Proizvajalec | Import | Update | Pristop | Kje se ohranijo naučene točke |
|---|---|---|---|---|
| Epson Hidria | ✅ | ✅ | re-generacija + prekrivanje | `.pts` po `sLabel="p<Postaja>"` |
| KUKA Hella | ✅ | ✅ | kirurško | `*_motion.dat` (ne prepiše se) |
| ABB Hidria | ✅ | ✅ | kirurško | `Global.mod` robtarget deklaracije |
| Yamaha | ✅ | ✅ | re-generacija + prekrivanje | `[PNT]` po imenu (tudi `pSafeL/pSafeR/pAboveStation`) |
| Kawasaki | ✅ | ✅ | re-generacija + prekrivanje | točke po imenu `p<Postaja>`, `#pHome` |

### 1.6 Dve načeli, ki jih posodabljalniki spoštujejo

1. **Datotek z naučenimi koordinatami se ne prepiše na slepo.** Epson `.pts`, KUKA `*_motion.dat`,
   ABB `Global.mod` robtargeti, Yamaha `[PNT]`, Kawasaki točke se generirajo kot ničle, prave
   vrednosti nastanejo šele ob učenju na robotu — zato se ohranijo (kirurško z vstavljanjem, sicer s
   prekrivanjem po imenu).
2. **"Origin completeness".** Vsaka "pojdi na postajo X" funkcija ima notranje stikalo "od kod
   prihajam", ki mora poznati **vse** postaje kot možen izvor — sicer robot ob premiku stran od nove
   postaje pade v napako. Kirurški posodabljalniki to eksplicitno dodajo; re-generacijski to dobijo
   samodejno.

### 1.7 Posebnosti posameznih proizvajalcev

- **Epson:** slog "simple program" je v V3 privzet (brez UI stikala); stanje "simulation" se ob Update
  samodejno **zazna** iz uvožene `.prg` (edina razlika, ki jo prinese, je `Or 1 = 1` v homingu), zato
  ni treba, da uporabnik ročno nastavlja stikalo.
- **Epson — pomožni programi:** ob uvozu se upoštevata `Main.prg` in robotske `.prg` datoteke (tiste
  s `Function ..._go...()` funkcijami, npr. `robot.prg`). Datoteke brez teh funkcij (pomožni programi
  za komunikacijo, strojni vid ipd., npr. `getOrientation.prg`, `HandIO.prg`) se **preskočijo** in ne
  pokvarijo uvoza (prej je nepoznana `.prg` ustavila celoten uvoz). Posodobitev cilja natanko robotsko
  `<program>.prg` in teh pomožnih datotek ne spreminja.
- **Epson — branje naučenih koordinat:** ob uvozu se v tabelo preberejo koordinate `rX/rY/rZ` iz
  `<program>1.pts` (rotacije `rU/rV/rW` se zaenkrat NE berejo). Ker se v resničnih (ročno urejenih)
  projektih imena postaj, imena točk v kodi in oznake v `.pts` ne ujemajo vedno enako (npr. postaja
  `ParTake1`, koda `pParttake1`, `.pts` `pPartTake1`), je ujemanje **neobčutljivo na velikost črk**:
  najprej po imenu postaje (brez končnih pozicijskih številk, npr. `CheckWeight` → `pCheckweight1`),
  sicer po prvi `p...` referenci v telesu go-funkcije. Branje je le za PRIKAZ; ohranjanje točk ob
  Update poteka neodvisno (prekrivanje `.pts` po `sLabel`). Če ima projekt več točkovnih datotek
  (`robot1.pts`, `robot2.pts` ...), se cilja natanko `<program>1.pts`.
- **Znana omejitev (zaznava št. pozicij):** pri ročno urejenih Epson projektih orodje ob uvozu
  večpozicijskih postaj morda ne zazna pravilnega števila pozicij (npr. `CheckWeight`/`Etalon` se
  prikažeta kot 1 pozicija, čeprav imata več) — V3 generator ne uporablja markerja
  `go<Postaja>MaxZHeight`, na katerega se opira detekcija. Za take postaje **Update ni priporočljiv**,
  dokler zaznava ni izboljšana (regeneracija bi jih zapisala kot enopozicijske). Prikaz koordinat
  primarne točke pa je pravilen.
- **Yamaha:** en robot na projekt (MAIN odsek upošteva le prvega); celoten program je v eni datoteki
  `BackupFile.all`.
- **Kawasaki:** najbližjo točko ob homingu išče po **imenu** (`DISTANCE`), ne po številčnem indeksu,
  zato tu ni številčne pasti kot pri Epson/Yamaha.
- **Omejitev (obstoječa):** nove postaje se dodajajo samo **na konec** seznama. KUKA/ABB uvoz je
  omejen na en robot na projekt (projektno skupne datoteke se sicer podvojijo). Pri ABB se ob Update
  `OtherFunctions.mod` in `EIORobot.cfg`/`EIOSimulacija.cfg` ne osvežijo — zanje je potreben poln
  Generate.

---

## 2. Popravek generiranja Excel IO tabele — `GenerateExcellIO.cs`

Napaka: generirana `IOTable.xlsx` je ob odpiranju javljala "Excel je našel nečitljivo vsebino /
popravljeno", byte.bit naslovi (npr. `3.0`) pa so bili napačno formatirani.

- **`UpdateCellText`**: `cell.CellValue` se zdaj postavi na `null`. Prej je celica hkrati imela
  `<v>` (CellValue) in `<is>` (InlineString), kar je neveljaven OOXML in vzrok korupcije.
- **ABB byte.bit naslov** (npr. `"3.0"`): zdaj gre skozi `UpdateCellText` (ostane besedilo) namesto
  skozi `UpdateCellNumber`, ki bi ga kot število skrčil na `3` (izguba bita).
- Prava cela števila (indeks postaje, število pozicij) ostanejo `Number` (to je locale-varno).

---

## 3. Zaščita ob manjkajočih predlogah — `Template.cs`

- Dodano polje `public static string LoadError = null;`.
- Tihi `catch {}` v statičnem konstruktorju je zdaj `catch (Exception ex) { FormatsLoaded = false;
  LoadError = ex.Message; }`, da lahko orodje uporabniku prikaže vzrok, če se predloge ne naložijo.

---

## 4. Predloge v virsko kontrolo

Prej so bile predloge (`Templates/`) samo v izhodni mapi `bin/…` (niso bile v virski kontroli, niti
jih ni kopiral noben korak gradnje — tvegano).

- Vsebina `Templates/` je zdaj v virskem drevesu: `TemplateGenerator.Wpf/Templates/`.
- **`TemplateGenerator.Wpf.csproj`** — dodan `Content Include="Templates\**\*.*"` s
  `CopyToOutputDirectory=PreserveNewest`, tako da se predloge ob gradnji samodejno kopirajo poleg
  `.exe` (in poleg testnega `.dll`).

---

## 5. Avtomatizirani testi — nov projekt `TemplateGenerator.Tests`

- Nov projekt (net8.0, xUnit), dodan v `TemplateGenerator.sln`.
- **`TemplateGenerator.Tests.csproj`** — reference na xUnit, `Microsoft.NET.Test.Sdk`,
  `DocumentFormat.OpenXml` (za Excel test) in `Content Include` predlog (kopija poleg testnega `.dll`).
- **`TestHelpers.cs`**:
  - `ModuleInitializer`, ki nastavi `TGR_SUPPRESS_EXPLORER=1` (testi ne odpirajo Raziskovalca).
  - `BuildAndGenerate(...)` — požene `ShellViewModel` kot vmesnik (privzeti program "robot" + postaje).
  - `ResolveProjectDir(...)` — poišče časovno označeno podmapo (Epson/Yamaha/Kawasaki).
  - `IsSubsequenceOfLines(...)` — preveri, da se ob Update nobena obstoječa vrstica ne izgubi.
  - `FirstDirDifference(...)` — primerja besedilne datoteke dveh map (za bajt-identičnost).
- **Testni razredi** (skupaj **27 testov**):
  - `EpsonImportUpdateTests` — round-trip uvoza; Update bajt-identičen svežemu Generate (z večpozicijsko
    postajo); ohranjanje naučenih `.pts` koordinat.
  - `KukaHellaImportUpdateTests` — round-trip; Update ohrani obstoječe vrstice + doda novo postajo;
    origin-completeness; ohranjanje naučenih `motion.dat`; **regresija za `$config.dat`** (SIGNAL
    vrstice ločene).
  - `AbbHidriaImportUpdateTests` — round-trip; Update; origin-completeness; ohranjanje robtargetov.
  - `YamahaImportUpdateTests` — round-trip (postaje/free/positions); Update bajt-identičen; ohranjanje
    naučenih točk (postajna + `pSafeL`).
  - `KawasakiImportUpdateTests` — round-trip; Update bajt-identičen; ohranjanje naučenih točk.
  - `ShellViewModelRoutingTests` — samodejna zaznava vseh 5 proizvajalcev; jasna napaka za neznano mapo.
  - `ExcelIOTests` — regresija: nobena InlineString celica nima več `<v>`; byte.bit `"3.0"` ostane
    besedilo.
- Zagon: `dotnet test TemplateGenerator.sln`.
- Preverjena veljavnost testov: z namernim pokvarjenjem (izklop popravka/prekrivanja) ustrezni test
  pade; po vrnitvi je spet vse zeleno.

---

## 6. Dokumentacija

- **`README.md`** (novo) — uporabniška: kaj je, komu namenjeno, zagon, gradnja (+ obvoz VS-zaklepa),
  uporaba Generate in Import→Update, tabela podpore po proizvajalcih, znane omejitve, testi.
- **`CLAUDE.md`** (novo) — razvijalska: arhitektura, pogon predlog, oba pristopa k posodobitvi,
  posebnosti proizvajalcev, načeli (naučene točke / origin completeness), Excel popravek, testi,
  navodila kaj ne spreminjati.
- **`SPREMEMBE.md`** (ta dokument).

---

## 7. Hrošči, najdeni in odpravljeni med delom

- **KUKA `$config.dat`** — nova `SIGNAL do..._FREE $OUT[n]` vrstica se je ob Update prilepila na
  konec obstoječe vrstice (`...$OUT[33]SIGNAL doSTATION2_FREE $OUT[34]`, neveljavna KRL koda). Zdaj se
  vstavlja za koncem cele vrstice. Regresijski test to varuje.
- **Kawasaki prekrivanje točk** — regularni izraz za vrstice točk je zaradi `\s` (ki vključuje
  prelome vrstic) požrl več vrstic naenkrat in pokvaril prelome. Popravljeno na `[-\d. ]`.

---

## 8. Kaj namenoma NI spremenjeno

- **Generatorji (`Template.cs`, regije po proizvajalcih) niso "poravnani" z referenčno različico.**
  Drevesi (V3 in referenca) sta se namenoma razšli in obstoječi robotski programi so odvisni od
  točnega izhoda V3 generatorja. Zato izhod Generate ostaja nespremenjen; Import/Update se mu samo
  prilagaja.
- Mrtvi mapi `GenerateExcelFile/` in `GenerateExcelIO/` v korenu (star COM-Interop predhodnik) sta
  ostali nedotaknjeni.

---

## 9. Seznam spremenjenih / dodanih datotek

**Spremenjeno:**
- `TemplateGenerator.Core/Classes/Template.cs` (LoadError)
- `TemplateGenerator.Core/Classes/GenerateExcellIO.cs` (Excel popravek)
- `TemplateGenerator.Core/ViewModels/ShellViewModel.cs` (Import/Update, guard, OpenFolder)
- `TemplateGenerator.Wpf/Views/ShellView.xaml` in `ShellView.xaml.cs` (gumba + ročnika)
- `TemplateGenerator.Wpf/TemplateGenerator.Wpf.csproj` (kopiranje predlog)
- `TemplateGenerator.sln` (dodan testni projekt)

**Dodano:**
- `TemplateGenerator.Core/Classes/EpsonProjectImporter.cs`, `EpsonProjectUpdater.cs`
- `TemplateGenerator.Core/Classes/KukaHellaProjectImporter.cs`, `KukaHellaProjectUpdater.cs`
- `TemplateGenerator.Core/Classes/AbbHidriaProjectImporter.cs`, `AbbHidriaProjectUpdater.cs`
- `TemplateGenerator.Core/Classes/YamahaProjectImporter.cs`, `YamahaProjectUpdater.cs`
- `TemplateGenerator.Core/Classes/KawasakiProjectImporter.cs`, `KawasakiProjectUpdater.cs`
- `TemplateGenerator.Wpf/Templates/**` (predloge v virski kontroli)
- `TemplateGenerator.Tests/**` (testni projekt)
- `README.md`, `CLAUDE.md`, `SPREMEMBE.md`
