using namespace GliderUI
using namespace GliderUI.Avalonia
using namespace GliderUI.Avalonia.Media
using namespace GliderUI.Avalonia.Controls

if (-not (Get-Module GliderUI)) {
    Import-Module GliderUI
}

$win = [Window]::new()
$win.Title = 'Remove Object'
$win.Width = 400
$win.Height = 220

$listBox = [ListBox]::new()
$listBox.Margin = 16

$isClosed = $false
$win.Content = $listBox
$win.AddClosed({
        $script:isClosed = $true
    })
$win.Show()

$addCount = 0
while (-not $isClosed) {
    $items = $listBox.Items
    if ($items.Count -gt 3) {
        $item = $items[0]
        $items.Remove($item) | Out-Null

        # Due to the RPC architecture, objects are not freed automatically.
        # This is usually not a problem, but if you repeatedly create and destroy objects, you should remove them explicitly to prevent memory leaks.
        Remove-GLIObject $item
    }

    $text = [TextBlock]::new()
    $text.Text = "Item $addCount"
    $items.Add($text) | Out-Null
    $addCount++

    Start-Sleep -Milliseconds 200
}

# Explicitly frees all GliderUI objects.
# This is not necessary if the process terminates here, but useful to free up resources if your script continues running in the background.
Remove-GLIObject -All
