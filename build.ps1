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
    exec { dotnet pack src/Fixie -c Release --no-restore --no-build --nologo }
    exec { dotnet pack src/Fixie.Console -c Release --no-restore --no-build --nologo }
    exec { dotnet pack src/Fixie.TestAdapter -c Release --no-restore --no-build --nologo }
}

main {
    step { Build }
    step { Test }
    step { Pack }
}