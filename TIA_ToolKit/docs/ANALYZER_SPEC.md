# TIA_ToolKit.Analyzer – specifikacija

Statična analiza izvoza **Export all** (SimaticML V18–V20). Analizator ne potrebuje TIA Portala.
Kot vhod vzame mapo exporta (`manifest.json`, `PLC\`, `HMI\`), kot izhod izdela Excel poročilo v enaki obliki
kot ročne analize linij 8601-089A (BMW HEAT 6 L1), 8601-117A (Purem 2) in 8601-121A/122A (PUREM TWEC).

## Uporaba
- **GUI** (`TIA_ToolKit.exe`): gumb **»Analiza exporta (napake) …«**. Izbereš mapo projekta ali nadrejeno mapo z več projekti.
  Povezava s TIA ni potrebna.
- **CLI**: `TIA_ToolKit.Cli.exe analyze <exportDir> [-o <porocilo.xlsx>]`.
- Poročilo `ANALIZA_TIA_ToolKit_<projekt>.xlsx` se zapiše v **nadrejeno** mapo exporta, ker Export all ob ponovnem izvozu
  svojo mapo v celoti izbriše.
- Ob ponovni analizi se **Status** in **Opomba** ohranita. Povezana sta prek skritega stolpca *Ključ* (pravilo + lokacija + network),
  ne prek ID-ja, ker se ID lahko premakne.

## Arhitektura
```
ExportReader  →  ExportProject (PlcProgram → Block → Network: STL | LAD-graf | SCL tekst; PlcTag)
RuleSet.All() →  IRule.Check(project, plc) → Finding[]
ExportAnalyzer →  oštevilčenje (S/F/C/H/T/M + zaporedna številka po kategoriji, prioriteti, pravilu)
ExcelReport    →  Povzetek (formule) | Napake (tblNapake) | Legenda | Seznami (skrit, imenovan obseg SeznamStatus)
```
- **LAD/F_LAD** se prebere kot graf: `LadPart` (Contact/Coil/…, `Operand`, `Negated`) in žice (`PWR` ali pin dela → cilji).
  Pomožna metoda `SourceOf(uid)` vrne, kaj napaja vhod dela.
- **STL** se prebere kot zaporedje `StlStatement(Op, Operand)`. Globalni operand je zapisan kot `"Ime.Član"`, lokalni kot `#ime`.
- **Spustni seznam Status** je imenovan obseg na skritem listu. Seznam, zapisan kot dobesedni niz `"a,b,c"`, se pri slovenskem
  ločilu seznama (`;`) prebere kot ena sama postavka, kar je bila napaka v prvih ročnih tabelah.
- **ClosedXML** zapiše Excel brez nameščenega Excela in brez COM.

## Pravila – izvedeno (v0.2)

| ID | Kat. | Prio | Kaj najde | Primer iz ročnih analiz |
|---|---|---|---|---|
| SAF-ACK-REQ | Safety | 1 | F_LAD: `"…ACK_REQ"` → Contact → Coil `"…ACK_REI"` (samodejna reintegracija F-I/O) | G NW22, H NW22, Purem2 NW37 |
| SAF-ACK-CLOCK | Safety | 1 | STL: `A "Clock_x"` / `= …ACK…` (potrditev F-komunikacije s taktom) | A, B, C, D, E, G, H – Safety NW16/19/20/22 |
| SAF-PWR-COIL | Safety | 2* | F_LAD: Coil, napajana neposredno iz PWR (pogoj trdo TRUE) | `PREVIOUS/NEXT_MACHINE_OK` na A, B, C, D, G, H |
| SAF-FORCED | Safety | 2* | STL: `SET` → `S`/`=` signala VDC/EMG/AIR/vrata/_iSig/Bypass/ENABLED (ostanek zagona) | G `SET S #VDC_OK … = #EMG_OK`, G `removee SET S "S_Door_Close_iSig"`, B `= "BT1_ROBOT_OUT_SYS_EMG_OK"` |
| PLC-DOUBLE | Kop./Funk. | 3* | Globalni bit zapisan z `=` (ali Coil) večkrat – v več networkih ali v istem | `warning_Robot_Calibratin_ON` 6× (Purem2), `SERVICE_ON` 3×, 122A FieldBus NW3 6× `Error_FieldBus_SMC_2` |
| DUP-OPERAND | Kop. | 3* | Isti operand 2× v istem AND-členu ali enak OR-člen 2× (STL člen po členu, LAD zaporedni kontakti) | 122A OPERATION NW14 `RIGHT_PALLET_PRESENT` 2×, Unit_T20 NW124 `O BAD_PART / O BAD_PART` |
| CONTRA | Safety/Funk. | 2 | `X` in `NOT X` v istem AND-členu – vedno FALSE | 122A Main_Safety_RTG1 NW20 pokrov laserja |
| NW-COMMENTED | Funk. | 2–3* | Network samo s komentarji (≥ 20 vrstic, 0 ukazov) – zakomentirana logika | 122A Unit_40 NW65 (499 vrstic) |
| HANDSHAKE-STUB | Pomožni | 4* | `A …_START / S …_END / R …_START` brez dejanja | 122A Ignition_Comm NW19 |
| IDX-UNIT | Kop. | 3* | Redek indeks polja v bloku (≤ 2×, glavni ≥ 4× pogostejši); izvzeti naštevanje in vzorec, skupen ≥ 2 blokoma | 122A Unit_20 `OFF[3]`, `PIECE[13]`; Unit_40 `PIECE[4]` |
| XREF-WRITEONLY | Funk. | 3–4* | Rezultat (BAD/NOK/ERROR/ALARM/CANCEL) se zapisuje, v PLC se ne bere (vklj. parametre klicev, SCL, nadrejene strukture) | 122A `Etalon_All_BAD` |
| HMI-ADDR | HMI | 2–4 | Absolutni HMI tag → PLC član (DbLayout): DB ne obstaja, izven DB, napačen tip, rezervni člen, neujemanje imen | 122A `T20_Robot_Stop_REQ`→`TEST[49]`, `U60_Dosing_EMPTY` v Int, `ERROR_ON`→`Empty_17` |
| HMI-UNIT | Kop. | 2–3* | Vnos na zaslonu enote vezan na tag druge enote; isti tag na več vnosnih poljih | 122A Postaja_50→`U60_Timers`, Postaja_60 `[1..3]` 3×, Etaloniranje M19 |
| HMI-AUTH | HMI | 3* | Nevaren ukaz brez avtorizacije na zaslonu, dosegljivem brez prijave (BFS po navigaciji, vklj. predloge) | 122A Disable_DataBase/LN, Enable_Manual_Control, kalibracija, StopRuntime |
| HMI-SCRIPT | HMI | 2–4 | Gesla v VB skriptah in SCL konstantah, absolutne poti, neobstoječi SmartTags, neklicane skripte (upošteva dogodke tagov) | 122A LogINLevel, Start_PDB, Boxes, Etalons `'Default.'` |
| HMI-OVERWRITE | Funk. | 3* | Vnos na HMI piše člen, ki ga PLC prepiše (`L X / T člen`, `:=`); izvzeto nalaganje recepta (> 15 kopij) in %I | 122A meje U20/U30 |
| HMI-HIDDEN | HMI | 4* | Vnosno polje, prekrito z drugim upravljalnim objektom (dosegljivo s Tab) | 122A `U10_Timers[0]` na 9 zaslonih |
| HMI-SETBIT | HMI | 4 | Gumb SetBit/InvertBit na bit, ki ga PLC ne bere | 122A `LN_Get_Material_Cancell_REQ`, `Recipe_Save_Change` |
| OB-ERROR | Pomožni | 3 | OB82/86/121/122 prazni ali manjkajo | vsi projekti |
| SCL-OEE | Pomožni | 4 | `1 - Bad/Good` v SCL | vsi projekti |
| TAG-OVERLAP | Tagi | 3* | Prekrivanje naslovov različnih širin (brez Clock/System bajta) | B08 `%IW1822` v `%ID1820` (Purem2), D `%IW100` čez vrata |
| TAG-SYSBYTE | Tagi | 4* | `System_Byte`=%MB1 in program piše M1.0–M1.3 | `StartUp` = M1.2 na vseh strojih linije |

\* Zaupanje »Preveriti«: pravilo najde kandidata, ali gre res za napako, presodi človek ali LLM.

Preizkus v0.1: Stroj B 19 ugotovitev (11 s), Stroj G 16, 122A 10. Pravila ujamejo vse ustrezne točke iz ročnih tabel,
na B pa so našla tudi nov primer: prisiljen `BT1/BT2_ROBOT_OUT_SYS_EMG_OK`.

**Preizkus v0.2 (122A, primerjava z LLM tabelo, stolpec »Zaznavnost«):** od 30 napak »Program« jih ToolKit najde **26** (v0.1: 4);
zgreši H4 (CTD naložen samo ob Stop – preveč specifično), M10 je pokrit s HMI-SETBIT, M4/M5 pa sta po umerjenem izračunu postavitve
dvomljivi (glej DbLayout). Najde tudi C10 in M2 (»Program + LLM«) ter nove kandidate (Unit_T10 NW97, Unit_80 `Bad_parts_Full`,
skrite receptne meje na Etaloniranje E1–E3, `%IB1306`/`%IW1306`). 73 ugotovitev, ~14 s. Napredek po korakih: `docs\NAPREDEK_ANALIZATOR.md`.

**DbLayout (odmiki standardnih DB-jev):** Bool po bitih, 1-bajtni tipi na bajt, ostalo na sodi bajt; Struct/UDT/FB-instanca in polja
na sodem bajtu; vsak razdelek FB (Input/Output/InOut/Static) na sodem bajtu; IEC_TIMER/TON_TIME = 16 B. Umerjeno na 122A (vse točke globalnih
DB-jev in Unit_80_DB). Neumerjeno: InOut String (6-B kazalec?) in R_TRIG/F_TRIG → `UncertainFrom`; naslovi za to mejo se ne ocenjujejo.
CLI: `TIA_ToolKit.Cli layout <export> <DB> [filter]`, `dump <export> <blok> [nw]`, `hmi <export> <zaslon>`.

## Pravila – naslednja faza (po donosu)
Ocena iz tabele Stroj B (stolpec »Zaznavnost«) je 70 % napak »Program«, 20 % »Program + LLM« in 9 % »Samo LLM«.
Izvedeno v v0.2: IDX-UNIT, HMI-ADDR, HMI-SETBIT, HMI-SCRIPT (glej tabelo zgoraj). Ostaja:

1. **XREF-UNIT** – blok enote uporablja simbol druge enote ali linije. Žetoni se izpeljejo iz imena bloka in mape
   (`S_`/`R_`, `Stator`/`Rotor`, `B05`↔`B10`, `A01_1`↔`A01`).
   *Primeri:* Rotor piše `Unit_S_G50_Bad_DB_Settings.FULL_1`, stator kliče `Manual_Remove_Part_R_Save_SQL`, B05 `Unit_B10_Settings_DB`.
2. **SIB-DIFF** – primerjava sorodnih blokov (normalizacija imen in indeksov, nato diff network za networkom).
   Poročajo se le razlike v operandih, ne v strukturi.
   *Primeri:* krimp 4 `Pressure_Archive[2]`, `Gripper_1_C` namesto `_2_C`, `Vision_Pos_1` namesto `_2`.
3. **HMI-IDX** – HMI polje `[k]` = PLC `[k+1]`. Indeks stikala na zaslonu se primerja z indeksom, ki ga enota iz napisa vrstice
   dejansko bere. *Primeri:* ServiceOnOff na B in 121A.
4. Umeritev DbLayout za InOut String in R_TRIG/F_TRIG (potreben export z znanimi odmiki ali preverba v TIA).

Dodatno:
- **SAF-NONF**: ne-F operand (standardni DB, `%M`) v pogoju `DOOR_OK`/`START_MACHINE_ON` (programerski ključ kot HMI bit).
- **SAF-UNLOCK**: `R "…Lock…"` pod `ON …EMG_OK` ali časovnikom brez `MOVING` (odklep vrat brez mirovanja).
- **SCL-DIV0**: deljenje brez zaščite pred 0.
- **STL-TEMP-LATCH**: S/R na `#temp`.
- **STL-SNOR**: S brez R.
- **TAKT-TOGGLE**: `X #TOGGLE` brez fronte.
- **HMI-SCRIPT** (dopolnitev): deljenje brez zaščite v skriptah; brisanje števcev prek `SetServiceTagArrayOn` (indeks v `ServiceInteger`) za HMI-AUTH.

## LLM korak (izbirno, pozneje)
Za kandidate z zaupanjem »Preveriti« in za kategorije, ki jih pravila ne pokrijejo (pomen napisov, tok handshaka), se Claudu
pošlje samo izrezek: networki in DB člani okoli kandidata. Celoten projekt se ne pošilja.
**Pred uporabo je treba preveriti pravila podjetja glede pošiljanja PLC kode zunanji storitvi.**

## Regresijski testi
Ročne tabele so referenca (pričakovane ugotovitve):
- 8601-089A: `D:\Claude\TiaFullExport\8601-089A BMW HEAT 6 L1\ANALIZA_NAPAK_*.xlsx`;
- 8601-117A: `D:\Claude\TIA_Purem2_Analiza\`;
- 8601-121A/122A: `D:\Claude\TiaFullExport\8601-12xA…\`.

Naslednji korak je projekt `TIA_ToolKit.Tests`, ki za vsak export preveri, ali so ugotovitve, označene kot »Program«, najdene.
