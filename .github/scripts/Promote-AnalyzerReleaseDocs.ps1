[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)]
  [string]$PackageVersion,

  [Parameter()]
  [string]$UnshippedPath = ".\Moq.AutoMocker.Generators\AnalyzerReleases.Unshipped.md",

  [Parameter()]
  [string]$ShippedPath = ".\Moq.AutoMocker.Generators\AnalyzerReleases.Shipped.md"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Normalize-LineEndings {
  param([string]$Content)

  return $Content -replace "`r`n?", "`n"
}

function Has-RuleEntries {
  param([string[]]$Lines)

  foreach ($line in $Lines) {
    $trimmed = $line.Trim()

    if ([string]::IsNullOrWhiteSpace($trimmed)) {
      continue
    }

    if ($trimmed.StartsWith("### ", [System.StringComparison]::Ordinal)) {
      continue
    }

    if ($trimmed -eq "Rule ID | Category | Severity | Notes") {
      continue
    }

    if ($trimmed -match '^-+\|-+\|-+\|-+$') {
      continue
    }

    if ($trimmed.StartsWith("## Release ", [System.StringComparison]::Ordinal)) {
      continue
    }

    return $true
  }

  return $false
}

if ($PackageVersion.Contains("-", [System.StringComparison]::Ordinal)) {
  Write-Host "Package version '$PackageVersion' is a prerelease. Skipping analyzer release promotion."
  exit 0
}

$unshippedContent = [System.IO.File]::ReadAllText($UnshippedPath)
$shippedContent = [System.IO.File]::ReadAllText($ShippedPath)

$normalizedUnshipped = Normalize-LineEndings -Content $unshippedContent
$normalizedShipped = Normalize-LineEndings -Content $shippedContent
$unshippedLines = $normalizedUnshipped.Split("`n")

if (-not (Has-RuleEntries -Lines $unshippedLines)) {
  Write-Host "No unshipped analyzer entries found. Skipping analyzer release promotion."
  exit 0
}

$releaseHeading = "## Release $PackageVersion"
if ($normalizedShipped -match "(?m)^$([regex]::Escape($releaseHeading))\s*$") {
  Write-Host "Release heading '$releaseHeading' already exists. Skipping analyzer release promotion."
  exit 0
}

$releaseBlock = "{0}`n`n{1}" -f $releaseHeading, $normalizedUnshipped.Trim()
$newShipped = if ([string]::IsNullOrWhiteSpace($normalizedShipped)) {
  "$releaseBlock`n"
}
else {
  "{0}`n`n{1}`n" -f $normalizedShipped.TrimEnd(), $releaseBlock
}

$cleanUnshippedLines = foreach ($line in $unshippedLines) {
  if ($line.Trim() -match '^[A-Za-z]{2,}\d{4}\s*\|') {
    continue
  }

  $line.TrimEnd()
}

$newUnshipped = ($cleanUnshippedLines -join "`n").TrimEnd()
if ([string]::IsNullOrWhiteSpace($newUnshipped)) {
  $newUnshipped = @(
    "### New Rules",
    "",
    "Rule ID | Category | Severity | Notes",
    "--------|----------|----------|-------"
  ) -join "`n"
}

$utf8Bom = [System.Text.UTF8Encoding]::new($true)
[System.IO.File]::WriteAllText($ShippedPath, ($newShipped -replace "`n", "`r`n"), $utf8Bom)
[System.IO.File]::WriteAllText($UnshippedPath, (($newUnshipped -replace "`n", "`r`n") + "`r`n"), $utf8Bom)

Write-Host "Promoted analyzer release entries for version '$PackageVersion'."
