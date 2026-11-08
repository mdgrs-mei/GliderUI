<#
.SYNOPSIS
Releases a GliderUI object or all GliderUI objects.

.DESCRIPTION
Due to the RPC architecture, objects that are managed by GliderUI module are not freed automatically even if they are no longer used.
This function explicitly releases those GliderUI objects. Use this when repeatedly creating and removing objects to prevent memory leaks.

.PARAMETER InputObject
The GliderUI object to release. This parameter accepts objects from the pipeline.

.PARAMETER All
Releases all GliderUI objects.

.INPUTS
GliderUI.IGliderUIObject

.OUTPUTS
None.

.EXAMPLE
$text = [TextBlock]::new()
Remove-GLIObject $text

.EXAMPLE
$text1 = [TextBlock]::new()
$text2 = [TextBlock]::new()
$text1, $text2 | Remove-GLIObject

.EXAMPLE
Remove-GLIObject -All
#>
function Remove-GLIObject {
    [CmdletBinding(DefaultParameterSetName = 'Object')]
    param (
        [Parameter(ParameterSetName = 'Object', Mandatory = $true, Position = 0, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)]
        [GliderUI.IGliderUIObject]$InputObject,

        [Parameter(ParameterSetName = 'All', Mandatory = $true, ValueFromPipelineByPropertyName = $true)]
        [Switch]$All
    )

    process {
        if ($All) {
            [GliderUI.Engine]::Get().RemoveAllGliderUIObjects()
        } else {
            [GliderUI.Engine]::Get().RemoveGliderUIObject($InputObject)
        }
    }
}
