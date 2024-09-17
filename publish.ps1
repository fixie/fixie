param([switch]$ci)

$ErrorActionPreference = "Stop"

$packages = get-childitem packages/*.nupkg

if ([string]::IsNullOrEmpty($env:PACKAGE_API_KEY)) {
    $severity = "::error::"
    $exitCode = 1

    if ($ci) {
        $severity = "::warning::"
        $exitCode = 0
    }

    Write-Host "$($severity)$($MyInvocation.MyCommand.Name): PACKAGE_API_KEY is empty or not set. No packages will be pushed."

    exit $exitCode
} else {
    foreach ($package in $packages) {
        dotnet nuget push $package --source $env:PACKAGE_URL --api-key $env:PACKAGE_API_KEY
        if ($lastexitcode -ne 0) { throw $lastexitcode }
    }
}