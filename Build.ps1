param (
    [ValidateSet('Debug', 'Release')]
    [String]$Configuration = 'Debug',

    [Switch]$BuildAllRuntimes,

    [Switch]$ExportApi
)

$originalProgressPreference = $ProgressPreference
$ProgressPreference = 'SilentlyContinue'

$coreNetVersion = 'net8.0'
$serverNetVersion = 'net9.0'
$serverRids = @(
    'win-x64'
    'osx-arm64'
    'linux-x64'
)
if ($IsWindows) {
    $defaultServerRid = 'win-x64'
    $executableExtension = '.exe'
} else {
    $defaultServerRid = [System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier
    $executableExtension = ''
}

$copyExtensions = @('.dll', '.pdb')
$src = "$PSScriptRoot/src"
$apiSrc = "$src/GliderUI.ApiExporter"
$coreSrc = "$src/GliderUI"
$depSrc = "$src/GliderUI.Common"
$serverSrc = "$src/GliderUI.Server"

$apiPublish = [System.IO.Path]::GetFullPath("$apiSrc/bin/$Configuration/$serverNetVersion/$defaultServerRid/publish/")
$corePublish = [System.IO.Path]::GetFullPath("$coreSrc/bin/$Configuration/$coreNetVersion/publish/")
$depPublish = [System.IO.Path]::GetFullPath("$depSrc/bin/$Configuration/$coreNetVersion/publish/")

$apiXml = "$apiSrc/Api.xml"
$apiExporter = "$apiPublish/GliderUI.ApiExporter$executableExtension"

$outDir = "$PSScriptRoot/module/GliderUI/bin/$coreNetVersion"
$outDeps = "$outDir/Dependencies"
$outServer = "$PSScriptRoot/module/GliderUI/bin/$serverNetVersion"

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

Remove-Item -Path $outDir -Recurse -ErrorAction Ignore
Remove-Item -Path $outServer -Recurse -ErrorAction Ignore

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

Push-Location $coreSrc
dotnet publish -c $Configuration -o $corePublish
Pop-Location

# Filter deps files.
Get-ChildItem -Path $depPublish -Recurse -File | Where-Object {
    $_.Extension -notin $copyExtensions
} | Remove-Item -Force

$deps = [System.Collections.Generic.List[string]]::new()
Get-ChildItem -Path $depPublish -Recurse -File | ForEach-Object {
    $deps.Add($_.FullName.Replace($depPublish, ''))
}

# Filter core dlls.
Get-ChildItem -Path $corePublish -Recurse -File | Where-Object {
    $path = $_.FullName.Replace($corePublish, '')
    ($_.Extension -notin $copyExtensions) -or ($deps.Contains($path))
} | Remove-Item -Force

# Remove empty folders of core dlls.
Get-ChildItem -Path $corePublish -Recurse -Directory | Where-Object {
    -not (Get-ChildItem -Path $_.FullName -Recurse -File)
} | Remove-Item -Force

# Output.
CopyFolderItems -FolderPath $corePublish -Destination $outDir
CopyFolderItems -FolderPath $depPublish -Destination $outDeps


# Build servers.
function BuildServer($Rid) {
    $publishFolder = [System.IO.Path]::GetFullPath("$serverSrc/bin/$Configuration/$serverNetVersion/$Rid/publish/")
    $outServerRuntime = "$outServer/$Rid"

    Push-Location $serverSrc
    dotnet publish -c $Configuration -o $publishFolder -r $Rid
    Pop-Location

    # Output.
    CopyFolderItems -FolderPath $publishFolder -Destination $outServerRuntime
}

if ($BuildAllRuntimes) {
    foreach ($rid in $serverRids) {
        BuildServer $rid
    }
} else {
    BuildServer $defaultServerRid
}
$ProgressPreference = $originalProgressPreference
