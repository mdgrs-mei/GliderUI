param (
    [String]$NuGetApiKey
)

$privateScripts = @(Get-ChildItem $PSScriptRoot/../module/GliderUI/Private/*.ps1 -Exclude _*)
foreach ($private:script in $privateScripts) {
    . $script.FullName
}

Write-Host 'Publishing GliderUI...'
Publish-Module -Path './module/GliderUI' -NuGetApiKey $NuGetApiKey

foreach ($rid in $script:supportedServerRids) {
    Write-Host "Publishing server $rid..."
    Publish-Module -Path "./module/GliderUI.Server.$rid" -NuGetApiKey $NuGetApiKey
}
