using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Windows.Input;

namespace PCL_CE.Neo.UI.Controls;

public sealed partial class MyButton : UserControl
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(MyButton),
        new PropertyMetadata(string.Empty, OnTextChanged));

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command),
        typeof(ICommand),
        typeof(MyButton),
        new PropertyMetadata(null));

    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
        nameof(CommandParameter),
        typeof(object),
        typeof(MyButton),
        new PropertyMetadata(null));

    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
        nameof(IsEnabled),
        typeof(bool),
        typeof(MyButton),
        new PropertyMetadata(true, OnIsEnabledChanged));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
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

    public new bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    public MyButton()
    {
        InitializeComponent();
        UpdateText();
        UpdateEnabledState();

        PanBack.PointerEntered += OnPointerEntered;
        PanBack.PointerExited += OnPointerExited;
        PanBack.PointerPressed += OnPointerPressed;
        PanBack.PointerReleased += OnPointerReleased;
        PanBack.Tapped += OnTapped;
        PanBack.KeyDown += OnKeyDown;
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MyButton button)
        {
            button.UpdateText();
        }
    }

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MyButton button)
        {
            button.UpdateEnabledState();
        }
    }

    private void UpdateText()
    {
        LabText.Text = Text;
    }

    private void UpdateEnabledState()
    {
        PanBack.Opacity = IsEnabled ? 1.0 : 0.5;
        PanBack.IsHitTestVisible = IsEnabled;
    }

    private void OnPointerEntered(object sender, PointerEventArgs e)
    {
        if (IsEnabled)
        {
            ScaleTransform.ScaleX = 1.02;
            ScaleTransform.ScaleY = 1.02;
            TextScaleTransform.ScaleX = 1.02;
            TextScaleTransform.ScaleY = 1.02;
        }
    }

    private void OnPointerExited(object sender, PointerEventArgs e)
    {
        ScaleTransform.ScaleX = 1.0;
        ScaleTransform.ScaleY = 1.0;
        TextScaleTransform.ScaleX = 1.0;
        TextScaleTransform.ScaleY = 1.0;
    }

    private void OnPointerPressed(object sender, PointerEventArgs e)
    {
        if (IsEnabled)
        {
            ScaleTransform.ScaleX = 0.98;
            ScaleTransform.ScaleY = 0.98;
            TextScaleTransform.ScaleX = 0.98;
            TextScaleTransform.ScaleY = 0.98;
        }
    }

    private void OnPointerReleased(object sender, PointerEventArgs e)
    {
        if (IsEnabled)
        {
            ScaleTransform.ScaleX = 1.0;
            ScaleTransform.ScaleY = 1.0;
            TextScaleTransform.ScaleX = 1.0;
            TextScaleTransform.ScaleY = 1.0;
        }
    }

    private void OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (IsEnabled && Command?.CanExecute(CommandParameter) == true)
        {
            Command.Execute(CommandParameter);
        }
    }

    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
        {
            if (IsEnabled && Command?.CanExecute(CommandParameter) == true)
            {
                Command.Execute(CommandParameter);
            }
        }
    }
}
