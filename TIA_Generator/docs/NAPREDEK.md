# TIA_Generator – dnevnik napredka

## 2026-11-04 – Faza 1 (model stroja) narejena

**Odločitve uporabnika (spec pogl. 9):**
- koda v `D:\C#\TIA_Generator`, branje exporta prek **ProjectReference** na `TIA_ToolKit.Analyzer` (ne kopija);
- besedila: **slovar po imenu člana + ročni popravek** v modelu (brez pobiranja iz starih zaslonov – lahko kasneje);
- HMI tagi enot **simbolni** (ControllerTag); izjema le časovniki starih projektov (glej spodaj);
- obseg te seje: samo faza 1.

**Rezultat na Purem support pin (merilo spec pogl. 8):**
- 29 enot ✔, B02_1 → DB141 ✔, PLC indeks 41 / HMI 40 ✔, 4 časovniki + 6 Spare ✔, 6 cilindrov B02_1 iz DB4000 ✔ (Blocking, Ver, Ver_Limit, Gripper, Epson_Vacuum, Crimp);
- 383 parametrov, 108 receptnih, 117 cilindrov, 715 alarmov; besedila: slovar 101, sestavljena ~914, manjka ~316.

**Preizkušeno še na:** PHEV Rotor (12 enot, 18 cilindrov), PHEV Stator (14 enot, 21 cilindrov – DB4000 `U_02` = mapa »Unit 2 020«), 122A (11 enot, 19 cil.), BMW A (5), BMW G (14 enot, linije R/S, cilindri z O/C).

**Ugotovitve iz exportov (pomembno za fazo 2+):**
- **Noben** pregledan projekt nima časovnikov kot `Array[0..9] of S5Time` – vsi imajo 10 posameznih članov S5Time na začetku Settings DB. Stari HMI je to reševal z absolutnim tagom `%DB141.DBX0.0`, `Array [0..9] of S5Time`. Simbolni tag zahteva spremembo PLC (polje) → odločitev pred fazo 2.
- Purem HMI tag `Cylinder_Control` = `Array [0..399] of Bool` na `%DB4000.DBX0.0`, DB4000 pa ima bite do 477 (npr. `B02_1_Crimp_F_CMD` = 426) → stari tag je **prekratek**. Model nosi `global.cylinderControlArraySize`.
- Podpostaje (A01_1..3) in dozirni moduli (D01..D04) berejo `OFF[n]` nadrejene postaje, lastna stanja pa pišejo s svojim indeksom (51–53, 37/38) → `inheritsOffFromIndex`.
- Purem indeksi B02_x (41–44) in A01_x (51–53) so izven razponov standarda 2.0 (`IDX-RANGE`) – pričakovano za star projekt.
- `Enable_Manual_Control` ni prvi bit DB4000 v PHEV/122A/BMW (`CYL-ENABLE-POS`).

### Odločitev: časovniki v PLC v polje (2026-11-04)
Uporabnik je izbral **»PLC v polje«**: 10 članov S5Time v vsakem `Unit_<oznaka>_DB_Settings` se v PLC zamenja s poljem
`Delay : Array[0..9] of S5Time` na začetku DB-ja (ime `Delay` po primeru v spec; standard ime ne predpiše). HMI tag `<oznaka>_Timers` je simbolni
`"Unit_<oznaka>_DB_Settings".Delay` (`requiresPlcChange: true`, dokler sprememba ni narejena).
- Model nosi za vsak časovnik `member → arrayElement` (`Delay_Position_Error → Delay[0]`) in `usedIn` (bloki + networki, ki ga uporabljajo).
- Ukaz `plcchanges <model> [-o x.csv]` izpiše seznam sprememb. Purem: 29 DB-jev, 290 članov, 104 mest uporabe → `D:\Claude\TiaFullExport\Purem support pin\Project1_V20_PLC_spremembe.csv`.
- Novo opozorilo **TIMER-FOREIGN**: časovnik enote uporablja FB druge enote. Purem: `Unit_A01_1` NW 2 bere `Unit_A01_DB_Settings.Delay_Position_Error`; BMW G: `Unit_R_GT2_Robot` NW 190 bere `Unit_S_GT1_Robot_DB_Settings.Delay_Vision_XY_Tout` (verjetno napaka kopiranja Stator → Rotor).
- Po spremembi PLC: Export all → `model` znova; `TIMERS-NOT-ARRAY` mora izginiti (veja `array` je že podprta).

