# Please visit https://livecharts.dev/ to learn how to use LiveCharts2 library.
# The sample code here is also based on the above website.

using namespace GliderUI
using namespace GliderUI.Avalonia
using namespace GliderUI.Avalonia.Controls
using namespace GliderUI.Avalonia.Markup.Xaml
using namespace GliderUI.LiveChartsCore.SkiaSharpView
using namespace GliderUI.LiveChartsCore.SkiaSharpView.Avalonia

if (-not (Get-Module GliderUI)) {
    Import-Module GliderUI
}

function Main() {
    $win = [Window]::new()
    $win.Title = 'LiveCharts2'
    $win.Width = 1400
    $win.Height = 800

    $basicChart = CreateBasicChart
    $xamlGradientChart = CreateXamlGradientChart
    $pieChart = CreateBasicPieChart
    $scatterChart = CreateXamlScatterChart

    [Grid]::SetRow($basicChart, 0)
    [Grid]::SetColumn($basicChart, 0)
    [Grid]::SetRow($xamlGradientChart, 0)
    [Grid]::SetColumn($xamlGradientChart, 1)
    [Grid]::SetRow($pieChart, 1)
    [Grid]::SetColumn($pieChart, 0)
    [Grid]::SetRow($scatterChart, 1)
    [Grid]::SetColumn($scatterChart, 1)

    $row0 = [RowDefinition]::new()
    $row0.Height = [GridLength]::new(1, 'Star')
    $row1 = [RowDefinition]::new()
    $row1.Height = [GridLength]::new(1, 'Star')

    $col0 = [ColumnDefinition]::new()
    $col0.Width = [GridLength]::new(1, 'Star')
    $col1 = [ColumnDefinition]::new()
    $col1.Width = [GridLength]::new(1, 'Star')

    $grid = [Grid]::new()
    $grid.Margin = 4
    $grid.RowSpacing = 4
    $grid.ColumnSpacing = 4
    $grid.RowDefinitions.Add($row0)
    $grid.RowDefinitions.Add($row1)
    $grid.ColumnDefinitions.Add($col0)
    $grid.ColumnDefinitions.Add($col1)

    $grid.Children.Add($basicChart)
    $grid.Children.Add($xamlGradientChart)
    $grid.Children.Add($pieChart)
    $grid.Children.Add($scatterChart)

    $win.Content = $grid
    $win.Show()
    $win.WaitForClosed()
}

# Use LiveCharts2's DataPointerDown event to handle single click.
# You have direct access to chart points through the EventArgs which represent the clicked series and data.
$clickCallback = {
    param ($argumentList, $s, $chartPoints)

    'Click' | Write-Host -ForegroundColor Green
    foreach ($chartPoint in $chartPoints) {
        "$($chartPoint.Context.Series.Name): $($chartPoint.Context.DataSource | Out-String)" | Write-Host -NoNewline
    }
}

# Use Avalonia's PointerPressed event to detect double click.
# You have to manually calculate the chart points from the clicked position.
$doubleClickCallback = {
    param ($argumentList, $s, $pointerPressedEventArgs)
    if ($pointerPressedEventArgs.ClickCount -ne 2) {
        return
    }

    $chart = $s
    $position = $pointerPressedEventArgs.GetPosition($chart)
    $point = [GliderUI.LiveChartsCore.Drawing.LvcPoint]::new($position.X, $position.Y)
    $chartPoints = $chart.GetPointsAt($point, 'Automatic', 'PointerDownEvent')

    'DoubleClick' | Write-Host -ForegroundColor Red
    foreach ($chartPoint in $chartPoints) {
        "$($chartPoint.Context.Series.Name): $($chartPoint.Context.DataSource | Out-String)" | Write-Host -NoNewline
    }
}

function CreateBasicChart() {
    $chart = [CartesianChart]::new()
    $chart.LegendPosition = 'Right'

    $s1 = [GliderUI.LiveChartsCore.SkiaSharpView.LineSeries[double]]::new()
    $s1.Name = 'Mary'
    $s1.Values = [GliderUI.System.Collections.ObjectModel.ObservableCollection[double]]::new()
    $s1.Values.Add(5)
    $s1.Values.Add(10)
    $s1.Values.Add(8)
    $s1.Values.Add(4)

    $s2 = [GliderUI.LiveChartsCore.SkiaSharpView.ColumnSeries[double]]::new()
    $s2.Name = 'Ana'
    $s2.Values = [GliderUI.System.Collections.ObjectModel.ObservableCollection[double]]::new()
    $s2.Values.Add(4)
    $s2.Values.Add(7)
    $s2.Values.Add(3)
    $s2.Values.Add(8)

    $chart.Series.Add($s1)
    $chart.Series.Add($s2)

    $chart.AddDataPointerDown($script:clickCallback)
    $chart.AddPointerPressed($script:doubleClickCallback)
    $chart
}

function CreateBasicPieChart {
    $chart = [PieChart]::new()
    $chart.LegendPosition = 'Right'

    $data = @{
        'Mary' = 10
        'John' = 20
        'Alice' = 30
        'Bob' = 40
        'Charlie' = 50
    }

    $data.Keys | ForEach-Object {
        # In PieChart, a Series represents one area.
        $series = [PieSeries[int]]::new()
        $series.Values = [GliderUI.System.Collections.Generic.List[int]]::new()
        $series.Values.Add($data[$_])
        $series.Name = $_
        $series.ShowDataLabels = $true
        $chart.Series.Add($series)
    }

    $chart.AddDataPointerDown($script:clickCallback)
    $chart.AddPointerPressed($script:doubleClickCallback)
    $chart
}

