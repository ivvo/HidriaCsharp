# RTG – Generator programov za robotske celice (V3)

Namizno orodje (WPF), ki iz preprostega vnosa (robot + postaje) **zgenerira programsko kodo** za
robotske celice več proizvajalcev, ter zna obstoječ, že generiran projekt **prebrati nazaj (Import)**
in vanj **kirurško dodati nove postaje (Update)** brez izgube naučenih koordinat.

Podprti proizvajalci pri generiranju: **Epson RC+**, **KUKA Hella**, **ABB Hidria**, **ABB
Simulacija**, **Yamaha (RCX)**, **Kawasaki (AS)**.

> Ta različica ("V3") izhaja iz iste osnove kot referenčna verzija, a jo je nekdo naprej razvijal
> ločeno (dodan Kawasaki, drugačen Yamaha generator, bogatejši HMI, simulacijski način). Funkciji
> Import/Update in popravki so bili v to različico preneseni naknadno – glej "Kaj je novo" spodaj.

## Komu je namenjeno

Razvijalcem/integratorjem robotskih celic v podjetju, ki iz standardiziranih predlog generirajo
osnovno strukturo robotskega programa (homing, gibanja med postajami, IO signali, seznam točk) in ga
nato na robotu dokončajo (naučijo dejanske koordinate točk). Ni namenjeno končnim uporabnikom na
liniji – je razvojno orodje.

## Zagon

Prevedena aplikacija:

```
TemplateGenerator.Wpf\bin\Debug\net5.0-windows\TemplateGenerator.Wpf.exe
```

