using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace PCL_CE.Neo.UI.Controls;

public sealed partial class MyCheckBox : UserControl
{
    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
        nameof(IsChecked),
        typeof(bool),
        typeof(MyCheckBox),
        new PropertyMetadata(false, OnIsCheckedChanged));

    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label),
        typeof(string),
        typeof(MyCheckBox),
        new PropertyMetadata(string.Empty, OnLabelChanged));

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description),
        typeof(string),
        typeof(MyCheckBox),
        new PropertyMetadata(string.Empty));

    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public event EventHandler<bool>? CheckedChanged;

    public MyCheckBox()
    {
        InitializeComponent();
        UpdateLabel();
        UpdateCheckedState();
    }

    private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MyCheckBox checkBox)
        {
            checkBox.UpdateCheckedState();
            checkBox.CheckedChanged?.Invoke(checkBox, checkBox.IsChecked);
        }
    }

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MyCheckBox checkBox)
        {
            checkBox.UpdateLabel();
        }
    }

    private void UpdateLabel()
    {
        LabelText.Text = Label;
    }

    private void UpdateCheckedState()
    {
        if (IsChecked)
        {
            CheckIndicator.Visibility = Visibility.Visible;
            CheckIndicator.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 52, 152, 219));
            CheckIndicator.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 52, 152, 219));
        }
        else
        {
            CheckIndicator.Visibility = Visibility.Collapsed;
            CheckIndicator.Background = new SolidColorBrush(Colors.Transparent);
            CheckIndicator.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 189, 195, 199));
        }
    }

    private void OnTapped(object sender, TappedRoutedEventArgs e)
    {
        IsChecked = !IsChecked;
    }
}
