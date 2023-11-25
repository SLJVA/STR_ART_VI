using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System.Drawing;
using System.Drawing.Imaging;

namespace STR_ART_VI.Model
{
    public static class ImageUtilities
    {
        public static BitmapImage CreateBitmap(string imagePath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(imagePath);
            bitmap.EndInit();

            return bitmap;
        }

        public static BitmapImage ResizeImage(BitmapImage sourceImage, int newWidth, int newHeight)
        {
            // Konwersja BitmapImage na Bitmap
            BitmapImage bitmapImage = sourceImage;
            BitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            // Utworzenie nowego obrazu o podanych rozmiarach
            BitmapImage resizedImage = new BitmapImage();
            resizedImage.BeginInit();
            resizedImage.DecodePixelWidth = newWidth;
            resizedImage.DecodePixelHeight = newHeight;
            resizedImage.CacheOption = BitmapCacheOption.OnLoad;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                encoder.Save(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                resizedImage.StreamSource = memoryStream;
                resizedImage.EndInit();
                resizedImage.Freeze(); // Zamrożenie obrazu, aby można go było używać na wątku interfejsu użytkownika
            }

            return resizedImage;
        }

        // Funkcja do zapisywania BitmapImage do pliku PNG
        public static void SaveBitmapImageToPng(BitmapImage bitmapImage, string fileName)
        {
            fileName += ".png";
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

            // Utwórz obiekt PngBitmapEncoder
            PngBitmapEncoder encoder = new PngBitmapEncoder();

            // Dodaj klatkę z BitmapImage do encoder
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            // Utwórz strumień do zapisywania danych
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                // Zapisz dane do pliku
                encoder.Save(stream);
            }
        }


        public static BitmapImage GenerateBitmapImage(int width, int height, System.Drawing.Color backgroundColor)
        {
            // Utwórz nowy obiekt WriteableBitmap o określonych rozmiarach i formatie pikseli
            WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            // Uzyskaj dane pikseli z obiektu WriteableBitmap
            Int32Rect rect = new Int32Rect(0, 0, width, height);
            int stride = (width * bitmap.Format.BitsPerPixel + 7) / 8;
            byte[] pixelData = new byte[stride * height];

            // Ustaw kolor tła w formie danych pikseli
            byte[] backgroundColorBytes = { backgroundColor.B, backgroundColor.G, backgroundColor.R, backgroundColor.A };
            for (int i = 0; i < pixelData.Length; i += 4)
            {
                Array.Copy(backgroundColorBytes, 0, pixelData, i, 4);
            }

            // Wypełnij obiekt WriteableBitmap danymi pikseli
            bitmap.WritePixels(rect, pixelData, stride, 0);

            // Konwertuj WriteableBitmap na BitmapImage
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(stream);
                stream.Position = 0;

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }

