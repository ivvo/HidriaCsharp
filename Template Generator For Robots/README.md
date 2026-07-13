# Template Generator za robote

Namizno orodje (Windows, WPF), ki iz preprostega seznama "Programov" (robotov) in "Postaj" znotraj
vsakega programa generira ogrodje kode za robotsko celico — namesto da bi se enaka struktura
(homing, dispatch, gibanje med postajami, IO oznake ...) vsakič znova ročno pisala za Epson RC+,
KUKA Hella, ABB Hidria ali ABB Simulacija.

Poleg generiranja "od začetka" zna orodje tudi **uvoziti že prej generiran, dejansko delujoč
projekt** nazaj v svoj model in vanj **dodati novo postajo brez polnega prepisa** — obstoječa,
morda ročno popravljena koda in **na robotu naučene koordinate točk ostanejo nedotaknjene**.

## Komu je namenjeno

Inženirjem, ki pripravljajo/vzdržujejo programe za robotske celice. Osnovno generiranje je za
popolnoma nove celice; Import/Update je za primer, ko celica že obratuje in je treba dodati eno
postajo, ne da bi bilo treba vse na novo generirati (in s tem tvegati izgubo že naučenih pozicij).

## Kako zagnati

Potreben je Windows (aplikacija je WPF).

**Samo zagon** (če je že zgrajeno): dvoklik na

```
TemplateGenerator.Wpf\bin\Debug\net5.0-windows\TemplateGenerator.Wpf.exe
```

**Gradnja iz izvorne kode**: odpri `TemplateGenerator.sln` v Visual Studiu (Build → Build Solution),
ali iz ukazne vrstice:

```
dotnet build TemplateGenerator.sln
```

