$packages = get-childitem artifacts/*.nupkg | where-object {!$_.Name.EndsWith(".symbols.nupkg")}

foreach ($package in $packages) {
    dotnet nuget push $package --source $env:NUGET_URL --api-key $env:NUGET_API_KEY
    if ($lastexitcode -ne 0) { throw $lastexitcode }
}