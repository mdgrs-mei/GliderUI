using namespace GliderUI
using namespace GliderUI.Avalonia
using namespace GliderUI.Avalonia.Controls
using namespace GliderUI.Avalonia.Markup.Xaml

if (-not (Get-Module GliderUI)) {
    Import-Module GliderUI
}

$xamlString = @'
<Window
    xmlns="https://github.com/avaloniaui"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Title="DataGrid">
    <Grid RowDefinitions="*,Auto">
        <DataGrid x:Name="data_grid" Grid.Row="0"
            CanUserReorderColumns="True"
            CanUserResizeColumns="True"
            CanUserSortColumns="True">

            <DataGrid.Columns>
                <DataGridTextColumn Header="NPM(KB)" Binding="{Binding NPM, StringFormat={}{0:F2}}" CanUserSort="True" SortMemberPath="NPM" />
                <DataGridTextColumn Header="PM(MB)" Binding="{Binding PM, StringFormat={}{0:F2}}" CanUserSort="True" SortMemberPath="PM" />
                <DataGridTextColumn Header="WS(MB)" Binding="{Binding WS, StringFormat={}{0:F2}}" CanUserSort="True" SortMemberPath="WS" />
                <DataGridTextColumn Header="Id" Binding="{Binding Id}" CanUserSort="True" SortMemberPath="Id"/>
                <DataGridTextColumn Header="StartTime" Binding="{Binding StartTime}" CanUserSort="True" SortMemberPath="StartTime" />
                <DataGridTextColumn Header="ProcessName" Binding="{Binding ProcessName}" CanUserSort="True" SortMemberPath="ProcessName" />
            </DataGrid.Columns>
        </DataGrid>
        <Button x:Name="button" Grid.Row="1"
            Margin="4"
            HorizontalAlignment="Stretch"
            HorizontalContentAlignment="Center"
            Classes="accent"
            Content="Remove" />
    </Grid>
</Window>
'@

$win = [AvaloniaRuntimeXamlLoader]::Parse($xamlString, $null)
$win.Width = 1200
$win.Height = 800

$dataGrid = $win.FindControl('data_grid')
$button = $win.FindControl('button')

# DataGrid does not support column sorting for DataSource types by default.
# You need to add DataSourcePropertyComparer for the sorting to work.
foreach ($col in $dataGrid.Columns) {
    $sortMemberPath = $col.SortMemberPath
    if ($sortMemberPath) {
        $col.CustomSortComparer = [DataSourcePropertyComparer]::new($sortMemberPath)
    }
}

# DataSource is the only type that supports data binding.
# Use ObservableCollection to reflect the item removal or addition to the UI (Two-way binding).
$sources = [GliderUI.System.Collections.ObjectModel.ObservableCollection[DataSource]]::new()
Get-Process | ForEach-Object {
    # DataSource supports dynamic property generation similar to PSCustomObject.
    $data = [DataSource]@{
        NPM = ($_.NPM / 1KB)
        PM = ($_.PM / 1MB)
        WS = ($_.WS / 1MB)
        Id = $_.Id
        SI = $_.SessionId.ToString()
        ProcessName = $_.ProcessName
        StartTime = if ($_.StartTime) { [GliderUI.System.DateTime]::new($_.StartTime.Ticks) } else { $null }
    }
    $sources.Add($data)
}
$dataGrid.ItemsSource = $sources

$removeCallback = [EventCallback]@{
    # Disable double click to avoid confusion.
    DisabledControlsWhileProcessing = $button

    ScriptBlock = {
        $removedItems = [System.Collections.Generic.List[DataSource]]::new()
        foreach ($item in $dataGrid.SelectedItems) {
            $removedItems.Add($item)
        }

        foreach ($item in $removedItems) {
            $sources.Remove($item) | Out-Null
        }
    }
}
$button.AddClick($removeCallback)

$win.Show()
$win.WaitForClosed()
