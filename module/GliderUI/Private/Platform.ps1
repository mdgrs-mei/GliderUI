$script:serverNetVersion = 'net9.0'
$script:supportedServerRids = @(
    'win-x64'
    'osx-arm64'
    'linux-x64'
    'linux-arm64'
)

function GetExecutableExtension() {
    if ($IsWindows) {
        '.exe'
    } else {
        ''
    }
}

function GetRid() {
    if ($IsWindows) {
        'win-x64'
    } else {
        [System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier
    }
}

function GetServerModuleName() {
    $rid = GetRid
    "GliderUI.Server.$rid"
}

function GetServerExePath($ModuleRoot) {
    $serverModulePath = GetServerModuleName
    $serverExtension = GetExecutableExtension

    if ($null -ne $ModuleRoot) {
        $serverModulePath = "$ModuleRoot/$serverModulePath"
    }
    $serverModule = Get-Module $serverModulePath -ListAvailable
    if ($null -eq $serverModule) {
        Write-Error "Server [$serverModulePath] not installed."
        return
    }

    "$(Split-Path $serverModule.Path -Parent)/bin/$serverNetVersion/GliderUI.Server$serverExtension"
}

function WriteErrorIfRidNotSupported() {
    $rid = GetRid
    if ($supportedServerRids -contains $rid) {
        $false
    } else {
        Write-Error "Server runtime id [$rid] is not supported. Supported runtime ids are [$supportedServerRids]."
        $true
    }
}
