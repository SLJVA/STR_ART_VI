using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using STR_ART_VI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace STR_ART_VI.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public MainWindowViewModel()
        {
            CloseWindowCommand = new RelayCommand(CloseWindow, CanCloseWindow);

        }
        public IRelayCommand CloseWindowCommand { get; }

        private void CloseWindow()
        {
            Application.Current.Shutdown();
        }
        private bool CanCloseWindow()
        {
         
            return true;
        }
    }
}
