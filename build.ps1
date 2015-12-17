Framework '4.5.2'

properties {
    $birthYear = 2013
    $maintainers = "Patrick Lioi"

    $configuration = 'Release'
    $src = resolve-path '.\src'
    $tools = resolve-path '.\tools'
    $build = if ($env:build_number -ne $NULL) { $env:build_number } else { '0' }
    $version = [IO.File]::ReadAllText('.\VERSION.txt') + '.' + $build
    $projects = @(gci $src -rec -filter *.csproj)
    $nonPublishedProjects = "Build","Fixie.Tests","Fixie.Samples"
}

task default -depends Test

task Package -depends Test {
    rd .\package -recurse -force -ErrorAction SilentlyContinue | out-null
    mkdir .\package -ErrorAction SilentlyContinue | out-null
    exec { & $tools\NuGet.exe pack $src\Fixie\Fixie.csproj -Symbols -Prop Configuration=$configuration -OutputDirectory .\package }

    write-host
    write-host "To publish these packages, issue the following command:"
    write-host "   nuget push .\package\Fixie.$version.nupkg"
}

task Test -depends Compile {
    run-tests "Fixie.Console.exe"
}

task Test32 -depends Compile {
    run-tests "Fixie.Console.x86.exe"
}

function run-tests($exe) {
    $fixieRunner = resolve-path ".\build\$exe"
    exec { & $fixieRunner $src\Fixie.Tests\bin\$configuration\Fixie.Tests.dll $src\Fixie.Samples\bin\$configuration\Fixie.Samples.dll }
}

task Compile -depends SanityCheckOutputPaths, AssemblyInfo, License {
  rd .\build -recurse -force  -ErrorAction SilentlyContinue | out-null
  exec { msbuild /t:clean /v:q /nologo /p:Configuration=$configuration $src\Fixie.sln }
  exec { msbuild /t:build /v:q /nologo /p:Configuration=$configuration $src\Fixie.sln }
}

task SanityCheckOutputPaths {
    $blankLine = ([System.Environment]::NewLine + [System.Environment]::NewLine)
    $expected = "..\..\build\"

    foreach ($project in $projects) {
        $projectName = [System.IO.Path]::GetFileNameWithoutExtension($project)

        $lines = [System.IO.File]::ReadAllLines($project.FullName, [System.Text.Encoding]::UTF8)

        if (!($nonPublishedProjects -contains $projectName)) {
            foreach($line in $lines) {
                if ($line.Contains("<OutputPath>")) {

                    $outputPath = [regex]::Replace($line, '\s*<OutputPath>(.+)</OutputPath>\s*', '$1')

                    if($outputPath -ne $expected){
                        $summary = "The project '$projectName' has a suspect *.csproj file."
                        $detail = "Expected OutputPath to be $expected for all configurations."

                        Write-Host -ForegroundColor Yellow "$($blankLine)$($summary)  $($detail)$($blankLine)"
                        throw $summary
                    }
                }
            }
        }
    }
}

task AssemblyInfo {
    $copyright = get-copyright

    foreach ($project in $projects) {
        $projectName = [System.IO.Path]::GetFileNameWithoutExtension($project)

        if ($projectName.Contains(".x86")) {
            continue;
        }

        if ($projectName -eq "Build") {
            continue;
        }

        regenerate-file "$($project.DirectoryName)\Properties\AssemblyInfo.cs" @"
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: AssemblyProduct("Fixie")]
[assembly: AssemblyTitle("$projectName")]
[assembly: AssemblyVersion("$version")]
[assembly: AssemblyFileVersion("$version")]
[assembly: AssemblyCopyright("$copyright")]
[assembly: AssemblyCompany("$maintainers")]
[assembly: AssemblyConfiguration("$configuration")]
"@
    }
}

task License {
    $copyright = get-copyright

    regenerate-file "LICENSE.txt" @"
The MIT License (MIT)
$copyright

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
"@
}

function get-copyright {
    $date = Get-Date
    $year = $date.Year
    $copyrightSpan = if ($year -eq $birthYear) { $year } else { "$birthYear-$year" }
    return "Copyright © $copyrightSpan $maintainers"
}

function regenerate-file($path, $newContent) {
    $oldContent = [IO.File]::ReadAllText($path)

    if ($newContent -ne $oldContent) {
        $relativePath = Resolve-Path -Relative $path
        write-host "Generating $relativePath"
        [System.IO.File]::WriteAllText($path, $newContent, [System.Text.Encoding]::UTF8)
    }
}