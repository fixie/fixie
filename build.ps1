param([int]$buildNumber=0)

. .\build-helpers

$birthYear = 2013
$maintainers = "Patrick Lioi"
$configuration = 'Release'

$revision = "{0:D4}" -f [convert]::ToInt32($buildNumber, 10)

function clean {
    delete-folder .\artifacts
    @(gci .\src -rec -filter bin) | % { delete-folder $_.FullName }
    @(gci .\src -rec -filter obj) | % { delete-folder $_.FullName }
}

function dotnet-restore {
    exec { & dotnet restore --verbosity Warning }
}

function dotnet-pack($project) {
    exec { & dotnet pack .\src\$project --output .\artifacts --configuration $configuration --version-suffix $revision }
}

function dotnet-build($project) {
    exec { & dotnet build .\src\$project --configuration $configuration --version-suffix $revision }
}

function dotnet-test($project) {
    exec { & dotnet test .\src\$project --configuration $configuration }
}

run-build {
    step { clean }
    step { license }

    step { dotnet-restore }

    step { dotnet-pack Fixie }
    step { dotnet-pack Fixie.Execution }
    step { dotnet-pack Fixie.Runner }

    step { dotnet-build Fixie.TestDriven }
    step { dotnet-build Fixie.Assertions }

    step { dotnet-test Fixie.Tests }
    step { dotnet-test Fixie.Samples }
}