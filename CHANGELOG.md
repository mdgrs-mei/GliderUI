# Changelog

## [0.5.0] - 2026-11-23

### Added

- Added `Remove-GLIObject`
- Added win-arm64 platform

### Fixed

- Fixed an error when uninstalling servers that were installed with the AllUsers scope
- Fixed wrong namespaces of nested Enums

### Changed

- Updated StreamJsonRpc to 2.26.150 for a vulnerability fix
- Updated LiveCharts2 to 2.2.0
- Updated Avalonia to 12.1.6

## [0.4.1] - 2026-09-06

### Fixed

- Fixed the server not initialized issue when reloading the module after installation

## [0.4.0] - 2026-09-04

### Added

- Added `Install-GLIServer`
- Added linux-arm64 platform

### Deprecated

- `Enable-GLIExecution` and `Remove-GLINonTargetPlatform` will be removed in the next release
  - Use `Install-GLIServer` instead

## [0.3.0] - 2026-07-13

### Added

- Added [LiveCharts2](https://github.com/Live-Charts/LiveCharts2) APIs

## [0.2.0] - 2026-03-23

### Added

- Added `WebView` package
- Added `Remove-GLINonTargetPlatform`
- Added ShouldProcess support to `Enable-GLIExecution`

### Changed

- Updated Avalonia version to 12.0.1

## [0.1.0] - 2026-03-23

### Added

- Added `DataSource` and `ObservableCollection`
- Added `DataGrid`

## [0.0.2] - 2026-03-16

### Fixed

- Fixed an issue where the interval of server command polling was too short

## [0.0.1] - 2026-03-15

### Added

- Initial release
