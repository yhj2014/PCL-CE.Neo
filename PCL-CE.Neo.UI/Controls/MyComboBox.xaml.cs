using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System.Collections.ObjectModel;

namespace PCL_CE.Neo.UI.Controls;

public sealed partial class MyComboBox : UserControl
{
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(ObservableCollection<string>),
        typeof(MyComboBox),
        new PropertyMetadata(null));

    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
        nameof(SelectedIndex),
        typeof(int),
        typeof(MyComboBox),
        new PropertyMetadata(-1, OnSelectedIndexChanged));

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem),
        typeof(string),
        typeof(MyComboBox),
        new PropertyMetadata(string.Empty));

    public ObservableCollection<string>? ItemsSource
    {
        get => (ObservableCollection<string>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public string SelectedItem
    {
        get => (string)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public event EventHandler<int>? SelectionChanged;

    private Popup? _popup;
    private ListBox? _listBox;

    public MyComboBox()
    {
        InitializeComponent();
        UpdateSelectedText();

        ComboBoxBorder.Tapped += OnComboBoxTapped;
    }

    private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MyComboBox comboBox)
        {
            comboBox.UpdateSelectedText();
            comboBox.SelectionChanged?.Invoke(comboBox, comboBox.SelectedIndex);
        }
    }

    private void UpdateSelectedText()
    {
        if (SelectedIndex >= 0 && ItemsSource != null && SelectedIndex < ItemsSource.Count)
        {
            SelectedItem = ItemsSource[SelectedIndex];
            SelectedText.Text = SelectedItem;
            SelectedText.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 44, 62, 80));
        }
        else
        {
            SelectedItem = string.Empty;
            SelectedText.Text = "请选择...";
            SelectedText.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 127, 140, 141));
        }
    }

    private void OnComboBoxTapped(object sender, TappedRoutedEventArgs e)
    {
        ShowPopup();
    }

    private void ShowPopup()
    {
        _popup = new Popup
        {
            IsLightDismissEnabled = true,
            Placement = Microsoft.UI.Xaml.Controls.Primitives.PlacementMode.Bottom
        };

        _listBox = new ListBox
        {
            ItemsSource = ItemsSource,
            SelectedIndex = SelectedIndex,
            Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)),
            BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 189, 195, 199)),
            BorderThickness = new Thickness(1)
        };

        _listBox.SelectionChanged += OnListBoxSelectionChanged;

        var container = new Border
        {
            Child = _listBox,
            MinWidth = ActualWidth
        };

        _popup.Child = container;
        _popup.SetValue(Canvas.LeftProperty, (double)ComboBoxBorder.GetValue(Canvas.LeftProperty));
        _popup.SetValue(Canvas.TopProperty, (double)ComboBoxBorder.GetValue(Canvas.TopProperty) + ActualHeight);
        _popup.IsOpen = true;
    }

    private void OnListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_listBox != null)
        {
            SelectedIndex = _listBox.SelectedIndex;
        }
        if (_popup != null)
        {
            _popup.IsOpen = false;
        }
    }
}
