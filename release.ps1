﻿$packages = get-childitem packages/*.nupkg

foreach ($package in $packages) {
    dotnet nuget push $package --source $env:PACKAGE_URL --api-key $env:PACKAGE_API_KEY
    if ($lastexitcode -ne 0) { throw $lastexitcode }
}