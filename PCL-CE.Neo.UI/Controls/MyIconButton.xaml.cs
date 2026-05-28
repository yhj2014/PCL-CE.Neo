using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System.Windows.Input;

namespace PCL_CE.Neo.UI.Controls;

public sealed partial class MyIconButton : UserControl
{
    public event RoutedEventHandler? Click;

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(string),
        typeof(MyIconButton),
        new PropertyMetadata(string.Empty, OnIconChanged));

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command),
        typeof(ICommand),
        typeof(MyIconButton),
        new PropertyMetadata(null));

    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
        nameof(CommandParameter),
        typeof(object),
        typeof(MyIconButton),
        new PropertyMetadata(null));

    public static readonly DependencyProperty TooltipProperty = DependencyProperty.Register(
        nameof(Tooltip),
        typeof(string),
        typeof(MyIconButton),
        new PropertyMetadata(string.Empty, OnTooltipChanged));

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public string Tooltip
    {
        get => (string)GetValue(TooltipProperty);
        set => SetValue(TooltipProperty, value);
    }

    public MyIconButton()
    {
        InitializeComponent();
        UpdateIcon();
        IconButtonBorder.Tapped += OnTapped;
    }

    private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MyIconButton button)
        {
            button.UpdateIcon();
        }
    }

    private static void OnTooltipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // 暂时不实现 Tooltip
    }

    private void UpdateIcon()
    {
        IconText.Text = Icon;
    }

    private void OnTapped(object sender, TappedRoutedEventArgs e)
    {
        Click?.Invoke(this, new RoutedEventArgs());
        if (Command?.CanExecute(CommandParameter) == true)
        {
            Command.Execute(CommandParameter);
        }
    }
}
