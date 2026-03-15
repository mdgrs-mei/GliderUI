using namespace GliderUI.Avalonia.Controls

if (-not (Get-Module GliderUI)) {
    Import-Module GliderUI
}

$win = [Window]::new()
$win.Title = 'Hello from PowerShell!'
$win.Width = 400
$win.Height = 200

$button = [Button]::new()
$button.Content = 'Click Me'
$button.HorizontalAlignment = 'Center'
$button.VerticalAlignment = 'Center'
$button.AddClick({
        $button.Content = 'Clicked!'
    })

$win.Content = $button
# Show() shows the window but does not block the script.
$win.Show()
$win.WaitForClosed()
