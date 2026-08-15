
param(
    [bool]$IsMainRunspace = $true,
    [String]$ServerModuleRoot
)

$coreNetVersion = 'net8.0'
$script:serverNetVersion = 'net9.0'
$script:supportedServerRids = @(
    'win-x64'
    'osx-arm64'
    'linux-x64'
    'linux-arm64'
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

if ($IsMainRunspace) {
    $serverModulePath = "GliderUI.Server.$serverRid"
    if ($null -ne $ServerModuleRoot) {
        $serverModulePath = "$ServerModuleRoot/$serverModulePath"
    }
    $serverModule = Get-Module $serverModulePath -ListAvailable
    if ($null -eq $serverModule) {
        Write-Error "Server [$serverModulePath] not installed."
        return
    }

    $script:serverPath = "$(Split-Path $serverModule.Path -Parent)/bin/$serverNetVersion/GliderUI.Server$serverExtension"
}

$publicScripts = @(Get-ChildItem $PSScriptRoot/Public/*.ps1)
foreach ($private:script in $publicScripts) {
    . $script.FullName
}

if ($IsMainRunspace) {
    if (-not $IsWindows) {
        & test -x $serverPath
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "[$serverPath] does not have execute permission. Skip initializing the module."
            return
        }
    }
}

$clientDll = "$PSScriptRoot/bin/$coreNetVersion/GliderUI.dll"
Import-Module $clientDll

$modulePath = $MyInvocation.MyCommand.Path
$useTimerEvent = $IsMainRunspace
[GliderUI.Engine]::Get().InitRunspace($serverPath, $host, $modulePath, $useTimerEvent)

$MyInvocation.MyCommand.ScriptBlock.Module.OnRemove = {
    [GliderUI.Engine]::Get().TermRunspace()
}
