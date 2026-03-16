using namespace GliderUI
using namespace GliderUI.Avalonia
using namespace GliderUI.Avalonia.Controls
using namespace GliderUI.Avalonia.Controls.Primitives

if (-not (Get-Module GliderUI)) {
    Import-Module GliderUI
}

$win = [Window]::new()
$win.Title = 'Basic Input'
$win.Width = 420
$win.Height = 400

$toggleSwitch = [ToggleSwitch]::new()
$toggleSwitch.AddIsCheckedChanged({
        Write-Host "Toggled [$($toggleSwitch.IsChecked)]"
    })

$toggleButton = [ToggleButton]::new()
$toggleButton.Content = 'ToggleButton'
$toggleButton.AddIsCheckedChanged({
        Write-Host "Toggled [$($toggleButton.IsChecked)]"
    })

$checkBox = [CheckBox]::new()
$checkBox.Content = 'CheckBox'
$checkBox.AddIsCheckedChanged({
        Write-Host "Checked [$($checkBox.IsChecked)]"
    })

$comboBox = [ComboBox]::new()
$comboBox.PlaceholderText = 'Placeholder'
$comboBox.Items.Add('Apple') | Out-Null
$comboBox.Items.Add('Banana') | Out-Null
$comboBox.Items.Add('Orange') | Out-Null
$comboBox.AddSelectionChanged({
        Write-Host "Selection changed to [$($comboBox.SelectedItem)]"
    })


$radioButtonOnChecked = {
    param ($argumentList, $senderButton)
    Write-Host "Selection changed to [$($senderButton.Content)]"
}
$radioButton1 = [RadioButton]::new()
$radioButton2 = [RadioButton]::new()
$radioButton3 = [RadioButton]::new()
$radioButton1.IsChecked = $true
$radioButton1.GroupName = 'Group'
$radioButton1.Content = 'Option1'
$radioButton1.AddChecked($radioButtonOnChecked)
$radioButton2.GroupName = 'Group'
$radioButton2.Content = 'Option2'
$radioButton2.AddChecked($radioButtonOnChecked)
$radioButton3.GroupName = 'Group'
$radioButton3.Content = 'Option3'
$radioButton3.AddChecked($radioButtonOnChecked)

$leftPanel = [StackPanel]::new()
$leftPanel.Spacing = 16
$leftPanel.Children.Add($toggleButton)
$leftPanel.Children.Add($toggleSwitch)
$leftPanel.Children.Add($checkBox)
$leftPanel.Children.Add($comboBox)
$leftPanel.Children.Add($radioButton1)
$leftPanel.Children.Add($radioButton2)
$leftPanel.Children.Add($radioButton3)

$horizontalSlider = [Slider]::new()
$horizontalSlider.Width = 200
$horizontalSlider.AddValueChanged({
        Write-Host "Slider value changed to [$($horizontalSlider.Value)]"
    })

$verticalSlider = [Slider]::new()
$verticalSlider.Orientation = 'Vertical'
$verticalSlider.Height = 200
$verticalSlider.TickPlacement = 'Outside'
$verticalSlider.TickFrequency = 20
$verticalSlider.AddValueChanged({
        Write-Host "Slider value changed to [$($verticalSlider.Value)]"
    })

$numericUpDown = [NumericUpDown]::new()
$numericUpDown.Value = 0
$numericUpDown.AddValueChanged({
        Write-Host "NumericUpDown value changed to [$($numericUpDown.Value)]"
    })

$rightPanel = [StackPanel]::new()
$rightPanel.Spacing = 16
$rightPanel.Children.Add($horizontalSlider)
$rightPanel.Children.Add($verticalSlider)
$rightPanel.Children.Add($numericUpDown)

$rootPanel = [StackPanel]::new()
$rootPanel.Orientation = 'Horizontal'
$rootPanel.Margin = 32
$rootPanel.Spacing = 32
$rootPanel.Children.Add($leftPanel)
$rootPanel.Children.Add($rightPanel)

$win.Content = $rootPanel
$win.Show()
$win.WaitForClosed()
