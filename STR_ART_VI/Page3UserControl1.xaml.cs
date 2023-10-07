using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

namespace STR_ART_VI
{

    public partial class Page3UserControl1 : UserControl
    {
        
        public Page3UserControl1()
        {
            InitializeComponent();
            var viewModel = new Page3_ViewModel();
            this.DataContext = viewModel;
        }

    }
}
