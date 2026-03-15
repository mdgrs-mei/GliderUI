
param(
    [bool]$UseTimerEvent = $true
)

$coreNetVersion = 'net8.0'
$serverNetVersion = 'net9.0'
$supportedServerRids = @(
    'win-x64'
    'osx-arm64'
    'linux-x64'
)

if ($IsWindows) {
    $serverRid = 'win-x64'
    $serverExtension = '.exe'
} else {
    $serverRid = [System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier
    $serverExtension = ''
}

if ($supportedServerRids -notcontains $serverRid) {
    Write-Error "Server runtime id [$serverRid] is not supported. Supported runtime ids are [$supportedServerRids]."
    return
}

$coreDll = "$PSScriptRoot/bin/$coreNetVersion/GliderUI.dll"
$script:serverPath = "$PSScriptRoot/bin/$serverNetVersion/$serverRid/GliderUI.Server$serverExtension"

$publicScripts = @(Get-ChildItem $PSScriptRoot/Public/*.ps1)
foreach ($private:script in $publicScripts) {
    . $script.FullName
}

if (-not $IsWindows) {
    & test -x $serverPath
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "[$serverPath] does not have execute permission. Skip initializing the module."
        return
    }
}

Import-Module $coreDll

$modulePath = $MyInvocation.MyCommand.Path
[GliderUI.Engine]::Get().InitRunspace($serverPath, $host, $modulePath, $UseTimerEvent)

$MyInvocation.MyCommand.ScriptBlock.Module.OnRemove = {
    [GliderUI.Engine]::Get().TermRunspace()
}
