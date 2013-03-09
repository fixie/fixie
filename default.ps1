Framework '4.0'

properties {
    $project = 'Fixie'
    $birthYear = 2013
    $maintainers = "Patrick Lioi"

    $configuration = 'Release'
    $src = resolve-path '.\src'
    $build = if ($env:build_number -ne $NULL) { $env:build_number } else { '0' }
    $version = [IO.File]::ReadAllText('.\VERSION.txt') + '.' + $build
}

task default -depends NUnitTest

task NUnitTest -depends SelfTest {
    $nunitRunner = join-path $src "packages\NUnit.Runners.2.6.2\tools\nunit-console.exe"
    & $nunitRunner $src\$project.Tests\bin\$configuration\$project.Tests.dll /nologo /nodots /framework:net-4.0

    if ($lastexitcode -gt 0)
    {
        throw "{0} unit tests failed." -f $lastexitcode
    }
    if ($lastexitcode -lt 0)
    {
        throw "Unit test run was terminated by a fatal error."
    }
}

task SelfTest -depends Compile {
    $fixieRunner = join-path $src "$project.Console\bin\$configuration\$project.Console.exe"
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
[assembly: AssemblyConfiguration(""$configuration"")]" | out-file "$src\CommonAssemblyInfo.cs" -encoding "ASCII"
}