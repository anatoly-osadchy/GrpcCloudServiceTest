using System.Collections.ObjectModel;
using System.Windows;

namespace Grpc.Client
{
    /// <summary>
    /// Interaction logic for WaitWindow.xaml
    /// </summary>
    public partial class WaitWindow : Window
    {
        public static readonly DependencyProperty LogCountProperty = DependencyProperty.Register(nameof(LogCount), typeof(int), typeof(WaitWindow), new PropertyMetadata(default(int)));

        public WaitWindow()
        {
            InitializeComponent();
        }

        public ObservableCollection<string> Logs { get; } = new();

        public int LogCount
        {
            get => (int)GetValue(LogCountProperty);
            set => SetValue(LogCountProperty, value);
        }
    }
}
