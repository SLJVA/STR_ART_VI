using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STR_ART_VI.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace STR_ART_VI.ViewModel
{
    public partial class NailQuadGenViewModel : ObservableObject 
    {
        public NailQuadGenViewModel()
        {
            LoadFilePaths();
            _selectedImageFile = _imageFiles.FirstOrDefault();
            GenBlankNailCircleImageCommand = new RelayCommand(GenBlankNailCircle);
        }

        public IRelayCommand GenBlankNailCircleImageCommand { get; }

        [ObservableProperty]
        private ImageSource? _generatedBitmapImage;

        // Utwórz listę plików w folderze ImageCollection
        [ObservableProperty]
        private ObservableCollection<string> _imageFiles = new();

        [ObservableProperty]
        private string? _selectedImageFile;

        [RelayCommand]
        private void AddElement()
        {
            //ImageFiles.Add("DAMIAN");
            LoadFilePaths();
        }

        [ObservableProperty]
        private string? _firstPointXText;
        [ObservableProperty]
        private string? _firstPointYText;

        public int FirstPointX;
        public int FirstPointY;

        [ObservableProperty]
        private string? _secondPointXText;
        [ObservableProperty]
        private string? _secondPointYText;

        public int SecondPointX;
        public int SecondPointY;

        [RelayCommand]
        private void SetFirstCorrner()
        {
            FirstPointX = int.Parse(FirstPointXText);
            FirstPointY = int.Parse(FirstPointYText);

        }
        [RelayCommand]
        private void SetSecondCorrner()
        {
            SecondPointX = int.Parse(SecondPointXText);
            SecondPointY = int.Parse(SecondPointYText);

        }
        [ObservableProperty]
        private string? _nailsCountText;
        public int NailsCount;

        public BitmapImage bitmap;

        [RelayCommand]
        private void DistributeNailsEqually()
        {
            string fileName = "punkty";
            NailsCount = int.Parse(NailsCountText);

            // Tworzenie listy punktów
            List<ImageUtilities.Punkt> listaPunktow = new List<ImageUtilities.Punkt>();

            (bitmap, listaPunktow) = ImageUtilities.DodajPikseleNaBokach(bitmap, FirstPointX, FirstPointY, SecondPointX, SecondPointY, System.Drawing.Color.DarkRed, NailsCount);
            GeneratedBitmapImage = bitmap;
            GCodeDrillHole.GcodeNailsFromPointList(listaPunktow, 60, 5, fileName, 9, 4, 20);
            ImageUtilities.SaveBitmapImageToPng(bitmap, fileName);
            SystemUtilities.ZapiszListePunktowDoPliku(fileName, listaPunktow);
        }



        private void LoadFilePaths()
        {
            ImageFiles = new ObservableCollection<string>(SystemUtilities.GetImageFilesFromFolder("ImageCollection"));
        }

        [RelayCommand]
        public void GenBlankNailCircle()
        {
            string fileName = "punkty";
            bitmap = ImageUtilities.GenerateBitmapImage(7200, 8200, System.Drawing.Color.LightSlateGray);
            bitmap = ImageUtilities.ZamalujObszar(bitmap, FirstPointX, FirstPointY, SecondPointX, SecondPointY, System.Drawing.Color.SandyBrown);
            


            GeneratedBitmapImage = bitmap;
            ImageUtilities.SaveBitmapImageToPng(bitmap, fileName);


        }
    }
}
