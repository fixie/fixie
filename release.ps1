dotnet nuget push 'artifacts/*.nupkg' --source https://api.nuget.org/v3/index.json --api-key $env:NUGET_API_KEY
if ($lastexitcode -ne 0) { throw $lastexitcode }