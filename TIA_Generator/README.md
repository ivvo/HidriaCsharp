# TIA_Generator

Generator HMI po **HMI Standardu 2.0** (Hidria). Iz PLC programa (TIA export »Export all«) predpripravi HMI – PC (WinCC RT Advanced) in panel (Comfort).
Specifikacija: `D:\Claude\Dokumentacije\Generator\HMI_GENERATOR_SPEC.md` (+ `primer_hmimodel_purem.json`).

## Stanje

| Faza | Vsebina | Stanje |
|---|---|---|
| 1 | `model`: model stroja (JSON) iz exporta | **narejeno** (2026-11-04) – glej `docs\NAPREDEK.md` |
| 2 | PC tag tabele + `Postaja_<oznaka>` za eno enoto | čaka na knjižnico predlog (čista predloga projekta, spec 9.1) |
| 3–6 | vse enote, panel, alarmi, GUI | – |

## Struktura

```
TIA_Generator.sln
  src\TIA_Generator.Core   (netstandard2.0)  Model\ (MachineModel, ModelJson), Extract\ (ModelExtractor, TextDictionary), Data\slovar.json
  src\TIA_Generator.Cli    (net48, TIA_Generator.exe)
  ..\TIA_ToolKit\src\TIA_ToolKit.Analyzer + .Common   (ProjectReference – ExportReader, DbLayout)
```

`TIA_ToolKit` mora biti na veji `tia-toolkit` (tam je Analyzer). Analyzer se ob gradnji prevede v `D:\C#\TIA_ToolKit\bin\x64\Debug\` (njegov Directory.Build.props) in prekopira k `TIA_Generator.exe`.

## Gradnja

```
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" TIA_Generator.sln -restore -p:Configuration=Debug -p:Platform=x64
```
Izhod: `bin\x64\Debug\TIA_Generator.exe`. Ne potrebuje TIA Portala.

## Uporaba

```
TIA_Generator model <exportDir> [-o <x.hmimodel.json>] [-s <slovar.json>]...
TIA_Generator show  <x.hmimodel.json> [<enota>]
TIA_Generator plcchanges <x.hmimodel.json> [-o <spremembe.csv>]
```
- `plcchanges` – potrebne spremembe PLC za standard 2.0: člani S5Time → `Delay[0..9]` (odločitev 2026-11-04) z bloki/networki, kjer so uporabljeni; CSV za Excel.
- `model` – privzeti izhod je **nadrejena** mapa exporta (`<projekt>.hmimodel.json`), ker Export all ob ponovnem izvozu briše svojo mapo.
- `-s` – dodatni slovar besedil (ista oblika kot `Data\slovar.json`), dopolni/prepiše vgrajenega.
- `show` – tabela enot (indeks PLC/HMI, Settings DB, časovniki, parametri, recept, cilindri, alarmi) ali JSON ene enote.

## Kako se model izpelje (pravila)

| Podatek | Pravilo |
|---|---|
| Enota | FB `Unit_<oznaka>`, ki ustreza vzorcu oznake (A01, B02_1, 010, T20, A01_R …) **ali** ima `Unit_<oznaka>_DB_Settings` (BMW G `Unit_R_G10_EQ_1`) |
| Del / skupina | mapa `Machine/<črka>/…` → del; drugače `Machine/<ime>/…` → skupina (Dosing, Rotor). Brez map po delih: del iz oznake (A01 → A) |
| Linija | oznaka `R_…`/`S_…` |
| Opis | ime mape za `" - "` (samo SL; EN/DE ročno) |
| PLC indeks | indeks `DB_Service_Settings.<polje>[n]` z največ različnimi funkcijami v FB; OFF/ENABLE_HMI_COMMAND/RESET_OFF/STEP štejejo pol (podpostaje jih berejo z indeksom nadrejene) → `inheritsOffFromIndex`. HMI = PLC − 1 |
| Časovniki | `Array[..] of S5Time` (standard 2.0) ali prvih ≤10 članov S5Time (stari projekti → `TIMERS-NOT-ARRAY`, HMI tag vseeno simbolni na `"…_DB_Settings".Delay`, `requiresPlcChange`); `*Spare*` → `show=false` |
| Parametri | ostali člani Settings DB, strukture razširjene (`CRIMP_POSITION.Value_Max`); `access` = display, če ime vsebuje ACT/Archive/Output/Status …, sicer input (hevristika) |
| Recept | člani `Recipe` (DB2000) s predpono enote; predpona nadrejene (`B02_`) → vse podpostaje |
| Cilindri | Bool člani DB4000 `<predpona>_<ime>_(F|B|O|C)[_CMD]`; predpona = oznaka, `U_<oznaka>`, `U<oznaka>`, `U_<št. mape>`; `arrayIndex` = bitni odmik (indeks v HMI polju `Cylinder_Control`) |
| Alarmi | Bool člani `DB_Error_Messages`; razred Warnings za `Warn*/Warin*`, sicer Errors; enota = najdaljša oznaka v imenu |
| Besedila | `Data\slovar.json`: cela fraza → `slovar`; vse besede znane → `sestavljeno` (predlog, preveri); sicer `manjka` (ročno) |

## Kode opozoril

`IDX-NONE`, `IDX-MIXED`, `IDX-RANGE` (izven razpona dela po standardu 5.3), `IDX-DUP`, `SETTINGS-MISSING`, `TIMERS-NOT-ARRAY`, `TIMERS-NONE`, `TIMER-FOREIGN` (časovnik enote v FB druge enote),
`DB-MISSING`, `CYL-ENABLE-POS` (Enable_Manual_Control ni prvi bit), `CYL-INCOMPLETE`, `CYL-ORPHAN`, `CYL-NOLAYOUT`.
