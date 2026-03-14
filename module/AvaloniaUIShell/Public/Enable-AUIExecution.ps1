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
    [CmdletBinding()]
    param ()

    if ($IsWindows) {
        return
    }

    $script:serverPath
    & test -x $script:serverPath
    if ($LASTEXITCODE -eq 0) {
        # Already has permission.
        return
    }

    & chmod '+x' $script:serverPath
}
