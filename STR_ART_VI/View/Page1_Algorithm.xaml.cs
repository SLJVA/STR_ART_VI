using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using STR_ART_VI.ViewModel;

namespace STR_ART_VI.View
{

    public partial class Page1_Algorithm : UserControl
    {
        
        public Page1_Algorithm()
        {
            InitializeComponent();
            var viewModel = new Page1_Algorithm_ViewModel();
            this.DataContext = viewModel;
        }

    }
}
