<#
.SYNOPSIS
Removes all server files that are not used on the current platform.

.DESCRIPTION
The module includes server files for all supported platforms, totaling hundreds of megabytes.
This function removes the files that are not needed on the current platform to save disk space.

.INPUTS
None.

.OUTPUTS
None.

#>
function Remove-GLINonTargetPlatform {
    [CmdletBinding(SupportsShouldProcess)]
    param ()

    $nonTargetRids = $script:supportedServerRids -ne $script:serverRid

    foreach ($rid in $nonTargetRids) {
        $serverFolder = "$PSScriptRoot/../bin/$script:serverNetVersion/$rid"
        if ($PSCmdlet.ShouldProcess($serverFolder, 'Remove-Item')) {
            Write-Host "Removing [$serverFolder]."
            if (Test-Path $serverFolder) {
                Remove-Item -Path $serverFolder -Recurse -Force
            }
        }
    }
}
