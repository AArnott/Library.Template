if ($env:AGENT_TEMPDIRECTORY) {
    # The DotNetCoreCLI uses an alternate location to publish these files
    $guidRegex = '^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$'
    $files = @()
    $files += Get-ChildItem $env:AGENT_TEMPDIRECTORY -Directory |? { $_.Name -match $guidRegex } |% { Get-ChildItem "$($_.FullName)\dotnet*.dmp","$($_.FullName)\testhost*.dmp","$($_.FullName)\Sequence_*.xml" -Recurse }
    $files += Get-ChildItem $env:AGENT_TEMPDIRECTORY\*.trx
    Write-Host (Get-ChildItem $env:AGENT_TEMPDIRECTORY\*.trx)
    Write-Host "Collected testResults: $files"
    @{
        $env:AGENT_TEMPDIRECTORY = $files;
    }
} else {
    $testRoot = Resolve-Path "$PSScriptRoot\..\..\test"
    @{
        $testRoot = (Get-ChildItem "$testRoot\TestResults" -Recurse -Directory | Get-ChildItem -Recurse -File);
    }
}
