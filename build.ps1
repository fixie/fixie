. $PSScriptRoot/build-helpers

function Build {
    exec { dotnet clean src -c Release --nologo -v minimal }
    exec { dotnet build src -c Release --nologo }
}

function Test {
    $fixie = resolve-path ./src/Fixie.Console/bin/Release/netcoreapp3.1/Fixie.Console.dll

    exec { dotnet $fixie *.Tests --configuration Release --no-build }
}

function Pack {
    remove-folder packages
    exec { dotnet pack -c Release --no-restore --no-build --nologo } src/Fixie
    exec { dotnet pack -c Release --no-restore --no-build --nologo } src/Fixie.Console
    exec { dotnet pack -c Release --no-restore --no-build --nologo } src/Fixie.TestAdapter
}

main {
    step { dotnet --version }
    step { Build }
    step { Test }
    step { Pack }
}