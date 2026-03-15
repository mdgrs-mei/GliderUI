using namespace GliderUI
using namespace GliderUI.Avalonia
using namespace GliderUI.Avalonia.Controls

if (-not (Get-Module GliderUI)) {
    Import-Module GliderUI
}

$win = [Window]::new()
$win.ExtendClientAreaToDecorationsHint = $true
$win.WindowStartupLocation = 'CenterScreen'
$win.CanMinimize = $false
$win.CanMaximize = $false
$win.TopMost = $true

$win.Width = 420
$win.Height = 300

$icon = [TextBlock]::new()
$icon.Text = '💫'
$icon.FontSize = 32

$title = [TextBlock]::new()
$title.Text = 'Upgrade PowerShell'
$title.FontSize = 28

$titlePanel = [StackPanel]::new()
$titlePanel.Orientation = 'Horizontal'
$titlePanel.Spacing = 8
$titlePanel.Margin = [Thickness]::new(0, 32, 0, 24)
$titlePanel.Children.Add($icon)
$titlePanel.Children.Add($title)

$bodyText = [TextBlock]::new()
$bodyText.Text = 'powershell 7.5.4 - macOS Tahoe Version 26.3.1'
$bodyText.Margin = [Thickness]::new(0, 0, 0, 12)

$status = [TextBlock]::new()
$status.Text = 'Ready to upgrade'
$status.Margin = [Thickness]::new(0, 0, 0, 24)

$pb = [ProgressBar]::new()
$pb.Margin = [Thickness]::new(0, 0, 0, 24)

$pressToClose = $false
$button = [Button]::new()
$button.Content = 'Upgrade'
$button.HorizontalAlignment = 'Right'
$button.HorizontalContentAlignment = 'Center'
$button.Width = 120
$button.Classes.Add('accent')
$button.AddClick({
        if ($script:pressToClose) {
            $win.Close()
            return
        }
        $button.IsEnabled = $false
        $status.Text = 'Downloading...'
        1..20 | ForEach-Object {
            $pb.Value = $_
            Start-Sleep -Milliseconds 100
        }

        $status.Text = 'Installing...'
        21..40 | ForEach-Object {
            $pb.Value = $_
            Start-Sleep -Milliseconds 100
        }

        $status.Text = 'Error!'
        Start-Sleep -Milliseconds 2000

        $status.Text = 'Restoring...'
        $pb.IsIndeterminate = $true
        Start-Sleep -Milliseconds 2000

        $status.Text = 'Installing...'
        $pb.IsIndeterminate = $false
        41..100 | ForEach-Object {
            $pb.Value = $_
            Start-Sleep -Milliseconds 50
        }

        $status.Text = '🎉Done!'

        $button.Content = 'Close'
        $script:pressToClose = $true
        $button.IsEnabled = $true
    })

$panel = [StackPanel]::new()
$panel.Margin = 32

$panel.Children.Add($titlePanel)
$panel.Children.Add($bodyText)
$panel.Children.Add($status)
$panel.Children.Add($pb)
$panel.Children.Add($button)

$win.Content = $panel
$win.Show()
$win.WaitForClosed()
