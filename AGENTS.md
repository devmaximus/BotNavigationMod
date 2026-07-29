# AGENTS.md — BotNavigationMod

AI agent guide for this repo. Read before editing.

---

## What This Project Is

**BotNavigationMod** is a BepInEx **client** plugin for SPT 4.0.x. It augments scav/group **patrol** navigation via config-gated strategies (fan-out, way diversity, staggered departure). It does **not** replace combat AI (SAIN) or PMC questing (QuestingBots).

| Fact | Value |
|------|-------|
| GitHub | [devmaximus/BotNavigationMod](https://github.com/devmaximus/BotNavigationMod) |
| Stack | C# / .NET Standard 2.1, Harmony, Unity NavMesh |
| SPT target | **4.0.x** |
| Plugin GUID | `com.devmaximus.botnavigationmod` |
| Config file | `BepInEx/config/com.devmaximus.botnavigationmod.cfg` |

**Decision (this fork):** Option C — S01 + S02 + S03.

### Path placeholders (portable)

| Token | Meaning |
|-------|---------|
| `<EFT>` | Live EscapeFromTarkov install root (your machine) |
| `$env:SPT_TARKOV_DIR` | Same as `<EFT>` with trailing `\`, preferred for scripts |
| `$env:SPT_CONFIG_HISTORY` | Optional path to workspace `configs/config-history` repo |

Never commit absolute game or workspace paths. Pass `-p:TarkovDir="<EFT>\"` or set `SPT_TARKOV_DIR`.

---

## Strategies

| ID | Class | Hook scope | Effect |
|----|-------|------------|--------|
| S01 | `PatrolPointOffsetStrategy` | `GoToPoint` | Perpendicular offset at patrol destinations |
| S02 | `PatrolWayDiversityStrategy` | `ChooseStartWay` / `TryToFindWay` | Spread members across zone `PatrolWay`s |
| S03 | `StaggeredTransitionStrategy` | `FindNextPoint` | Delay departures so groups do not leave in lockstep |

Primary S01 hook (decompile-corrected): **Prefix** `PatrollingData.GoToPoint()` (parameterless) — mutate `CurTargetPoint` before `BotOwner.GoToPoint`.

---

## Build and Deploy

```powershell
# One-time local override (gitignored), or use -p / env each build:
# copy Directory.Build.props.user.example → Directory.Build.props.user

$env:SPT_TARKOV_DIR = "<EFT>\"   # trailing backslash
dotnet build BotNavigationMod/BotNavigationMod.csproj -c Release
# equivalent: -p:TarkovDir="<EFT>\"
```

- **PostBuild** copies DLL → `<EFT>\BepInEx\plugins\BotNavigationMod\`
- Close the game if the DLL is locked.
- Deploy does **not** overwrite the live `.cfg`.

---

## Config Paths

| Role | Path |
|------|------|
| Live BepInEx cfg | `<EFT>\BepInEx\config\com.devmaximus.botnavigationmod.cfg` |
| Shipped stable defaults (repo) | `config/defaults/com.devmaximus.botnavigationmod.cfg` |
| Workspace history (canonical) | `{config-history}/EscapeFromTarkov/BepInEx/config/com.devmaximus.botnavigationmod.cfg/` |

This plugin is **client-only**. It does **not** ship an SPT server mod or write under `EscapeFromTarkov_Data\`.

### What we version for rollback (workspace convention)

Full-file snapshots under a sibling `configs/config-history/` git repo (when present), **not** under `EscapeFromTarkov_Data`.

| Tracked family | Relative path under `<EFT>` | When |
|----------------|----------------------------|------|
| **This plugin** | `BepInEx/config/com.devmaximus.botnavigationmod.cfg` | Before/after every live cfg tune |
| **SPT server JSON** (stack peers) | `SPT/SPT_Data/configs/pmc.json`, `SPT/user/mods/QuestingBots/config.json` | Before stack experiments |
| **Peer BepInEx cfg** | `BepInEx/config/com.danw.questingbots.cfg`, `com.mpstark.dynamicmaps.cfg` | Optional |

---

## Snapshot / Restore Protocol (MANDATORY)

### Before editing live config

Prefer `/config-snapshot` skill (Kind + Context required):

```powershell
pwsh -File <workspace>/.cursor/skills/utility/config-snapshot/scripts/snapshot.ps1 `
  -RelativePath "BepInEx/config/com.devmaximus.botnavigationmod.cfg" `
  -Kind pre `
  -Summary "describe upcoming edit" `
  -Context "Why this edit, what to restore if bad, work unit id." `
  -Plugin "BotNavigationMod"
```

Seed from shipped defaults if live cfg missing:

```powershell
$eft = $env:SPT_TARKOV_DIR.TrimEnd('\')
$live = Join-Path $eft "BepInEx\config\com.devmaximus.botnavigationmod.cfg"
$def  = Join-Path $PSScriptRoot "config\defaults\com.devmaximus.botnavigationmod.cfg"  # from repo root
New-Item -ItemType Directory -Force -Path (Split-Path $live) | Out-Null
Copy-Item -Force $def $live
```

### Restore to stable defaults

```powershell
pwsh -File ./scripts/restore-config.ps1 -Source Defaults -TarkovDir $env:SPT_TARKOV_DIR
```

### Restore from config-history version

```powershell
pwsh -File ./scripts/restore-config.ps1 -Source History -Version 1 -TarkovDir $env:SPT_TARKOV_DIR
```

Restart the **client** after cfg restore.

### Disable without deleting cfg

Set `[General] Enabled = false` in the live cfg (or flip per-strategy `Enabled` keys).

---

## Key Source Layout

```text
BotNavigationMod/
├── Plugin.cs
├── Framework/
├── Config/
├── Strategies/
├── Patches/
├── Helpers/
├── config/defaults/
└── scripts/restore-config.ps1
```

---

## Conventions

- `TarkovDir` via `-p`, `SPT_TARKOV_DIR`, or gitignored `Directory.Build.props.user` — never committed absolutes.
- No LINQ in hot strategy `Execute` paths.
- Strategies declare `HookScope`; registry filters by calling patch.
- Config: store `ConfigEntry<T>` — never snapshot `.Value` once at Awake for toggles.
- Hostility / karma fixes are **out of scope**.

---

## DO NOT

| Rule | Reason |
|------|--------|
| Commit `Directory.Build.props.user` or absolute `<EFT>` paths | Portability |
| Push without explicit approval | Policy |
| Overwrite live `.cfg` from PostBuild | Deploy copies DLL only |
| Use GUID / cfg prefix other than `com.devmaximus.botnavigationmod` | Author identity |
| Use `gh` CLI | Banned in this workspace |

---

## Related

| Artifact | Notes |
|----------|-------|
| Config history | Sibling workspace `configs/config-history/` when developing in the multi-repo layout |
| Snapshot skill | workspace `.cursor/skills/utility/config-snapshot/` |
