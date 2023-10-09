using System.Windows;
using System.Windows.Controls;
using STR_ART_VI.ViewModel;

namespace STR_ART_VI.View
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var viewModel = new MainWindowViewModel();
            this.DataContext = viewModel;
        }
    }
}
