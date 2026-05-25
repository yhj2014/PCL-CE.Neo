using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace PCL_CE.Neo.UI.Controls;

public sealed partial class MyTextBox : UserControl
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(MyTextBox),
        new PropertyMetadata(string.Empty, OnTextChanged));

    public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
        nameof(Placeholder),
        typeof(string),
        typeof(MyTextBox),
        new PropertyMetadata(string.Empty, OnPlaceholderChanged));

    public static readonly DependencyProperty IsPasswordProperty = DependencyProperty.Register(
        nameof(IsPassword),
        typeof(bool),
        typeof(MyTextBox),
        new PropertyMetadata(false));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public bool IsPassword
    {
        get => (bool)GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
    }

    private bool _isFocused = false;

    public MyTextBox()
    {
        InitializeComponent();
        UpdatePlaceholder();
        UpdateText();
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MyTextBox textBox)
        {
            textBox.UpdateText();
        }
    }

    private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MyTextBox textBox)
        {
            textBox.UpdatePlaceholder();
        }
    }

    private void UpdateText()
    {
        if (InputTextBox.Text != Text)
        {
            InputTextBox.Text = Text;
        }
        UpdatePlaceholderVisibility();
    }

    private void UpdatePlaceholder()
    {
        PlaceholderText.Text = Placeholder;
        UpdatePlaceholderVisibility();
    }

    private void UpdatePlaceholderVisibility()
    {
        var showPlaceholder = string.IsNullOrEmpty(InputTextBox.Text) && !_isFocused;
        PlaceholderText.Visibility = showPlaceholder ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        Text = InputTextBox.Text;
        UpdatePlaceholderVisibility();
    }

    private void OnGotFocus(object sender, RoutedEventArgs e)
    {
        _isFocused = true;
        InputBorder.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 52, 152, 219));
        UpdatePlaceholderVisibility();
    }

    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
        _isFocused = false;
        InputBorder.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 189, 195, 199));
        UpdatePlaceholderVisibility();
    }
}
