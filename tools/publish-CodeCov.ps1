<#
.SYNOPSIS
    Uploads code coverage to codecov.io
.PARAMETER CodeCovToken
    Code coverage token to use
.PARAMETER PathToCodeCoverage
    Path to root of code coverage files
.PARAMETER Name
    Name to upload with codecoverge
.PARAMETER Flags
    Flags to upload with codecoverge
.PARAMETER CalcNSFlags
    Calculate Flags from filename of test.
#>
[CmdletBinding()]
Param (
    [Parameter(Mandatory=$true)]
    [string]$CodeCovToken,
    [Parameter(Mandatory=$true)]
    [string]$PathToCodeCoverage,
    [string]$Name,
    [string]$Flags,
    [switch]$CalcNSFlags
)

$RepoRoot = (Resolve-Path "$PSScriptRoot/..").Path

Get-ChildItem -Recurse -LiteralPath $PathToCodeCoverage -Filter "*.cobertura.xml" | % {
    $relativeFilePath = Resolve-Path -relative $_.FullName

    $TestTypeFlag = ""
    $OSTypeFlag = ""

    if ($CalcNSFlags)
    {
        if ($relativeFilePath -ilike "*.Unit*")
        {
            $TestTypeFlag = ",Unit"
        }
        elseif ($relativeFilePath -ilike "*.Integration*")
        {
            $TestTypeFlag = ",Integration"
        }
        elseif ($relativeFilePath -ilike "*.Local*")
        {
             $TestTypeFlag = ",Local"
        }
        elseif ($relativeFilePath -ilike "*.Device*")
        {
            $TestTypeFlag = ",Device"
        }

        if ($relativeFilePath -ilike "*.Windows*")
        {
            $OSTypeFlag = ",Windows"
        }
        elseif ($relativeFilePath -ilike "*.WinUI*")
        {
            $OSTypeFlag = ",WinUI"
        }
        elseif ($relativeFilePath -ilike "*.WPF*")
        {
            $OSTypeFlag = ",WPF"
        }
        elseif ($relativeFilePath -ilike "*.MacOS*")
        {
            $OSTypeFlag = ",MacOS"
        }
        elseif ($relativeFilePath -ilike "*.MacCatalyst*")
        {
            $OSTypeFlag = ",MacCatalyst"
        }
        elseif ($relativeFilePath -ilike "*.OSX*")
        {
            $OSTypeFlag = ",MacOS"
        }
        elseif ($relativeFilePath -ilike "*.Android*")
        {
            $OSTypeFlag = ",Android"
        }
        elseif ($relativeFilePath -ilike "*.IOS*")
        {
            $OSTypeFlag = ",IOS"
        }
        elseif ($relativeFilePath -ilike "*.Linux*")
        {
            $OSTypeFlag = ",Linux"
        }
        elseif ($relativeFilePath -ilike "*.NetCore*")
        {
            $OSTypeFlag = ",NetCore"
        }
        elseif ($relativeFilePath -ilike "*.Core*")
        {
            $OSTypeFlag = ",Core"
        }
    }

    Write-Host "Uploading: $relativeFilePath" -ForegroundColor Yellow
    & (& "$PSScriptRoot/Get-CodeCovTool.ps1") -t $CodeCovToken -f $relativeFilePath -R $RepoRoot -F $Flags$TestTypeFlag$OSTypeFlag -n $Name
}
