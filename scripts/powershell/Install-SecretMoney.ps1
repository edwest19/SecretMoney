# SecretMoney environment setup
# Developed with Microsoft Copilot assistance.
#
# This script DOES NOT download, copy, install, extract, or modify
# Managing Your Money or any original MYM installation media.

[CmdletBinding(SupportsShouldProcess)]
param(
    [string]$OneDrivePath = $env:OneDrive
)

$ErrorActionPreference = 'Stop'

Write-Host ''
Write-Host 'SecretMoney environment setup' -ForegroundColor Cyan
Write-Host '-----------------------------'
Write-Host ''

# Verify Windows.
if (-not $IsWindows -and $PSVersionTable.PSEdition -eq 'Core') {
    throw 'SecretMoney setup currently targets Windows.'
}

# Verify OneDrive path.
if ([string]::IsNullOrWhiteSpace($OneDrivePath)) {
    throw 'OneDrive path could not be determined. Supply -OneDrivePath explicitly.'
}

if (-not (Test-Path -LiteralPath $OneDrivePath -PathType Container)) {
    throw "OneDrive directory does not exist: $OneDrivePath"
}

$OneDrivePath = (Resolve-Path -LiteralPath $OneDrivePath).Path

$MymRoot = Join-Path $OneDrivePath 'MYM'
$DriveC  = Join-Path $MymRoot 'DriveC'
$DriveD  = Join-Path $MymRoot 'DriveD'

Write-Host "OneDrive: $OneDrivePath"
Write-Host "MYM root: $MymRoot"
Write-Host ''

# Check useful prerequisites without installing anything.
$Winget = Get-Command winget.exe -ErrorAction SilentlyContinue
$SevenZip = Join-Path $env:ProgramFiles '7-Zip\7z.exe'

if ($Winget) {
    Write-Host '[OK] winget is available.' -ForegroundColor Green
}
else {
    Write-Warning 'winget was not found.'
}

if (Test-Path -LiteralPath $SevenZip) {
    Write-Host "[OK] 7-Zip found: $SevenZip" -ForegroundColor Green
}
else {
    Write-Warning '7-Zip was not found in the standard Program Files location.'
}

$DosBoxPackage = $null

if ($Winget) {
    try {
        $DosBoxPackage = winget list --id joncampbell123.DOSBox-X --exact 2>$null |
            Select-String 'joncampbell123.DOSBox-X'
    }
    catch {
        $DosBoxPackage = $null
    }
}

if ($DosBoxPackage) {
    Write-Host '[OK] DOSBox-X appears to be installed.' -ForegroundColor Green
}
else {
    Write-Warning 'DOSBox-X was not confirmed as installed.'
}

Write-Host ''
Write-Host 'Runtime directories:' -ForegroundColor Cyan
Write-Host "  DOS C: -> $DriveC"
Write-Host "  DOS D: -> $DriveD"
Write-Host ''

foreach ($Directory in @($MymRoot, $DriveC, $DriveD)) {
    if (Test-Path -LiteralPath $Directory -PathType Container) {
        Write-Host "[EXISTS] $Directory"
        continue
    }

    if ($PSCmdlet.ShouldProcess($Directory, 'Create directory')) {
        New-Item -ItemType Directory -Path $Directory | Out-Null
        Write-Host "[CREATED] $Directory" -ForegroundColor Green
    }
}

Write-Host ''
Write-Host 'Setup complete.' -ForegroundColor Green
Write-Host ''
Write-Host 'DOSBox-X mount commands:'
Write-Host ''
Write-Host "  MOUNT C `"$DriveC`""
Write-Host "  MOUNT D `"$DriveD`""
Write-Host ''
Write-Host 'IMPORTANT:' -ForegroundColor Yellow
Write-Host 'ging Your Money files were downloaded or copied.'
Write-Host '  Original MYM software and installation media must be supplied separately by the user.'
Write-Host '  Do not place original MYM files or personal financial databases in the SecretMoney repository.'
Write-Host ''
