#Requires -Modules RestartableSession

$root = Split-Path $PSScriptRoot -Parent
Enter-RSSession -OnStart {
    param ($root)
    $build = "$root/Build.ps1"

    $server = Get-Process -Name GliderUI.Server -ErrorAction SilentlyContinue
    if ($server) {
        Write-Host 'Waiting for server close...'
        $server.WaitForExit()
    }

    & $build Debug

    $isForegroundRunspace = $true
    $modulePath = "$root/module"
    Import-Module "$modulePath/GliderUI" -ArgumentList $isForegroundRunspace, $modulePath

    function Restart {
        Restart-RSSession
    }
    function Pester {
        & "$root/tests/RunPesterTests.ps1"
    }
} -OnStartArgumentList $root -ShowProcessId
