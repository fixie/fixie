param([int]$buildNumber)

. $PSScriptRoot/build-helpers

$versionPrefix = "3.0.0"
$prerelease = $true

$authors = "Patrick Lioi"
$copyright = copyright 2013 $authors
$configuration = 'Release'
$versionSuffix = if ($prerelease) { "beta-{0:D4}" -f $buildNumber } else { "" }

function Build {
    mit-license $copyright

    generate "$PSScriptRoot/src/Directory.Build.props" @"
<Project>
    <PropertyGroup>
        <Product>Fixie</Product>
        <VersionPrefix>$versionPrefix</VersionPrefix>
        <VersionSuffix>$versionSuffix</VersionSuffix>
        <Authors>$authors</Authors>
        <Copyright>$copyright</Copyright>
        <PackageLicenseExpression>MIT</PackageLicenseExpression>
        <PackageProjectUrl>https://fixie.github.io</PackageProjectUrl>
        <PackageIcon>icon.png</PackageIcon>
        <RepositoryUrl>https://github.com/fixie/fixie</RepositoryUrl>
        <PackageOutputPath>..\..\packages</PackageOutputPath>
        <IncludeSymbols>true</IncludeSymbols>
        <LangVersion>latest</LangVersion>
        <Nullable>enable</Nullable>
        <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    </PropertyGroup>
</Project>
"@

    exec { dotnet clean src -c $configuration --nologo -v minimal }
    exec { dotnet build src -c $configuration --nologo }
}

function Test {
    $fixie = resolve-path ./src/Fixie.Console/bin/$configuration/netcoreapp3.1/Fixie.Console.dll

    exec { dotnet $fixie *.Tests --configuration $configuration --no-build }
}

function Pack {
    remove-folder packages
    exec { dotnet pack -c $configuration --no-restore --no-build --nologo } src/Fixie
    exec { dotnet pack -c $configuration --no-restore --no-build --nologo } src/Fixie.Console
    exec { dotnet pack -c $configuration --no-restore --no-build --nologo } src/Fixie.TestAdapter
}

main {
    step { dotnet --version }
    step { Build }
    step { Test }
    step { Pack }
}