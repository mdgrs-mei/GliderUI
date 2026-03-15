using namespace GliderUI
using namespace GliderUI.Avalonia
using namespace GliderUI.Avalonia.Controls
using namespace GliderUI.Avalonia.Platform.Storage

if (-not (Get-Module GliderUI)) {
    Import-Module GliderUI
}

$win = [Window]::new()
$win.Title = 'Storage Provider'
$win.Width = 600
$win.Height = 240

# OpenFilePicker
$textBox0 = [TextBox]::new()
$textBox0.IsReadonly = $true
$button0 = [Button]::new()
$button0.Content = 'Pick Single File'
$button0.AddClick({
        param ($TextBox, $s, $e)

        $options = [FilePickerOpenOptions]::new()
        $options.Title = 'Pick Single File'

        $files = $win.StorageProvider.OpenFilePickerAsync($options).WaitForCompleted()
        $TextBox.Text = $files.Path.AbsolutePath
    }, $textBox0)
[Grid]::SetRow($textBox0, 0)
[Grid]::SetColumn($textBox0, 0)
[Grid]::SetRow($button0, 0)
[Grid]::SetColumn($button0, 1)

$textBox1 = [TextBox]::new()
$textBox1.IsReadonly = $true
$button1 = [Button]::new()
$button1.Content = 'Pick Multiple Files'
$button1.AddClick({
        param ($TextBox, $s, $e)

        $options = [FilePickerOpenOptions]::new()
        $options.Title = 'Pick Multiple Files'
        $options.AllowMultiple = $true

        $files = $win.StorageProvider.OpenFilePickerAsync($options).WaitForCompleted()
        $TextBox.Text = $files.Path.AbsolutePath -join ';'
    }, $textBox1)
[Grid]::SetRow($textBox1, 1)
[Grid]::SetColumn($textBox1, 0)
[Grid]::SetRow($button1, 1)
[Grid]::SetColumn($button1, 1)

# SaveFilePickerAsync
$textBox2 = [TextBox]::new()
$textBox2.IsReadonly = $true
$button2 = [Button]::new()
$button2.Content = 'Save File'
$button2.AddClick({
        param ($TextBox, $s, $e)

        $options = [FilePickerSaveOptions]::new()
        $options.Title = 'Save File'
        $options.SuggestedFileName = 'DefaultFileName'
        $options.DefaultExtension = '.ps1'

        $file = $win.StorageProvider.SaveFilePickerAsync($options).WaitForCompleted()
        $TextBox.Text = $file.Path.AbsolutePath
    }, $textBox2)
[Grid]::SetRow($textBox2, 2)
[Grid]::SetColumn($textBox2, 0)
[Grid]::SetRow($button2, 2)
[Grid]::SetColumn($button2, 1)

# FolderPicker
$textBox3 = [TextBox]::new()
$textBox3.IsReadonly = $true
$button3 = [Button]::new()
$button3.Content = 'Pick Folder'
$button3.AddClick({
        param ($TextBox, $s, $e)

        $options = [FolderPickerOpenOptions]::new()
        $options.Title = 'Pick Folder'

        $folder = $win.StorageProvider.OpenFolderPickerAsync($options).WaitForCompleted()
        $TextBox.Text = $folder.Path.AbsolutePath
    }, $textBox3)
[Grid]::SetRow($textBox3, 3)
[Grid]::SetColumn($textBox3, 0)
[Grid]::SetRow($button3, 3)
[Grid]::SetColumn($button3, 1)

$row0 = [RowDefinition]::new()
$row0.Height = [GridLength]::Auto
$row1 = [RowDefinition]::new()
$row1.Height = [GridLength]::Auto
$row2 = [RowDefinition]::new()
$row2.Height = [GridLength]::Auto
$row3 = [RowDefinition]::new()
$row3.Height = [GridLength]::Auto

$col0 = [ColumnDefinition]::new()
$col0.Width = [GridLength]::new(1, 'Star')
$col1 = [ColumnDefinition]::new()
$col1.Width = [GridLength]::Auto

$grid = [Grid]::new()
$grid.Margin = 16
$grid.RowSpacing = 16
$grid.ColumnSpacing = 4
$grid.RowDefinitions.Add($row0)
$grid.RowDefinitions.Add($row1)
$grid.RowDefinitions.Add($row2)
$grid.RowDefinitions.Add($row3)
$grid.ColumnDefinitions.Add($col0)
$grid.ColumnDefinitions.Add($col1)

$grid.Children.Add($textBox0)
$grid.Children.Add($button0)
$grid.Children.Add($textBox1)
$grid.Children.Add($button1)
$grid.Children.Add($textBox2)
$grid.Children.Add($button2)
$grid.Children.Add($textBox3)
$grid.Children.Add($button3)

$win.Content = $grid
$win.Show()
$win.WaitForClosed()
