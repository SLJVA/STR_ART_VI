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

namespace STR_ART_VI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void AddUserControl(UserControl userControl)
        {
            MenuPanel.Children.Clear();
            MenuPanel.Children.Add(userControl);
        }

        private void Wbtn1_Click(object sender, RoutedEventArgs e)
        {
            Page1UserControl1 page = new Page1UserControl1();
            AddUserControl(page);
        }
        private void Wbtn2_Click(object sender, RoutedEventArgs e)
        {
            Page2UserControl1 page = new Page2UserControl1();
            AddUserControl(page);
        }

        private void Wbtn3_Click(object sender, RoutedEventArgs e)
        {
            Page3UserControl1 page = new Page3UserControl1();
            AddUserControl(page);
        }
    }
}
