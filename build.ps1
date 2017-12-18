param([int]$buildNumber)

. .\build-helpers

$versionPrefix = "2.0.0"
$prerelease = $true

$authors = "Patrick Lioi"
$copyright = copyright 2013 $authors
$configuration = 'Release'
$versionSuffix = if ($prerelease) { "alpha-{0:D4}" -f $buildNumber } else { "" }

function License {
    mit-license $copyright
}

function Assembly-Properties {
    generate "src\Directory.build.props" @"
<Project>
    <PropertyGroup>
        <Product>Fixie</Product>
        <VersionPrefix>$versionPrefix</VersionPrefix>
        <VersionSuffix>$versionSuffix</VersionSuffix>
        <Authors>$authors</Authors>
        <Copyright>$copyright</Copyright>
        <PackageLicenseUrl>https://github.com/fixie/fixie/blob/master/LICENSE.txt</PackageLicenseUrl>
        <PackageProjectUrl>https://fixie.github.io</PackageProjectUrl>
        <PackageIconUrl>https://raw.github.com/fixie/fixie/master/img/fixie_256.png</PackageIconUrl>
        <RepositoryUrl>https://github.com/fixie/fixie</RepositoryUrl>
        <PackageOutputPath>..\..\packages</PackageOutputPath>
        <IncludeSymbols>true</IncludeSymbols>
        <LangVersion>latest</LangVersion>
    </PropertyGroup>
</Project>
"@
}

function Clean {
    exec { dotnet clean src -c $configuration /nologo }
}

function Restore {
    exec { dotnet restore src -s https://api.nuget.org/v3/index.json }
}

function Build {
    exec { dotnet build src -c $configuration /nologo }
}

function Test {
    $fixie = resolve-path .\src\Fixie.Console\bin\$configuration\netcoreapp1.0\dotnet-fixie.dll

    exec { dotnet $fixie --configuration $configuration --no-build } src/Fixie.Tests
    exec { dotnet $fixie --configuration $configuration --no-build } src/Fixie.Samples
}

function Package {
    exec { dotnet pack -c $configuration --no-build /nologo } src\Fixie
    exec { dotnet pack -c $configuration --no-build /nologo } src\Fixie.Console
}

run-build {
    step { License }
    step { Assembly-Properties }
    step { Clean }
    step { Restore }
    step { Build }
    step { Test }
    step { Package }
}