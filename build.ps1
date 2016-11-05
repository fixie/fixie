param([int]$buildNumber=0)

$birthYear = 2013
$maintainers = "Patrick Lioi"
$configuration = 'Release'

$revision = "{0:D4}" -f [convert]::ToInt32($buildNumber, 10)

function main {
    clean
    license

    dotnet-restore

    dotnet-pack Fixie
    dotnet-pack Fixie.Execution
    dotnet-pack Fixie.Runner

    dotnet-build Fixie.TestDriven
    dotnet-build Fixie.Assertions

    dotnet-test Fixie.Tests
    dotnet-test Fixie.Samples
}

function clean {
    heading "clean"
    rd .\artifacts -recurse -force -ErrorAction SilentlyContinue | out-null

    foreach ($folder in @(gci .\src -rec -filter bin)) {
       write-host "Removing $($folder.FullName)"
       rd $folder.FullName -recurse -force -ErrorAction SilentlyContinue | out-null
    }

    foreach ($folder in @(gci .\src -rec -filter obj)) {
       write-host "Removing $($folder.FullName)"
       rd $folder.FullName -recurse -force -ErrorAction SilentlyContinue | out-null
    }
}

function dotnet-restore {
    heading "dotnet restore"
    exec { & dotnet restore --verbosity Warning }
}

function dotnet-test($project) {
    heading "dotnet test $project"
    exec { & dotnet test .\src\$project --configuration $configuration }
}

function dotnet-build($project) {
    heading "dotnet build $project"
    exec { & dotnet build .\src\$project --configuration $configuration --version-suffix $revision }
}

function dotnet-pack($project) {
    heading "dotnet pack $project"
    exec { & dotnet pack .\src\$project --output .\artifacts --configuration $configuration --version-suffix $revision }
}

function license {
    heading "license"
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

function heading($text) {
    write-host $text -fore CYAN
    write-host
}

try {
    main
    write-host
    write-host "Build Succeeded!" -fore GREEN
    exit 0
} catch [Exception] {
    write-host
    write-host $_.Exception.Message -fore DARKRED
    write-host
    write-host "Build Failed!" -fore DARKRED
    exit 1
}