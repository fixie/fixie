$ErrorActionPreference = "Stop"

function step($command) {
    write-host ([Environment]::NewLine + $command.ToString().Trim()) -fore CYAN
    & $command
    if ($lastexitcode -ne 0) { throw $lastexitcode }
}

$fixie = "src/Fixie.Console/bin/Release/net5.0/Fixie.Console.dll"

if (test-path artifacts) { remove-item artifacts -Recurse }

step { dotnet clean src -c Release --nologo -v minimal }
step { dotnet build src -c Release --nologo }
step { dotnet $fixie *.Tests -c Release --no-build }
step { dotnet pack src/Fixie -o artifacts -c Release --no-build --nologo }
step { dotnet pack src/Fixie.Console -o artifacts -c Release --no-build --nologo }

# While Fixie.TestAdapter packs with a nuspec file, we cannot include --no-build here.
# If we did, the MinVer-calculated version would fail to apply to the package.
step { dotnet pack src/Fixie.TestAdapter -o artifacts -c Release --nologo }