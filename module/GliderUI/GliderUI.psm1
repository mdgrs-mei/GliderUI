
param(
    [bool]$IsForegroundRunspace = $true,
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

$clientDll = "$PSScriptRoot/bin/$($script:clientNetVersion)/GliderUI.dll"
Import-Module $clientDll

$script:isMainRunspace = $false
if ($IsForegroundRunspace) {
    $script:isMainRunspace = [GliderUI.Engine]::Get().AcquireMainRunspace()
}

$MyInvocation.MyCommand.ScriptBlock.Module.OnRemove = {
    [GliderUI.Engine]::Get().TermRunspace()
    if ($script:isMainRunspace) {
        [GliderUI.Engine]::Get().ReleaseMainRunspace()
    }
}

if ($script:isMainRunspace) {
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

$modulePath = $MyInvocation.MyCommand.Path
$useTimerEvent = $IsForegroundRunspace
if ($script:isMainRunspace) {
    [GliderUI.Engine]::Get().InitMainRunspace($serverPath, $host, $modulePath, $useTimerEvent)
} else {
    [GliderUI.Engine]::Get().InitSubRunspace($useTimerEvent)
}

