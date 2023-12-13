using System;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace STR_ART_VI.Model
{
    public static class SystemUtilities
    {

        public static int? ParseStringToInt(string value)
        {
            if (!int.TryParse(value, out int parsedValue))
            {
                return null;
            }

            return parsedValue;
        }

        // Funkcja do uzyskiwania listy plików w danym folderze
        public static List<string> GetImageFilesFromFolder(string folderPath)
        {
            // Uzyskaj pełną ścieżkę do folderu ImageCollection wewnątrz folderu projektu
            string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.FullName;
            string imageCollectionDirectory = Path.Combine(projectDirectory, folderPath);

            // Uzyskaj listę plików obrazów w folderze
            string[] imageFiles = Directory.GetFiles(imageCollectionDirectory, "*.png");

            // Utwórz nową listę, zawierającą tylko nazwy plików
            List<string> imageFileNames = new List<string>();
            foreach (string filePath in imageFiles)
            {
                // Dodaj do listy tylko nazwę pliku, nie całą ścieżkę
                imageFileNames.Add(Path.GetFileName(filePath));
            }

            // Zwróć listę nazw plików
            return imageFileNames;
        }

        public static void SaveImage(string imagePath, BitmapSource image)
        {
            var directoryName = Path.GetDirectoryName(imagePath);

            if(directoryName is null)
            {
                throw new Exception("Directory not found.");
            }

            string newImagePath = Path.Combine(directoryName, Path.GetFileNameWithoutExtension(imagePath) + "_3000pix" + Path.GetExtension(imagePath));
            SaveImage(image, newImagePath);
        }

        private static void SaveImage(BitmapSource imageSource, string filePath)
        {
            BitmapEncoder? encoder = null;
            string extension = Path.GetExtension(filePath).ToLower();

            // Wybór odpowiedniego kodera na podstawie rozszerzenia pliku
            switch (extension)
            {
                case ".jpg":
                    encoder = new JpegBitmapEncoder();
                    break;
                case ".png":
                    encoder = new PngBitmapEncoder();
                    break;
                case ".bmp":
                    encoder = new BmpBitmapEncoder();
                    break;
                default:
                    return;
            }

            // Zapisanie obrazu do pliku
            encoder.Frames.Add(BitmapFrame.Create(imageSource));
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }

        public static void SaveTextToFileUsingFileDialog(string textToSave)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // Ustawienie filtra plików na pliki tekstowe .txt
            saveFileDialog.Filter = "Pliki tekstowe (*.txt)|*.txt";
            saveFileDialog.FilterIndex = 1;

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                try
                {
                    // Zapis tekstu do wybranego pliku
                    File.WriteAllText(filePath, textToSave);
                    
                }
                catch (Exception ex)
                {
                    
                }
            }
        }

        public static void ZapiszListePunktowDoPliku(string fileName, List<ImageUtilities.Punkt> listaPunktow)
        {
            // Nazwa pliku tekstowego
            fileName += ".txt";
            // Uzyskaj ścieżkę do folderu ImageCollection wewnątrz folderu projektu
            string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            string imageCollectionDirectory = Path.Combine(projectDirectory, "ImageCollection");
            // Sprawdź, czy folder ImageCollection istnieje, jeśli nie, utwórz go
            if (!Directory.Exists(imageCollectionDirectory))
            {
                Directory.CreateDirectory(imageCollectionDirectory);
            }
            // Utwórz pełną ścieżkę docelową do pliku PNG
            string filePath = Path.Combine(imageCollectionDirectory, fileName);
            try
            {
                // Utwórz FileStream do zapisu do pliku
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    // Utwórz kulturę, która używa kropki jako separatora dziesiętnego
                    CultureInfo culture = CultureInfo.InvariantCulture;
                    using (StreamWriter writer = new StreamWriter(fileStream))
                    {
                        foreach (ImageUtilities.Punkt punkt in listaPunktow)
                        {
                            // zapisanie do pliku w formacie "xX,yY;"
                            string punktX = (punkt.X).ToString(culture);
                            string punktY = (punkt.Y).ToString(culture);
                            writer.Write($"x{punktX},y{punktY};");
                        }
                    }
                }


                Console.WriteLine("Plik został pomyślnie zapisany.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wystąpił błąd podczas zapisu pliku: {ex.Message}");
            }
        }

        public static string OpenAndReadTextFile()
        {
            string filePath = GetFilePath();

            if (filePath != null)
            {
                try
                {
                    string fileContent = File.ReadAllText(filePath);
                    return fileContent;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Wystąpił błąd podczas odczytu pliku: {ex.Message}");
                }
            }

            return null;
        }

        public static string GetFilePath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Pliki tekstowe (*.txt)|*.txt|Wszystkie pliki (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }

            return null;
        }
    }
}
