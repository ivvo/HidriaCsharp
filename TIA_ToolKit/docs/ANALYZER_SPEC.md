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

## Pravila – izvedeno (v0.1)

| ID | Kat. | Prio | Kaj najde | Primer iz ročnih analiz |
|---|---|---|---|---|
| SAF-ACK-REQ | Safety | 1 | F_LAD: `"…ACK_REQ"` → Contact → Coil `"…ACK_REI"` (samodejna reintegracija F-I/O) | G NW22, H NW22, Purem2 NW37 |
| SAF-ACK-CLOCK | Safety | 1 | STL: `A "Clock_x"` / `= …ACK…` (potrditev F-komunikacije s taktom) | A, B, C, D, E, G, H – Safety NW16/19/20/22 |
| SAF-PWR-COIL | Safety | 2* | F_LAD: Coil, napajana neposredno iz PWR (pogoj trdo TRUE) | `PREVIOUS/NEXT_MACHINE_OK` na A, B, C, D, G, H |
| SAF-FORCED | Safety | 2* | STL: `SET` → `S`/`=` signala VDC/EMG/AIR/vrata/_iSig/Bypass/ENABLED (ostanek zagona) | G `SET S #VDC_OK … = #EMG_OK`, G `removee SET S "S_Door_Close_iSig"`, B `= "BT1_ROBOT_OUT_SYS_EMG_OK"` |
| PLC-DOUBLE | Kop./Funk. | 3* | Globalni bit zapisan z `=` (ali Coil) v več networkih | `warning_Robot_Calibratin_ON` 6× (Purem2), `SERVICE_ON` 3× |
| OB-ERROR | Pomožni | 3 | OB82/86/121/122 prazni ali manjkajo | vsi projekti |
| SCL-OEE | Pomožni | 4 | `1 - Bad/Good` v SCL | vsi projekti |
| TAG-OVERLAP | Tagi | 3* | Prekrivanje naslovov različnih širin (brez Clock/System bajta) | B08 `%IW1822` v `%ID1820` (Purem2), D `%IW100` čez vrata |
| TAG-SYSBYTE | Tagi | 4* | `System_Byte`=%MB1 in program piše M1.0–M1.3 | `StartUp` = M1.2 na vseh strojih linije |

\* Zaupanje »Preveriti«: pravilo najde kandidata, ali gre res za napako, presodi človek ali LLM.

Preizkus na exportih: Stroj B 19 ugotovitev (11 s), Stroj G 16, 122A 10. Pravila ujamejo vse ustrezne točke iz ročnih tabel,
na B pa so našla tudi nov primer: prisiljen `BT1/BT2_ROBOT_OUT_SYS_EMG_OK`.

## Pravila – naslednja faza (po donosu)
Ocena iz tabele Stroj B (stolpec »Zaznavnost«) je 70 % napak »Program«, 20 % »Program + LLM« in 9 % »Samo LLM«.
Največ doda naslednjih šest pravil:

1. **IDX-UNIT** – indeks `DB_Service_Settings.OFF/TEST/STEP/RESET/MOVING[n]`, `DB_SCADA_OUT.*[n]`, `DB_Counter_*[n]` in `TAKT_STATION[n]`,
   ki ga uporablja enota, se razlikuje od njenega lastnega indeksa. Lastni indeks se ugotovi po večini (najpogostejši indeks v bloku)
   ali iz preslikave v `unitmap.json`.
   *Primeri:* D01 `OFF[11]`, Stator Good `STEP[16]`, BT1 `ERROR[8]`, H02 vse na [23].
2. **XREF-UNIT** – blok enote uporablja simbol druge enote ali linije. Žetoni se izpeljejo iz imena bloka in mape
   (`S_`/`R_`, `Stator`/`Rotor`, `B05`↔`B10`, `A01_1`↔`A01`).
   *Primeri:* Rotor piše `Unit_S_G50_Bad_DB_Settings.FULL_1`, stator kliče `Manual_Remove_Part_R_Save_SQL`, B05 `Unit_B10_Settings_DB`.
3. **SIB-DIFF** – primerjava sorodnih blokov (normalizacija imen in indeksov, nato diff network za networkom).
   Poročajo se le razlike v operandih, ne v strukturi.
   *Primeri:* krimp 4 `Pressure_Archive[2]`, `Gripper_1_C` namesto `_2_C`, `Vision_Pos_1` namesto `_2`.
4. **HMI-ADDR** – absolutni HMI tag (brez ControllerTag) se razreši v odmik non-optimized DB-ja (standardna postavitev).
   Če ime člena ne ustreza imenu taga, gre za napako; prav tako, če DB ali odmik ne obstajata.
   *Primeri:* `ALARM_ON`=`%DB1.DBX0.0`, `DB_Settings_END`@116, `Unit_B02_4_Timers` na B02_1–3, `U_030_Mot_Rot`@DB103.
5. **HMI-IDX** – HMI polje `[k]` = PLC `[k+1]`. Indeks stikala na zaslonu se primerja z indeksom, ki ga enota iz napisa vrstice
   dejansko bere. *Primeri:* ServiceOnOff na B in 121A.
6. **HMI-SETBIT** – SetBit na bit, ki ga PLC nikoli ne bere ali ne resetira, ter neposreden zapis HMI v `%Q`.

Dodatno:
- **SAF-NONF**: ne-F operand (standardni DB, `%M`) v pogoju `DOOR_OK`/`START_MACHINE_ON` (programerski ključ kot HMI bit).
- **SAF-UNLOCK**: `R "…Lock…"` pod `ON …EMG_OK` ali časovnikom brez `MOVING` (odklep vrat brez mirovanja).
- **SCL-DIV0**: deljenje brez zaščite pred 0.
- **STL-TEMP-LATCH**: S/R na `#temp`.
- **STL-SNOR**: S brez R.
- **TAKT-TOGGLE**: `X #TOGGLE` brez fronte.
- **HMI-SCRIPT**: skripta ni klicana, uporablja neobstoječ tag ali deli brez zaščite; gesla v `LogINLevel`.

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
