# TIA_ToolKit (Hidria)

Orodje za TIA Portal projekte (V18–V20). Je naslednik `TIA_HMIColorTool`; original v `D:\C#\TIA_HMIColorTool` ostane nespremenjen.

| Projekt | Cilj | Vsebina |
|---|---|---|
| `TIA_ToolKit.Gui` | net48 WinExe | Glavna aplikacija `TIA_ToolKit.exe` (prej MainForm iz HMIColorTool + gumb »Analiza exporta«) |
| `TIA_ToolKit.Openness` | net48 | Priklop na TIA (Openness), export/import HMI zaslonov, **Export all** |
| `TIA_ToolKit.HmiColors` | net48 | Standardizacija barv HMI (zamenjava v XML), izris zaslonov v PNG |
| `TIA_ToolKit.Analyzer` | netstandard2.0 | **Statična analiza exporta** → Excel (brez TIA) – glej `docs\ANALYZER_SPEC.md` |
| `TIA_ToolKit.Cli` | net48 Exe | `TIA_ToolKit.Cli.exe analyze <exportDir> [-o porocilo.xlsx]` |
| `TIA_ToolKit.Common` | netstandard2.0 | Skupne pomožne funkcije |

## Gradnja
```
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" TIA_ToolKit.sln -restore -p:Configuration=Debug -p:Platform=x64
```
- Izhod gre v `bin\x64\Debug\` (V20). Za drugo verzijo TIA dodaj `-p:TiaVersion=V18`; rezultat je `bin\x64\Debug\V18\TIA_ToolKit_V18.exe`.
- Openness DLL-ji se ne kopirajo; naloži jih `AssemblyResolve` iz `C:\Program Files\Siemens\Automation\Portal <V>\PublicAPI\<V>`.
- Analyzer in CLI ne potrebujeta TIA, le NuGet paket ClosedXML.

## Dokumentacija
- `docs\ANALYZER_SPEC.md` – pravila analize, arhitektura in naslednji koraki.
- `docs\HMI_COLOR_TOOL_SPEC.md` – barve HMI (iz TIA_HMIColorTool).
- `docs\legacy\` – prvotni konzolni prototipi (se ne prevajajo).
