# AGENTS.md — BotNavigationMod

AI agent guide for `plugins/ai/BotNavigationMod`. Read this before editing.

---

## What This Project Is

**BotNavigationMod** is a BepInEx **client** plugin for SPT 4.0.x. It augments scav/group **patrol** navigation via config-gated strategies (fan-out, way diversity, staggered departure). It does **not** replace combat AI (SAIN) or PMC questing (QuestingBots).

| Fact | Value |
|------|-------|
| Repo | `plugins/ai/BotNavigationMod/` (own git repo) |
| Branch | `feature/option-c-patrol-suite` |
| Stack | C# / .NET Standard 2.1, Harmony, Unity NavMesh |
| Live game | `D:\Games\EscapeFromTarkov` |
| SPT | **4.0.13** · Tarkov client **0.16.9.40087** |
| Plugin GUID | `com.mike.botnavigationmod` |
| Work unit | `.cursor-template/docs/work/active/20260729_091502_bot-navigation-mod/` |

**Decision:** Option C — S01 + S02 + S03.

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
cd D:\Work\local\Test\plugins\ai\BotNavigationMod
dotnet build BotNavigationMod/BotNavigationMod.csproj -c Release
```

- **PostBuild** copies DLL → `D:\Games\EscapeFromTarkov\BepInEx\plugins\BotNavigationMod\`
- Close the game if the DLL is locked.
- Deploy does **not** overwrite the live `.cfg`.

---

## Config Paths

| Role | Path |
|------|------|
| Live BepInEx cfg | `D:\Games\EscapeFromTarkov\BepInEx\config\com.mike.botnavigationmod.cfg` |
| Shipped stable defaults (repo) | `config/defaults/com.mike.botnavigationmod.cfg` |
| Workspace history (canonical) | `configs/config-history/EscapeFromTarkov/BepInEx/config/com.mike.botnavigationmod.cfg/` |

This plugin is **client-only**. It does **not** ship an SPT server mod or write under `EscapeFromTarkov_Data\`.

### What we version for rollback (workspace convention)

Same pattern as QuestingBots / DynamicMaps — full-file snapshots under `configs/config-history/` (own git repo), **not** under `EscapeFromTarkov_Data` (Managed/StreamingAssets are not live-editable knobs).

| Tracked family | Example live path | When |
|----------------|-------------------|------|
| **This plugin** | `BepInEx/config/com.mike.botnavigationmod.cfg` | Before/after every live cfg tune |
| **SPT server JSON** (stack peers) | `SPT/SPT_Data/configs/pmc.json`, `SPT/user/mods/QuestingBots/config.json` | Before stack experiments that might interact with spawn density |
| **Peer BepInEx cfg** | `BepInEx/config/com.danw.questingbots.cfg`, `com.mpstark.dynamicmaps.cfg` | Optional — when validating markers / spawn side-by-side |

`EscapeFromTarkov_Data\` is **not** snapshotted here (no editable JSON/.cfg knobs for this mod). Rollback of game binaries is out of scope — use game reinstall / previous install folder if needed.

---

## Snapshot / Restore Protocol (MANDATORY)

### Before editing live config

Prefer `/config-snapshot` skill (Kind + Context required):

```powershell
pwsh -File D:\Work\local\Test\.cursor\skills\utility\config-snapshot\scripts\snapshot.ps1 `
  -RelativePath "BepInEx/config/com.mike.botnavigationmod.cfg" `
  -Kind pre `
  -Summary "describe upcoming edit" `
  -Context "Why this edit, what to restore if bad, work unit id." `
  -WorkUnit "20260729_091502_bot-navigation-mod" `
  -Plugin "BotNavigationMod"
```

If the live cfg does not exist yet (never loaded plugin), seed from shipped defaults then snapshot:

```powershell
$live = "D:\Games\EscapeFromTarkov\BepInEx\config\com.mike.botnavigationmod.cfg"
$def  = "D:\Work\local\Test\plugins\ai\BotNavigationMod\config\defaults\com.mike.botnavigationmod.cfg"
New-Item -ItemType Directory -Force -Path (Split-Path $live) | Out-Null
Copy-Item -Force $def $live
pwsh -File D:\Work\local\Test\configs\config-history\scripts\snapshot-config.ps1 `
  -Install EscapeFromTarkov `
  -RelativePath "BepInEx/config/com.mike.botnavigationmod.cfg" `
  -Summary "baseline: shipped Option C defaults" `
  -Commit
```

### Restore to stable defaults (fast)

```powershell
pwsh -File D:\Work\local\Test\plugins\ai\BotNavigationMod\scripts\restore-config.ps1 -Source Defaults
```

### Restore from config-history version

```powershell
pwsh -File D:\Work\local\Test\plugins\ai\BotNavigationMod\scripts\restore-config.ps1 -Source History -Version 1
# or omit -Version to restore latest snapshot
```

Restart the **client** after cfg restore. Restart **SPT.Server** only if you restored SPT JSON peers.

### Disable without deleting cfg

Set `[General] Enabled = false` in the live cfg (or flip per-strategy `Enabled` keys).

---

## Key Source Layout

```text
BotNavigationMod/
├── Plugin.cs
├── Framework/     # INavigationStrategy, registry, interceptor, HookScope
├── Config/        # BepInEx ConfigEntry wrappers (live .Value reads)
├── Strategies/    # S01 / S02 / S03
├── Patches/       # Harmony targets (decompile-corrected signatures)
├── Helpers/
├── config/defaults/   # Stable shipped cfg for rollback
└── scripts/           # restore-config.ps1
```

---

## Conventions

- Match CorpseFixPlugin-style csproj refs (`TarkovDir`, PostBuild copy).
- No LINQ in hot strategy `Execute` paths.
- Strategies declare `HookScope`; registry filters by calling patch.
- Config: store `ConfigEntry<T>` — never snapshot `.Value` once at Awake for toggles.
- Hostility / karma fixes are **out of scope** (separate findings / work unit).

---

## DO NOT

| Rule | Reason |
|------|--------|
| Push without explicit user approval | Workspace policy |
| Destructive git (`reset --hard`, `clean -fd`, …) | Hook-blocked |
| Overwrite live `.cfg` from PostBuild | Deploy copies DLL only |
| Modify immutable `config-history/.../versions/vNNN_*` files | Create a new `vNNN` instead |
| Treat `EscapeFromTarkov_Data` as config-history | Not our knobs; use SPT_Data / BepInEx |
| Use `gh` CLI | Banned |

---

## Related Artifacts

| Artifact | Path |
|----------|------|
| Work plan | `.cursor-template/docs/work/active/20260729_091502_bot-navigation-mod/bot-navigation-mod.plan.md` |
| Phase status | `…/implement/phase-status.md` |
| Config history repo | `configs/config-history/` |
| Snapshot script | `configs/config-history/scripts/snapshot-config.ps1` |
