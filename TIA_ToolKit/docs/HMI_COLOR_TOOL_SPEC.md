# HMI Color Standardization Tool — spec za nadaljnji razvoj

## Cilj

GUI orodje (namesto konzole), ki:
1. Poišče vse HMI targete v TIA Portal projektu (WinCC RT Advanced, morda tudi mobilni paneli).
2. Exporta VSE zaslone (in po možnosti Screen Templates) izbranih targetov v XML.
3. Omogoči uporabniku, da definira/uredi mapiranje barv (stara RGB → nova RGB), idealno s klikljivimi swatchi za odobreno paleto.
4. Izvede bulk find-replace teh barv čez vse exportirane XML datoteke (dry-run opcija pred dejanskim pisanjem).
5. Uvozi spremenjene XML datoteke nazaj v TIA Portal.
6. NE shrani projekta samodejno — uporabnik ročno pregleda in shrani v TIA Portalu.

## Okolje

- TIA Portal **V20**, projekt uporablja **WinCC RT Advanced** (Comfort/Advanced family, ne Unified).
- Projekt ima **več HMI targetov** hkrati — potrjeno v realnem projektu: `HMI_RT_1 [WinCC RT Advanced]` (gnezden znotraj `PC-System [SIMATIC PC station]`) IN `HMI_2 [KTP700F Mobile]` (top-level naprava). Orodje mora dovoliti izbiro, kateri target(i) se obdelajo — NE avtomatsko vzeti prvega najdenega.
- Openness zahteva **.NET Framework 4.8** (ne .NET Core/5+), referenco na `Siemens.Engineering.dll` iz `C:\Program Files\Siemens\Automation\Portal V20\PublicAPI\V20\`, in članstvo v Windows skupini **"Siemens TIA Openness"** (pri Ivotu že potrjeno članstvo).
- Program se povezuje na **že odprt** TIA Portal proces (`TiaPortal.GetProcesses()[0].Attach()`), ne odpira novega projekta sam.

## Potrjeni delujoči Openness vzorci (iz realnega testiranja)

### Priklop in iskanje HMI targetov (deluje, testirano)
```csharp
var processes = TiaPortal.GetProcesses();
TiaPortal tiaPortal = processes[0].Attach();
Project project = tiaPortal.Projects[0];

// Rekurzivno iskanje HMI targetov - deluje TUDI, ko je HMI gnezden
// znotraj PC-System naprave (ne samo top-level naprave)
static IEnumerable<HmiTarget> FindAllHmiTargets(Project project)
{
    foreach (var device in GetAllDevices(project))
    {
        var target = GetHmiTargetFromDevice(device);
        if (target != null) yield return target;
    }
}
// device.DeviceItems rekurzivno -> item.GetService<SoftwareContainer>().Software as HmiTarget
```
Polna implementacija je v priloženem `ExportOneHmiScreen.cs` / `ImportOneHmiScreen.cs`.

### Export zaslona (potrjeno delujoče, testirano na resničnem projektu)
```csharp
Screen screen = ...; // najden rekurzivno po hmiTarget.ScreenFolder.Screens / .Folders
screen.Export(new FileInfo(outFile), ExportOptions.WithDefaults);
```
Rekurzivno iskanje po vseh podmapah (`ScreenUserFolder.Folders`) je nujno — zasloni so lahko gnezdeni 2-3 nivoje globoko (npr. `Screens/Glavni_Meni_Staticni/Nastavitve/<zaslon>`).

### Import zaslona (NEPOTRJENO ImportOptions ime — PREVERI!)
```csharp
ScreenComposition targetCollection = ...; // mapa, kjer zaslon trenutno živi
targetCollection.Import(new FileInfo(importFilePath), ImportOptions.Override);
```
`ImportOptions.Override` je najboljše ugibanje po analogiji z drugimi Openness Import metodami (PLC bloki uporabljajo enak vzorec) — NI bilo mogoče potrditi iz uradne dokumentacije (JS-rendered stran). **Prva stvar za preveriti v Claude Code: zgradi minimalen test in preglej IntelliSense/compiler error za pravo ime `ImportOptions` vrednosti.**

## Potrjena oblika barv v XML exportu (iz resničnega exporta)

Barve so zapisane decimalno, z vejico IN presledkom:
```xml
<BackColor>24, 106, 154</BackColor>
<BorderColor>0, 163, 194</BorderColor>
<ForeColor>198, 209, 225</ForeColor>
```
Regex za find-replace mora upoštevati `\s*,\s*` med vrednostmi. Barve se pojavljajo v treh vlogah, ki jih NE smemo mešati pri prikazu uporabniku (border je enako pomemben kot fill):
- `BackColor` (ozadje/fill) — a POZOR: če je `BackFillStyle=Transparent`, se `BackColor` dejansko ne prikaže (pogosto placeholder `255, 0, 255`) — te primere izpusti iz "aktivnih" barv.
- `BorderColor` (obroba) — velja samo, če je `BorderWidth > 0`.
- `ForeColor` (barva besedila) — na `TextField`, `Button`, `IOField`.

## Odobrena barvna paleta (Hidria standard)

```
29, 83, 131
24, 106, 154
0, 126, 172
0, 147, 190
0, 163, 194
42, 180, 198
116, 198, 199
```

Primer zahtevane zamenjave: `59, 101, 151` → `29, 83, 131` (glavna stara modra v novo temnejšo modro).

## Obstoječe datoteke (izhodišče, priloži jih v Claude Code projekt)

- `HmiColorReplacer.cs` — samostojen text-based XML color replacer (regex, brez Siemens odvisnosti). Deluje kot ločena konzolna utility, logiko lahko prevzameš v GUI.
- `ExportOneHmiScreen.cs` — deluje, testirano na enem zaslonu. Vsebuje delujočo `FindAllHmiTargets` in rekurzivno iskanje zaslonov po mapah.
- `ImportOneHmiScreen.cs` — deluje razen neznanega `ImportOptions` imena (glej zgoraj).

## GUI zahteve

- **WinForms** (.NET Framework 4.8) — hitrejši razvoj za interno orodje kot WPF.
- Seznam vseh najdenih HMI targetov s checkboxi (uporabnik izbere, kateri se obdelajo — privzeto nič ni izbrano, da se izogne nesreči).
- Urejevalnik barvnega mapiranja: seznam parov (stara barva → nova), z:
  - swatch prikazom obeh barv,
  - klikljivimi gumbi odobrene palete (7 barv zgoraj) za hitro izbiro nove barve,
  - color picker za poljubno barvo,
  - možnostjo dodajanja/brisanja parov.
- Gumbi za ločene faze: **Export vse**, **Zamenjaj barve (dry-run)**, **Zamenjaj barve (dejansko)**, **Import vse**.
- Log okno (TextBox/ListBox), ki izpisuje napredek in napake — enako podrobno kot trenutni konzolni izpis (koliko targetov, koliko zaslonov, koliko zamenjav na datoteko).
- Program NIKOLI ne shrani TIA projekta samodejno.

## Kaj še ni bilo testirano / naslednji koraki

1. Potrditev pravega imena `ImportOptions` (glej zgoraj).
2. Export/import **Screen Templates** (ne samo navadnih zaslonov) — `Nastavitve_ETALON` template verjetno nosi barve, ki jih deduje več zaslonov.
3. Testiranje na **drugem HMI targetu** (`HMI_2 [KTP700F Mobile]`) — morda ima drugačno strukturo/manj zaslonov.
4. Preverba, kako `screen.Export`/`Import` ravna s slikami/graphic lists, ki jih zaslon referencira (npr. `icons8-settings-80` v `GraphicView`).
