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

function dotnet-fixie {
    $fixie = resolve-path .\src\Fixie.Runner\bin\$configuration\netcoreapp1.0\dotnet-fixie.dll

    $dotnet_fixie = { dotnet $fixie --configuration $configuration --no-build }
    $dotnet_fixie_x86 = { dotnet $fixie --configuration $configuration --no-build --x86 --framework net452 }

    exec src/Fixie.Tests $dotnet_fixie
    exec src/Fixie.Tests $dotnet_fixie_x86

    exec src/Fixie.Samples $dotnet_fixie
    exec src/Fixie.Samples $dotnet_fixie_x86
}

function Nuspec {
    $version = $versionPrefix
    if ($versionSuffix -ne "") {
        $version = "$version-$versionSuffix"
    }

    generate "src\Fixie\Fixie.nuspec" @"
<?xml version="1.0"?>
<package>
  <metadata>
    <id>Fixie</id>
    <version>$version</version>
    <authors>$authors</authors>
    <owners>$authors</owners>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <licenseUrl>https://github.com/fixie/fixie/blob/master/LICENSE.txt</licenseUrl>
    <projectUrl>https://fixie.github.io</projectUrl>
    <iconUrl>https://raw.github.com/fixie/fixie/master/img/fixie_256.png</iconUrl>
    <description>A convention-based test framework.</description>
    <copyright>$copyright</copyright>
    <repository url="https://github.com/fixie/fixie" />
    <references>
      <reference file="Fixie.dll" />
    </references>
    <dependencies>
      <group targetFramework="netcoreapp1.0">
        <dependency id="Microsoft.NET.Test.Sdk" version="15.3.0" />
        <dependency id="System.Reflection.TypeExtensions" version="4.3.0" />
        <dependency id="System.Runtime.Serialization.Json" version="4.3.0" />
      </group>
    </dependencies>
  </metadata>
  <files>
    <!-- Reference Library -->

    <file target="lib\net452" src="..\Fixie\bin\Release\net452\Fixie.dll" />
    <file target="lib\netstandard1.5" src="..\Fixie\bin\Release\netstandard1.5\Fixie.dll" />

    <!-- TestDriven.NET Adapter -->

    <file target="lib\net452" src="..\Fixie.TestDriven\bin\Release\net452\Fixie.dll.tdnet" />
    <file target="lib\net452" src="..\Fixie.TestDriven\bin\Release\net452\Fixie.TestDriven.dll" />
    <file target="lib\net452" src="..\Fixie.TestDriven\bin\Release\net452\TestDriven.Framework.dll" />

    <!-- Visual Studio Adapter -->

    <file target="build\net452" src="bin\Release\net452\Fixie.dll" />
    <file target="build\net452" src="bin\Release\net452\Fixie.VisualStudio.TestAdapter.dll" />
    <file target="build\net452" src="bin\Release\net452\Mono.Cecil.dll" />
    <file target="build\net452" src="bin\Release\net452\Mono.Cecil.Rocks.dll" />
    <file target="build\net452" src="bin\Release\net452\Mono.Cecil.Pdb.dll" />
    <file target="build\netcoreapp1.0" src="bin\Release\netcoreapp1.0\Fixie.VisualStudio.TestAdapter.dll" />

    <!-- Run Time Support -->

    <file target="build\netcoreapp1.0" src="..\Fixie\bin\Release\netstandard1.5\Fixie.dll" />

    <file target="build\netcoreapp1.0\Fixie.props" src="Fixie.build.props" />

  </files>
</package>
"@

    generate "src\Fixie.Runner\Fixie.Runner.nuspec" @"
<?xml version="1.0"?>
<package>
  <metadata>
    <id>Fixie.Console</id>
    <version>$version</version>
    <authors>$authors</authors>
    <owners>$authors</owners>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <licenseUrl>https://github.com/fixie/fixie/blob/master/LICENSE.txt</licenseUrl>
    <projectUrl>https://fixie.github.io</projectUrl>
    <iconUrl>https://raw.github.com/fixie/fixie/master/img/fixie_256.png</iconUrl>
    <description>`dotnet fixie` console test runner for the Fixie test framework.</description>
    <copyright>$copyright</copyright>
    <repository url="https://github.com/fixie/fixie" />
    <packageTypes>
      <packageType name="DotNetCliTool" />
    </packageTypes>
    <dependencies>
      <group targetFramework="netcoreapp1.0">
        <dependency id="Microsoft.NETCore.App" version="1.1.2" exclude="Build,Analyzers" />
      </group>
    </dependencies>
  </metadata>
  <files>

    <file target="prefercliruntime" src="..\Fixie.Runner\prefercliruntime" />

    <file target="lib\netcoreapp1.0" src="..\Fixie.Runner\bin\Release\netcoreapp1.0\dotnet-fixie.dll" />
    <file target="lib\netcoreapp1.0" src="..\Fixie.Runner\bin\Release\netcoreapp1.0\dotnet-fixie.runtimeconfig.json" />
    <file target="lib\netcoreapp1.0" src="..\Fixie.Runner\bin\Release\netcoreapp1.0\dotnet-fixie.targets" />
    <file target="lib\netcoreapp1.0" src="..\Fixie.Runner\bin\Release\netcoreapp1.0\Fixie.dll" />

    <file target="tools\netcoreapp1.0" src="..\Fixie.Console\bin\Release\netcoreapp1.0\Fixie.Console.dll" />
    <file target="tools\netcoreapp1.0" src="..\Fixie.Console\bin\Release\netcoreapp1.0\Fixie.Console.runtimeconfig.json" />
    <file target="tools\netcoreapp1.0" src="..\Fixie.Console\bin\Release\netcoreapp1.0\Fixie.Console.x86.dll" />
    <file target="tools\netcoreapp1.0" src="..\Fixie.Console\bin\Release\netcoreapp1.0\Fixie.Console.x86.runtimeconfig.json" />
    <file target="tools\netcoreapp1.0" src="..\Fixie.Console\bin\Release\netcoreapp1.0\Fixie.dll" />

    <file target="tools\net452" src="..\Fixie.Console\bin\Release\net452\Fixie.Console.exe" />
    <file target="tools\net452" src="..\Fixie.Console\bin\Release\net452\Fixie.Console.runtimeconfig.json" />
    <file target="tools\net452" src="..\Fixie.Console\bin\Release\net452\Fixie.Console.x86.exe" />
    <file target="tools\net452" src="..\Fixie.Console\bin\Release\net452\Fixie.Console.x86.runtimeconfig.json" />
    <file target="tools\net452" src="..\Fixie.Console\bin\Release\net452\Fixie.dll" />

  </files>
</package>
"@
}

function Package {
    $pack = { dotnet pack -c $configuration --no-build /nologo }
    exec src\Fixie $pack
    exec src\Fixie.Runner $pack
}

run-build {
    step { License }
    step { Assembly-Properties }
    step { Clean }
    step { Restore }
    step { Build }
    step { dotnet-fixie }
    step { Nuspec }
    step { Package }
}