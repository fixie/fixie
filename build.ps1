param($buildNumber)

. .\build-helpers

$birthYear = 2013
$maintainers = "Patrick Lioi"
$configuration = 'Release'
$prerelease = $true

if ($buildNumber -eq $null) {
    $revision = "dev-" + (Get-Date).ToString("yyyyMMdd-HHmmss")
} else {
    if ($prerelease) {
        $revision = "alpha-{0:D4}" -f [convert]::ToInt32($buildNumber, 10)
    } else {
        $revision = ""
    }
}

function clean {
    delete-folder .\artifacts
    @(gci .\src -rec -filter bin) | % { delete-folder $_.FullName }
    @(gci .\src -rec -filter obj) | % { delete-folder $_.FullName }
}

function dotnet-restore($project, $source="https://api.nuget.org/v3/index.json") {
    exec { & dotnet restore .\src\$project --verbosity Information --source $source }
}

function dotnet-pack($project) {
    if ($revision) {
        exec { & dotnet pack .\src\$project --output .\artifacts --configuration $configuration --version-suffix $revision }
    } else {
        exec { & dotnet pack .\src\$project --output .\artifacts --configuration $configuration }
    }
}

function dotnet-build($project) {
    if ($revision) {
        exec { & dotnet build .\src\$project --configuration $configuration --version-suffix "$revision" }
    } else {
        exec { & dotnet build .\src\$project --configuration $configuration }
    }
}

function dotnet-test($project) {
    exec { & dotnet test .\src\$project --configuration $configuration }
}

run-build {
    step { clean }
    step { license }


    step { dotnet-restore Fixie }
    step { dotnet-restore Fixie.Execution }
    step { dotnet-restore Fixie.Runner }

    step { dotnet-pack Fixie }
    step { dotnet-pack Fixie.Execution }
    step { dotnet-pack Fixie.Runner }


    step { dotnet-restore Fixie.TestDriven }
    step { dotnet-restore Fixie.Assertions }

    step { dotnet-build Fixie.TestDriven }
    step { dotnet-build Fixie.Assertions }


    $artifacts = resolve-path .\artifacts
    step { dotnet-restore Fixie.Tests $artifacts }
    step { dotnet-restore Fixie.Samples $artifacts }

    step { dotnet-test Fixie.Tests }
    step { dotnet-test Fixie.Samples }
}