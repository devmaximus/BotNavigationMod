<#
.SYNOPSIS
  Restore BotNavigationMod BepInEx config to shipped defaults or a config-history version.

.PARAMETER Source
  Defaults — copy from repo config/defaults
  History  — copy from config-history snapshot

.PARAMETER Version
  History only: version number (e.g. 1). Omit = latest snapshot.

.PARAMETER TarkovDir
  Live EFT/SPT install root (trailing slash optional).
  Default: $env:SPT_TARKOV_DIR

.PARAMETER ConfigHistoryRoot
  Workspace config-history repo root.
  Default: $env:SPT_CONFIG_HISTORY or auto-detect ../../../../configs/config-history from this script
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    [Parameter(Mandatory)]
    [ValidateSet('Defaults', 'History')]
    [string] $Source,

    [int] $Version = 0,

    [string] $TarkovDir = $env:SPT_TARKOV_DIR,

    [string] $ConfigHistoryRoot = $env:SPT_CONFIG_HISTORY
)

$ErrorActionPreference = 'Stop'

$PluginGuid = 'com.devmaximus.botnavigationmod'
$CfgName = "$PluginGuid.cfg"

$pluginRoot = Split-Path -Parent $PSScriptRoot
$defaultsCfg = Join-Path $pluginRoot "config\defaults\$CfgName"

if ([string]::IsNullOrWhiteSpace($TarkovDir)) {
    throw "TarkovDir unset. Pass -TarkovDir or set env SPT_TARKOV_DIR to your EscapeFromTarkov install root."
}
$TarkovDir = $TarkovDir.TrimEnd('\', '/') + '\'
$liveCfg = Join-Path $TarkovDir "BepInEx\config\$CfgName"

if ([string]::IsNullOrWhiteSpace($ConfigHistoryRoot)) {
    $probe = (Resolve-Path (Join-Path $pluginRoot '..\..\..\configs\config-history') -ErrorAction SilentlyContinue)
    if ($probe) { $ConfigHistoryRoot = $probe.Path }
}
if ([string]::IsNullOrWhiteSpace($ConfigHistoryRoot) -or -not (Test-Path -LiteralPath $ConfigHistoryRoot)) {
    throw "ConfigHistoryRoot unset/missing. Pass -ConfigHistoryRoot or set env SPT_CONFIG_HISTORY."
}

$historyRoot = Join-Path $ConfigHistoryRoot "EscapeFromTarkov\BepInEx\config\$CfgName"

function Get-HistorySource {
    param([int] $VersionNum)

    $versionsDir = Join-Path $historyRoot 'versions'
    if (-not (Test-Path -LiteralPath $versionsDir)) {
        throw "No config-history for BotNavigationMod yet at $versionsDir. Snapshot first."
    }

    $dirs = @(Get-ChildItem -Path $versionsDir -Directory |
        Where-Object { $_.Name -match '^v(\d+)_' } |
        Sort-Object { [int]($_.Name -replace '^v(\d+)_.*', '$1') })

    if ($dirs.Count -eq 0) {
        throw "No version folders under $versionsDir"
    }

    if ($VersionNum -gt 0) {
        $match = $dirs | Where-Object { [int]($_.Name -replace '^v(\d+)_.*', '$1') -eq $VersionNum } | Select-Object -First 1
        if (-not $match) {
            throw "Version $VersionNum not found under $versionsDir"
        }
        return (Join-Path $match.FullName $CfgName)
    }

    $latest = $dirs[-1]
    return (Join-Path $latest.FullName $CfgName)
}

$src = if ($Source -eq 'Defaults') {
    if (-not (Test-Path -LiteralPath $defaultsCfg)) {
        throw "Missing defaults file: $defaultsCfg"
    }
    $defaultsCfg
}
else {
    Get-HistorySource -VersionNum $Version
}

if (-not (Test-Path -LiteralPath $src)) {
    throw "Source file not found: $src"
}

New-Item -ItemType Directory -Force -Path (Split-Path $liveCfg) | Out-Null

if ($PSCmdlet.ShouldProcess($liveCfg, "Restore from $src")) {
    Copy-Item -LiteralPath $src -Destination $liveCfg -Force
    Write-Host "Restored: $liveCfg"
    Write-Host "From:     $src"
    Write-Host "Restart the Tarkov client for cfg to apply."
}
