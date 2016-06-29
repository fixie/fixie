param([string]$target)

function main {
    step { Restore }
}

function Restore {
    exec { .\tools\NuGet.exe restore .\Fixie.sln -ConfigFile nuget.config -RequireConsent -o ".\src\packages" }
}

function exec($cmd) {
    $global:lastexitcode = 0
    & $cmd
    if ($lastexitcode -ne 0) {
        throw "Error executing command:$cmd"
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