<#
.SYNOPSIS
    This script translates the variables returned by the _all.ps1 script
    into commands that instruct Azure Pipelines to actually set those variables for other pipeline tasks to consume.

    The build or release definition may have set these variables to override
    what the build would do. So only set them if they have not already been set.
#>

[CmdletBinding()]
param (
)

(& "$PSScriptRoot\_all.ps1").GetEnumerator() | % {
    # Always use ALL CAPS for env var names since Azure Pipelines converts variable names to all caps and on non-Windows OS, env vars are case sensitive.
    $keyCaps = $_.Key.ToUpper()
    if ((Test-Path "env:$keyCaps") -and (Get-Content "env:$keyCaps")) {
        Write-Host "Skipping setting $keyCaps because variable is already set to '$(Get-Content env:$keyCaps)'." -ForegroundColor Cyan
    } else {
        Write-Host "$keyCaps=$($_.Value)" -ForegroundColor Yellow

        # AzDO variables are stored as environment variables which has a max length of ~32k characters.
        if ($_.Key -eq "InsertJsonValues" -and $($_.Value).Length -gt 32000) {
            # If the InsertJsonValues value is longer than that,
            # write the value to a temp file and use the InsertJsonValuesFile variable that InsertVSPayload understands.
            Write-Warning "The value of InsertJsonValues is longer than 32k characters."
            Write-Warning "Writing to a temp file and setting INSERTJSONVALUESFILE to the location of the file."

            $tempDir = "$env:AGENT_TEMPDIRECTORY"
            if (-not $tempDir) {
                Write-Warning "AGENT_TEMPDIRECTORY environment variable is not set. Defaulting to %TMP% which is set to '$env:TMP'."
                $tempDir = $env:TMP
            }

            $tmpFile = Join-Path "$tempDir" ("InsertJsonValuesFile_{0}.txt" -f ([guid]::NewGuid()))
            Write-Host "Writing contents of InsertJsonValues to file: $tmpFile."
            Set-Content -Path $tmpFile -Value $($_.Value)

            # Set the InsertJsonValuesFile variable to the temp file path.
            $insertJsonValuesFileKeyCaps = "INSERTJSONVALUESFILE"
            Write-Host "$insertJsonValuesFileKeyCaps=$tmpFile" -ForegroundColor Yellow
            if ($env:TF_BUILD) {
                Write-Host "##vso[task.setvariable variable=$insertJsonValuesFileKeyCaps]$tmpFile"
                Write-Host "##vso[task.setvariable variable=$insertJsonValuesFileKeyCaps;isOutput=true]$tmpFile"
            } elseif ($env:GITHUB_ACTIONS) {
                Add-Content -LiteralPath $env:GITHUB_ENV -Value "$insertJsonValuesFileKeyCaps=$tmpFile"
            }

            Set-Item -LiteralPath "env:$insertJsonValuesFileKeyCaps" -Value $tmpFile
        } elseif ($($_.Value).Length -gt 32000) {
            Write-Warning "The value is too long (> 32k characters). Skipping setting the environment variables."
        } else {
            if ($env:TF_BUILD) {
                # Create two variables: the first that can be used by its simple name and accessible only within this job.
                Write-Host "##vso[task.setvariable variable=$keyCaps]$($_.Value)"
                # and the second that works across jobs and stages but must be fully qualified when referenced.
                Write-Host "##vso[task.setvariable variable=$keyCaps;isOutput=true]$($_.Value)"
            } elseif ($env:GITHUB_ACTIONS) {
                Add-Content -LiteralPath $env:GITHUB_ENV -Value "$keyCaps=$($_.Value)"
            }

            Set-Item -LiteralPath "env:$keyCaps" -Value $_.Value
        }
    }
}
