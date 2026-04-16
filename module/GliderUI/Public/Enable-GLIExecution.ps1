<#
.SYNOPSIS
Adds execute permission to the server executable on a Unix based OS.

.DESCRIPTION
This function adds execute permission to the server executable on a Unix based OS.
On Linux and macOS, run this function once after installation using the same user account that installed the module.

.INPUTS
None.

.OUTPUTS
None.

#>
function Enable-GLIExecution {
    [CmdletBinding(SupportsShouldProcess)]
    param ()

    if ($IsWindows) {
        Write-Host 'No need to add execute permission on Windows.'
        return
    }

    & test -x $script:serverPath
    if ($LASTEXITCODE -eq 0) {
        # Already has permission.
        Write-Host "[$script:serverPath] already has permission."
        return
    }

    if ($PSCmdlet.ShouldProcess($script:serverPath, "chmod '+x'")) {
        Write-Host "Adding execute permission to [$script:serverPath]."
        & chmod '+x' $script:serverPath
    }
}
