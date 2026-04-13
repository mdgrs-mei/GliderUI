using namespace GliderUI
using namespace GliderUI.Avalonia
using namespace GliderUI.Avalonia.Media
using namespace GliderUI.Avalonia.Controls

if (-not (Get-Module GliderUI)) {
    Import-Module GliderUI
}

$defaultAddress = 'https://learn.microsoft.com/en-us/'

$win = [Window]::new()
$win.Title = 'WebView'

$backIconGeometry = 'M12.7347,4.20949 C13.0332,3.92233 13.508,3.93153 13.7952,4.23005 C14.0823,4.52857 14.0731,5.00335 13.7746,5.29051 L5.50039,13.25 L24.2532,13.25 C24.6674,13.25 25.0032,13.5858 25.0032,13.9999982 C25.0032,14.4142 24.6674,14.75 24.2532,14.75 L5.50137,14.75 L13.7746,22.7085 C14.0731,22.9957 14.0823,23.4705 13.7952,23.769 C13.508,24.0675 13.0332,24.0767 12.7347,23.7896 L3.30673,14.7202 C2.89776,14.3268 2.89776,13.6723 3.30673,13.2788 L12.7347,4.20949 Z'
$backIcon = [PathIcon]::new()
$backIcon.Data = [Geometry]::Parse($backIconGeometry)
$backButton = [Button]::new()
$backButton.Content = $backIcon
$backButton.IsEnabled = $false
[Grid]::SetRow($backButton, 0)
[Grid]::SetColumn($backButton, 0)

$backButton.AddClick({
        $webView.GoBack()
    })

$addressBar = [TextBox]::new()
$addressBar.Text = $defaultAddress
[Grid]::SetRow($addressBar, 0)
[Grid]::SetColumn($addressBar, 1)

$goButton = [Button]::new()
$goButton.Content = 'Go'
$goButton.Classes.Add('accent')
[Grid]::SetRow($goButton, 0)
[Grid]::SetColumn($goButton, 2)

$goButton.AddClick({
        $webView.Source = $addressBar.Text
    })

$webView = [NativeWebView]::new()
$webView.Source = $defaultAddress
[Grid]::SetRow($webView, 1)
[Grid]::SetColumnSpan($webView, 3)

$webView.AddNavigationStarted({
        param ($argumentList, $s, $navigationStartingEventArgs)
        $addressBar.Text = $navigationStartingEventArgs.Request
    })

$webView.AddNavigationCompleted({
        $backButton.IsEnabled = $webView.CanGoBack
    })

$row0 = [RowDefinition]::new()
$row0.Height = [GridLength]::Auto
$row1 = [RowDefinition]::new()
$row1.Height = [GridLength]::new(1, 'Star')

$col0 = [ColumnDefinition]::new()
$col0.Width = [GridLength]::Auto
$col1 = [ColumnDefinition]::new()
$col1.Width = [GridLength]::new(1, 'Star')
$col2 = [ColumnDefinition]::new()
$col2.Width = [GridLength]::Auto

$grid = [Grid]::new()
$grid.Margin = 4
$grid.RowSpacing = 4
$grid.ColumnSpacing = 4
$grid.RowDefinitions.Add($row0)
$grid.RowDefinitions.Add($row1)
$grid.ColumnDefinitions.Add($col0)
$grid.ColumnDefinitions.Add($col1)
$grid.ColumnDefinitions.Add($col2)
$grid.Children.Add($backButton)
$grid.Children.Add($addressBar)
$grid.Children.Add($goButton)
$grid.Children.Add($webView)

$win.Content = $grid
$win.Show()
$win.WaitForClosed()
