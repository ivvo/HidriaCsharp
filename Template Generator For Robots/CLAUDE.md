# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A WPF desktop tool that generates robot-cell program code (Epson RC+, KUKA Hella, ABB Hidria, ABB
Simulacija) from a UI where a user defines "Programs" (robots) and "Stations" within each program.
It also has a newer feature to import an already-generated project back into the model and
surgically patch in newly-added stations without a full regenerate.

## Build

The real, active solution is `TemplateGenerator.sln` (2 projects: `TemplateGenerator.Core` +
`TemplateGenerator.Wpf`). The root also contains `GenerateExcelFile/` and `GenerateExcelIO/` and an
empty `GenerateExcellIO/` — these are **not** referenced by `TemplateGenerator.sln` and are dead/
legacy (Excel-COM-Interop-based predecessor of `GenerateExcellIO.cs` inside `Core`). Don't "fix"
things there unless explicitly asked.

```
dotnet build TemplateGenerator.sln
```

If Visual Studio has the solution open and running, `TemplateGenerator.Wpf.dll`/`.pdb` will be
locked and the full build fails with `MSB3027`. Build just `TemplateGenerator.Core` in that case —
it's where almost all logic lives and it doesn't lock:

```
dotnet build TemplateGenerator.Core\TemplateGenerator.Core.csproj
```

