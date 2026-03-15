using namespace GliderUI
using namespace GliderUI.Avalonia
using namespace GliderUI.Avalonia.Controls

if (-not (Get-Module GliderUI)) {
    Import-Module GliderUI
}

$win = [Window]::new()
$win.Title = 'Hello from PowerShell!'
$win.Width = 420
$win.Height = 420

$icon = [TextBlock]::new()
$icon.Text = '🌏'
$icon.VerticalAlignment = 'Center'
$icon.FontSize = 32

$title = [TextBlock]::new()
$title.Text = 'Sign In'
$title.VerticalAlignment = 'Center'
$title.FontSize = 24

$titlePanel = [StackPanel]::new()
$titlePanel.Orientation = 'Horizontal'
$titlePanel.Spacing = 8
$titlePanel.Margin = [Thickness]::new(0, 0, 0, 24)
$titlePanel.Children.Add($icon)
$titlePanel.Children.Add($title)

$nameHeader = [TextBlock]::new()
$nameHeader.Text = 'Account Name'
$nameHeader.Margin = [Thickness]::new(0, 0, 0, 4)

$name = [TextBox]::new()
$name.Watermark = 'Your account name'
$name.Margin = [Thickness]::new(0, 0, 0, 24)

$passwordHeader = [TextBlock]::new()
$passwordHeader.Text = 'Password'
$passwordHeader.Margin = [Thickness]::new(0, 0, 0, 4)

$password = [TextBox]::new()
$password.Watermark = 'Your fake password'
$password.PasswordChar = '*'

$forgotPassword = [HyperlinkButton]::new()
$forgotPassword.Content = 'Forgot your password?'
$forgotPassword.NavigateUri = 'https://github.com/'
$forgotPassword.Margin = [Thickness]::new(0, 0, 0, 24)
$forgotPassword.Padding = [Thickness]::new(0, 5, 0, 6)

$status = [TextBlock]::new()
$status.Text = ''
$status.VerticalAlignment = 'Center'

$button = [Button]::new()
$button.Content = 'Login'
$button.HorizontalAlignment = 'Right'
$button.HorizontalContentAlignment = 'Center'
$button.Width = 120
$button.Classes.Add('accent')
$buttonCallback = [EventCallback]@{
    ScriptBlock = {
        param ($argumentList, $s, $e)
        $status.Text = '{0} - Logging in...' -f $name.Text
        Start-Sleep -Milliseconds 3000
        $status.Text = 'Success!'
    }
    DisabledControlsWhileProcessing = @($button, $name, $password, $forgotPassword)
}
$button.AddClick($buttonCallback)

$buttonPanel = [StackPanel]::new()
$buttonPanel.Orientation = 'Horizontal'
$buttonPanel.Spacing = 16
$buttonPanel.HorizontalAlignment = 'Right'
$buttonPanel.Children.Add($status)
$buttonPanel.Children.Add($button)

$panel = [StackPanel]::new()
$panel.Margin = 32

$panel.Children.Add($titlePanel)
$panel.Children.Add($nameHeader)
$panel.Children.Add($name)
$panel.Children.Add($passwordHeader)
$panel.Children.Add($password)
$panel.Children.Add($forgotPassword)
$panel.Children.Add($buttonPanel)

$win.Content = $panel
$win.Show()
$win.WaitForClosed()
