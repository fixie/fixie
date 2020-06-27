function generate($path, $content) {
    $oldContent = [IO.File]::ReadAllText($path)

    if ($content -ne $oldContent) {
        $relativePath = Resolve-Path -Relative $path
        write-host "Generating $relativePath"
        [System.IO.File]::WriteAllText($path, $content, [System.Text.Encoding]::UTF8)
    }
}

function remove-folder($path) {
    remove-item $path -Recurse -Force -ErrorAction SilentlyContinue | out-null
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