            // Zwróć gotowy obiekt BitmapImage
            return bitmapImage;
        }

        public static BitmapSource DetectEdges(BitmapSource sourceImage)
        {
            // Konwersja na obraz w skali szarości
            FormatConvertedBitmap grayScaleBitmap = new FormatConvertedBitmap(sourceImage, PixelFormats.Gray8, null, 0);

            // Wykrywanie krawędzi (konturów) z użyciem operatora Sobela
            BitmapSource edgeImage = SobelEdgeDetector.DetectEdges(grayScaleBitmap);

            return edgeImage;
        }

        public static BitmapSource ApplyRedPixels(BitmapSource image, int redPixelCount, string ImagePath)
        {
            int width = image.PixelWidth;
            int height = image.PixelHeight;

            var directoryName = Path.GetDirectoryName(ImagePath);
            string newImagePath = Path.Combine(directoryName, "red_pixels.txt");


            // Pobierz współrzędne białych pikseli
            List<(int x, int y)> whitePixelCoordinates = GetWhitePixelCoordinates(image);

            // Lista przechowująca pozycje czerwonych pikseli
            List<(int x, int y)> redPixelCoordinates = new List<(int x, int y)>();

            if (redPixelCount > whitePixelCoordinates.Count)
            {
                redPixelCount = whitePixelCoordinates.Count;

                throw new ArgumentException("Too many red pixels.");
            }

            // Ustawienie ziarna generatora liczb losowych
            Random random = new Random();

            // Oblicz odstęp między pikselami w celu równomiernego rozmieszczenia
            double interval = (double)whitePixelCoordinates.Count / redPixelCount;

            // Tworzenie docelowego obrazu z modyfikacjami
            RenderTargetBitmap resultImage = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            resultImage.Clear();

            // Tworzenie kontekstu renderowania
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                // Rysowanie oryginalnego obrazu
                drawingContext.DrawImage(image, new Rect(0, 0, width, height));

                // Iteracja po wybranych pikselach białych i nanieś czerwone piksele
                double position = 0;
                int count = 0;

                while (count < redPixelCount)
                {
                    // Oblicz indeks piksela białego na podstawie pozycji
                    int index = (int)(position * interval);
                    (int x, int y) = whitePixelCoordinates[index];

                    // Nanieś czerwony piksel
                    drawingContext.DrawRectangle(System.Windows.Media.Brushes.Red, null, new Rect(x, y, 1, 1));

                    // Dodaj pozycję czerwonego piksela do listy
                    redPixelCoordinates.Add((x, y));

                    // Zapisz pozycje czerwonych pikseli do pliku
                    SaveRedPixelCoordinatesToFile(redPixelCoordinates, newImagePath);

                    count++;
                    position += 1;

                    // Jeśli przekroczono liczbę czerwonych pikseli lub nie ma już dostępnych białych pikseli, przerwij pętlę
                    if (count >= redPixelCount || position >= whitePixelCoordinates.Count)
                        break;
                }
            }

            // Zakończenie renderowania i zwrócenie docelowego obrazu
            resultImage.Render(drawingVisual);
            return resultImage;
        }

        private static List<(int x, int y)> GetWhitePixelCoordinates(BitmapSource image)
        {
            int width = image.PixelWidth;
            int height = image.PixelHeight;
            int stride = width * ((image.Format.BitsPerPixel + 7) / 8);
            byte[] pixels = new byte[height * stride];
            image.CopyPixels(pixels, stride, 0);

            List<(int x, int y)> whitePixelCoordinates = new List<(int x, int y)>();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (pixels[y * stride + x] == 255)
                        whitePixelCoordinates.Add((x, y));
                }
            }

            return whitePixelCoordinates;
        }

        private static void SaveRedPixelCoordinatesToFile(List<(int x, int y)> redPxelCoordinates, string newImagePath)
        {
            //Scieżka pliku
            string filePath = newImagePath;

            //Zapisanie pozycji czerwonych pikseli do pliku
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach ((int x, int y) in redPxelCoordinates)
                {
                    writer.WriteLine($"{x}, {y}");
                }
            }
        }


        public static WriteableBitmap GenerateWhiteImage(int width, int height)
        {
            // Tworzenie nowego obrazu o określonych rozmiarach
            var whiteBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            // Wypełnienie obrazu kolorem białym
            byte[] whitePixels = new byte[4 * width * height];
            for (int i = 0; i < whitePixels.Length; i += 4)
            {
                whitePixels[i] = 255; // Składowa niebieska
                whitePixels[i + 1] = 255; // Składowa zielona
                whitePixels[i + 2] = 255; // Składowa czerwona
                whitePixels[i + 3] = 255; // Składowa alfa
            }

            whiteBitmap.WritePixels(new Int32Rect(0, 0, width, height), whitePixels, width * 4, 0);

            return whiteBitmap;
        }

        public static void SaveImageToFile(WriteableBitmap image)
        {
            string? filePath = FileDialogUtilities.SaveImageFile();
            if (filePath != null)
            {
                // Zapis obrazu do pliku
                using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(fileStream);
                }

            }
        }

        public static BitmapImage CropImageToSquare(BitmapImage sourceImage)
        {
            // Ustal proporcje obrazu źródłowego
            double sourceWidth = sourceImage.PixelWidth;
            double sourceHeight = sourceImage.PixelHeight;
            double aspectRatio = sourceWidth / sourceHeight;

            // Określ nowy rozmiar prostokąta docelowego
            double newWidth, newHeight;
            if (aspectRatio > 1)
            {
                newWidth = sourceHeight;
                newHeight = sourceHeight;
            }
            else
            {
                newWidth = sourceWidth;
                newHeight = sourceWidth;
            }

            // Oblicz współrzędne przycinania
            double xOffset = (sourceWidth - newWidth) / 2;
            double yOffset = (sourceHeight - newHeight) / 2;

            // Utwórz kawałek obrazu
            CroppedBitmap croppedImage = new CroppedBitmap(sourceImage, new Int32Rect((int)xOffset, (int)yOffset, (int)newWidth, (int)newHeight));

            // Konwertuj do formatu BitmapImage
            MemoryStream memoryStream = new MemoryStream();
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(croppedImage));
            encoder.Save(memoryStream);

            BitmapImage resultImage = new BitmapImage();
            resultImage.BeginInit();
            resultImage.StreamSource = new MemoryStream(memoryStream.ToArray());
            resultImage.EndInit();

            return resultImage;
        }

        public static BitmapImage ConvertToBlackAndWhite(BitmapImage originalImage)
        {
            // Tworzenie obiektu FormatConvertedBitmap, który będzie zawierał przekształcony obraz
            FormatConvertedBitmap grayscaleImage = new FormatConvertedBitmap();
            grayscaleImage.BeginInit();
            grayscaleImage.Source = originalImage;
            grayscaleImage.DestinationFormat = PixelFormats.Gray8;
            grayscaleImage.EndInit();

            // Tworzenie nowego obiektu BitmapImage na podstawie obrazu w odcieniach szarości
            BitmapImage blackAndWhiteImage = new BitmapImage();
            blackAndWhiteImage.BeginInit();
            blackAndWhiteImage.StreamSource = new MemoryStream(ImageToByteArray(grayscaleImage));
            blackAndWhiteImage.EndInit();

            return blackAndWhiteImage;
        }

        public static byte[] ImageToByteArray(BitmapSource imageSource)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(imageSource));

            using (MemoryStream stream = new MemoryStream())
            {
                encoder.Save(stream);
                return stream.ToArray();
            }
        }

        public static BitmapImage DetectEdges(BitmapImage sourceImage, byte highThreshold, byte lowThreshold, double gaussianSigma)
        {
            // Konwersja obrazu BitmapImage na obiekt Bitmap
            Bitmap sourceBitmap = BitmapImageToBitmap(sourceImage);

            // Konwersja obrazu na format skali szarości
            Grayscale grayscaleFilter = new Grayscale(0.2125, 0.7154, 0.0721);
            Bitmap grayscaleImage = grayscaleFilter.Apply(sourceBitmap);

            // Inicjalizacja obiektu dla detekcji krawędzi
            CannyEdgeDetector edgeDetector = new CannyEdgeDetector();

            edgeDetector.HighThreshold = highThreshold;
            edgeDetector.LowThreshold = lowThreshold;
            edgeDetector.GaussianSigma = gaussianSigma;

            // Przetwarzanie obrazu w celu wykrycia krawędzi
            Bitmap edgeImage = edgeDetector.Apply(grayscaleImage);

            // Konwersja obrazu Bitmap na BitmapImage
            BitmapImage edgeBitmapImage = BitmapToBitmapImage(edgeImage);

            return edgeBitmapImage;
        }

        public static Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            Bitmap bitmap;
            using (MemoryStream memory = new MemoryStream())
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Save(memory);
                bitmap = new Bitmap(memory);
            }
            return bitmap;
        }

        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memory;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }

        public static BitmapImage InvertColors(BitmapImage inputImage, int threshold)
        {
            // Konwertuj BitmapImage na Bitmap
            Bitmap inputBitmap;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(inputImage));
                encoder.Save(memoryStream);
                inputBitmap = new Bitmap(memoryStream);
            }

            // Przetwórz obraz
            for (int x = 0; x < inputBitmap.Width; x++)
            {
                for (int y = 0; y < inputBitmap.Height; y++)
                {
                    System.Drawing.Color pixelColor = inputBitmap.GetPixel(x, y);
                    int averageColor = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;

                    if (averageColor < threshold)
                    {
                        inputBitmap.SetPixel(x, y, System.Drawing.Color.White);
                    }
                    else
                    {
                        inputBitmap.SetPixel(x, y, System.Drawing.Color.Black);
                    }
                }
            }

            // Konwertuj z powrotem na BitmapImage
            BitmapImage outputImage;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                inputBitmap.Save(memoryStream, ImageFormat.Bmp);
                outputImage = new BitmapImage();
                outputImage.BeginInit();
                outputImage.StreamSource = new MemoryStream(memoryStream.ToArray());
                outputImage.EndInit();
            }

            return outputImage;
        }


        public static (BitmapImage obrazNiski, BitmapImage obrazWysoki) RozdzielObrazy(BitmapImage obrazWejsciowy)
        {
            int szerokosc = obrazWejsciowy.PixelWidth;
            int wysokosc = obrazWejsciowy.PixelHeight;

            BitmapSource obrazNiski = null;
            BitmapSource obrazWysoki = null;

            // Utwórz formaty dla obrazów wyjściowych
            FormatConvertedBitmap obrazNiskiKonwertowany = new FormatConvertedBitmap();
            FormatConvertedBitmap obrazWysokiKonwertowany = new FormatConvertedBitmap();

            obrazNiskiKonwertowany.BeginInit();
            obrazNiskiKonwertowany.Source = obrazWejsciowy;
            obrazNiskiKonwertowany.DestinationFormat = PixelFormats.Gray8;
            obrazNiskiKonwertowany.EndInit();

            obrazWysokiKonwertowany.BeginInit();
            obrazWysokiKonwertowany.Source = obrazWejsciowy;
            obrazWysokiKonwertowany.DestinationFormat = PixelFormats.Gray8;
            obrazWysokiKonwertowany.EndInit();

            // Utwórz tablice bajtów dla obrazów wyjściowych
            byte[] pikseleNiskie = new byte[szerokosc * wysokosc];
            byte[] pikseleWysokie = new byte[szerokosc * wysokosc];

            obrazNiskiKonwertowany.CopyPixels(pikseleNiskie, szerokosc, 0);
            obrazWysokiKonwertowany.CopyPixels(pikseleWysokie, szerokosc, 0);

            for (int i = 0; i < pikseleNiskie.Length; i++)
            {
                if (pikseleNiskie[i] <= 128)
                {
                    pikseleNiskie[i] = 0;
                    pikseleWysokie[i] = 255;
                }
                else
                {
                    pikseleNiskie[i] = 255;
                    pikseleWysokie[i] = 0;
                }
            }

            obrazNiski = BitmapSource.Create(szerokosc, wysokosc, 96, 96, PixelFormats.Gray8, null, pikseleNiskie, szerokosc);
            obrazWysoki = BitmapSource.Create(szerokosc, wysokosc, 96, 96, PixelFormats.Gray8, null, pikseleWysokie, szerokosc);

            BitmapImage obrazNiskiBitmapImage = ConvertBitmapSourceToBitmapImage(obrazNiski);
            BitmapImage obrazWysokiBitmapImage = ConvertBitmapSourceToBitmapImage(obrazWysoki);

            return (obrazNiskiBitmapImage, obrazWysokiBitmapImage);
        }

        public static BitmapImage ConvertBitmapSourceToBitmapImage(BitmapSource bitmapSource)
        {
            using (var memoryStream = new System.IO.MemoryStream())
            {
                var encoder = new PngBitmapEncoder(); // Możesz użyć innego kodera, w zależności od potrzeb
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(memoryStream);

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Opcjonalnie, aby zablokować obraz

                return bitmapImage;
            }
        }


        public static (BitmapImage obrazPrzetworzonyBitmapImage, int[] zbiorNowychOdcieni) PrzetworzObraz(BitmapImage obrazWejsciowy, int iloscOdcieni)
        {
            int szerokosc = obrazWejsciowy.PixelWidth;
            int wysokosc = obrazWejsciowy.PixelHeight;

            BitmapSource obrazPrzetworzony = null;

            // Utwórz formaty dla obrazu wyjściowego
            FormatConvertedBitmap obrazKonwertowany = new FormatConvertedBitmap();

            obrazKonwertowany.BeginInit();
            obrazKonwertowany.Source = obrazWejsciowy;
            obrazKonwertowany.DestinationFormat = PixelFormats.Gray8;
            obrazKonwertowany.EndInit();

            // Utwórz tablicę bajtów dla obrazu wyjściowego
            byte[] pikselePrzetworzone = new byte[szerokosc * wysokosc];

            obrazKonwertowany.CopyPixels(pikselePrzetworzone, szerokosc, 0);

            int wielkoscPrzedzialu = 255 / iloscOdcieni;
            int y = 0;

            int[] zbiorNowychOdcieni = new int[iloscOdcieni];
            for (int i = 0; i <= iloscOdcieni; i++)
            {
                y = y + wielkoscPrzedzialu;
                zbiorNowychOdcieni[i] = y;
            }

            for (int i = 0; i < pikselePrzetworzone.Length; i++)
            {
                for (int j = 1; j < iloscOdcieni; j++)
                {
                    if ((pikselePrzetworzone[i] <= zbiorNowychOdcieni[j]) && (pikselePrzetworzone[i] > (zbiorNowychOdcieni[j] - wielkoscPrzedzialu)))
                    {
                        pikselePrzetworzone[i] = (byte)(zbiorNowychOdcieni[j] - wielkoscPrzedzialu);
                        break;
                    }
                    if (pikselePrzetworzone[i] > zbiorNowychOdcieni[iloscOdcieni-1])
                    {
                        pikselePrzetworzone[i] = 255;
                    }
                }            
            }

            obrazPrzetworzony = BitmapSource.Create(szerokosc, wysokosc, 96, 96, PixelFormats.Gray8, null, pikselePrzetworzone, szerokosc);

            BitmapImage obrazPrzetworzonyBitmapImage = ConvertBitmapSourceToBitmapImage(obrazPrzetworzony);

            return (obrazPrzetworzonyBitmapImage, zbiorNowychOdcieni);
        }


        public static BitmapImage DodajCzerwonePiksle(BitmapImage obrazWejsciowy)
        {
            int szerokosc = obrazWejsciowy.PixelWidth;
            int wysokosc = obrazWejsciowy.PixelHeight;

            FormatConvertedBitmap obrazKonwertowanyGray = new FormatConvertedBitmap();
            obrazKonwertowanyGray.BeginInit();
            obrazKonwertowanyGray.Source = obrazWejsciowy;
            obrazKonwertowanyGray.DestinationFormat = PixelFormats.Gray8;
            obrazKonwertowanyGray.EndInit();

            byte[] pikseleGray = new byte[szerokosc * wysokosc];
            obrazKonwertowanyGray.CopyPixels(pikseleGray, szerokosc, 0);

            int iloscPikseli = 0;

            foreach (byte piksel in pikseleGray)
            {
                if (piksel == 31)
                {
                    iloscPikseli++;
                }
            }
            int polowaIlosciPikseli = iloscPikseli / 2;

            // Konwertuj obraz na format PixelFormats.Bgr32, który obsługuje kanały Red, Green, Blue oraz Alpha (przezroczystość).
            FormatConvertedBitmap obrazKonwertowany = new FormatConvertedBitmap();
            obrazKonwertowany.BeginInit();
            obrazKonwertowany.Source = obrazWejsciowy;
            obrazKonwertowany.DestinationFormat = PixelFormats.Bgr32;
            obrazKonwertowany.EndInit();

            

            byte[] piksele = new byte[szerokosc * wysokosc * 4]; // Format Bgr32 ma 4 bajty na piksel (Blue, Green, Red, Alpha)
            Random random = new Random(); // Inicjalizacja generatora liczb losowych

            int j = 0;

            for (int i = 0; i < piksele.Length; i += 4)
            {

                if (pikseleGray[j] == 31)
                {
                    int losowaWartosc = random.Next(10); // Losuje 0 lub 1
                    if (losowaWartosc == 1)
                    {
                        // Dodaj czerwony piksel
                        piksele[i] = 0;     // Blue
                        piksele[i + 1] = 0; // Green
                        piksele[i + 2] = 255; // Red
                        piksele[i + 3] = 255; // Alpha

                    }
                    
                }
                j++;
            }

            BitmapSource obrazZCzerwonymiPikselami = BitmapSource.Create(szerokosc, wysokosc, 96, 96, PixelFormats.Bgr32, null, piksele, szerokosc * 4);

            BitmapImage obrazZCzerwonymiPikselamiBitmapImage = ConvertBitmapSourceToBitmapImage(obrazZCzerwonymiPikselami);

            return obrazZCzerwonymiPikselamiBitmapImage;
        }

        public static BitmapImage DodajPiksel(BitmapImage originalBitmapImage, int x, int y, System.Drawing.Color kolor)
        {
            // Konwertuj BitmapImage na WriteableBitmap
            WriteableBitmap writeableBitmap = new WriteableBitmap(originalBitmapImage);

            // Sprawdź, czy podane współrzędne są w granicach bitmapy
            if (x >= 0 && x < writeableBitmap.PixelWidth && y >= 0 && y < writeableBitmap.PixelHeight)
            {
                // Oblicz indeks piksela w tablicy pikseli
                int pixelIndex = y * writeableBitmap.PixelWidth + x;

                // Ustaw kolor piksela na podany kolor
                writeableBitmap.WritePixels(new Int32Rect(x, y, 1, 1), new byte[] { kolor.B, kolor.G, kolor.R, kolor.A }, 4, 0);

                // Konwertuj WriteableBitmap z powrotem na BitmapImage
                using (var stream = new System.IO.MemoryStream())
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));
                    encoder.Save(stream);
                    stream.Position = 0;

                    BitmapImage modifiedBitmapImage = new BitmapImage();
                    modifiedBitmapImage.BeginInit();
                    modifiedBitmapImage.StreamSource = stream;
                    modifiedBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    modifiedBitmapImage.EndInit();

                    return modifiedBitmapImage;
                }
            }
            else
            {
                Console.WriteLine("Współrzędne są poza granicami bitmapy.");
                return originalBitmapImage; // Zwracamy oryginalny obraz w przypadku błędu
            }
        }      
        public static BitmapImage ZamalujObszar(BitmapImage originalBitmapImage, int x1, int y1, int x2, int y2, System.Drawing.Color kolor)
        {
            // Konwertuj BitmapImage na WriteableBitmap
            WriteableBitmap writeableBitmap = new WriteableBitmap(originalBitmapImage);
            // Zapisz rozmiar obrazu
            int maxX = writeableBitmap.PixelWidth;
            int maxY = writeableBitmap.PixelHeight;
            // Sprawdź, czy podane współrzędne są w granicach bitmapy
            if (x1 >= 0 && x1 < maxX && y1 >= 0 && y1 < maxY && x2 >= 0 && x2 < maxX && y2 >= 0 && y2 < maxY)
            {
                for(int y = y1; y <= y2; y++)
                {
                    for (int x = x1; x <= x2; x++)
                    {

                        // Ustaw kolor piksela na podany kolor
                        writeableBitmap.WritePixels(new Int32Rect(x, y, 1, 1), new byte[] { kolor.B, kolor.G, kolor.R, kolor.A }, 4, 0);
                    }
                }
                // Konwertuj WriteableBitmap z powrotem na BitmapImage
                using (var stream = new System.IO.MemoryStream())
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));
                    encoder.Save(stream);
                    stream.Position = 0;

                    BitmapImage modifiedBitmapImage = new BitmapImage();
                    modifiedBitmapImage.BeginInit();
                    modifiedBitmapImage.StreamSource = stream;
                    modifiedBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    modifiedBitmapImage.EndInit();

                    return modifiedBitmapImage;
                }
            }
            else
            {
                return originalBitmapImage;
            }
        }


        public static (BitmapImage, List<ImageUtilities.Punkt>) DodajPikseleNaBokach(BitmapImage originalBitmapImage, int x1, int y1, int x2, int y2, System.Drawing.Color kolor, int nailsCount)
        {
            // Konwertuj BitmapImage na WriteableBitmap
            WriteableBitmap writeableBitmap = new WriteableBitmap(originalBitmapImage);

            // Tworzenie listy punktów
            List<Punkt> listaPunktow = new List<Punkt>();

            // Zapisz rozmiar obrazu
            int maxX = writeableBitmap.PixelWidth;
            int maxY = writeableBitmap.PixelHeight;

            // Sprawdź, czy podane współrzędne są w granicach bitmapy
            if (x1 >= 0 && x1 < maxX && y1 >= 0 && y1 < maxY && x2 >= 0 && x2 < maxX && y2 >= 0 && y2 < maxY)
            {

                // Ustaw kolor piksela na podany kolor
                writeableBitmap.WritePixels(new Int32Rect(x1, y1, 1, 1), new byte[] { kolor.B, kolor.G, kolor.R, kolor.A }, 4, 0);
                listaPunktow.Add(new Punkt(x1,y1));
                writeableBitmap.WritePixels(new Int32Rect(x2, y2, 1, 1), new byte[] { kolor.B, kolor.G, kolor.R, kolor.A }, 4, 0);
                listaPunktow.Add(new Punkt(x2, y2));
                writeableBitmap.WritePixels(new Int32Rect(x1, y2, 1, 1), new byte[] { kolor.B, kolor.G, kolor.R, kolor.A }, 4, 0);
                listaPunktow.Add(new Punkt(x1, y2));
                writeableBitmap.WritePixels(new Int32Rect(x2, y1, 1, 1), new byte[] { kolor.B, kolor.G, kolor.R, kolor.A }, 4, 0);
                listaPunktow.Add(new Punkt(x2, y1));

                // Oblicz dystans pomiędzy punktami
                int distanceBetwenNails = (y2 - y1) / (nailsCount + 1);

                //Sprawdz czy parzysta
                if (CzyParzysta(nailsCount))
                {
                    (writeableBitmap, var listaPunktow1) = RozmiescPoLini(true, y1, x1, x2, nailsCount, writeableBitmap, distanceBetwenNails, kolor);
                    listaPunktow.AddRange(listaPunktow1);
                    (writeableBitmap, var listaPunktow2) = RozmiescPoLini(true, y2, x1, x2, nailsCount, writeableBitmap, distanceBetwenNails, kolor);
                    listaPunktow.AddRange(listaPunktow2);
                    (writeableBitmap, var listaPunktow3) = RozmiescPoLini(false, x1, y1, y2, nailsCount, writeableBitmap, distanceBetwenNails, kolor);
                    listaPunktow.AddRange(listaPunktow3);
                    (writeableBitmap, var listaPunktow4) = RozmiescPoLini(false, x2, y1, y2, nailsCount, writeableBitmap, distanceBetwenNails, kolor);
                    listaPunktow.AddRange(listaPunktow4);
                }
                else
                {

                }


                // Konwertuj WriteableBitmap z powrotem na BitmapImage
                using (var stream = new System.IO.MemoryStream())
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));
                    encoder.Save(stream);
                    stream.Position = 0;

                    BitmapImage modifiedBitmapImage = new BitmapImage();
                    modifiedBitmapImage.BeginInit();
                    modifiedBitmapImage.StreamSource = stream;
                    modifiedBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    modifiedBitmapImage.EndInit();

                    return (modifiedBitmapImage, listaPunktow);
                }
            }
            else
            {
                return (originalBitmapImage, listaPunktow);
            }
        }

        static bool CzyParzysta(int liczba)
        {
            return liczba % 2 == 0;
        }

        static (WriteableBitmap, List<Punkt>) RozmiescPoLini(bool isX, int stala, int start, int stop, int nailsCount, WriteableBitmap image, int distanceBetwenNails, System.Drawing.Color kolor)
        {

            // Tworzenie listy punktów
            List<Punkt> listaPunktow = new List<Punkt>();
            // Górna krawedz czworokata
            int j = 1;
            int p = 1;
            switch(isX)
            {
                case true:
                    for (int i = 0; i < nailsCount; i++)
                    {
                        if (CzyParzysta(i))
                        {
                            int x = start + (j * distanceBetwenNails);
                            // Ustaw kolor piksela na podany kolor
                            image.WritePixels(new Int32Rect(x, stala, 1, 1), new byte[] { kolor.B, kolor.G, kolor.R, kolor.A }, 4, 0);
                            j++;
                            listaPunktow.Add(new Punkt(x, stala));
                        }
                        else
                        {
                            int x = stop - (p * distanceBetwenNails);
                            image.WritePixels(new Int32Rect(x, stala, 1, 1), new byte[] { kolor.B, kolor.G, kolor.R, kolor.A }, 4, 0);
                            p++;
                            listaPunktow.Add(new Punkt(x, stala));
                        }
                    }
                    break;
                case false:
                    for (int i = 0; i < nailsCount; i++)
                    {
                        if (CzyParzysta(i))
                        {
                            int y = start + (j * distanceBetwenNails);
                            // Ustaw kolor piksela na podany kolor
                            image.WritePixels(new Int32Rect(stala, y, 1, 1), new byte[] { kolor.B, kolor.G, kolor.R, kolor.A }, 4, 0);
                            j++;
                            listaPunktow.Add(new Punkt(stala, y));
                        }
                        else
                        {
                            int y = stop - (p * distanceBetwenNails);
                            image.WritePixels(new Int32Rect(stala, y, 1, 1), new byte[] { kolor.B, kolor.G, kolor.R, kolor.A }, 4, 0);
                            p++;
                            listaPunktow.Add(new Punkt(stala, y));
                        }
                    }
               break;
            }

                
            
            return (image, listaPunktow);
        }

        static BitmapImage WriteableNaBitmapImage(WriteableBitmap image)
        {
            using (var stream = new System.IO.MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(stream);
                stream.Position = 0;

                BitmapImage modifiedBitmapImage = new BitmapImage();
                modifiedBitmapImage.BeginInit();
                modifiedBitmapImage.StreamSource = stream;
                modifiedBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                modifiedBitmapImage.EndInit();

                return modifiedBitmapImage;
            }
        }
        static WriteableBitmap BitmapImageNaWriteable(BitmapImage image)
        {
            // Konwertuj BitmapImage na WriteableBitmap
            WriteableBitmap writeableBitmap = new WriteableBitmap(image);
            return writeableBitmap;
        }
        // Klasa reprezentująca punkt
        public class Punkt
        {
            public double X { get; set; }
            public double Y { get; set; }

            public Punkt(double x, double y)
            {
                X = x;
                Y = y;
            }
        }
    }
}