Poleg .exe MORA biti mapa `Templates\` (predloge) – kopira se samodejno ob gradnji. Če predloge
manjkajo, orodje ob generiranju javi jasno napako ("Predloge se niso naložile …") in ne zapiše
ničesar.

## Gradnja

Aktivna rešitev je `TemplateGenerator.sln` (projekti: `TemplateGenerator.Core`,
`TemplateGenerator.Wpf`, `TemplateGenerator.Tests`). Mapi `GenerateExcelFile\` in `GenerateExcelIO\`
v korenu sta mrtvi (stari COM-Interop predhodnik) in nista del rešitve.

```
dotnet build TemplateGenerator.sln
```

Če je rešitev odprta v Visual Studiu in aplikacija teče, je `TemplateGenerator.Wpf.dll` zaklenjen in
polna gradnja spodleti z `MSB3027`. V tem primeru zgradi samo jedro (tam je skoraj vsa logika in se
ne zaklene):

```
dotnet build TemplateGenerator.Core\TemplateGenerator.Core.csproj
```

## Uporaba

### Nov projekt (Generate)

1. **1° Robot name** – vpiši ime robota in **Add Robot** (privzeto je že dodan robot "robot" s
   postajo "Home").
2. **2° Station name** – vpiši ime postaje (največ 9 znakov, samo črke/številke), po potrebi obkljukaj
   **Add station free signal**, in **Add Station**. V tabeli lahko urediš `Pallet`, `Positions`,
   koordinate (X/Y/Z) in komentar.
3. **3° Select template** – izberi proizvajalca; po želji **Make program for simulation**.
4. **Generate** – izberi ciljno mapo. Epson/Yamaha/Kawasaki se zapišejo v časovno označeno podmapo
   (`<Proizvajalec>GeneratedTemplate_<datum_ura>`), KUKA/ABB neposredno v izbrano mapo.

### Dodajanje postaje v že obstoječ projekt (Import → Update)

Namenjeno **že delujoči celici**, kjer je treba dodati postajo brez izgube naučenih koordinat.

1. **Import Project** – izberi mapo z že generiranim projektom (za Epson/Yamaha izberi časovno
   označeno podmapo, ki neposredno vsebuje datoteke). Proizvajalec se zazna samodejno iz strukture
   mape. Tabela postaj se napolni z obstoječim stanjem.
2. Dodaj novo postajo (samo **na konec** seznama).
3. **Update Project** na isti mapi. Orodje najprej naredi varnostno kopijo cele mape
   (`<mapa>_backup_<datum_ura>`), nato vstavi kodo za novo postajo.

#### Podpora Import/Update po proizvajalcih

| Proizvajalec | Import (branje) | Update (dodaj postajo) |
|---|---|---|
| **KUKA Hella** | ✅ | ✅ (kirurško vstavljanje) |
| **ABB Hidria** | ✅ | ✅ kirurško (glej omejitve – EIO/OtherFunctions) |
| **Yamaha** | ✅ | ✅ re-generacija + ohranitev naučenih točk |
| **Epson Hidria** | ✅ | ✅ re-generacija + ohranitev naučenih točk (.pts) |
| **Kawasaki** | ✅ | ✅ re-generacija + ohranitev naučenih točk |

## Kaj je novo v tej različici

- **Import/Update za vse proizvajalce** (Epson, KUKA Hella, ABB Hidria, Yamaha, Kawasaki) — branje
  obstoječega projekta + dodajanje postaj; gumba **Import Project** / **Update Project** v vmesniku.
  Pri Epson/Yamaha/Kawasaki posodobitev deluje z re-generacijo kode + ohranitvijo naučenih točk (po
  imenu točke); pri KUKA/ABB s kirurškim vstavljanjem.
- **Popravek Excel IO tabele** (`IOTable.xlsx`): odpravljena korupcija ("nečitljiva vsebina /
  popravljeno") in napačno formatiranje byte.bit naslovov (npr. `3.0` → `3`).
- **Popravek KUKA `$config.dat`**: nova SIGNAL vrstica se ne prilepi več na obstoječo.
- Zaščita ob manjkajočih predlogah (`FormatsLoaded` / jasno sporočilo).

## Znane omejitve

- **Nove postaje samo na konec** seznama (ne vrivanje na sredino, preimenovanje ali brisanje prek
  Import/Update poti).
- **Epson / Yamaha / Kawasaki**: en robot/program na projekt; ob Update se koda **re-generira**, zato
  morebitni ročni popravki v generirani kodi niso ohranjeni – **naučene točke in komentarji pa SO**
  (ohranijo se po imenu točke: Epson `.pts` po `sLabel`, Yamaha `[PNT]` vključno z
  `pSafeL`/`pSafeR`/`pAboveStation`, Kawasaki po imenu točke `p<Postaja>` in `#pHome`).
- **KUKA/ABB Import/Update** (kirurško): en robot na projekt (projektno skupne datoteke se pri več
  robotih podvojijo – obstoječa omejitev generatorja); ohranijo se tudi ročni popravki kode.
- **ABB**: ob Update se `OtherFunctions.mod` (iskanje najbližje točke ob homingu) in
  `EIORobot.cfg`/`EIOSimulacija.cfg` ne osvežijo – zanje je potreben poln **Generate**.
- Nekaj elementov vmesnika ni funkcionalnih (neodvisno od teh sprememb): **Remove Robot/Station** in
  gumb **test button**.

## Testi

Projekt `TemplateGenerator.Tests` (xUnit, net8.0) pokriva Import/Update za KUKA/ABB/Yamaha,
usmerjanje po proizvajalcu, zaklenjeno Epson posodobitev in popravek Excel tabele.

```
dotnet test TemplateGenerator.sln
```

Najmočnejši test je **Yamaha bajt-identičnost**: posodobljen projekt mora biti bajt-za-bajt enak
svežemu Generate celotnega (končnega) seznama postaj. Podrobnosti arhitekture: [CLAUDE.md](CLAUDE.md).

## Varnostna kopija

Pred vsakim **Update** se naredi popolna varnostna kopija projektne mape
(`<mapa>_backup_<datum_ura>`). Kljub temu je za produkcijske projekte priporočljivo imeti ločeno
varnostno kopijo.
