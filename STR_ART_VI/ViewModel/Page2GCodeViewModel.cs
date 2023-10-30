using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STR_ART_VI.Model;

namespace STR_ART_VI.ViewModel
{
    public partial class Page2GCodeViewModel : ObservableObject
    {
        private List<(int x, int y)> redPixelCoordinates = new List<(int x, int y)>();
        private List<(int x, int y)> greenPixelCoordinates = new List<(int x, int y)>();
        private BitmapSource _generatedImage;
        public BitmapSource GeneratedImage
        {
            get => _generatedImage;
            set
            {
                SetProperty(ref _generatedImage, value);
                GenerateImageCommand.NotifyCanExecuteChanged();
            }
        }


        public Page2GCodeViewModel()
        {
            GenerateImageCommand = new RelayCommand(GenerateImageGC);
            OpenFileCommand = new RelayCommand(OpenFile);
           
        }

        public IRelayCommand GenerateImageCommand { get; }
        public IRelayCommand OpenFileCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public void GenerateImageGC()
        {
            UpdateImage();

        }
        private void OpenFile()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string fileContents = File.ReadAllText(openFileDialog.FileName);

                string[] coordinatePairs = fileContents.Split(';');

                redPixelCoordinates.Clear();

                foreach (string pair in coordinatePairs)
                {
                    string[] parts = pair.Split(',');
                    if (parts.Length == 2 && parts[0].StartsWith("x") && parts[1].StartsWith("y") &&
                        int.TryParse(parts[0].Substring(1), out int x) && int.TryParse(parts[1].Substring(1), out int y))
                    {
                        redPixelCoordinates.Add((x, y));
                    }
                }

