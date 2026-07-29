<#
.SYNOPSIS
  Create github.com/devmaximus/BotNavigationMod (if missing) and push current branch + main.
  Uses GITHUB_TOKEN env + git HTTPS. Does not invoke gh CLI.
#>
[CmdletBinding()]
param(
    [string] $Owner = 'devmaximus',
    [string] $Repo = 'BotNavigationMod',
    [string] $Token = $env:GITHUB_TOKEN
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($Token)) {
    throw 'GITHUB_TOKEN env var is required'
}

$headers = @{
    Authorization           = "Bearer $Token"
    Accept                  = 'application/vnd.github+json'
    'User-Agent'            = 'BotNavigationMod-publish'
    'X-GitHub-Api-Version'  = '2022-11-28'
}

$me = Invoke-RestMethod -Uri 'https://api.github.com/user' -Headers $headers
Write-Host "Authenticated as: $($me.login)"
if ($me.login -ne $Owner) {
    throw "Token user '$($me.login)' != expected owner '$Owner'"
}

$existing = $null
try {
    $existing = Invoke-RestMethod -Uri "https://api.github.com/repos/$Owner/$Repo" -Headers $headers
    Write-Host "Repo exists: $($existing.html_url)"
}
catch {
    $createBody = @{
        name        = $Repo
        description = 'SPT BepInEx client plugin — scav/group patrol fan-out, way diversity, staggered departure (Option C)'
        private     = $false
        has_issues  = $true
        has_projects = $false
        has_wiki    = $false
        auto_init   = $false
    } | ConvertTo-Json

    $existing = Invoke-RestMethod -Method Post -Uri 'https://api.github.com/user/repos' `
        -Headers $headers -Body $createBody -ContentType 'application/json'
    Write-Host "Created: $($existing.html_url)"
}

$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot
try {
    $pushUrl = "https://x-access-token:${Token}@github.com/$Owner/$Repo.git"
    git remote remove origin 2>$null
    git remote add origin "https://github.com/$Owner/$Repo.git"
    git push $pushUrl HEAD:feature/option-c-patrol-suite
    git push $pushUrl HEAD:main
    git remote set-url origin "https://github.com/$Owner/$Repo.git"
    git fetch origin
    git branch --set-upstream-to=origin/feature/option-c-patrol-suite 2>$null
    Write-Host "Pushed feature/option-c-patrol-suite and main"
    Write-Host "Remote: https://github.com/$Owner/$Repo"
    Write-Host "Note: origin URL is token-free HTTPS."
}
finally {
    Pop-Location
}
