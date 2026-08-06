using CadWithAi.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CadWithAi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel = new MainViewModel();
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this._viewModel;
        }

        private void HelixViewport3D_Loaded(object sender, RoutedEventArgs e)
        {
            this._viewModel.SetHelixViewport3D(sender as HelixToolkit.Wpf.HelixViewport3D);
        }
    }
}