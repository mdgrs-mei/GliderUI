using namespace GliderUI
using namespace GliderUI.Avalonia
using namespace GliderUI.Avalonia.Controls

if (-not (Get-Module GliderUI)) {
    Import-Module GliderUI
}

$win = [Window]::new()
$win.Title = 'Cancel Long-Running EventCallback'
$win.Width = 420
$win.Height = 160

# Use a synchronized hashtable to store objects that are accessed from multiple runspaces.
$syncHash = [Hashtable]::Synchronized(@{})

$progressBar = [ProgressBar]::new()
$progressBar.Margin = 16
$progressBar.Value = 0
$syncHash.progressBar = $progressBar

$cancelButton = [Button]::new()
$cancelButton.HorizontalAlignment = 'Stretch'
$cancelButton.HorizontalContentAlignment = 'Center'
$cancelButton.Content = 'Cancel'
$cancelButton.Classes.Add('accent')
$cancelButton.IsEnabled = $false
$syncHash.isCancel = $false
$syncHash.cancelButton = $cancelButton

# This cancel button's callback runs in the main runspace as [GliderUI.EventCallbackRunspaceMode]::MainRunspaceAsyncUI is the default mode.
$cancelButton.AddClick({
        $syncHash.isCancel = $true
    })

$startButton = [Button]::new()
$startButton.HorizontalAlignment = 'Stretch'
$startButton.HorizontalContentAlignment = 'Center'
$startButton.Content = 'Start'
$startButton.Classes.Add('accent')

# Create a custom callback using the EventCallback class to control the runspace mode.
$longRunningCallback = [EventCallback]::new()

# RunspacePoolAsyncUI runs the callback on a background thread, allowing the cancel button's callback to run in parallel.
$longRunningCallback.RunspaceMode = 'RunspacePoolAsyncUI'

# Since the runspace pool callbacks can run in parallel, disable the button to avoid being clicked multiple times.
$longRunningCallback.DisabledControlsWhileProcessing = $startButton

# Pass objects via ArgumentList as runspace pool callbacks run in separate runspaces like ThreadJobs.
$longRunningCallback.ArgumentList = $syncHash
$longRunningCallback.ScriptBlock = {
    param ($syncHash)
    $syncHash.isCancel = $false
    $syncHash.cancelButton.IsEnabled = $true

    1..100 | ForEach-Object {
        if ($syncHash.isCancel) {
            $syncHash.progressBar.Value = 0
            return
        }

        # Properties of GliderUI objects are thread-safe and can be updated from any thread.
        $syncHash.progressBar.Value = $_

        # Do some long-running work.
        Start-Sleep -Milliseconds 20
    }

    $syncHash.cancelButton.IsEnabled = $false
}
$startButton.AddClick($longRunningCallback)

$panel = [StackPanel]::new()
$panel.Margin = 16
$panel.Spacing = 16
$panel.Orientation = 'Vertical'
$panel.Children.Add($progressBar)
$panel.Children.Add($startButton)
$panel.Children.Add($cancelButton)

$win.Content = $panel
$win.Show()
$win.WaitForClosed()