# On the LiveCharts2 website, examples often use XAML so this AvaloniaRuntimeXamlLoader approach should be easier.
function CreateXamlGradientChart {
    $xamlString = @'
<UserControl
    xmlns="https://github.com/avaloniaui"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:lvc="clr-namespace:LiveChartsCore.SkiaSharpView.Avalonia;assembly=LiveChartsCore.SkiaSharpView.Avalonia">

    <lvc:CartesianChart
        LegendPosition="Right">
        <lvc:CartesianChart.Series>
            <lvc:XamlColumnSeries
                SeriesName="John"
                Values="{Binding Values1}"
                Fill="{lvc:LinearGradientPaint
                    Colors='#FF8C94, #DCEDC2',
                    StartPoint='0.5, 0',
                    EndPoint='0.5, 1'}"/>
            <lvc:XamlLineSeries
                SeriesName="Charles"
                Values="{Binding Values2}"
                GeometrySize="22"
                Fill="{x:Null}"
                Stroke="{lvc:LinearGradientPaint
                    Colors='#2D4059, #FFD360',
                    StrokeWidth=10}"
                GeometryStroke="{lvc:LinearGradientPaint
                    Colors='#2D4059, #FFD360',
                    StrokeWidth=10}"/>
        </lvc:CartesianChart.Series>
    </lvc:CartesianChart>

</UserControl>
'@

    $userControl = [AvaloniaRuntimeXamlLoader]::Parse($xamlString, $null)

    $data = [DataSource]::new()
    $data.Values1 = [GliderUI.System.Collections.ObjectModel.ObservableCollection[int]]::new()
    $data.Values2 = [GliderUI.System.Collections.ObjectModel.ObservableCollection[int]]::new()

    $data.Values1.Add(3)
    $data.Values1.Add(7)
    $data.Values1.Add(2)
    $data.Values1.Add(9)
    $data.Values1.Add(4)

    $data.Values2.Add(4)
    $data.Values2.Add(2)
    $data.Values2.Add(8)
    $data.Values2.Add(5)
    $data.Values2.Add(3)

    $userControl.DataContext = $data

    $chart = $userControl.Content
    $chart.AddDataPointerDown($script:clickCallback)
    $chart.AddPointerPressed($script:doubleClickCallback)

    $userControl
}

function CreateXamlScatterChart {
    $xamlString = @'
<UserControl
    xmlns="https://github.com/avaloniaui"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:lvc="clr-namespace:LiveChartsCore.SkiaSharpView.Avalonia;assembly=LiveChartsCore.SkiaSharpView.Avalonia">

    <lvc:CartesianChart
        LegendPosition="Right">
        <lvc:CartesianChart.Series>

            <lvc:XamlScatterSeries
                SeriesName="January"
                Values="{Binding Values1}"
                GeometrySize="100"
                MinGeometrySize="5"/>

            <!--
                The StackGroup property defines the series that
                share the weight scale.
            -->

            <lvc:XamlScatterSeries
                SeriesName="February"
                Values="{Binding Values2}"
                GeometrySize="100"
                MinGeometrySize="5"
                StackGroup="1"/>

            <lvc:XamlScatterSeries
                SeriesName="March"
                Values="{Binding Values3}"
                GeometrySize="100"
                MinGeometrySize="5"
                StackGroup="1"/>

        </lvc:CartesianChart.Series>
    </lvc:CartesianChart>

</UserControl>
'@

    $userControl = [AvaloniaRuntimeXamlLoader]::Parse($xamlString, $null)

    $data = [DataSource]::new()
    $data.Values1 = [GliderUI.System.Collections.ObjectModel.ObservableCollection[GliderUI.LiveChartsCore.Defaults.WeightedPoint]]::new()
    $data.Values2 = [GliderUI.System.Collections.ObjectModel.ObservableCollection[GliderUI.LiveChartsCore.Defaults.WeightedPoint]]::new()
    $data.Values3 = [GliderUI.System.Collections.ObjectModel.ObservableCollection[GliderUI.LiveChartsCore.Defaults.WeightedPoint]]::new()

    $data.Values1.Add([GliderUI.LiveChartsCore.Defaults.WeightedPoint]::new(1.5, 4, 5))
    $data.Values1.Add([GliderUI.LiveChartsCore.Defaults.WeightedPoint]::new(2, 2.5, 4))
    $data.Values1.Add([GliderUI.LiveChartsCore.Defaults.WeightedPoint]::new(3, 3, 1))
    $data.Values1.Add([GliderUI.LiveChartsCore.Defaults.WeightedPoint]::new(2.5, 5, 2))

    $data.Values2.Add([GliderUI.LiveChartsCore.Defaults.WeightedPoint]::new(4, 4, 6))
    $data.Values2.Add([GliderUI.LiveChartsCore.Defaults.WeightedPoint]::new(2.7, 3.1, 10))
    $data.Values2.Add([GliderUI.LiveChartsCore.Defaults.WeightedPoint]::new(5.1, 2.2, 4))

    $data.Values3.Add([GliderUI.LiveChartsCore.Defaults.WeightedPoint]::new(3.5, 3.5, 2))
    $data.Values3.Add([GliderUI.LiveChartsCore.Defaults.WeightedPoint]::new(4, 6, 3))
    $data.Values3.Add([GliderUI.LiveChartsCore.Defaults.WeightedPoint]::new(5.5, 3.2, 9))
    $data.Values3.Add([GliderUI.LiveChartsCore.Defaults.WeightedPoint]::new(1.0, 2, 5))
    $data.Values3.Add([GliderUI.LiveChartsCore.Defaults.WeightedPoint]::new(2.8, 4.3, 4))

    $userControl.DataContext = $data

    $chart = $userControl.Content
    $chart.AddDataPointerDown($script:clickCallback)
    $chart.AddPointerPressed($script:doubleClickCallback)

    $userControl
}

Main
