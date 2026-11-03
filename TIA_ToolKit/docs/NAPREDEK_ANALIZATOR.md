# Napredek analizatorja – izboljšave po primerjavi z LLM analizo 122A

Navodilo za nadaljevanje (katerakoli seja): vzemi prvi neodkljukan korak, ga izvedi, zgradi, preizkusi, odkljukaj tukaj in naredi commit na veji `tia-toolkit` (ne push, ne merge v main).

- Referenca (LLM, stolpec »Zaznavnost«): `D:\Claude\TiaFullExport\8601-122A - ADV-PUREM TWEC Končna kontrola\ANALIZA_NAPAK_8601-122A_PUREM_TWEC_Koncna_kontrola.xlsx`
- Export za test: `C:\TIA_ColorExport\TestaAA\20260413_8601-122A - ADV-PUREM TWEC Koncna kontrola`
- Gradnja: VS2022 MSBuild `TIA_ToolKit.sln -restore -p:Configuration=Debug -p:Platform=x64`
- Test: `bin\x64\Debug\TIA_ToolKit.Cli.exe analyze "<export>" -o <scratch>.xlsx`, nato preveri, da je LLM ID iz stolpca »Pričakovano« najden.
- Izhodišče (03.11.2026): ToolKit najde 4 od 30 »Program« napak (S4, H5, H6, C7 delno) + 6× TAG-OVERLAP (5 šum, T6 %IB1306/%IW1306 verjetno prava).

## Faza 1 – PLC hitre zmage
- [x] 1.1 PLC-DOUBLE: štej zapise, ne networkov (večkratni Assign v istem networku). Pričakovano: C5 (FieldBus NW3 Error_FieldBus_SMC_2), C7 (Safety NW9 warning_Door_9_Opened) – ✔ najde oba + F10 (Semaphore_S1/S2_oSIG, OPERATION NW23). CLI dobil ukaz `dump <export> <blok> [nw]` za razvoj pravil.
- [x] 1.2 DUP-OPERAND: isti operand 2× v isti A/O verigi (STL; LAD vzporedni kontakti). Pričakovano: F10 (Unit_T20 NW124 O BAD_PART O BAD_PART), C6 (OPERATION NW14 RIGHT_PALLET_PRESENT 2×) – ✔ oba (Rules\LogicRules.cs). Novi kandidati: Unit_T10 NW97 A UT10_Gripper_B_F 2×, Unit_30 NW85 U30_Sealing_F_CMD 2×, Unit_T20 RE_MEASURE 2× (NW57–67).
- [x] 1.3 CONTRA: `A X` in `AN X` v isti AND verigi (vedno FALSE). Pričakovano: S2 (Main_Safety_RTG1 NW20 Safety_Lase_Cover_CH1_SW) – ✔ (LAD zaporedni kontakti)
- [x] 1.4 NW-COMMENTED: network brez ukazov, z veliko komentarja (zakomentirana logika). Pričakovano: F1 (Unit_40 NW65) – ✔ (+ Etalons NW24, ERROR_BLOCK NW2, Unit_00 NW27; ≥100 vrstic = Visoka)
- [x] 1.5 HANDSHAKE-STUB: `A …_START / S …_END / R …_START` brez dejanja vmes. Pričakovano: H7 (Ignition_Comm NW19) – ✔
- [x] 1.6 TAG-OVERLAP: bajtni tagi s privzetim imenom `Tag_n`, ki jih program ne uporablja → ena skupna ugotovitev, prioriteta 4. T6 ostane. – ✔ (6 → 2 ugotovitvi)

Stanje po fazi 1 (122A): 20 ugotovitev; od 30 LLM »Program« najde 10 (S2, S4, F1, F10, C5, C6, C7, H5, H6, H7), + nove kandidate (Unit_T10 NW97, Unit_30 NW85, %IB1306/%IW1306).

## Faza 2 – indeksi in navzkrižne reference
- [x] 2.1 IDX-UNIT: indeks v bloku enote odstopa od večine (TRACKING.PIECE[n], DB_Service_Settings.OFF/TEST/STEP[n]) + indeks, uporabljen samo 1× v programu. Pričakovano: C1 (Unit_20 NW5 OFF[3]), C3 (Unit_20 NW38 PIECE[13]), C4 (Unit_40 PIECE[4]) – ✔ natanko vse tri, brez šuma (Rules\IndexRules.cs: redek = ≤2× in glavni ≥4× pogostejši; izvzeti networki z ≥4 indeksi (naštevanje) in redki indeksi, ki se enako pojavijo v ≥2 blokih (predaja s prijemala PIECE[1]/[2])).
- [x] 2.2 XREF-WRITEONLY: globalni bit se piše, nikjer bere (filter imen BAD/ERROR/ALARM/REQ). Pričakovano: F3 (Etalon_All_BAD), M10 (LN_Get_Material_Cancell_REQ – po fazi 3 tudi HMI stran) – ✔ F3 (Rules\XrefRules.cs; ExportReader zdaj polni Network.Refs = vse globalne reference vklj. parametre klicev in SCL). Izvzeto: spremenljiv indeks (brisanje v zanki), statusna polja ≥3 indeksov (ERROR[n] za HMI), DB-ji SQL/SCADA/HMI/Messages, izhodi %Q. Novi kandidati: Unit_80 Bad_parts_Full, Unit_60_DB.BAD_PART (piše T20), T10/T20_Calibration_NOK, L/R_Machine_NOK, L/R_Box_String_NOK. M10 ostane za fazo 3 (piše ga samo HMI → potrebuje HMI bralnik).

## Faza 3 – HMI (bralnik + pravila)
- [ ] 3.1 HmiReader: tagi (ime, PLC tag / absolutni naslov, tip), zasloni (objekti, vezave, Authorization, položaj/velikost, dogodki SetBit/skripte), skripte (tekst).
- [ ] 3.2 DB layout: odmiki članov non-optimized DB (standardna postavitev S7: Bool biti, poravnava na sodi bajt, Struct/Array/UDT/FB instance).
- [ ] 3.3 HMI-ADDR: absolutni naslov → član DB; ime se ne ujema / izven DB / rezervni bit (Empty_*). Pričakovano: M1, M4, M5, M6, M7, T1
- [ ] 3.4 HMI-UNIT: zaslon Postaja_NN veže tage druge enote (U60_* na Postaja_50). Pričakovano: C9, C10
- [ ] 3.5 HMI-AUTH: stikalo/gumb na Disable/OFF/Manual/Delete/Start/Calib brez Authorization. Pričakovano: M8, M9, M11
- [ ] 3.6 HMI-SCRIPT: gesla v besedilu, poti C:\Users\, neobstoječi tagi, neklicane skripte. Pričakovano: M3, M12
- [ ] 3.7 HMI-OVERWRITE: InOutput polje piše tag, ki ga PLC vsak cikel prepiše (T / :=). Pričakovano: F11
- [ ] 3.8 HMI-HIDDEN: vnosno polje, ki ga prekriva drugo polje. Pričakovano: C11

## Zaključek
- [ ] Posodobi docs\ANALYZER_SPEC.md (tabela pravil) in README.md; končna primerjava: koliko od 30 »Program« najde ToolKit.
