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
        }

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
        public void ThreadArt()
        {
            
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

    }
}