Mapa `Templates\` (predloge kode po proizvajalcu) je del izvorne kode
(`TemplateGenerator.Wpf\Templates\`) in se ob vsaki gradnji sama prekopira poleg `.exe`-ja — ni je
treba ročno prestavljati.

## Uporaba

### Nov projekt od začetka

1. V prvo polje vnesi ime programa/robota (npr. "Robot1") in klikni **Add Program/Robot** (ali
   Enter). Vsak nov program samodejno dobi eno postajo z imenom "Home".
2. V spustnem seznamu desno zgoraj izberi program, na katerega se nanašajo naslednje postaje.
3. Vnesi ime postaje, po potrebi obkljukaj **Station free** (postaja ima "zasedeno/prosto"
   signal), klikni **Add Station** (ali Enter). Ime sme vsebovati samo črke in številke.
4. Za postajo s paleto ali več pozicijami (`Pallet`, `Positions`) ročno uredi ustrezno celico v
   tabeli spodaj po tem, ko je postaja že dodana — v vnosni vrstici teh dveh polj ni.
5. V vrstici **Select template** izberi proizvajalca (Epson Hidria / KUKA Hella / ABB Hidria /
   ABB Simulacija).
6. Klikni **Generate**, izberi ciljno mapo. Orodje zapiše vse datoteke tega proizvajalca in odpre
   mapo v Raziskovalcu. Sporočilo o uspehu/napaki se izpiše v vrstici na dnu okna.

### Dodajanje postaje v že obstoječ (prej generiran) projekt

To je namenjeno **že delujoči celici**, kjer je treba dodati eno postajo, ne da bi se izgubile
naučene koordinate ali morebitni ročni popravki v generirani kodi.

1. Klikni **Import Project**, izberi mapo z že generiranim projektom. Proizvajalec se zazna
   samodejno iz strukture mape (`*_Motion.mod` → ABB Hidria, `R1\Program\` → KUKA Hella, `*.prg` →
   Epson Hidria, `*.all` → Yamaha). Tabela postaj se napolni z obstoječim stanjem.
2. Dodaj novo postajo enako kot zgoraj (korak 3): ime, po potrebi **Station free**, **Add Station**.
   Nova postaja se lahko doda samo **na konec** obstoječega seznama.
3. Klikni **Update Project** na isti mapi. Orodje najprej naredi varnostno kopijo cele mape
   (`<ime_mape>_backup_<datum_ura>` poleg izvirne), nato v obstoječe datoteke kirurško vstavi samo
   kodo, ki se nanaša na novo postajo — preostala koda (vključno z ročnimi popravki in naučenimi
   koordinatami v `.pts`/`*_motion.dat`/`Global.mod`/`.all` `[PNT]`) ostane bit-za-bit nedotaknjena.

Import/Update je podprt za **Epson Hidria, KUKA Hella, ABB Hidria in Yamaha** (za Yamaho je celoten
program — koda, naučene točke `[PNT]` in IO oznake — v eni `.all` datoteki na robota).

## Znane omejitve

- **En program/robot na projekt** za uvoz/posodobitev pri KUKA Hella in ABB Hidria (generator ima
  neodvisno od te funkcionalnosti obstoječo omejitev, da so `$config.dat`/`robotFunctions.src`
  oz. `Global.mod`/`Communication.mod`/`robot_Main.mod` projektno skupne, ne po-robotske datoteke).
- Nove postaje je mogoče samo **dodajati na konec** seznama — ne vrivati na sredino, preimenovati
  ali brisati prek Import/Update poti.
- Pri ABB Hidria se `OtherFunctions.mod` (iskanje najbližje točke ob homingu) in
  `EIORobot.cfg`/`EIOSimulacija.cfg` ob **Update** ne osvežita — zanju je potreben poln **Generate**.
- Nekaj obstoječih UI elementov ni funkcionalnih (neodvisno od letošnjih sprememb): gumb
  **Remove Robot/Program** in **Remove Station** nimata delujoče logike v ozadju, checkbox
  **Generate Simple Program** ni povezan z dejanskim nastavitvijo (obnašanje je fiksno "simple").

## Kako deluje v kozi (za razvijalce)

Polna arhitektura je opisana v [CLAUDE.md](CLAUDE.md). Na kratko:

- **`TemplateGenerator.Core\Classes\Template.cs`** — ob zagonu prebere `.txt` predloge iz
  `Templates\<Proizvajalec>\` in jih z `string.Format` po postajah/programih sestavi v kodo. To je
  edina pot za **Generate**.
- **`*ProjectImporter.cs`** (Epson/KukaHella/AbbHidria/Yamaha) — z regex vzorci prepozna postaje in
  njihove zastavice (`StationFreeEnabled`/`Pallet`/`Positions`) iz že generirane kode nazaj v model.
- **`*ProjectUpdater.cs`** — v obstoječe datoteke vstavi samo kodo za na novo dodano postajo. Dve
  ključni pravili, ki ju je treba upoštevati ob vsakem dotiku te kode: (1) datoteke z na robotu
  naučenimi koordinatami (`.pts`, `*_motion.dat`, `Global.mod`, Yamaha `.all` `[PNT]`) se nikoli ne
  prepišejo v celoti, samo dopolnijo; (2) vsaka "pojdi na postajo X" funkcija mora po dodajanju nove
  postaje znati priti tudi **iz** te nove postaje (t. i. "origin completeness"), ne samo do nje.
  Yamaha ima poleg tega isto številčno-indeksno past kot Epson: `*findClosestPoint:` išče najbližjo
  naučeno točko po zaporednem indeksu (`FOR i = 0 TO N`, `P[i]`), zato mora primarna točka nove
  postaje priti na pravo mesto, obstoječe dodatne točke pa se prenumerirajo navzgor.
- **`ShellViewModel.cs`** — MVVM model UI-ja; `ImportProject`/`UpdateProject` samodejno zaznata
  proizvajalca in usmerita na pravi Importer/Updater.

## Testiranje

`TemplateGenerator.Tests` (xUnit) je razvijalsko orodje brez uporabniškega vmesnika — nihče, ki
uporablja aplikacijo, se z njim ne sreča. Poganja se z:

```
dotnet test TemplateGenerator.Tests\TemplateGenerator.Tests.csproj
```

Vsak test si sam ustvari svojo prazno mapo v temp direktoriju (`TestHelpers.CreateTempDir`) in jo
po sebi počisti (`Dispose`, vključno z morebitno varnostno kopijo, ki jo naredi `*ProjectUpdater`)
— testi se ne prekrivajo, tudi če xUnit razrede požene vzporedno.

### `TestHelpers.cs` — skupna infrastruktura

- **`CreateTempDir()`** / **`DeleteDirSafely()`** — prazna delovna mapa za en test in njeno
  čiščenje (v `try/catch`, da neuspešno čiščenje ne podre samega testa).
- **`BuildAndGenerate(template, outDir, stations...)`** — požene isti tok kot uporabnik v UI-ju:
  `ShellViewModel.AddProgram()` → `AddStation()` za vsako postajo → `GenerateProject(outDir)`.
  Vrne `ShellViewModel`, da lahko test primerja izvirni model z uvoženim.
- **`IsSubsequenceOfLines(before, after)`** — preveri, da se vsaka vrstica iz `before` v istem
  vrstnem redu pojavi v `after` (torej: samo vstavljanje, nič odstranjeno/prerazporejeno). Uporabljajo
  ga KUKA/ABB testi, kjer `Update` nikoli ne spremeni obstoječe vrstice, samo doda nove.
  - **`AssertOnlyExpectedLineChanges(before, after, dovoljeniVzorci...)`** — strožja/prilagodljivejša
  različica za Epson: vsaka vrstica iz `before`, ki je dobesedno ni v `after`, mora ustrezati enemu
  od podanih regex vzorcev (npr. `For index = 0 To \d+`, ker se Epsonova homing zanka namerno
  poveča ob vsaki dodani postaji) — sicer test pade z izpisom natančno katere vrstice so izginile.
- **`ExtractBetween(content, startMarker, endMarker)`** — izreže telo ene funkcije/procedure (npr.
  med `"Function robot1_goStation1()"` in `"Fend"`), da testi preverjajo vsebino ene same funkcije
  namesto cele datoteke.

### Testi po proizvajalcu (`EpsonImportUpdateTests`, `KukaHellaImportUpdateTests`, `AbbHidriaImportUpdateTests`)

V vsakem razredu ista skupina scenarijev (imena metod se med proizvajalci razlikujejo le v
podrobnostih formata datotek):

- **`Import_RoundTrips_StationListAndFlags`** — generiraj → uvozi → primerjaj imena postaj in
  `StationFreeEnabled` uvoženega modela z izvirnim. Preveri, da `*ProjectImporter` pravilno
  razbere nazaj to, kar je bilo generirano.
- **`Update_AppendsNewStation_AndPreservesExistingLines`** — generiraj 2 postaji → uvozi → dodaj
  3. postajo → posodobi → preveri (a) da so vse obstoječe vrstice ostale (glej zgoraj), (b) da se
  koda nove postaje dejansko pojavi (`"Function robot1_goStation3()"` ipd.).
- **`Update_NewStation_BecomesOriginInExistingGoFunctions`** — po dodajanju postaje preveri, da
  `Function robot1_goStation1()` (obstoječa, nedotaknjena postaja) vsebuje sklic na novo postajo kot
  možen izvor (`"ROBOT1_FROM_STATION3"`). To je regresijski test za bug, ki sem ga odkril in
  popravil med gradnjo te funkcionalnosti: brez tega bi robot ob premiku *stran* od nove postaje
  javil napako.
- **`Update_PreservesTaughtCoordinates`** — ročno "nauči" eno koordinato v generirani datoteki z
  regexom (simulira, kar inženir naredi na robotu s pendantom), dodaj postajo, posodobi, preveri
  bit-za-bit da je stara koordinata ostala in da je nova postaja dobila svojo (ničelno) točko.
  Regresijski test za bug, kjer je zgodnja različica te funkcije ob dodajanju postaje **pobrisala
  že naučene koordinate**.
- **`Import_ThrowsClearException_When...`** — na prazni/neustrezni mapi preveri, da `*ProjectImporter`
  vrže svojo tipizirano izjemo (`EpsonImportException` ipd.) z razumljivim sporočilom, namesto da
  bi tiho vrnil napačne podatke.

Dodatno v `EpsonImportUpdateTests`:
- **`Update_WithPalletStation_InsertsNewPrimaryPointAtCorrectIndex_AndBumpsHomingLoopBound`** —
  edini test, specifičen za en proizvajalec: naredi postajo s `Pallet = true` (dodatne P2/P3/P4
  točke v `.pts`), doda novo postajo, preveri da nova primarna točka pristane na pravem indeksu
  (ne na koncu datoteke) in da se Epsonova `For index = 0 To N` homing zanka pravilno poveča. To je
  regresijski test za bug, ki ga je uporabnik dejansko naletel na resničnem PickPlace projektu.

Dodatno v `KukaHellaImportUpdateTests`:
- **`Update_ExtendsConfigDatEnumLists`** — preveri, da `$config.dat`-ov `ENUM ROBOT_FROM_LOC`/
  `ENUM ROBOT_TO_LOC` seznam dobi novo postajo dodano na konec.

### `ShellViewModelRoutingTests.cs`

- **`ImportProject_AutoDetectsCorrectVendor`** (en test, pognan trikrat — enkrat na proizvajalca) —
  generiraj z enim proizvajalcem, kliči `ImportProject` **brez** predhodno izbranega `SelectedTemplate`,
  preveri da se pravi proizvajalec zazna samo iz oblike mape.
  - **`ImportProject_ReportsClearError_ForUnrecognizedFolder`** — na prazni mapi preveri jasno
  sporočilo namesto neusmerjene napake.

### Kako "pokvariti nalašč" in preveriti, da test dejansko nekaj lovi

Med gradnjo sem to preveril tudi obratno: v `EpsonProjectUpdater.PatchExistingGoFunctionsForNewOrigin`
sem začasno dodal `return content;` na začetek (fix se sploh ne izvede), pognal `dotnet test` —
natanko `Update_NewStation_BecomesOriginInExistingGoFunctions` je padel, ostalih 20 je ostalo
zelenih — kar potrjuje, da test meri to, kar naj bi meril, ne da je "vedno zelen". Enak trik je
uporaben vsakič, ko se doda nov test, da se prepriča, da dejansko nekaj preverja.
