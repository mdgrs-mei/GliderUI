
param(
    [bool]$IsMainRunspace = $true,
    [String]$ServerModuleRoot
)

$privateScripts = @(Get-ChildItem $PSScriptRoot/Private/*.ps1 -Exclude _*)
$publicScripts = @(Get-ChildItem $PSScriptRoot/Public/*.ps1)
foreach ($private:script in ($privateScripts + $publicScripts)) {
    . $script.FullName
}

if (WriteErrorIfRidNotSupported) {
    return
}

if ($IsMainRunspace) {
    $serverPath = GetServerExePath $ServerModuleRoot
    if ($null -eq $serverPath) {
        return
    }

    if (-not $IsWindows) {
        & test -x $serverPath
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "[$serverPath] does not have execute permission. Skip initializing the module."
            return
        }
    }
}

$coreNetVersion = 'net8.0'
$clientDll = "$PSScriptRoot/bin/$coreNetVersion/GliderUI.dll"
Import-Module $clientDll

$modulePath = $MyInvocation.MyCommand.Path
$useTimerEvent = $IsMainRunspace
[GliderUI.Engine]::Get().InitRunspace($serverPath, $host, $modulePath, $useTimerEvent)

$MyInvocation.MyCommand.ScriptBlock.Module.OnRemove = {
    [GliderUI.Engine]::Get().TermRunspace()
}
