function copyright($startYear, $authors) {
    $date = Get-Date
    $currentYear = $date.Year
    $copyrightSpan = if ($currentYear -eq $startYear) { $currentYear } else { "$startYear-$currentYear" }
    return "Copyright © $copyrightSpan $authors"
}

function generate($path, $content) {
    $oldContent = [IO.File]::ReadAllText($path)

    if ($content -ne $oldContent) {
        $relativePath = Resolve-Path -Relative $path
        write-host "Generating $relativePath"
        [System.IO.File]::WriteAllText($path, $content, [System.Text.Encoding]::UTF8)
    }
}

function mit-license($copyright) {
    generate "LICENSE.txt" @"
The MIT License (MIT)
$copyright

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
"@
}

function exec($command, $path) {
    if ($null -eq $path) {
        $global:lastexitcode = 0
        & $command
    } else {
        Push-Location $path
        $global:lastexitcode = 0
        & $command
        Pop-Location
    }

    if ($lastexitcode -ne 0) {
        throw "Error executing command: $command"
    }
}

function step($block) {
    $command = $block.ToString().Trim()

    write-host
    write-host $command -fore CYAN

    &$block
}

function main($mainBlock) {
    try {
        &$mainBlock
        write-host
        write-host "Build Succeeded" -fore GREEN
        exit 0
    } catch [Exception] {
        write-host
        write-host $_.Exception.Message -fore DARKRED
        write-host
        write-host "Build Failed" -fore DARKRED
        exit 1
    }
}