There is no test project and no lint config in this repo. `TemplateGenerator.Wpf` is a `WinExe`
(net5.0-windows, needs Windows) — there's no headless way to exercise it; verification in this repo
means writing a throwaway console harness (netX.0) with a `ProjectReference` to
`TemplateGenerator.Core.csproj`, driving `ShellViewModel` directly, and running it from inside
`TemplateGenerator.Wpf\bin\Debug\net5.0-windows\` (see "Templates lookup" below for why the working
directory matters).

## Architecture

### Solution layout

- **`TemplateGenerator.Core`** (`netstandard2.0`) — all business logic: template engine, models,
  MvvmCross `ShellViewModel`. No UI dependency.
- **`TemplateGenerator.Wpf`** (`net5.0-windows`, `UseWPF`) — thin WPF shell. `App.xaml.cs` wires
  `MvxWpfSetup<Core.App>`; MvvmCross convention resolves `ShellViewModel` → `ShellView.xaml` as the
  actual window content. **`MainWindow.xaml` is vestigial and not shown** — don't add UI there.

### Template generation engine (one-way: model → files)

`TemplateGenerator.Core\Classes\Template.cs` is a large `internal` (not `public`) static class, one
region per vendor (`EPSON`, `KUKA Hella`, `ABB Hidria`, `ABB Simulacija`). Its static constructor
reads every `*.txt` template skeleton from `./Templates/<Vendor>/...` **relative to the running
process's working directory** — the only place these actually exist on disk is
`TemplateGenerator.Wpf\bin\Debug\net5.0-windows\Templates\` (not source-controlled, not copied to
`Core`'s own `bin`). Any code that calls into `Template.*` — including a standalone test harness —
must run with that folder as its CWD, or `Template.FormatsLoaded` silently ends up `false` and every
`Get*` call returns garbage from empty strings.

Each `GetXxxFunc(ProgramModel/ObservableCollection<ProgramModel>)` method does its own
`StringBuilder` looping over `program.Stations` to fill `{0}`/`{1}`/... placeholders in the loaded
template text via `string.Format`. Because `Template` is `internal`, only code inside `Core.dll`
(e.g. `ShellViewModel`, the `*ProjectUpdater` classes) can call it directly.

`ShellViewModel.GenerateProject(path)` is the write-only entry point: branches on `SelectedTemplate`
and writes every vendor-specific file for every `Program`. `GenerateSimpleProgram` (bool) selects
between two code-style variants for Epson/KUKA; the XAML checkbox for it is **not actually data-bound**
to the view model, so in the running app it's stuck at its default (`true`).

### Import & Update (round-tripping an existing, already-generated project)

For each vendor there's a matching pair in `TemplateGenerator.Core\Classes\`:
`EpsonProjectImporter`/`EpsonProjectUpdater`, `KukaHellaProjectImporter`/`KukaHellaProjectUpdater`,
`AbbHidriaProjectImporter`/`AbbHidriaProjectUpdater`. `ShellViewModel.ImportProject(path)`
auto-detects the vendor from folder shape (`*_Motion.mod` → ABB Hidria, `R1/Program/` → KUKA Hella,
`*.prg` → Epson) and dispatches to the matching `Importer`; `UpdateProject(path)` remembers which
vendor was imported (`_importedVendor`) and dispatches to the matching `Updater`.

- **Importer**: regexes the vendor's per-station function name (e.g. Epson
  `Function {prog}_go(\w+)\(\)`) to recover station order, then inspects each function body for
  vendor-specific literal markers to recover `StationFreeEnabled`/`Pallet`/`Positions`. Throws a
  vendor-specific `*ImportException` with a Slovenian message on anything unexpected rather than
  guessing.
- **Updater**: given the full `ProgramModel` (now including newly-appended stations) plus the list
  of just the *new* ones, surgically inserts only the new stations' code into the existing files —
  it does not touch anything belonging to pre-existing stations, so hand-edits made after a previous
  `Generate` survive. `BackupFolder(path)` snapshots the whole project dir before any edit.
- **Hard constraint enforced by all three importers**: one program/robot per project. KUKA's
  `$config.dat`/`robotFunctions.src` and ABB's `Global.mod`/`Communication.mod`/`robot_Main.mod` are
  project-wide (not per-robot) in the underlying generator, and duplicate/collide for >1 robot — a
  pre-existing generator limitation, not something the Updaters work around.
- **Hard constraint enforced by design**: new stations may only be *appended* at the end of a
  program's station list — never inserted in the middle, renamed, or removed via this path.

Two correctness properties every `*ProjectUpdater` must preserve when touched — regressions here
have shipped twice already:

1. **Never fully regenerate a file that can contain externally-taught data.** Epson's `.pts`,
   KUKA's `*_motion.dat`, and ABB's `Global.mod` `robtarget`/`E6POS` declarations are always written
   as zero-valued placeholders by `Template.cs`; real coordinates only exist after someone teaches
   them on the physical robot via the pendant. Regenerating these files wipes that calibration.
   Existing point blocks must be parsed and carried over untouched; only genuinely new points get
   appended (see "Point ordering" below for Epson specifically).
2. **"Origin completeness."** Every generated `go<Station>()`-style function contains an inner
   dispatch ("if I'm arriving *from* station X, do movement-for-this-target") that must enumerate
   *every* station as a possible origin. Adding a new station as a new *destination* without also
   adding it as a possible *origin* in every other station's existing dispatch means the robot
   errors out when leaving the new station. Each Updater has a
   `PatchExisting...FunctionsForNewOrigin`-style method for this — when adding new per-station
   logic, check whether it needs a symmetric "as origin" counterpart too.

**Epson-specific point ordering gotcha**: Epson's Homing routine finds the nearest known point by
raw numeric index (`P(index)`, looping `For index = 0 To Stations.Count-1`), not by name — unlike
KUKA/ABB, which look up points by name and are unaffected by this. `Template.GeneratePointsFunc`
lays out `.pts` in two passes: first one "primary" point per station at index == that station's
position in `Stations` (0-based), *then* any pallet/multi-position "extra" points for those same
stations, continuing from `Stations.Count` onward. So a newly-appended station's primary point must
be inserted at index `previousStationCount` — which may require renumbering already-existing extra
points that currently occupy that slot — never just appended at the tail. `EpsonProjectUpdater`
also increments the Homing `For index = 0 To N` bound for every station added; forgetting either half
of this means Homing never considers the new station as a candidate location.

### Generic text-surgery pattern

All three `*ProjectUpdater` classes independently implement the same small set of primitives (not
shared into a common base — the block delimiters differ per vendor: Epson `Function ... Fend`, KUKA
`DEF/GLOBAL DEF ... END` with `\bEND\b` used to avoid matching inside `ENDIF`/`ENDLOOP`, ABB
`PROC ... ENDPROC`): find a named block, insert a line/branch immediately before some marker inside
it, or insert a new line just before the block's own closing token. The one recurring bug class when
extending these: if the "insert before marker" marker string doesn't include *all* of its own
leading whitespace, the insertion point lands mid-indent and splits the original line in two — every
such helper backs up over leading spaces/tabs to the true start of the line before inserting
(`BackUpToStartOfLine` / the `\n[ \t]*` regex variants) for exactly this reason.

### Models

`ProgramModel` (one robot/program) holds `ObservableCollection<StationModel>`. `StationModel` has
`RobotStationName`, `StationFreeEnabled`, `Pallet`, `Positions` (default `1`); its constructor
derives `RobotStationNameToUpper` (snake_case-ish upper form used throughout generated code) from
the name. KUKA's generators never read `Pallet`/`Positions` at all — pallet/multi-position support
is Epson/ABB-only.
