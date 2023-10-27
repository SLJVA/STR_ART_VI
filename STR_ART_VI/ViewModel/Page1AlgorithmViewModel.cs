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
    public partial class Page1AlgorithmViewModel : ObservableObject
    {
        public Page1AlgorithmViewModel()
        {
            LoadImageCommand = new RelayCommand(LoadImage, CanLoadImage);
            SaveImageCommand = new RelayCommand(SaveImage, CanSaveImage);
            GenerateWhiteImageCommand = new RelayCommand(GenWhiteImage, CanGenWhiteImage);
            GenerateStringArtCommand = new RelayCommand(GenStrArt, CanGenStrArt);
        }



        public IRelayCommand LoadImageCommand { get; }
        public IRelayCommand SaveImageCommand { get; }
        public IRelayCommand GenerateWhiteImageCommand { get; }
        public IRelayCommand GenerateStringArtCommand { get; }

        [ObservableProperty]
        private string _newImageSize = string.Empty;

        [ObservableProperty]
        private string _redPixelCount = string.Empty;

        [ObservableProperty]
        private ImageSource? _processedImage;

        [ObservableProperty]
        public string _imagePath;

        [ObservableProperty]
        private ImageSource? _whiteGen;

        partial void OnNewImageSizeChanged(string value)
        {
            GenerateStringArtCommand.NotifyCanExecuteChanged();
        }

        partial void OnRedPixelCountChanged(string value)
        {
            LoadImageCommand.NotifyCanExecuteChanged();
        }

        //to sie wywoluje gdy wlasciwosc ProccessedImage zmienia wartosc, sprawdza to czy komenda sie moze wykonac, metoda => (CanSaveImage)
        partial void OnProcessedImageChanged(ImageSource? value)
        {
            SaveImageCommand.NotifyCanExecuteChanged();
        }

        //to sie wywoluje gdy wlasciwosc ImagePath zmienia wartosc, sprawdza to czy komenda sie moze wykonac, metoda => (CanSaveImage)
        partial void OnImagePathChanged(string value)
        {
            SaveImageCommand.NotifyCanExecuteChanged();
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

        private void SaveImage()
        {
            //sprawdzam warunek jeszcze raz, zeby nie bylo warningow
            if (string.IsNullOrEmpty(ImagePath) || ProcessedImage is null)
            {
                return;
            }

            //Zapisanie obrazu do pliku
            SystemUtilities.SaveImage(ImagePath, (BitmapSource)ProcessedImage);
        }

        //logika sprawdzania, czy mozna zapisac zdjecie
        private bool CanSaveImage()
        {
            // jezeli sciezka nie istnieje lub zdjecie nie istnieje, to przycisk zapisu bedzie nieaktywny
            if (string.IsNullOrEmpty(ImagePath) || ProcessedImage is null)
            {
                return false;
            }

            return true;
        }

        private void GenWhiteImage()
        {
            WriteableBitmap img = ImageUtilities.GenerateWhiteImage(600, 600);
            
            ImageUtilities.SaveImageToFile(img);
        }

        private bool CanGenWhiteImage()
        {
            return true;
        
        }

        private void GenStrArt()
        {
            //Wczytanie ścieżki pliku z obrazem
            ImagePath = FileDialogUtilities.OpenImageFile();

            if (ImagePath is null)
            {
                return;
            }

            //Stworzenie bitmapy na podstawie wczytanej ścieżki pliku
            var bitmap = ImageUtilities.CreateBitmap(ImagePath);
            

            bitmap = ImageUtilities.CropImageToSquare(bitmap);

            int? newImageSize = SystemUtilities.ParseStringToInt(NewImageSize);

            if (newImageSize is null)
            {
                return;
            }

            bitmap = ImageUtilities.ResizeImage(bitmap, newImageSize.Value, newImageSize.Value);


            WhiteGen = bitmap;

            WriteableBitmap writeableBitmap = new WriteableBitmap(bitmap);
            ImageUtilities.SaveImageToFile(writeableBitmap);

        }

        private bool CanGenStrArt()
        {
            return true;

        }




    }

}
