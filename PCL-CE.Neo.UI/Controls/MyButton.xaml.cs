using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Windows.Input;

namespace PCL_CE.Neo.UI.Controls;

public sealed partial class MyButton : UserControl
{
    public event RoutedEventHandler? Click;

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
        get => base.IsEnabled;
        set => base.IsEnabled = value;
    }

    public MyButton()
    {
        InitializeComponent();
        UpdateText();

        PanBack.Tapped += OnTapped;
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MyButton button)
        {
            button.UpdateText();
        }
    }

    private void UpdateText()
    {
        LabText.Text = Text;
    }

    private void OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (IsEnabled)
        {
            Click?.Invoke(this, new RoutedEventArgs());
            if (Command?.CanExecute(CommandParameter) == true)
            {
                Command.Execute(CommandParameter);
            }
        }
    }
}
