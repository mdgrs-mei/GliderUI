<#
.SYNOPSIS

.DESCRIPTION

.INPUTS
None.

.OUTPUTS
None.

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
