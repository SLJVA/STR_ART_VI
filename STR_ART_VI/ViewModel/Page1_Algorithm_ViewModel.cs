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
    public partial class Page1_Algorithm_ViewModel : ObservableObject
    {
        public Page1_Algorithm_ViewModel()
        {
            LoadImageCommand = new RelayCommand(LoadImage, CanLoadImage);
        }



        public IRelayCommand LoadImageCommand { get; }

        [ObservableProperty]
        private string _redPixelCount = string.Empty;

        [ObservableProperty]
        private ImageSource? _processedImage;

        [ObservableProperty]
        public string _imagePath;


        partial void OnRedPixelCountChanged(string value)
        {
            LoadImageCommand.NotifyCanExecuteChanged();
        }


        private void LoadImage()
        {
            ImagePath = FileDialogUtilities.OpenImageFile();

            if (ImagePath is null)
            {
                return;
            }

            var bitmap = ImageUtilities.CreateBitmap(ImagePath);

            //Powiększenie obrazu do rozmiaru 3000x3000
            var resizedImage = ImageUtilities.ResizeImage(bitmap, 3000, 3000);

            // Wykrycie krawędzi (konturów)
            var edgeImage = ImageUtilities.DetectEdges(resizedImage);

            //Wywołanie metody ApplyRedPixels dla przetworzonego obrazu
            int? redPixelCount = SystemUtilities.ParseStringToInt(RedPixelCount);

            if (redPixelCount is null)
            {
                return;
            }

            BitmapSource? processedImage = null;
            try
            {
                processedImage = ImageUtilities.ApplyRedPixels(edgeImage, redPixelCount.Value, ImagePath);
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Red pixel count exceeds the number of white pixels. Adjusted to the maximum possible value.");
            }

            if (processedImage is null)
            {
                return;
            }

            //Wyświetlenie przetworzonego obrazu
            ProcessedImage = processedImage;

        }
        private bool CanLoadImage()
        {
            if (SystemUtilities.ParseStringToInt(RedPixelCount) is null)
            {
                return false;
            }

            return true;
        }
    }

}