                UpdateImage();
            }
        }

        private void DrawLineBetweenTwoPixels(byte[] pixels, int width, int x1, int y1, int x2, int y2)
        {
            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            int sx = (x1 < x2) ? 1 : -1;
            int sy = (y1 < y2) ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                int pixelIndex = (y1 * width + x1) * 4;
                pixels[pixelIndex] = 0;   // R
                pixels[pixelIndex + 1] = 0; // G
                pixels[pixelIndex + 2] = 0; // B
                pixels[pixelIndex + 3] = 255; // A

                if (x1 == x2 && y1 == y2)
                {
                    break;
                }

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x1 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y1 += sy;
                }
            }
        }

        private void GenerateImage()
        {
            UpdateImage();
        }

        private double CalculateAngle(double x1, double y1, double x2, double y2)
        {
            double angle = Math.Atan2(y2 - y1, x2 - x1) * (180.0 / Math.PI);
            return angle;
        }


        private void UpdateImage()
        {
            int width = 740;
            int height = 800;
            int Z = 60;
            int speed;

            StringBuilder greenPixelCoordinatesText = new StringBuilder(); // To store the coordinates
            byte[] pixels = new byte[width * height * 4];

            //Wygenerowanie szarego obrazu o podanych rozmiarach
            for (int i = 0; i < width * height * 4; i += 4)
            {
                byte grayValue = 128;
                pixels[i] = grayValue;
                pixels[i + 1] = grayValue;
                pixels[i + 2] = grayValue;
                pixels[i + 3] = 255;
            }

            //Rysowanie czarnej lini pomiędzy gwoździami
            for (int pixelIndex = 0; pixelIndex < redPixelCoordinates.Count - 1; pixelIndex++)
            {
                var firstPixel = redPixelCoordinates[pixelIndex];
                var secondPixel = redPixelCoordinates[pixelIndex + 1];
                DrawLineBetweenTwoPixels(pixels, width, firstPixel.y, firstPixel.x, secondPixel.y, secondPixel.x);
            }

            //Rysowanie czerwonych gwoździ
            foreach (var coordinate in redPixelCoordinates)
            {
                int y = coordinate.x;
                int x = coordinate.y;

                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    int redPixelIndex = (y * width + x) * 4;
                    pixels[redPixelIndex] = 255;
                    pixels[redPixelIndex + 1] = 0;
                    pixels[redPixelIndex + 2] = 0;
                    pixels[redPixelIndex + 3] = 255;
                }
            }

            if (redPixelCoordinates.Count >= 2)
            {
                for (int pixelIndex = 0; pixelIndex < redPixelCoordinates.Count - 2; pixelIndex++)
                {
                    // Pobranie koordynatów kolejnych 3 czerwonych pikseli (gwoździ)
                    var firstPixel = redPixelCoordinates[pixelIndex];
                    var secondPixel = redPixelCoordinates[pixelIndex + 1];
                    var thirdPixel = redPixelCoordinates[pixelIndex + 2];


                    double angle = CalculateAngle(firstPixel.x, firstPixel.y, secondPixel.x, secondPixel.y);
                    double angle2 = CalculateAngleOriented(firstPixel.x, firstPixel.y, secondPixel.x, secondPixel.y, thirdPixel.x, thirdPixel.y);


                    // Calculate the angle between the vectors
                    double anglebetween = CalculateAngle(firstPixel.x, firstPixel.y, secondPixel.x, secondPixel.y, thirdPixel.x, thirdPixel.y);
                    
                    // Tworzenie zielonych pikseli dla danego segmentu czerwonych pikseli
                    List<(int x, int y, int z, int f)> greenPixelCoordinates = new List<(int x, int y, int z, int f)>();
                    int vel = 20;
                    // Dodanie 3 zielonych pikseli dookoła gwoździa
                    for (int i = -90; i <= 90; i+=90)
                    {
                        if (i == -90)
                        {
                            vel = 100;
                        }
                        else
                        {
                            vel = 20;
                        }

                        double newAngle = angle + i; // Dodawanie 90 stopni do kąta
                        double d_newX = secondPixel.x + 18 * Math.Cos(newAngle * (Math.PI / 180.0)); // Obliczanie nowych współrzędnych X
                        double d_newY = secondPixel.y + 18 * Math.Sin(newAngle * (Math.PI / 180.0)); // Obliczanie nowych współrzędnych Y
                        int i_newX = (int)Math.Floor(d_newX);
                        int i_newY = (int)Math.Floor(d_newY);
                        int i_newZ;
                        if (i == 90 && (angle2 < 75 || angle2 > 195))
                        {
                            i_newZ = 38;
                        }
                        else
                        {
                            i_newZ = 36;
                        }
                        greenPixelCoordinates.Add((i_newX, i_newY, i_newZ, vel));
                        greenPixelCoordinatesText.Append($"x{i_newX},y{i_newY},z{i_newZ},f{vel};");
                       
                    }

                    // Dodanie kolejnych 2 zielonych pikseli jeśli
                    if (angle2 >= 75 && angle2 <= 195)
                    {
                        for (int i = 180; i <= 270;)
                        {
                            double newAngle = angle + i; // Dodawanie 90 stopni do kąta
                            double d_newX = secondPixel.x + 20 * Math.Cos(newAngle * (Math.PI / 180.0)); // Obliczanie nowych współrzędnych X
                            double d_newY = secondPixel.y + 20 * Math.Sin(newAngle * (Math.PI / 180.0)); // Obliczanie nowych współrzędnych Y
                            int i_newX = (int)Math.Floor(d_newX);
                            int i_newY = (int)Math.Floor(d_newY);
                            int i_newZ;
                            if (i == 270)
                            {
                                i_newZ = 38;
                            }
                            else
                            {
                                i_newZ = 36;
                            }

                            greenPixelCoordinates.Add((i_newX, i_newY, i_newZ, vel));
                            greenPixelCoordinatesText.Append($"x{i_newX},y{i_newY},z{i_newZ},f{vel};");
                            i = i + 90;
                        }
                    }
                    // Generowanie zielonych pikseli dla danego segmentu
                    foreach (var greenPixel in greenPixelCoordinates)
                    {
                        int newY = greenPixel.x;
                        int newX = greenPixel.y;

                        if (newX >= 0 && newX < width && newY >= 0 && newY < height)
                        {
                            int newPixelIndex = (newY * width + newX) * 4;
                            pixels[newPixelIndex] = 0;
                            pixels[newPixelIndex + 1] = 255;
                            pixels[newPixelIndex + 2] = 0;
                            pixels[newPixelIndex + 3] = 255;
                        }
                    }


                }
            }




            SaveGreenPixelCoordinatesToFile(greenPixelCoordinatesText.ToString());
            ConvertAndSaveTextToFile(greenPixelCoordinatesText.ToString(), "sciezka_do_pliku.txt");
            GeneratedImage = BitmapSource.Create(width, height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null, pixels, width * 4);
            // Ścieżka do pliku, w którym chcemy zapisać obraz
            string filePath = "sciezka_do_pliku.png";

            // Utwórz kodera PNG
            PngBitmapEncoder encoder = new PngBitmapEncoder();

            // Dodaj wygenerowany obraz do kodera
            encoder.Frames.Add(BitmapFrame.Create(GeneratedImage));

            // Zapisz obraz do pliku
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(fs);
            }




        }

        static double CalculateAngle(double aX, double aY, double bX, double bY, double cX, double cY)
        {
            double licznik = ((Math.Abs(aX) - Math.Abs(bX)) * (Math.Abs(cX) - Math.Abs(bX))) + ((Math.Abs(aY) - Math.Abs(bY)) * (Math.Abs(cY) - Math.Abs(bY)));

            double mianownik1 = Math.Sqrt(((Math.Abs(aX) - Math.Abs(bX)) * (Math.Abs(aX) - Math.Abs(bX))) + ((Math.Abs(aY) - Math.Abs(bY)) * (Math.Abs(aY) - Math.Abs(bY))));

            double mianownik2 = Math.Sqrt(((Math.Abs(cX) - Math.Abs(bX)) * (Math.Abs(cX) - Math.Abs(bX))) + ((Math.Abs(cY) - Math.Abs(bY)) * (Math.Abs(cY) - Math.Abs(bY))));

            double cosAngle = licznik / (mianownik1 * mianownik2);

            double angleRad = Math.Acos(cosAngle);

            double angle = angleRad * (180.0 / Math.PI); // Convert radians to degrees



            if (licznik < 0)
            {
                angle = angle + 180;
            }

            return angle;
        }
        static double CalculateAngleOriented(double aX, double aY, double bX, double bY, double cX, double cY)
        {
            double x1 = (Math.Abs(aX) - Math.Abs(bX));
            double y1 = (Math.Abs(aY) - Math.Abs(bY));
            double x2 = (Math.Abs(cX) - Math.Abs(bX));
            double y2 = (Math.Abs(cY) - Math.Abs(bY));

            //double angleRad = Math.Atan2(x1 * x2 + y1 * y2, x1 * y2 - y1 * x2);
            //double angleDeg = angleRad * (180.0 / Math.PI);
            //return angleDeg;

            double angle1 = Math.Atan2(y1, x1);
            double angle2 = Math.Atan2(y2, x2);
            double angleDifference = angle2 - angle1;

            if (angleDifference < 0)
                angleDifference += 2 * Math.PI; // Ensure a positive angle

            return angleDifference * (180.0 / Math.PI); // Convert radians to degrees

        }


        private void SaveGreenPixelCoordinatesToFile(string coordinates)
        {
            string filePath = "green_pixel_coordinates.txt";

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.Write(coordinates); // Write the coordinates to the file
                }

                Console.WriteLine("Green pixel coordinates saved to file.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while saving the coordinates: {ex.Message}");
            }
        }

        private void ConvertAndSaveTextToFile(string inputText, string outputPath)
        {
            // Rozdziel tekst na elementy
            string[] coordinatesArray = inputText.Split(';');

            // Utwórz StringBuilder do przechowywania przetworzonego tekstu
            StringBuilder outputText = new StringBuilder();

            // Dodaj prefiks "G1 " i zamień ',' na ' ' w każdym elemencie
            foreach (string coordinate in coordinatesArray)
            {
                string[] parts = coordinate.Split(',');
                if (parts.Length == 4)
                {
                    string x = parts[0].Substring(1); // Pominięcie 'x'
                    string y = parts[1].Substring(1); // Pominięcie 'y'
                    string z = parts[2].Substring(1); // Pominięcie 'z'
                    string f = parts[3].Substring(1);

                    // Dodaj do wynikowego tekstu w odpowiednim formacie
                    outputText.Append($"G1 X{x} Y{y} Z{z} F{f},");
                }
            }

            // Usuń ostatnią przecinkę i spacje
            if (outputText.Length > 0)
            {
                outputText.Length -= 1;
            }

            // Zapisz przetworzony tekst do pliku
            File.WriteAllText(outputPath, outputText.ToString());
        }














    }
}
