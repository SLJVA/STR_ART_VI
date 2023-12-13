using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Serilog;

namespace STR_ART_VI.Model
{
    public class MathStrArt
    {
        // Przekształcenie bitmapy w skali szarości do jednowymiarowej tablicy wartości kolejnych pikseli (kolejność wierszowa)
        public static byte[] GrayscaleBitmapToFlatArray(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            byte[] pixelArray = new byte[width * height];

            int index = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);

                    // Konwertuj kolor piksela na wartość w odcieniach szarości
                    byte grayscaleValue = (byte)(0.299 * pixelColor.R + 0.587 * pixelColor.G + 0.114 * pixelColor.B);

                    // Zapisz wartość w tablicy
                    pixelArray[index++] = grayscaleValue;
                }
            }

            return pixelArray;

        }

        // Narysowanie linii pomiędzy 2 punktami i zwrócenie bitmapy
        public static Bitmap DrawLineOnBitmap(Point startPoint, Point endPoint, int width, int height)
        {
            // Utwórz nową bitmapę o określonym rozmiarze i białym tle
            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // Ustaw tło na białe
                g.Clear(Color.White);

                // Ustaw kolor linii (czarny w tym przypadku)
                Pen pen = new Pen(Color.Black);

                // Narysuj linię pomiędzy dwoma punktami
                g.DrawLine(pen, startPoint, endPoint);
            }
            return bitmap;
        }
        public static List<Point> PlacePointsOnRectangle(int width, int height, int numberOfPoints)
        {
            if (numberOfPoints < 4 || numberOfPoints % 4 != 0)
            {
                throw new ArgumentException("Number of points should be at least 4 and a multiple of 4.");
            }

            List<Point> points = new List<Point>();
            width -= 1;
            height -= 1;

            // Dodaj punkty na wierzchołkach prostokąta
            points.Add(new Point(0, 0));
            points.Add(new Point(width, 0));
            points.Add(new Point(0, height));
            points.Add(new Point(width, height));

            // Dodaj punkty równomiernie na każdym z czterech boków prostokąta
            int pointsPerSide = numberOfPoints / 4;

            
            if (width-2 >= pointsPerSide)
            {
                
                for (int i = 0; i < pointsPerSide/2; i++)
                {
                    double distance = ((double)width / (pointsPerSide + 1)) * (i + 1);
                    int pointFromLeft = (int)Math.Round(distance);
                    int pointFromRight = width - pointFromLeft;
                    Log.Information($"Obliczono dystans pomiędzy punktami na szerokości double: {distance} i int {pointFromLeft}");
                    points.Add(new Point(pointFromLeft, 0));
                    points.Add(new Point(pointFromRight, 0));
                    points.Add(new Point(pointFromLeft, height));
                    points.Add(new Point(pointFromRight, height));
                }
            }
            else
            {
                System.Windows.MessageBox.Show($"Liczba punktów przypadająca na jeden bok ({pointsPerSide}) jest mniejsza od szerokości ({width}) boku z wykluczniem początku i końca boku");
            }
            if (height - 2 >= pointsPerSide)
            {
                for (int i = 0; i < pointsPerSide / 2; i++)
                {
                    double distance = ((double)height / (pointsPerSide + 1)) * (i + 1);
                    int pointFromTop = (int)Math.Round(distance);
                    int pointFromBottom = height - pointFromTop;
                    Log.Information($"Obliczono dystans pomiędzy punktami na szerokości double: {distance} i int {pointFromTop}");
                    points.Add(new Point(0, pointFromTop));
                    points.Add(new Point(0, pointFromBottom));
                    points.Add(new Point(width, pointFromTop));
                    points.Add(new Point(width, pointFromBottom));
                }
            }
            else
            {
                System.Windows.MessageBox.Show($"Liczba punktów przypadająca na jeden bok ({pointsPerSide}) jest mniejsza od wysokości ({height}) boku z wykluczniem początku i końca boku");
            }





            return points;
        }



        public static Bitmap DrawLineOnBitmap(Bitmap existingBitmap, Point startPoint, Point endPoint)
        {
            // Upewnij się, że bitmapa istnieje
            if (existingBitmap == null)
            {
                throw new ArgumentNullException(nameof(existingBitmap));
            }

            using (Graphics g = Graphics.FromImage(existingBitmap))
            {
                // Ustaw kolor linii (czarny w tym przypadku)
                using (Pen pen = new Pen(Color.Black))
                {
                    // Narysuj linię pomiędzy dwoma punktami
                    g.DrawLine(pen, startPoint, endPoint);
                }
            }

            // Zwróć zmodyfikowaną bitmapę
            return existingBitmap;
        }

    }
}
