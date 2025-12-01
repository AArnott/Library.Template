$RepoRoot = [System.IO.Path]::GetFullPath("$PSScriptRoot\..\..")
$BuildConfiguration = $env:BUILDCONFIGURATION
if (!$BuildConfiguration) {
    $BuildConfiguration = 'Debug-Windows'
}

$PackagesRoot = "$RepoRoot/bin/Packages/$BuildConfiguration"

if (!(Test-Path $PackagesRoot))  { return }

@{
    "$PackagesRoot" = (Get-ChildItem $PackagesRoot -Recurse)
}
