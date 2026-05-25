using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PCL_CE.Neo.UI.Controls;

public sealed partial class Card : UserControl
{
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header),
        typeof(string),
        typeof(Card),
        new PropertyMetadata(string.Empty));

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public Card()
    {
        InitializeComponent();
    }
}
