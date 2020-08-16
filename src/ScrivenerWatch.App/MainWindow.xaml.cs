using ScrivenerWatch.App.Model;
using System.Windows;

namespace ScrivenerWatch.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            this.DataContext = new ApplicationModel();
        }
    }
}
