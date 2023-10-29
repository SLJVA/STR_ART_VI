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
            for (int i = 0; i < iloscOdcieni; i++)
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

                //for (int i = 0; i < pikselePrzetworzone.Length; i++)
                //{
                //    if (pikselePrzetworzone[i] <= 31)
                //    {
                //        pikselePrzetworzone[i] = 31;
                //    }
                //    else if (pikselePrzetworzone[i] <= 63)
                //    {
                //        pikselePrzetworzone[i] = 63;
                //    }
                //    else if (pikselePrzetworzone[i] <= 95)
                //    {
                //        pikselePrzetworzone[i] = 95;
                //    }
                //    else if (pikselePrzetworzone[i] <= 127)
                //    {
                //        pikselePrzetworzone[i] = 127;
                //    }
                //    else if (pikselePrzetworzone[i] <= 159)
                //    {
                //        pikselePrzetworzone[i] = 159;
                //    }
                //    else if (pikselePrzetworzone[i] <= 191)
                //    {
                //        pikselePrzetworzone[i] = 191;
                //    }
                //    else if (pikselePrzetworzone[i] <= 223)
                //    {
                //        pikselePrzetworzone[i] = 223;
                //    }
                //    else
                //    {
                //        pikselePrzetworzone[i] = 255;
                //    }
                //}



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



    }

}

