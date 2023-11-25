using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;

namespace STR_ART_VI.Model
{
    public static class GCodeDrillHole
    {
        public static string gCodeDrill(double ringRadius, int ringWidth, int nailsCount, double centreX, double centreY, double minZaxis, double plyThickness, double offsetZ)
        {
            string gCodeDrillHole = "";

            // Obliczenie nowego promienia okręgu uwzględniając szerokość pierścienia
            double nailCircleRadius = ringRadius - (ringWidth / 2);

            // Obliczenie pozycji gwoździ po okręgu
            double[] pointsX;
            double[] pointsY;
            CalculateCirclePoints(centreX, centreY, nailCircleRadius, nailsCount, out pointsX, out pointsY);

            // Generowania G-CODE do wiercenia otworów
            double safeHeight = 0 + minZaxis + plyThickness + offsetZ;
            string safeHeightString = safeHeight.ToString();
            double drillDepth = minZaxis + 1;
            string drillDepthString = drillDepth.ToString();

            // Zadeklarowanie prędkości wiercenia i prędkości przejazdu
            int drillSpeed = 10;
            string drillSpeedString = drillSpeed.ToString();
            int movementSpeed = 50;
            string movementSpeedString = movementSpeed.ToString();

            //Wyjazd na odpowiednią wysokość
            gCodeDrillHole += $"G1 X0 Y0 Z{safeHeight} F{movementSpeed},";

            // Utwórz kulturę, która używa kropki jako separatora dziesiętnego
            CultureInfo culture = CultureInfo.InvariantCulture;

            for (int i = 0; i < pointsX.Length; i++)
            {
                gCodeDrillHole += $"G1 X{pointsX[i].ToString(culture)} Y{pointsY[i].ToString(culture)} Z{safeHeight} F{movementSpeed},";
                gCodeDrillHole += "dremel,";
                gCodeDrillHole += $"G1 X{pointsX[i].ToString(culture)} Y{pointsY[i].ToString(culture)} Z{drillDepth} F{drillSpeed},";
                gCodeDrillHole += $"G1 X{pointsX[i].ToString(culture)} Y{pointsY[i].ToString(culture)} Z{safeHeight} F{drillSpeed},";
                gCodeDrillHole += "dremel,";
            }

            return gCodeDrillHole;       
        }

        public static void GcodeNailsFromPointList(List<ImageUtilities.Punkt> listaPunktow,int movementVel, int drillingVel, string fileName, int constructionOffset, int plywoodHeight, int movementHeight)
        {
            // Nazwa pliku tekstowego
            fileName += "_gcode.txt";
            // Uzyskaj ścieżkę do folderu ImageCollection wewnątrz folderu projektu
            string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            string imageCollectionDirectory = Path.Combine(projectDirectory, "ImageCollection");
            // Sprawdź, czy folder ImageCollection istnieje, jeśli nie, utwórz go
            if (!Directory.Exists(imageCollectionDirectory))
            {
                Directory.CreateDirectory(imageCollectionDirectory);
            }
            
            // Obliczenie wysokości przejazdowej
            int safeHeight = constructionOffset + plywoodHeight + movementHeight;

            // Obliczenie wysokości do której będziemy wiercić
            int drillHeight = constructionOffset;

            // Podzielenie wartości z listy przez 10
            foreach (ImageUtilities.Punkt punkt in listaPunktow)
            {
                punkt.X /= 10;
                punkt.Y /= 10;
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

                        //Wyjechanie na wysokość przejazdową
                        writer.Write($"G1 X0 Y0 Z{safeHeight} F{movementVel},");

                        foreach (ImageUtilities.Punkt punkt in listaPunktow)
                        {
                            string punktX = (punkt.X).ToString(culture);
                            string punktY = (punkt.Y).ToString(culture);
                            writer.Write($"G1 X{punktX} Y{punktY} Z{safeHeight} F{movementVel},dremel,");
                            writer.Write($"G1 X{punktX} Y{punktY} Z{drillHeight} F{drillingVel},");
                            writer.Write($"G1 X{punktX} Y{punktY} Z{safeHeight} F{drillingVel},dremel,");
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


        public static void CalculateCirclePoints(double centerX, double centerY, double radius, int numberOfPoints, out double[] pointsX, out double[] pointsY)
        {
            pointsX = new double[numberOfPoints];
            pointsY = new double[numberOfPoints];

            for (int i = 0; i < numberOfPoints; i++)
            {
                double angle = (2 * Math.PI * i) / numberOfPoints;
                double x = centerX + (radius * Math.Cos(angle));
                double y = centerY + (radius * Math.Sin(angle));

                pointsX[i] = Math.Round(x, 2);
                pointsY[i] = Math.Round(y, 2);
            }
        }

    }
}
