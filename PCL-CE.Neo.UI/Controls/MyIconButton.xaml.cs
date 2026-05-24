using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Windows.Input;

namespace PCL_CE.Neo.UI.Controls;

public sealed partial class MyIconButton : UserControl
{
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

        IconButtonBorder.PointerEntered += OnPointerEntered;
        IconButtonBorder.PointerExited += OnPointerExited;
        IconButtonBorder.PointerPressed += OnPointerPressed;
        IconButtonBorder.PointerReleased += OnPointerReleased;
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
        if (d is MyIconButton button)
        {
            button.IconButtonBorder.ToolTip = button.Tooltip;
        }
    }

    private void UpdateIcon()
    {
        IconText.Text = Icon;
    }

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        IconButtonBorder.Background = new UI.Media.SolidColorBrush(UI.Colors.LightGray);
    }

    private void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        IconButtonBorder.Background = new UI.Media.SolidColorBrush(UI.Colors.Transparent);
    }

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        IconButtonBorder.Opacity = 0.7;
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        IconButtonBorder.Opacity = 1.0;
    }

    private void OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (Command?.CanExecute(CommandParameter) == true)
        {
            Command.Execute(CommandParameter);
        }
    }
}
