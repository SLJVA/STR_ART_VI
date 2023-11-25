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

    public partial class NailsQuadGeneratorWindow : UserControl
    {
        public NailsQuadGeneratorWindow()
        {
            InitializeComponent();
            var viewModel = new NailQuadGenViewModel();
            DataContext = viewModel;
        }
    }
}
