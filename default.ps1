Framework '4.0'

properties {
    $project = "Fixie"
    $birthYear = 2013
    $maintainers = "Patrick Lioi"
    $description = "A convention-based test framework."

    $configuration = 'Release'
    $src = resolve-path '.\src'
    $build = if ($env:build_number -ne $NULL) { $env:build_number } else { '0' }
    $version = [IO.File]::ReadAllText('.\VERSION.txt') + '.' + $build
}

task default -depends xUnitTest

task Package -depends xUnitTest {
    rd .\package -recurse -force -ErrorAction SilentlyContinue | out-null
    mkdir .\package -ErrorAction SilentlyContinue | out-null
    exec { & $src\.nuget\NuGet.exe pack $src\$project\$project.csproj -Symbols -Prop Configuration=$configuration -OutputDirectory .\package }

    write-host
    write-host "To publish these packages, issue the following command:"
    write-host "   nuget push .\package\$project.$version.nupkg"
}

task xUnitTest -depends SelfTest {
    $xunitRunner = join-path $src "packages\xunit.runners.1.9.1\tools\xunit.console.clr4.exe"
    exec { & $xunitRunner $src\$project.Tests\bin\$configuration\$project.Tests.dll }
}

task SelfTest -depends Compile {
    $fixieRunner = resolve-path ".\build\$project.Console.exe"
    & $fixieRunner $src\$project.Tests\bin\$configuration\$project.Tests.dll

    if ($lastexitcode -gt 0)
    {
         "{0} unit tests failed." -f $lastexitcode
    }
    if ($lastexitcode -lt 0)
    {
         "Unit test run was terminated by a fatal error."
    }
}

task Compile -depends CommonAssemblyInfo {
  rd .\build -recurse -force  -ErrorAction SilentlyContinue | out-null
  exec { msbuild /t:clean /v:q /nologo /p:Configuration=$configuration $src\$project.sln }
  exec { msbuild /t:build /v:q /nologo /p:Configuration=$configuration $src\$project.sln }
}

task CommonAssemblyInfo {
    $date = Get-Date
    $year = $date.Year
    $copyrightSpan = if ($year -eq $birthYear) { $year } else { "$birthYear-$year" }
    $copyright = "Copyright (c) $copyrightSpan $maintainers"

"using System.Reflection;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: AssemblyProduct(""$project"")]
[assembly: AssemblyVersion(""$version"")]
[assembly: AssemblyFileVersion(""$version"")]
[assembly: AssemblyCopyright(""$copyright"")]
[assembly: AssemblyCompany(""$maintainers"")]
[assembly: AssemblyDescription(""$description"")]
[assembly: AssemblyConfiguration(""$configuration"")]" | out-file "$src\CommonAssemblyInfo.cs" -encoding "ASCII"
}