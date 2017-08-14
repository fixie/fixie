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
    </PropertyGroup>
</Project>
"@
}

function Clean {
    exec { dotnet clean src -c $configuration }
}

function Restore {
    exec { dotnet restore src -s https://api.nuget.org/v3/index.json }
}

function Build {
    exec { dotnet build src -c $configuration }
}

function Test-Console-x64 {
    Run-Tests "Fixie.Console"
}

function Test-Console-x86 {
    Run-Tests "Fixie.Console.x86"
}

function Run-Tests($runner) {
    $fixie = resolve-path .\src\Fixie.Console\bin\$configuration\net452\$runner.exe
    exec { & $fixie .\src\Fixie.Tests\bin\$configuration\net452\Fixie.Tests.dll }
    exec { & $fixie .\src\Fixie.Samples\bin\$configuration\net452\Fixie.Samples.dll }
}

function dotnet-test {
    exec { dotnet test src/Fixie.Tests/Fixie.Tests.csproj -c $configuration --no-build }

    exec { dotnet test src/Fixie.Samples/Fixie.Samples.csproj -c $configuration --no-build `
        --test-adapter-path ../Fixie.Tests/bin/$configuration/net452 --framework net452 `
    }

    exec { dotnet test src/Fixie.Samples/Fixie.Samples.csproj -c $configuration --no-build `
        --test-adapter-path ../Fixie.Tests/bin/$configuration/netcoreapp1.0 --framework netcoreapp1.0 `
    }
}

function dotnet-fixie {
    $fixie = resolve-path .\src\Fixie.Runner\bin\$configuration\netcoreapp1.0\dotnet-fixie.dll
    exec src/Fixie.Tests { dotnet $fixie --configuration $configuration --no-build }
    exec src/Fixie.Samples { dotnet $fixie --configuration $configuration --no-build }
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
    <title>Fixie</title>
    <authors>$authors</authors>
    <owners>$authors</owners>
    <licenseUrl>https://github.com/fixie/fixie/blob/master/LICENSE.txt</licenseUrl>
    <projectUrl>https://fixie.github.io</projectUrl>
    <iconUrl>https://raw.github.com/fixie/fixie/master/img/fixie_256.png</iconUrl>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <description>A convention-based test framework.</description>
    <references>
      <reference file="Fixie.dll" />
    </references>
  </metadata>
  <files>
    <file src="..\Fixie\bin\Release\net452\Fixie.dll" target="lib\net452" />
    <file src="..\Fixie.Console\bin\Release\net452\Fixie.Console.exe" target="lib\net452" />
    <file src="..\Fixie.Console\bin\Release\net452\Fixie.Console.x86.exe" target="lib\net452" />
    <file src="..\Fixie.Execution\bin\Release\net452\Fixie.Execution.dll" target="lib\net452" />
    <file src="..\Fixie.TestDriven\bin\Release\net452\Fixie.dll.tdnet" target="lib\net452" />
    <file src="..\Fixie.TestDriven\bin\Release\net452\Fixie.TestDriven.dll" target="lib\net452" />
    <file src="..\Fixie.TestDriven\bin\Release\net452\TestDriven.Framework.dll" target="lib\net452" />
    <file src="..\Fixie.VisualStudio.TestAdapter\bin\Release\net452\Fixie.VisualStudio.TestAdapter.dll" target="lib\net452" />
    <file src="..\Fixie.VisualStudio.TestAdapter\bin\Release\net452\Mono.Cecil.dll" target="lib\net452" />
    <file src="..\Fixie.VisualStudio.TestAdapter\bin\Release\net452\Mono.Cecil.Rocks.dll" target="lib\net452" />
    <file src="..\Fixie.VisualStudio.TestAdapter\bin\Release\net452\Mono.Cecil.Pdb.dll" target="lib\net452"/>
  </files>
</package>
"@

    generate "src\Fixie.Runner\Fixie.Runner.nuspec" @"
<?xml version="1.0"?>
<package>
  <metadata>
    <id>Fixie.Runner</id>
    <version>$version</version>
    <title>Fixie.Runner</title>
    <authors>$authors</authors>
    <owners>$authors</owners>
    <licenseUrl>https://github.com/fixie/fixie/blob/master/LICENSE.txt</licenseUrl>
    <projectUrl>https://fixie.github.io</projectUrl>
    <iconUrl>https://raw.github.com/fixie/fixie/master/img/fixie_256.png</iconUrl>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <description>Test runners for the Fixie test framework.</description>
    <packageTypes>
      <packageType name="DotNetCliTool" />
    </packageTypes>
    <dependencies>
      <group targetFramework=".NETCoreApp1.0">
        <dependency id="Microsoft.NETCore.App" version="1.1.2" exclude="Build,Analyzers" />
      </group>
    </dependencies>
  </metadata>
  <files>

    <file target="lib\netcoreapp1.0" src="..\Fixie.Runner\bin\Release\netcoreapp1.0\dotnet-fixie.dll" />
    <file target="lib\netcoreapp1.0" src="..\Fixie.Runner\bin\Release\netcoreapp1.0\dotnet-fixie.runtimeconfig.json" />
    <file target="lib\netcoreapp1.0" src="..\Fixie.Runner\bin\Release\netcoreapp1.0\dotnet-fixie.targets" />
    <file target="lib\netcoreapp1.0" src="..\Fixie.Runner\bin\Release\netcoreapp1.0\Fixie.dll" />

    <file target="tools\netcoreapp1.0" src="..\Fixie.Console\bin\Release\netcoreapp1.0\Fixie.Console.dll" />
    <file target="tools\netcoreapp1.0" src="..\Fixie.Console\bin\Release\netcoreapp1.0\Fixie.Console.runtimeconfig.json" />
    <file target="tools\netcoreapp1.0" src="..\Fixie.Console\bin\Release\netcoreapp1.0\Fixie.Console.x86.dll" />
    <file target="tools\netcoreapp1.0" src="..\Fixie.Console\bin\Release\netcoreapp1.0\Fixie.Console.x86.runtimeconfig.json" />
    <file target="tools\netcoreapp1.0" src="..\Fixie.Console\bin\Release\netcoreapp1.0\Fixie.Execution.dll" />
    <file target="tools\netcoreapp1.0" src="..\Fixie.Console\bin\Release\netcoreapp1.0\Fixie.dll" />

    <file target="tools\net452" src="..\Fixie.Console\bin\Release\net452\Fixie.Console.exe" />
    <file target="tools\net452" src="..\Fixie.Console\bin\Release\net452\Fixie.Console.runtimeconfig.json" />
    <file target="tools\net452" src="..\Fixie.Console\bin\Release\net452\Fixie.Console.x86.exe" />
    <file target="tools\net452" src="..\Fixie.Console\bin\Release\net452\Fixie.Console.x86.runtimeconfig.json" />
    <file target="tools\net452" src="..\Fixie.Console\bin\Release\net452\Fixie.Execution.dll" />
    <file target="tools\net452" src="..\Fixie.Console\bin\Release\net452\Fixie.dll" />

  </files>
</package>
"@
}

function Package {
    exec { dotnet pack src\Fixie `
                    -c $configuration `
                    -o ..\..\packages `
                    --include-symbols `
                    --no-build `
                    /p:NuspecFile=Fixie.nuspec }

    exec { dotnet pack src\Fixie.Runner `
                    -c $configuration `
                    -o ..\..\packages `
                    --include-symbols `
                    --no-build `
                    /p:NuspecFile=Fixie.Runner.nuspec }
}

run-build {
    step { License }
    step { Assembly-Properties }
    step { Clean }
    step { Restore }
    step { Build }
    step { Test-Console-x64 }
    step { Test-Console-x86 }
    step { dotnet-test }
    step { dotnet-fixie }
    step { Nuspec }
    step { Package }
}