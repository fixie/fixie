param([string]$target)

$birthYear = 2013
$maintainers = "Patrick Lioi"
$configuration = 'Release'
$version = "2.0.0-alpha"

function main {
    step { Restore }
    step { AssemblyInfo }
    step { License }
    step { Compile }
}

function Restore {
    exec { .\tools\NuGet.exe restore .\Fixie.sln -ConfigFile nuget.config -RequireConsent -o ".\src\packages" }
}

function Compile {
    Set-Alias msbuild (get-msbuild-path)
    rd .\build -recurse -force  -ErrorAction SilentlyContinue | out-null
    exec { msbuild /t:clean /v:q /nologo /p:Configuration=$configuration .\Fixie.sln }
    exec { msbuild /t:build /v:q /nologo /p:Configuration=$configuration .\Fixie.sln }
}

function AssemblyInfo {
    $assemblyVersion = $version
    if ($assemblyVersion.Contains("-")) {
        $assemblyVersion = $assemblyVersion.Substring(0, $assemblyVersion.IndexOf("-"))
    }

    $copyright = get-copyright

    $projects = @(gci .\src -rec -filter *.xproj)
    foreach ($project in $projects) {
        $projectName = [System.IO.Path]::GetFileNameWithoutExtension($project)

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

try {
    main
    write-host
    write-host "Build Succeeded!" -fore GREEN
    write-host
    summarize-steps
    exit 0
} catch [Exception] {
    write-host
    write-host $_.Exception.Message -fore DARKRED
    write-host
    write-host "Build Failed!" -fore DARKRED
    exit 1
}