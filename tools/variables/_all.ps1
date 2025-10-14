#!/usr/bin/env pwsh

<#
.SYNOPSIS
    This script returns a hashtable of build variables that should be set
    at the start of a build or release definition's execution.
#>

[CmdletBinding(SupportsShouldProcess = $true)]
param (
)

$vars = @{}

Get-ChildItem "$PSScriptRoot\*.ps1" -Exclude "_*" | % {
    Write-Host "Computing $($_.BaseName) variable"
    # AzDO variables are stored as environment variables which has a max length of ~32k characters.
    if ($_.Length -gt 32000) {
        # If the InsertJsonValues script returns a value longer than that,
        # write the value to a temp file and use the InsertJsonValuesFile variable that InsertVSPayload understands.
        if ($_.BaseName -eq "InsertJsonValues") {
            Write-Warning "The script InsertJsonValues.ps1 is longer than 32k characters."
            Write-Warning "Writing to a temp file and setting INSERTJSONVALUESFILE to the location of the file."

            $tempDir = "$env:AGENT_TEMPDIRECTORY"
            if (-not $tempDir) {
                Write-Warning "AGENT_TEMPDIRECTORY environment variable is not set. Defaulting to %TMP% which is set to '$env:TMP'."
                $tempDir = $env:TMP
            }

            $tmpFile = Join-Path "$tempDir" ("InsertJsonValuesFile_{0}.txt" -f ([guid]::NewGuid()))
            Write-Host "Writing contents of InsertJsonValues to file: $tmpFile."
            $contents = & $_
            Set-Content -Path $tmpFile -Value $contents
	    
            # Write file to the new parameter.
            $vars["InsertJsonValuesFile"] = "$tmpFile"
        } else {
            Write-Error "The script $($_.BaseName).ps1 is too long (> 32k characters). Skipping setting the environment variable."
        }
    } else {
        $vars[$_.BaseName] = & $_
    }
}

$vars
