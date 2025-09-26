$vstsDropNames = & "$PSScriptRoot\VstsDropNames.ps1"
$BuildConfiguration = $env:BUILDCONFIGURATION
if (!$BuildConfiguration) {
    $BuildConfiguration = 'Debug'
}

$BasePath = "$PSScriptRoot\..\..\bin\Packages\$BuildConfiguration\Vsix"

if (Test-Path $BasePath) {
    $BasePath = (Resolve-Path $BasePath).Path
    $vsmanFiles = @()
    Get-ChildItem $BasePath *.vsman -Recurse -File |% {
        $version = (Get-Content $_.FullName | ConvertFrom-Json).info.buildVersion
        $fullPath = (Resolve-Path $_.FullName).Path

        # Fallback for older PowerShell versions - use Resolve-Path to ensure consistent comparison
        if ($fullPath.StartsWith($BasePath, [System.StringComparison]::OrdinalIgnoreCase)) {
            $rfn = $resolvedFilePath.Substring($BasePath.Length).TrimStart('\', '/')
        }
        else {
            # If file is not under base path, use just the filename
            Write-Verbose "File $($_.FullName) is not under base path $BasePath, using just the filename."
            $rfn = $_.Name
        }

        $fn = $_.Name
        $vsmanFiles += "$fn{$version}=https://vsdrop.corp.microsoft.com/file/v1/$vstsDropNames;$rfn"

    }

    [string]::Join(',', $vsmanFiles)
}
