$RepoRoot = Resolve-Path "$PSScriptRoot\..\.."

$coverageFiles = @(Get-ChildItem "$RepoRoot/*.cobertura.xml" -Recurse | Where-Object {$_.FullName -notlike "*/In/*"  -and $_.FullName -notlike "*\In\*" })

# Prepare code coverage reports for merging on another machine
Write-Host "Substituting $repoRoot with `"{reporoot}`""
$coverageFiles |% {
    $content = Get-Content -LiteralPath $_ |% { $_ -Replace [regex]::Escape($repoRoot), "{reporoot}" }
    Set-Content -LiteralPath $_ -Value $content -Encoding UTF8
}

if (!((Test-Path $RepoRoot\bin) -and (Test-Path $RepoRoot\obj))) { return }

@{
    $RepoRoot = (
        $coverageFiles +
        (Get-ChildItem "$RepoRoot\obj\*.cs" -Recurse)
    );
}
