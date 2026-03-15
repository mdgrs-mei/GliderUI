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
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <Window.Resources>
        <SolidColorBrush x:Key="Warning">Red</SolidColorBrush>
    </Window.Resources>

    <Window.Styles>
        <Style Selector="Button">
            <Setter Property="HorizontalAlignment" Value="Center" />
        </Style>
        <Style Selector="Button.small">
            <Setter Property="FontSize" Value="12" />
        </Style>
        <Style Selector="Button.big">
            <Setter Property="FontSize" Value="24" />
        </Style>
        <Style Selector="Button:pointerover /template/ ContentPresenter#PART_ContentPresenter">
            <Style.Animations>
                <Animation IterationCount="Infinite" Duration="0:0:2">
                    <KeyFrame Cue="0%">
                        <Setter Property="Background" Value="Red" />
                    </KeyFrame>
                    <KeyFrame Cue="100%">
                        <Setter Property="Background" Value="Blue" />
                    </KeyFrame>
                </Animation>
            </Style.Animations>
        </Style>
    </Window.Styles>

    <StackPanel x:Name="button_panel" Margin="20" Spacing="8" VerticalAlignment="Center">
        <Button Content="Button Small" Classes="small" />
        <Button Content="Button Big" Classes="big" />
        <Button Content="Button Red" Foreground="{DynamicResource Warning}" />
    </StackPanel>
</Window>
'@

$win = [AvaloniaRuntimeXamlLoader]::Parse($xamlString, $null)
$win.Width = 800
$win.Height = 400

# Find a child control by name. Controls can be named with "x:Name" attribute.
$panel = $win.FindControl('button_panel')
foreach ($button in $panel.Children) {
    $button.AddClick({
            param ($argumentList, $senderButton)
            $senderButton.Content = 'Clicked!'
        })
}
$win.Show()
$win.WaitForClosed()
