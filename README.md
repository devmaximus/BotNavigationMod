# BotNavigationMod

BepInEx client plugin for SPT 4.0.x that improves group patrol navigation.

| | |
|--|--|
| **Author** | [devmaximus](https://github.com/devmaximus) |
| **GitHub** | [devmaximus/BotNavigationMod](https://github.com/devmaximus/BotNavigationMod) |
| **GUID** | `com.devmaximus.botnavigationmod` |
| **Branch** | `feature/option-c-patrol-suite` (Option C: S01+S02+S03) |
| **Agents** | See [`AGENTS.md`](AGENTS.md) before editing |

## Strategies

| ID | Name | Hook scope | Effect |
|----|------|------------|--------|
| S01 | PatrolPointOffset | GoToPoint | Perpendicular fan-out at patrol destinations |
| S02 | PatrolWayDiversity | ChooseStartWay / TryToFindWay | Distributes group members across zone patrol ways |
| S03 | StaggeredTransition | FindNextPoint | Delays departure so groups do not leave in lockstep |

## Build

Set your live install root (do **not** commit machine-specific paths):

```powershell
$env:SPT_TARKOV_DIR = "C:\Path\To\EscapeFromTarkov\"
dotnet build BotNavigationMod/BotNavigationMod.csproj -c Release

# or:
dotnet build BotNavigationMod/BotNavigationMod.csproj -c Release `
  -p:TarkovDir="C:\Path\To\EscapeFromTarkov\"
```

Optional: copy `Directory.Build.props.user.example` → `Directory.Build.props.user` (gitignored).

PostBuild copies `BotNavigationMod.dll` to `<EFT>\BepInEx\plugins\BotNavigationMod\`. Close the game if the DLL is locked. Deploy does **not** overwrite the live `.cfg`.

## Config

| Role | Path |
|------|------|
| Live (first load) | `<EFT>\BepInEx\config\com.devmaximus.botnavigationmod.cfg` |
| Shipped defaults | `config/defaults/com.devmaximus.botnavigationmod.cfg` |

Master toggle: `[General] Enabled`. Per-strategy sections for S01/S02/S03 + `[Diagnostics]`.

### Restore defaults

```powershell
pwsh -File ./scripts/restore-config.ps1 -Source Defaults -TarkovDir $env:SPT_TARKOV_DIR
```

### Snapshot (workspace skill)

When developing in the multi-repo workspace, prefer `/config-snapshot` so Kind + Context land in `configs/config-history`.

## Out of scope

Scav hostility / karma / zone `AddEnemy` — see **ScavHostilityFix** (`com.devmaximus.scavhostilityfix`).
