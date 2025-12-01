#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Updates a Nuget Source name with arguments

.PARAMETER UpdateNugetSourceName
    Name of the nuget source
.PARAMETER UpdateNugetSourceArgs
    Arguments to pass to dotnet update source
#>
[CmdletBinding()]
Param (
    [Parameter()]
    [string]$UpdateNugetSourceName,
    [Parameter()]
    [string]$UpdateNugetSourcePath,
    [Parameter()]
    [string]$UpdateNugetUserName,
    [Parameter()]
    [string]$UpdateNugetPassword
)

$envVars = @{}

Write-Host "Updating Nuget Source: $UpdateNugetSourceName"
dotnet nuget update source $UpdateNugetSourceName -s $UpdateNugetSourcePath -u $UpdateNugetUserName -p $UpdateNugetPassword --store-password-in-clear-text

