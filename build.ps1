param([string]$target)

$birthYear = 2013
$maintainers = "Patrick Lioi"
$configuration = 'Release'
$version = "2.0.0-alpha"
$nonPublishedProjects = "Fixie.Tests","Fixie.Samples"

function main {
    try {
        step { Restore }
        step { SanityCheckOutputPaths }
        step { AssemblyInfo }
        step { License }
        step { Compile }
        step { Test }
        step { Test32 }

        if ($target -eq "package") {
            step { Package }
        }

        write-host
        write-host "Build Succeeded!" -fore GREEN
        write-host
        summarize-steps
        exit 0
    } catch [Exception] {
        write-host
        write-host $_.Exception.Message -fore RED
        write-host
        write-host "Build Failed!" -fore RED
        exit 1
    }
}

function Restore {
    .\tools\NuGet.exe restore .\src\Fixie.sln -source "https://nuget.org/api/v2/" -RequireConsent -o ".\src\packages"
}

function Package {
    rd .\package -recurse -force -ErrorAction SilentlyContinue | out-null
    mkdir .\package -ErrorAction SilentlyContinue | out-null
    exec { & .\tools\NuGet.exe pack .\src\Fixie\Fixie.csproj -Symbols -Prop Configuration=$configuration -OutputDirectory .\package }

    write-host
    write-host "To publish these packages, issue the following command:"
    write-host "   tools\NuGet push .\package\Fixie.$version.nupkg"
}

function Test {
    run-tests "Fixie.Console.exe"
}

function Test32 {
    run-tests "Fixie.Console.x86.exe"
}

function run-tests($exe) {
    $fixieRunner = resolve-path ".\build\$exe"
    exec { & $fixieRunner .\src\Fixie.Tests\bin\$configuration\Fixie.Tests.dll .\src\Fixie.Samples\bin\$configuration\Fixie.Samples.dll }
}

function Compile {
    Set-Alias msbuild (get-msbuild-path)
    rd .\build -recurse -force  -ErrorAction SilentlyContinue | out-null
    exec { msbuild /t:clean /v:q /nologo /p:Configuration=$configuration .\src\Fixie.sln }
    exec { msbuild /t:build /v:q /nologo /p:Configuration=$configuration .\src\Fixie.sln }
}

function SanityCheckOutputPaths {
    $blankLine = ([System.Environment]::NewLine + [System.Environment]::NewLine)
    $expected = "..\..\build\"

    $projects = @(gci .\src -rec -filter *.csproj)
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

function AssemblyInfo {
    $assemblyVersion = $version
    if ($assemblyVersion.Contains("-")) {
        $assemblyVersion = $assemblyVersion.Substring(0, $assemblyVersion.IndexOf("-"))
    }

    $copyright = get-copyright

    $projects = @(gci .\src -rec -filter *.csproj)
    foreach ($project in $projects) {
        $projectName = [System.IO.Path]::GetFileNameWithoutExtension($project)

        if ($projectName.Contains(".x86")) {
            continue;
        }

        regenerate-file "$($project.DirectoryName)\Properties\AssemblyInfo.cs" @"
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: AssemblyProduct("Fixie")]
[assembly: AssemblyTitle("$projectName")]
[assembly: AssemblyVersion("$assemblyVersion")]
[assembly: AssemblyFileVersion("$assemblyVersion")]
[assembly: AssemblyInformationalVersion("$version")]
[assembly: AssemblyCopyright("$copyright")]
[assembly: AssemblyCompany("$maintainers")]
[assembly: AssemblyConfiguration("$configuration")]
"@
    }
}

function License {
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

function exec($cmd) {
    $global:lastexitcode = 0
    & $cmd
    if ($lastexitcode -ne 0) {
        throw "Error executing command:$cmd"
    }
}

function get-msbuild-path {
    [cmdletbinding()]
    param(
        [Parameter(Position=0)]
        [ValidateSet('32bit','64bit')]
        [string]$bitness = '32bit'
    )
    process{

        # Find the highest installed version of msbuild.exe.

        $regLocalKey = $null

        if($bitness -eq '32bit') {
            $regLocalKey = [Microsoft.Win32.RegistryKey]::OpenBaseKey([Microsoft.Win32.RegistryHive]::LocalMachine,[Microsoft.Win32.RegistryView]::Registry32)
        } else {
            $regLocalKey = [Microsoft.Win32.RegistryKey]::OpenBaseKey([Microsoft.Win32.RegistryHive]::LocalMachine,[Microsoft.Win32.RegistryView]::Registry64)
        }

        $versionKeyName = $regLocalKey.OpenSubKey('SOFTWARE\Microsoft\MSBuild\ToolsVersions\').GetSubKeyNames() | Sort-Object {[double]$_} -Descending

        $keyToReturn = ('SOFTWARE\Microsoft\MSBuild\ToolsVersions\{0}' -f $versionKeyName)

        $path = ( '{0}msbuild.exe' -f $regLocalKey.OpenSubKey($keyToReturn).GetValue('MSBuildToolsPath'))

        return $path
    }
}

function step($block) {
    $name = $block.ToString().Trim()
    write-host "Executing $name" -fore CYAN
    $sw = [Diagnostics.Stopwatch]::StartNew()
    &$block
    $sw.Stop()

    if (!$script:timings) {
        $script:timings = @()
    }

    $script:timings += new-object PSObject -property @{
        Name = $name;
        Duration = $sw.Elapsed
    }
}

function summarize-steps {
    $script:timings | format-table -autoSize -property Name,Duration | out-string -stream | where-object { $_ }
}

main