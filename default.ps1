Framework '4.0'

properties {
    $birthYear = 2013
    $maintainers = "Patrick Lioi"
    $description = "A convention-based test framework."

    $configuration = 'Release'
    $src = resolve-path '.\src'
    $build = if ($env:build_number -ne $NULL) { $env:build_number } else { '0' }
    $version = [IO.File]::ReadAllText('.\VERSION.txt') + '.' + $build
    $projects = @(gci $src -rec -filter *.csproj)
}

task default -depends Test

task Package -depends Test {
    rd .\package -recurse -force -ErrorAction SilentlyContinue | out-null
    mkdir .\package -ErrorAction SilentlyContinue | out-null
    exec { & $src\.nuget\NuGet.exe pack $src\Fixie\Fixie.csproj -Symbols -Prop Configuration=$configuration -OutputDirectory .\package }

    write-host
    write-host "To publish these packages, issue the following command:"
    write-host "   nuget push .\package\Fixie.$version.nupkg"
}

task Test -depends Compile {
    $fixieRunner = resolve-path ".\build\Fixie.Console.exe"
    exec { & $fixieRunner $src\Fixie.Tests\bin\$configuration\Fixie.Tests.dll $src\Fixie.Samples\bin\$configuration\Fixie.Samples.dll }
}

task Compile -depends AssemblyInfo {
  rd .\build -recurse -force  -ErrorAction SilentlyContinue | out-null
  exec { msbuild /t:clean /v:q /nologo /p:Configuration=$configuration $src\Fixie.sln }
  exec { msbuild /t:build /v:q /nologo /p:Configuration=$configuration $src\Fixie.sln }
}

task AssemblyInfo {
    $date = Get-Date
    $year = $date.Year
    $copyrightSpan = if ($year -eq $birthYear) { $year } else { "$birthYear-$year" }
    $copyright = "Copyright © $copyrightSpan $maintainers"

    foreach ($project in $projects) {
        $projectName = [System.IO.Path]::GetFileNameWithoutExtension($project)
        $assemblyInfoPath = "$($project.DirectoryName)\Properties\AssemblyInfo.cs"

        write-host "Generating $assemblyInfoPath"

"using System.Reflection;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: AssemblyProduct(""Fixie"")]
[assembly: AssemblyTitle(""$projectName"")]
[assembly: AssemblyVersion(""$version"")]
[assembly: AssemblyFileVersion(""$version"")]
[assembly: AssemblyCopyright(""$copyright"")]
[assembly: AssemblyCompany(""$maintainers"")]
[assembly: AssemblyDescription(""$description"")]
[assembly: AssemblyConfiguration(""$configuration"")]" | out-file "$($project.DirectoryName)\Properties\AssemblyInfo.cs" -encoding "UTF8"
    }
}