### Grafični vmesnik (2026-11-04)
`TIA_Generator.Gui` (WinForms, net48, brez designerja, kot TIA_ToolKit.Gui): izdelava modela iz exporta v ozadju, odpiranje/shranjevanje modela,
mreža enot s filtrom, zavihki enote (Enota, Časovniki, Parametri, Recept, Cilindri, Alarmi) z urejanjem besedil SL/EN/DE, prikaza, PLC indeksa,
polja input/display in razreda alarma; opozorila (dvoklik → enota), povzetek, alarmi brez enote, dnevnik; izvoz sprememb PLC v CSV.
Preverjeno z `--snapshot` na Purem (odprt model) in BMW G (izdelava iz exporta). Popravljeni napaki iz preizkusa: prikaz napačne enote
(SelectionChanged pred CurrentRow) in prazna tabela opozoril (ModelWarning ima polja – vezava prek WarningRow).
Ni še preverjeno ročno: urejanje celic in shranjevanje prek GUI (koda gre prek iste ModelJson.Save kot CLI).

### Struktura stroja + PLC ogrodje (2026-11-04)
Uporabnik je pojasnil glavni cilj: HMI (in PLC ogrodje) iz **strukture stroja** – sektorji A/B/C…, število postaj v sektorju, podpostaje; en sektor = Unit_01…
Odločitve: vir = ročni vnos + predizpolnitev iz exporta; izhod = HMI + PLC ogrodje; videz zaslonov iz Purem (dokler ni čiste predloge); naprave PC + panel.
- `Core\Structure\MachineStructure.cs`: StructureDef/SectorDef/StationDef, `Create`, `Rebuild`, `AllocateIndices` (A01 → 11 …, podpostaje = naslednji prosti), `Validate`, `FromModel`, `ApplyToModel`.
- `Core\Generate\PlcSkeleton.cs`: klon `Unit_A05` (Purem »Spare«, 40 networkov); preverjeno: 3×5 + podpostaje → 18 enot, 25 zamenjav indeksa na enoto, 0 ostankov A05; povratno branje z `model` da iste indekse, Settings DB-je in časovnike kot polje.
- GUI: Nov stroj (Ctrl+N), Struktura, PLC ogrodje; razvojni `--snapdialog <png>`.
- **Ni preverjeno v TIA**: uvoz generiranih XML (Openness/VCI) – prvi pravi test.
- Naslednje: HMI iz strukture – tag tabele + `SeznamPostaj`, nato zasloni `Postaja_<oznaka>` in predloge `Nastavitve_Sektor_<del>` iz Purem; uvoz prek Openness.

**Odprto / naslednji koraki:**
1. Faza 2 potrebuje knjižnico predlog (čista predloga projekta ali Purem `Postaja_B02_1` + `Nastavitve_Sektor_B`).
2. Slovar: veliko »sestavljenih« besedil je dobesednih (»Krimpanje položaj max«) – dopolnjuj `phrases` ali lokalni `-s` slovar; opcija: pobiranje besedil iz starih zaslonov (TextField levo od IOField-a).
3. `access` parametrov in razred alarmov sta hevristika – preveri v modelu.
4. Projekt ni v gitu (`D:\C#` repo, mapa `TIA_Generator/` je neverzionirana); TIA_ToolKit je na neobjavljeni veji `tia-toolkit`.
