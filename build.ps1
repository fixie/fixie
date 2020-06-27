$ErrorActionPreference = "Stop"

function step($command) {
    write-host ([Environment]::NewLine + $command.ToString().Trim()) -fore CYAN
    & $command
    if ($lastexitcode -ne 0) { exit $lastexitcode }
}

$fixie = join-path (resolve-path .) src/Fixie.Console/bin/Release/netcoreapp3.1/Fixie.Console.dll

if (test-path packages) { remove-item packages -Recurse }

step { dotnet clean src -c Release --nologo -v minimal }
step { dotnet build src -c Release --nologo }
step { dotnet $fixie *.Tests --configuration Release --no-build }
step { dotnet pack src/Fixie -c Release --no-restore --no-build --nologo }
step { dotnet pack src/Fixie.Console -c Release --no-restore --no-build --nologo }
step { dotnet pack src/Fixie.TestAdapter -c Release --no-restore --no-build --nologo }