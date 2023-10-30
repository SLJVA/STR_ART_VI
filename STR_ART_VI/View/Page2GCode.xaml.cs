using System.Windows.Controls;
using STR_ART_VI.ViewModel;

namespace STR_ART_VI.View
{
    /// <summary>
    /// Interaction logic for Page1UserControl1.xaml
    /// </summary>
    public partial class Page2GCode : UserControl
    {

        public Page2GCode()
        {
            InitializeComponent();
            var viewModel = new Page2GCodeViewModel();
            DataContext = viewModel;
        }

    }
}
