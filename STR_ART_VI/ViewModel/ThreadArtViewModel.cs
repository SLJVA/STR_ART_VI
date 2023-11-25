using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using STR_ART_VI.Model;

namespace STR_ART_VI.ViewModel
{
    public partial class ThreadArtViewModel : ObservableObject
    {

        public ThreadArtViewModel()
        {
            LoadThreadArtCommand = new RelayCommand(ThreadArt);
            GenerateGcodeDrillCommand = new RelayCommand(GenerateGcodeDrill);
        }

        public IRelayCommand GenerateGcodeDrillCommand { get; }
        public IRelayCommand LoadThreadArtCommand { get; }
        public string ThreadArtContent;
        public int LiczbaGwozdzi;

        private string _nailCountValue;
        public string NailCountValue
        {
            get => _nailCountValue;
            set
            {
                SetProperty(ref _nailCountValue, value);
                LoadThreadArtCommand.NotifyCanExecuteChanged();
            }
        }
        private string _pointsValue;
        public string PointsValue
        {
            get => _pointsValue;
            set
            {
                SetProperty(ref _pointsValue, value);
                LoadThreadArtCommand.NotifyCanExecuteChanged();
            }
        }
        private string _pointArraySizeValue;
        public string PointArraySizeValue
        {
            get => _pointArraySizeValue;
            set
            {
                SetProperty(ref _pointArraySizeValue, value);
                LoadThreadArtCommand.NotifyCanExecuteChanged();
            }
        }

        public void GenerateGcodeDrill()
        {
            double ringRadius = 53.54;
            int ringWidth = 0;
            int nailsCount = 30;
            double centreX = 459.04;
            double centreY = 348.53;
            double minZaxis = 10;
            double plyThickness = 6;
            double offsetZ = 24;
            string gcode;
            gcode = GCodeDrillHole.gCodeDrill(ringRadius, ringWidth, nailsCount, centreX, centreY, minZaxis, plyThickness, offsetZ);
            SystemUtilities.SaveTextToFileUsingFileDialog(gcode);
        
        }


        public void ThreadArt()
        {
            //double center_x = centerX;
            //double center_y = centerY;
            double center_x = 430;
            double center_y = 375.5;
            //double diameter = Double.Parse(DiameterSizeMessage);
            double diameter = 480;
            int numberOfPoints = 200;

            double[] pointsX;
            double[] pointsY;

            CalculateCirclePoints(center_x, center_y, diameter, numberOfPoints, out pointsX, out pointsY);

            ThreadArtContent = OpenAndReadTextFile();
            Match iloscGwozdzi = Regex.Match(ThreadArtContent, @"Points: (-?\d\d\d)");
            if (iloscGwozdzi.Success)
            {
                LiczbaGwozdzi = int.Parse(iloscGwozdzi.Groups[1].Value);
                NailCountValue = LiczbaGwozdzi.ToString();
                
            }
            int[] pointNumbers = ExtractPointNumbers(ThreadArtContent);
            int pointArraySize = pointNumbers.Length - 1;
            PointArraySizeValue = pointArraySize.ToString();
            string[] pointStrings = Array.ConvertAll(pointNumbers, x => x.ToString());
            PointsValue = string.Join(", ", pointStrings);

            //Tutaj będzie pobranie listy kolejnych numerów gwoździ i przypisywanie im wpółrzędnych X i Y tworząc tablice wielkości ilości połączeń
            double[] instructionPointX = new double[pointNumbers.Length];
            double[] instructionPointY = new double[pointNumbers.Length];
            for (int i = 0; i < pointNumbers.Length; i++)
            {
                int index = pointNumbers[i] - 1; // Odejmujemy 1, ponieważ tablice w C# są indeksowane od 0
                if (index >= 0 && index < pointsX.Length)
                {
                    instructionPointX[i] = pointsX[index];
                    instructionPointY[i] = pointsY[index];
                }
            }
            int[] roundedInstructionPointX = new int[instructionPointX.Length];

            for (int i = 0; i < instructionPointX.Length; i++)
            {
                roundedInstructionPointX[i] = (int)Math.Round(instructionPointX[i]);
            }
            int[] roundedInstructionPointY = new int[instructionPointY.Length];

            for (int i = 0; i < instructionPointY.Length; i++)
            {
                roundedInstructionPointY[i] = (int)Math.Round(instructionPointY[i]);
            }

            // Tworzenie obiektu SaveFileDialog
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Pliki tekstowe (*.txt)|*.txt"; // Filtrowanie typu plików

            if (saveFileDialog.ShowDialog() == true)
            {
                // Pobranie wybranej ścieżki do zapisu pliku
                string filePath = saveFileDialog.FileName;

                // Tworzenie obiektu StreamWriter do zapisu do pliku
                using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    for (int i = 0; i < instructionPointX.Length; i++)
                    {
                        // Tworzenie formatu "xX.Y, Y.Z;"
                        string instruction = $"x{roundedInstructionPointX[i].ToString()},y{roundedInstructionPointY[i].ToString()};";

                        // Zapis instrukcji do pliku
                        writer.Write(instruction);
                    }
                }
            }
        }
        public static string OpenAndReadTextFile()
        {
            string selectedFilePath = string.Empty;
            string fileContent = string.Empty;

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Pliki tekstowe (*.txt)|*.txt",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedFilePath = openFileDialog.FileName;

                try
                {
                    fileContent = File.ReadAllText(selectedFilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Wystąpił błąd podczas odczytu pliku: " + ex.Message);
                }
            }

            return fileContent;
        }

        static int[] ExtractPointNumbers(string inputText)
        {
            // Definiujemy wyrażenie regularne, które dopasuje wzorce "Point_" oraz liczby po nich.
            string pattern = @"Point_(\d+)";
            Regex regex = new Regex(pattern);

            // Szukamy dopasowań w tekście.
            MatchCollection matches = regex.Matches(inputText);
           
            // Inicjalizujemy tablicę, aby przechowywać numery punktów.
            int[] pointNumbers = new int[matches.Count];

            for (int i = 0; i < matches.Count; i++)
            {
                // Parsujemy dopasowane numery punktów i zapisujemy je w tablicy.
                pointNumbers[i] = int.Parse(matches[i].Groups[1].Value);
            }

            return pointNumbers;
        }

        public static void CalculateCirclePoints(double centerX, double centerY, double diameter, int numberOfPoints, out double[] pointsX, out double[] pointsY)
        {
            pointsX = new double[numberOfPoints];
            pointsY = new double[numberOfPoints];

            for (int i = 0; i < numberOfPoints; i++)
            {
                double angle = (2 * Math.PI * i) / numberOfPoints;
                double x = centerX + ((diameter / 2) * Math.Cos(angle));
                double y = centerY + ((diameter / 2) * Math.Sin(angle));

                pointsX[i] = x;
                pointsY[i] = y;
            }
        }

    }
}
