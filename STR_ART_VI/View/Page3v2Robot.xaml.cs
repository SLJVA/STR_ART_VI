using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using STR_ART_VI.ViewModel;

namespace STR_ART_VI.View
{
    /// <summary>
    /// Interaction logic for Page3v2Robot.xaml
    /// </summary>
    public partial class Page3v2Robot : UserControl
    {
        public Page3v2Robot()
        {
            InitializeComponent();
            var viewModel = new Page3v2RobotViewModel();
            DataContext = viewModel;
        }
    }
}
