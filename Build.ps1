param (
    [ValidateSet('Debug', 'Release')]
    [String]$Configuration = 'Debug',

    [Switch]$BuildAllRuntimes,

    [Switch]$ExportApi
)

$originalProgressPreference = $ProgressPreference
$ProgressPreference = 'SilentlyContinue'

$privateScripts = @(Get-ChildItem $PSScriptRoot/module/GliderUI/Private/*.ps1 -Exclude _*)
foreach ($private:script in $privateScripts) {
    . $script.FullName
}

if (WriteErrorIfRidNotSupported) {
    return
}

$defaultServerRid = GetRid
$executableExtension = GetExecutableExtension

$copyExtensions = @('.dll', '.pdb')
$src = "$PSScriptRoot/src"
$apiSrc = "$src/GliderUI.ApiExporter"
$clientSrc = "$src/GliderUI"
$depSrc = "$src/RpcUIShell.Core"
$serverSrc = "$src/GliderUI.Server"

$apiPublish = [System.IO.Path]::GetFullPath("$apiSrc/bin/$Configuration/$($script:serverNetVersion)/$defaultServerRid/publish/")
$clientPublish = [System.IO.Path]::GetFullPath("$clientSrc/bin/$Configuration/$($script:clientNetVersion)/publish/")
$depPublish = [System.IO.Path]::GetFullPath("$depSrc/bin/$Configuration/$($script:clientNetVersion)/publish/")

$apiXml = "$apiSrc/Api.xml"
$apiExporter = "$apiPublish/GliderUI.ApiExporter$executableExtension"

$moduleDir = "$PSScriptRoot/module"
$clientOut = "$moduleDir/GliderUI/bin/$($script:clientNetVersion)"
$depOut = "$clientOut/Dependencies"

function CopyFolderItems($FolderPath, $Destination) {
    if (Test-Path $Destination) {
        Copy-Item -Path "$FolderPath/*" -Destination $Destination -Recurse
    } else {
        Copy-Item -Path $FolderPath -Destination $Destination -Recurse
    }
}

Push-Location $src
$dotnetExeVersion = dotnet --version
Write-Host "dotnet.exe version: $dotnetExeVersion"
Pop-Location

Remove-Item -Path $clientOut -Recurse -ErrorAction Ignore

if ($ExportApi) {
    Push-Location $apiSrc
    dotnet publish -c $Configuration -o $apiPublish
    Pop-Location

    Remove-Item -Path $apiXml -ErrorAction Ignore
    Start-Process -FilePath $apiExporter -ArgumentList $apiXml -Wait
}

Push-Location $depSrc
dotnet publish -c $Configuration -o $depPublish
Pop-Location

Push-Location $clientSrc
dotnet publish -c $Configuration -o $clientPublish
Pop-Location

# Filter deps files.
Get-ChildItem -Path $depPublish -Recurse -File | Where-Object {
    $_.Extension -notin $copyExtensions
} | Remove-Item -Force

$deps = [System.Collections.Generic.List[string]]::new()
Get-ChildItem -Path $depPublish -Recurse -File | ForEach-Object {
    $deps.Add($_.FullName.Replace($depPublish, ''))
}

# Filter client dlls.
Get-ChildItem -Path $clientPublish -Recurse -File | Where-Object {
    $path = $_.FullName.Replace($clientPublish, '')
    ($_.Extension -notin $copyExtensions) -or ($deps.Contains($path))
} | Remove-Item -Force

# Remove empty folders of client dlls.
Get-ChildItem -Path $clientPublish -Recurse -Directory | Where-Object {
    -not (Get-ChildItem -Path $_.FullName -Recurse -File)
} | Remove-Item -Force

# Output.
CopyFolderItems -FolderPath $clientPublish -Destination $clientOut
CopyFolderItems -FolderPath $depPublish -Destination $depOut

# Build servers.
function BuildServer($Rid) {
    $serverPublish = [System.IO.Path]::GetFullPath("$serverSrc/bin/$Configuration/$($script:serverNetVersion)/$Rid/publish/")
    $serverOut = "$moduleDir/GliderUI.Server.$Rid/bin/$($script:serverNetVersion)"
    Remove-Item -Path $serverOut -Recurse -ErrorAction Ignore

    Push-Location $serverSrc
    dotnet publish -c $Configuration -o $serverPublish -r $Rid
    Pop-Location

    # Output.
    CopyFolderItems -FolderPath $serverPublish -Destination $serverOut
}

if ($BuildAllRuntimes) {
    foreach ($rid in $script:supportedServerRids) {
        BuildServer $rid
    }
} else {
    BuildServer $defaultServerRid
}
$ProgressPreference = $originalProgressPreference
