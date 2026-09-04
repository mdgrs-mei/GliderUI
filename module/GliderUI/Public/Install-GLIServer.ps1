<#
.SYNOPSIS
Installs the GliderUI server module and optionally removes older versions.

.DESCRIPTION
Installs the GliderUI server module for the current module version and optionally uninstalls older installed versions.
On non-Windows platforms, the function also ensures the installed server executable has execute permission set.

.PARAMETER Prerelease
Passed to the Prerelease parameter of Install-PSResource.

.PARAMETER Repository
Passed to the Repository parameter of Install-PSResource.

.PARAMETER Scope
Passed to the Scope parameter of Install-PSResource.

.PARAMETER PassThru
Returns the installed module object from Install-PSResource.

.PARAMETER TrustRepository
Passed to the TrustRepository parameter of Install-PSResource.

.PARAMETER UninstallOldVersions
Removes older installed versions of the GliderUI server module after a successful install.

.INPUTS
None.

.OUTPUTS
Returns the installed module object when -PassThru is specified.

.EXAMPLE
Install-GLIServer

.EXAMPLE
Install-GLIServer -Scope CurrentUser -PassThru -UninstallOldVersions
#>
function Install-GLIServer {
    [CmdletBinding(SupportsShouldProcess)]
    param (
        [Parameter(ValueFromPipelineByPropertyName = $true)]
        [Switch]$Prerelease,

        [Parameter(ValueFromPipelineByPropertyName = $true)]
        [String[]]$Repository,

        [Parameter(ValueFromPipelineByPropertyName = $true)]
        [ValidateSet('CurrentUser', 'AllUsers')]
        [String]$Scope,

        [Parameter(ValueFromPipelineByPropertyName = $true)]
        [Switch]$PassThru,

        [Parameter(ValueFromPipelineByPropertyName = $true)]
        [Switch]$TrustRepository,

        [Parameter(ValueFromPipelineByPropertyName = $true)]
        [Switch]$UninstallOldVersions
    )

    $moduleName = GetServerModuleName
    $version = GetGliderUIVersion

    $installArguments = @{
        Name = $moduleName
        Version = $version
        PassThru = $true
        Prerelease = $PSBoundParameters.ContainsKey('Prerelease')
        TrustRepository = $PSBoundParameters.ContainsKey('TrustRepository')
    }

    if ($PSBoundParameters.ContainsKey('Repository')) {
        $installArguments.Add('Repository', $Repository)
    }
    if ($PSBoundParameters.ContainsKey('Scope')) {
        $installArguments.Add('Scope', $Scope)
    }

    if ($PSCmdlet.ShouldProcess($moduleName, "Install-PSResource for $version")) {
        $installedModule = Install-PSResource @installArguments
        if ($null -eq $installedModule) {
            return
        }

        if ($PassThru) {
            $installedModule
        }
    }

    if ($UninstallOldVersions) {
        $oldServerModules = Get-Module $moduleName -ListAvailable | Where-Object {
            $_.Version -ne $version
        }
        if ($oldServerModules -and $PSCmdlet.ShouldProcess($moduleName, "Uninstall-PSResource for $($oldServerModules.Version -join ',')")) {
            $oldServerModules | ForEach-Object {
                Uninstall-PSResource -Name $_.Name -Version $_.Version
            }
        }
    }

    if ($IsWindows) {
        return
    }

    $serverExePath = GetServerExePath
    & test -x $serverExePath
    if ($LASTEXITCODE -eq 0) {
        # Already has permission.
        Write-Host "[$serverExePath] already has permission."
        return
    }

    if ($PSCmdlet.ShouldProcess($serverExePath, "chmod '+x'")) {
        Write-Host "Adding execute permission to [$serverExePath]."
        & chmod '+x' $serverExePath
    }
}
