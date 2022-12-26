namespace ThreadsWpf;

/// <summary>
/// Interaction logic for OtherWindow.xaml
/// </summary>
public partial class OtherWindow : Window
{
    public static readonly DependencyProperty CounterProperty = DependencyProperty.Register(nameof(Counter), typeof(int), typeof(OtherWindow));
    public int Counter { get => (int)GetValue(CounterProperty); set => SetValue(CounterProperty, value); }

    public OtherWindow(int initCounter)
    {
        Counter = initCounter;
        InitializeComponent();
    }

    public void ButtonClick(object sender, RoutedEventArgs e) => ++Counter;
}
