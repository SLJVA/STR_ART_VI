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
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace STR_ART_VI.Model
{
    public class StrArtUtilities
    {
        public static Bitmap DrawLine(Bitmap obraz, int x1, int y1, int x2, int y2, System.Drawing.Color kolorek, int alpha)
        {
            using (Graphics g = Graphics.FromImage(obraz))
            {
                using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(alpha, kolorek)))
                {
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }
            return obraz;
        }

        public static Bitmap DrawLines(Bitmap obraz, List<ImageUtilities.Punkt> punkciki, int[] kolejneIndeksy, System.Drawing.Color kolorek, int alpha)
        {
            for (int i = 1; i < kolejneIndeksy.Length; i++)
            {
                int x1 = 0;
                int y1 = 0;
                int x2 = 0;
                int y2 = 0;

                int pkt1 = kolejneIndeksy[i - 1];
                int pkt2 = kolejneIndeksy[i];

                x1 = (int)punkciki[pkt1].X;
                y1 = (int)punkciki[pkt1].Y;
                x2 = (int)punkciki[pkt2].X;
                y2 = (int)punkciki[pkt2].Y;

                using (Graphics g = Graphics.FromImage(obraz))
                {
                    using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(alpha, kolorek)))
                    {
                        g.DrawLine(pen, x1, y1, x2, y2);
                    }
                }
            }
            

            return obraz;
        }


        public static Bitmap OpenImageToBitmap()
        {      
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp|All files (*.*)|*.*";
            Bitmap bitmap = null;
            if (openFileDialog.ShowDialog() == true)
            {
                string imagePath = openFileDialog.FileName;

                // Przykład użycia System.Drawing.Bitmap
                bitmap = LoadBitmap(imagePath);
                
            }
            return bitmap;
        }

        private static Bitmap LoadBitmap(string imagePath)
        {
            try
            {
                return new Bitmap(imagePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas wczytywania obrazu: {ex.Message}");
                return null;
            }
        }
        public static Bitmap ConvertToGrayscale(Bitmap original)
        {
            // Stwórz nowy obraz w skali szarości o takich samych wymiarach
            Bitmap grayscaleBitmap = new Bitmap(original.Width, original.Height);

            for (int x = 0; x < original.Width; x++)
            {
                for (int y = 0; y < original.Height; y++)
                {
                    // Pobierz kolor piksela z oryginalnego obrazu
                    System.Drawing.Color originalColor = original.GetPixel(x, y);

                    // Oblicz nowy kolor w skali szarości
                    int grayscaleValue = (int)(0.299 * originalColor.R + 0.587 * originalColor.G + 0.114 * originalColor.B);

                    // Ustaw nowy kolor w obrazie w skali szarości
                    System.Drawing.Color newColor = System.Drawing.Color.FromArgb(grayscaleValue, grayscaleValue, grayscaleValue);
                    grayscaleBitmap.SetPixel(x, y, newColor);
                }
            }

            return grayscaleBitmap;
        }

        public static void SaveBitmapToPNG(Bitmap bitmap)
        {
            if (bitmap != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PNG files (*.png)|*.png";
                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;

                    try
                    {
                        bitmap.Save(filePath, ImageFormat.Png);
                        MessageBox.Show($"Obraz został zapisany jako: {filePath}", "Zapisano");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Błąd podczas zapisywania obrazu: {ex.Message}", "Błąd zapisu");
                    }
                }
            }
        }

        public static void SaveBitmapToPngWithPath(Bitmap bitmap, string filePath)
        {
            if (bitmap != null && !string.IsNullOrEmpty(filePath))
            {
                try
                {
                    bitmap.Save(filePath, ImageFormat.Png);
                   
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas zapisywania obrazu: {ex.Message}", "Błąd zapisu");
                }
            }
        }

        public static (double averageBrightness, double brightPixelsFactor) CalculateAverageBrightness2(Bitmap bitmap, Bitmap blackWhiteBitmap, int x1, int y1, int x2, int y2, int plyWidth, int plyHeight, int step)
        {
            int width = plyWidth;
            int height = plyHeight;

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int stride = bitmapData.Stride;
            IntPtr scan0 = bitmapData.Scan0;

            byte[] pixels = new byte[height * stride];
            Marshal.Copy(scan0, pixels, 0, pixels.Length);

            bitmap.UnlockBits(bitmapData);

            BitmapData blackWhiteBitmapData = blackWhiteBitmap.LockBits(new Rectangle(0, 0, blackWhiteBitmap.Width, blackWhiteBitmap.Height), ImageLockMode.ReadWrite, blackWhiteBitmap.PixelFormat);
            int blackWhiteStride = blackWhiteBitmapData.Stride;
            IntPtr blackWhiteScan0 = blackWhiteBitmapData.Scan0;

            byte[] blackWhitePixels = new byte[blackWhiteBitmap.Height * blackWhiteStride];
            Marshal.Copy(blackWhiteScan0, blackWhitePixels, 0, blackWhitePixels.Length);

            blackWhiteBitmap.UnlockBits(blackWhiteBitmapData);

            int totalBrightness = 0;
            int pixelCount = 0;

            int aboveThresholdCount = 0;
            int totalPixelCount = 0;

            double m = (double)(y2 - y1) / (x2 - x1);
            double b = y1 - m * x1;

            for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x += step)
            {
                int y = (int)(m * x + b);

                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    int pixelIndex = y * stride + x * 4;
                    int brightness = (pixels[pixelIndex] + pixels[pixelIndex + 1] + pixels[pixelIndex + 2]) / 3;

                    totalBrightness += brightness;
                    pixelCount++;

                    totalPixelCount++;

                    if (brightness > 240 && IsBlackPixel(blackWhitePixels, x, y, blackWhiteStride))
                    {
                        aboveThresholdCount++;
                    }
                }
            }

            double brightPixelsFactor = (double)aboveThresholdCount / (double)totalPixelCount;
            double averageBrightness = (double)totalBrightness / pixelCount;
            return (averageBrightness, brightPixelsFactor);
        }

        private static bool IsBlackPixel(byte[] blackWhitePixels, int x, int y, int stride)
        {
            if (x >= 0 && x < stride / 4 && y >= 0 && y < blackWhitePixels.Length / stride)
            {
                int pixelIndex = y * stride + x * 4;
                return blackWhitePixels[pixelIndex] == 0; // Assuming 0 represents black in the second bitmap
            }
            return false;
        }


        public static double CalculateAverageBrightness(Bitmap bitmap, int x1, int y1, int x2, int y2, int step)
        {
            // Sprawdź, czy punkty są w granicach obrazu
            if (x1 < 0 || x1 > bitmap.Width || y1 < 0 || y1 > bitmap.Height ||
                x2 < 0 || x2 > bitmap.Width || y2 < 0 || y2 > bitmap.Height ||
                step <= 0)
            {
                throw new ArgumentException("Nieprawidłowe współrzędne punktów lub nieprawidłowy krok.");
            }

            int totalBrightness = 0;
            int pixelCount = 0;

            // Iteruj pomiędzy punktem 1 a punktem 2 z zadanym krokiem
            for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x += step)
            {
                for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y += step)
                {
                    System.Drawing.Color pixelColor = bitmap.GetPixel(x, y);
                    // Oblicz jasność piksela i dodaj do sumy
                    int brightness = (int)(0.299 * pixelColor.R + 0.587 * pixelColor.G + 0.114 * pixelColor.B);
                    totalBrightness += brightness;
                    pixelCount++;
                }
            }

            // Oblicz średnią jasność
            double averageBrightness = (double)totalBrightness / pixelCount;
            return averageBrightness;
        }


        public class Punkt
        {
            public double X { get; set; }
            public double Y { get; set; }

            public Punkt(double x, double y)
            {
                X = x;
                Y = y;
            }
            public Punkt DeepCopy()
            {
                return new Punkt(X, Y);
            }
        }
    }
}
