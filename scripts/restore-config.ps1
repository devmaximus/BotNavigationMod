<#
.SYNOPSIS
  Restore BotNavigationMod BepInEx config to shipped defaults or a config-history version.

.PARAMETER Source
  Defaults — copy from repo config/defaults
  History  — copy from configs/config-history snapshot

.PARAMETER Version
  History only: version number (e.g. 1). Omit = latest snapshot.

.PARAMETER WhatIf
  Show actions without writing.
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    [Parameter(Mandatory)]
    [ValidateSet('Defaults', 'History')]
    [string] $Source,

    [int] $Version = 0
)

$ErrorActionPreference = 'Stop'

$pluginRoot = Split-Path -Parent $PSScriptRoot
$liveCfg = 'D:\Games\EscapeFromTarkov\BepInEx\config\com.mike.botnavigationmod.cfg'
$defaultsCfg = Join-Path $pluginRoot 'config\defaults\com.mike.botnavigationmod.cfg'
$historyRoot = 'D:\Work\local\Test\configs\config-history\EscapeFromTarkov\BepInEx\config\com.mike.botnavigationmod.cfg'

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
        return (Join-Path $match.FullName 'com.mike.botnavigationmod.cfg')
    }

    $latest = $dirs[-1]
    return (Join-Path $latest.FullName 'com.mike.botnavigationmod.cfg')
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
