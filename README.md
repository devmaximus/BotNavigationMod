# BotNavigationMod

BepInEx client plugin for SPT 4.0.x that improves group patrol navigation.

**Author:** [devmaximus](https://github.com/devmaximus) · GUID `com.devmaximus.botnavigationmod`

## Strategies

| ID | Name | Hook scope | Effect |
|----|------|------------|--------|
| S01 | PatrolPointOffset | GoToPoint | Perpendicular fan-out at patrol destinations |
| S02 | PatrolWayDiversity | ChooseStartWay | Distributes group members across zone patrol ways |
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

PostBuild copies `BotNavigationMod.dll` to `<EFT>\BepInEx\plugins\BotNavigationMod\`.

## Config

Generated on first load: `BepInEx\config\com.devmaximus.botnavigationmod.cfg`

Shipped defaults: `config/defaults/com.devmaximus.botnavigationmod.cfg`

Master toggle: `[General] Enabled`.
