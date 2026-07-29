# BotNavigationMod

BepInEx client plugin for SPT 4.0.x that improves group patrol navigation.

## Strategies

| ID | Name | Hook scope | Effect |
|----|------|------------|--------|
| S01 | PatrolPointOffset | GoToPoint | Perpendicular fan-out at patrol destinations |
| S02 | PatrolWayDiversity | ChooseStartWay | Distributes group members across zone patrol ways |
| S03 | StaggeredTransition | FindNextPoint | Delays departure so groups do not leave in lockstep |

## Build

```powershell
cd D:\Work\local\Test\plugins\ai\BotNavigationMod
dotnet build BotNavigationMod/BotNavigationMod.csproj -c Release
```

PostBuild copies `BotNavigationMod.dll` to `<TarkovDir>\BepInEx\plugins\BotNavigationMod\`.

## Config

Generated on first load: `BepInEx\config\com.mike.botnavigationmod.cfg`

Master toggle: `[General] Enabled`.
