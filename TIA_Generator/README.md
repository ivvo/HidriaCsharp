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
  src\TIA_Generator.Gui    (net48 WinForms, TIA_GeneratorGui.exe) – grafični vmesnik, zagonski projekt
  src\TIA_Generator.Cli    (net48, TIA_Generator.exe)
  ..\TIA_ToolKit\src\TIA_ToolKit.Analyzer + .Common   (ProjectReference – ExportReader, DbLayout)
```

`TIA_ToolKit` mora biti na veji `tia-toolkit` (tam je Analyzer). Analyzer se ob gradnji prevede v `D:\C#\TIA_ToolKit\bin\x64\Debug\` (njegov Directory.Build.props) in prekopira k `TIA_Generator.exe`.

## Gradnja

```
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" TIA_Generator.sln -restore -p:Configuration=Debug -p:Platform=x64
```
Izhod: `bin\x64\Debug\TIA_Generator.exe`. Ne potrebuje TIA Portala.

## Struktura stroja (glavni cilj)

Stroj = sektorji in postaje po standardu 5.3:

| Primer | FB postaj | PLC indeks | HMI indeks |
|---|---|---|---|
| 3 sektorji po 5 postaj | `Unit_A01…A05`, `Unit_B01…B05`, `Unit_C01…C05` | A: 11–15, B: 21–25, C: 41–45 (osnova A 10, B 20, C 40, D 60, E 80 + št. postaje) | PLC − 1 |
| podpostaje A01 (2×) | `Unit_A01_1`, `Unit_A01_2` | naslednja prosta v razponu sektorja (16, 17) – urejljivo | PLC − 1 |
| en sektor, 4 postaje | `Unit_01…Unit_04` | 1–4 | 0–3 |

- **Nov stroj …** (Ctrl+N): vpišeš ime, sektorje (A–E, opis, število postaj), podpostaje in opise postaj; PLC indeksi se dodelijo samodejno (urejljivi), opozorila za izven razpona / podvojene.
- **Struktura …**: isto za obstoječ model – predizpolni se iz exporta (manjkajoče številke, npr. Purem B05, ostanejo izpuščene; Purem B02 je samo »nosilec« podpostaj B02_1..4). Nove postaje dobijo `origin: "struktura"`.
- **PLC ogrodje …**: za nove postaje ustvari SimaticML XML – klon vzorčne enote (privzeto Purem `Unit_A05` »Spare«, standardno ogrodje s 40 networki):
  `Unit_<oznaka>` (FB = PLC indeks), `Unit_<oznaka>_DB` (instanca = PLC indeks), `Unit_<oznaka>_DB_Settings` (100 + indeks; `Delay : Array[0..9] of S5Time` + `Part_n_BAD_Count`).
  V FB se zamenjajo oznaka (`Warning_OFF_A05` → `Warning_OFF_C03_1`), indeks v poljih `DB_*` (`OFF[15]` → `OFF[46]`) in časovniki (`Delay_Position_Error` → `Delay[0]`).
  Poročilo `PLC_ogrodje_porocilo.txt`: potrebni simboli (biti v DB_Error_Messages, PLC tagi `U_<oznaka>_RESET_ENABLED`), trki številk z obstoječimi bloki.
  Uvoz v TIA: Openness ali Version Control Interface (uvoz iz programa še ni narejen).
- CLI: `newmachine "<ime>" --sectors 3 --stations 5 --sub A01=2,C03=1 -o x.hmimodel.json`, `plcskeleton x.hmimodel.json [--template <export>] [--unit A05] -o <mapa>`.

## Grafični vmesnik – `TIA_GeneratorGui.exe`

Zagon: `bin\x64\Debug\TIA_GeneratorGui.exe` ali F5 v Visual Studiu (zagonski projekt `TIA_Generator.Gui`, profili *GUI*, *GUI – model Purem*, *GUI – export BMW G*).

- **Izdelaj model iz exporta …** (Ctrl+E) – izbira mape Export all; branje teče v ozadju, napredek v zavihku *Dnevnik*. Če je v mapi več exportov, ponudi izbiro.
- **Odpri model …** (Ctrl+O), **Shrani** (Ctrl+S), **Shrani kot …** – `*.hmimodel.json`; neshranjene spremembe so označene z `*` v naslovu.
- Levo: enote (filter po oznaki/opisu; stolpec *Opoz.* = število opozoril enote).
- Desno, zavihki izbrane enote: **Enota** (opis SL/EN/DE, PLC indeks → HMI indeks, prikaz na PC/panelu, podatki iz PLC in opozorila),
  **Časovniki** (prikaz da/ne, besedila, kje v PLC so uporabljeni), **Parametri** (input/display, enota mere, besedila), **Recept**, **Cilindri** (indeksi v `Cylinder_Control`), **Alarmi** (razred, besedila).
- Rumeno ozadje = urejljivo besedilo; ročno spremenjeno besedilo dobi izvor `rocno`.
- Spodaj: **Opozorila** (dvoklik → skoči na enoto), **Povzetek**, **Alarmi brez enote**, **Dnevnik**.
- **Spremembe PLC (CSV) …** – isto kot `plcchanges`.
- Lokalni slovar: `slovar_lokalni.json` poleg exe se samodejno doda vgrajenemu.
- Ukazna vrstica: `TIA_GeneratorGui.exe [<model.hmimodel.json>] [--export <dir>] [--unit <oznaka>] [--tab <n>] [--snapshot <png>]` (`--snapshot` = razvoj: izris okna v PNG).

## Uporaba (CLI)

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
