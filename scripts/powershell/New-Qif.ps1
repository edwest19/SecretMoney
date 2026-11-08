# SecretMoney QIF writer
# Developed with Microsoft Copilot assistance.
#
# Creates a simple banking QIF file compatible with the MYM 12
# banking-import behavior tested by the SecretMoney project.
#
# This script does not read, copy, modify, or distribute MYM files.

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [datetime]$Date,

    [Parameter(Mandatory)]
    [decimal]$Amount,

    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]$Payee,

    [string]$Memo,

    [string]$Category,

    [string]$CheckNumber,

    [string]$OutputPath = (Join-Path $env:OneDrive 'MYM\DriveD\IMPORT.QIF')
)

$ErrorActionPreference = 'Stop'

function Test-QifText {
    param(
        [string]$Value,
        [string]$FieldName
    )

    if ($null -eq $Value) {
        return
    }

    if ($Value -match "[`r`n]") {
        throw "$FieldName cannot contain CR or LF characters."
    }

    if ($Value -notmatch '^[\x20-\x7E]*$') {
        throw "$FieldName must contain ASCII characters only."
    }
}

Test-QifText -Value $Payee -FieldName 'Payee'
Test-QifText -Value $Memo -FieldName 'Memo'
Test-QifText -Value $Category -FieldName 'Category'
Test-QifText -Value $CheckNumber -FieldName 'CheckNumber'

if ([string]::IsNullOrWhiteSpace($Payee)) {
    throw 'Payee cannot be blank.'
}

$OutputPath = [Environment]::ExpandEnvironmentVariables($OutputPath)
$OutputDirectory = Split-Path -Parent $OutputPath

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = (Get-Location).Path
    $OutputPath = Join-Path $OutputDirectory $OutputPath
}

if (-not (Test-Path -LiteralPath $OutputDirectory -PathType Container)) {
    throw "Output directory does not exist: $OutputDirectory"
}

$DateText = $Date.ToString(
    'MM/dd/yyyy',
    [Globalization.CultureInfo]::InvariantCulture
)

$AmountText = $Amount.ToString(
    '0.00',
    [Globalization.CultureInfo]::InvariantCulture
)

$Lines = [System.Collections.Generic.List[string]]::new()

$Lines.Add('!Type:Bank')
$Lines.Add("D$DateText")
$Lines.Add("T$AmountText")
$Lines.Add("P$Payee")

if (-not [string]::IsNullOrWhiteSpace($Memo)) {
    $Lines.Add("M$Memo")
}

if (-not [string]::IsNullOrWhiteSpace($Category)) {
    $Lines.Add("L$Category")
}

if (-not [string]::IsNullOrWhiteSpace($CheckNumber)) {
    $Lines.Add("N$CheckNumber")
}

$Lines.Add('^')

# Use ASCII and CRLF for compatibility with legacy DOS-era software.
$Content = ($Lines -join "`r`n") + "`r`n"
[IO.File]::WriteAllText(
    $OutputPath,
    $Content,
    [Text.Encoding]::ASCII
)

Write-Host ''
Write-Host 'SecretMoney QIF created.' -ForegroundColor Green
Write-Host "Output: $OutputPath"
Write-Host "Date:   $DateText"
Write-Host "Amount: $AmountText"
Write-Host "Payee:  $Payee"
Write-Host ''
Write-Host 'No MYM program or database files were modified.'
