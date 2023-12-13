using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STR_ART_VI.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Serilog;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace STR_ART_VI.ViewModel
{
    public partial class NailQuadGenViewModel : ObservableObject 
    {
        public NailQuadGenViewModel()
        {
            DateTime now = DateTime.Now;
            // Utworzenie formatu daty i czasu, na przykład: rok_miesiac_dzien_godzina_minuta_sekunda
            string dateFormat = now.ToString("yyyy_MM_dd_HH_mm_ss");
            // Konfiguracja Serilog do wyświetlania w konsoli
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()  // Opcjonalne: logowanie także do konsoli
            // Pobranie aktualnej daty i czasu
            .WriteTo.File($@"D:\Csharp_projekty\STR_ART_VI\STR_ART_VI\log_{dateFormat}.txt")  
            .CreateLogger();
            Log.Information("Aplikacja została uruchomiona.");

            LoadFilePaths();
            _selectedImageFile = _imageFiles.FirstOrDefault();
            GenBlankNailCircleImageCommand = new RelayCommand(GenBlankNailCircle);
            

        }

        // Tworzenie listy punktów
        List<ImageUtilities.Punkt> gwozdzie= new List<ImageUtilities.Punkt>();
        int ImageViewCounter = 0;
        int PlywoodWidth = 0;
        int PlywoodHeight = 0;
        BitmapImage BlankView;
        System.Drawing.Bitmap LoadedImage;
        Bitmap StringArtImage;
        BitmapImage DistributedNailsView;
        // Podaj literę dysku (np. "C:\\") lub ścieżkę do dowolnego katalogu
        string drivePath = "D:\\";

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

        public class PointWithExclusions
        {
            public System.Drawing.Point Point { get; set; }
            public List<int> ExcludedIndexes { get; set; }

            public PointWithExclusions(System.Drawing.Point point, List<int> excludedIndexes)
            {
                Point = point;
                ExcludedIndexes = excludedIndexes ?? new List<int>();
            }
        }

        [RelayCommand]
        public void MatrixTest()
        {
            /*
            Log.Information("Rozpocząto MatrixTest");
            
            // Dane wejściowe
            var a1 = new DenseVector(new double[] { 1, 0, 0, 1, 0, 0, 0, 0, 0 });
            var a2 = new DenseVector(new double[] { 1, 0, 0, 1, 0, 0, 1, 0, 0 });
            var a3 = new DenseVector(new double[] { 1, 0, 0, 1, 1, 0, 0, 1, 0 });
            var a4 = new DenseVector(new double[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 });
            var a5 = new DenseVector(new double[] { 1, 1, 0, 0, 1, 1, 0, 0, 0 });
            var a6 = new DenseVector(new double[] { 1, 1, 1, 0, 0, 0, 0, 0, 0 });
            var a7 = new DenseVector(new double[] { 1, 1, 0, 0, 0, 0, 0, 0, 0 });
            var a8 = new DenseVector(new double[] { 0, 0, 0, 1, 0, 0, 1, 0, 0 });
            var a9 = new DenseVector(new double[] { 0, 0, 0, 1, 0, 0, 1, 1, 0 });
            var a10 = new DenseVector(new double[] { 0, 0, 0, 1, 1, 0, 0, 1, 1 });
            var a11 = new DenseVector(new double[] { 0, 0, 0, 1, 1, 1, 0, 0, 0 });
            var a12 = new DenseVector(new double[] { 0, 1, 1, 1, 1, 0, 0, 0, 0 });
            var a13 = new DenseVector(new double[] { 1, 1, 0, 1, 0, 0, 0, 0, 0 });
            var a14 = new DenseVector(new double[] { 0, 0, 0, 0, 0, 0, 1, 1, 0 });
            var a15 = new DenseVector(new double[] { 0, 0, 0, 0, 0, 0, 1, 1, 1 });
            var a16 = new DenseVector(new double[] { 0, 0, 0, 0, 1, 1, 1, 1, 0 });
            var a17 = new DenseVector(new double[] { 0, 0, 1, 0, 1, 0, 10, 0, 0 });
            var a18 = new DenseVector(new double[] { 0, 1, 0, 1, 1, 0, 1, 0, 0 });
            var a19 = new DenseVector(new double[] { 0, 0, 0, 0, 0, 0, 0, 1, 1 });
            var a20 = new DenseVector(new double[] { 0, 0, 0, 0, 0, 1, 0, 1, 1 });
            var a21 = new DenseVector(new double[] { 0, 0, 1, 0, 1, 1, 0, 1, 0 });
            var a22 = new DenseVector(new double[] { 0, 1, 0, 0, 1, 0, 0, 1, 0 });
            var a23 = new DenseVector(new double[] { 0, 0, 0, 0, 0, 1, 0, 0, 1 });
            var a24 = new DenseVector(new double[] { 0, 0, 1, 0, 0, 1, 0, 0, 1 });
            var a25 = new DenseVector(new double[] { 0, 1, 0, 0, 1, 1, 0, 0, 1 });
            var a26 = new DenseVector(new double[] { 0, 0, 1, 0, 0, 1, 0, 0, 0 });
            var a27 = new DenseVector(new double[] { 0, 1, 1, 0, 0, 10, 0, 0, 0 });
            var a28 = new DenseVector(new double[] { 0, 1, 1, 0, 0, 0, 0, 0, 0 });


            var A = DenseMatrix.OfColumns(new[] { a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16, a17, a18, a19, a20, a21, a22, a23, a24, a25, a26, a27, a28 });

            var b = new DenseVector(new double[] { 0, 0, 0, 0.1, 0.1, 0.1, 0, 0, 0 });

            // Krok 1: Transpozycja macierzy A
            var ATransposed = A.Transpose();

            // Krok 2: Iloczyn macierzy A i A^T
            var AAT = A * ATransposed;

            // Sprawdzenie, czy macierz AAT jest osobliwa
            if (AAT.Determinant() == 0)
            {
                Console.WriteLine("Macierz (A * A^T) jest osobliwa, nie ma odwrotności.");
                return;
            }

            // Krok 3: Odwrócenie macierzy AAT
            var AATInverse = AAT.Inverse();

            // Krok 4: Obliczenie X
            var X = ATransposed * AATInverse * b;        

            // Wyświetlenie wyniku
            Log.Information("Wynik: " + X);
            


            // Otwórz obraz i wpisz do Bitmap
            Bitmap obraz = StrArtUtilities.OpenImageToBitmap();
            byte[] pixelArray = MathStrArt.GrayscaleBitmapToFlatArray(obraz);
            // Przykładowe użycie wartości piksela w lewym górnym rogu
            byte pixelValue = pixelArray[2];
            byte pixelValue1 = pixelArray[1];
            Log.Information($"Wartość piksela w prawym górnym rogu: {pixelValue}");
            Log.Information($"Wartość piksela na środku u góry: {pixelValue1}");
           */

            // Konwersja listy punktów na listę punktów z wykluczeniami
            //List<PointWithExclusions> pointsWithExclusions = gwozdzie.Select((p, index) =>
            //    new PointWithExclusions(new System.Drawing.Point((int)p.X, (int)p.Y), new List<int> { /* indeksy punktów wykluczanych dla punktu */ })
            //).ToList();
            
            
            List<System.Drawing.Point> points = MathStrArt.PlacePointsOnRectangle(32, 32, 40);
            // Konwersja listy punktów na listę punktów z wykluczeniami
            List<PointWithExclusions> pointsWithExclusions = points.Select((p, index) =>
                new PointWithExclusions(new System.Drawing.Point((int)p.X, (int)p.Y), new List<int> { /* indeksy punktów wykluczanych dla punktu */ })
            ).ToList();

            // to do usunięcia
            foreach (var pointWithExclusion in pointsWithExclusions)
            {
                Log.Information($"Point: ({pointWithExclusion.Point.X}, {pointWithExclusion.Point.Y}), Exclusions: [{string.Join(", ", pointWithExclusion.ExcludedIndexes)}]");
            }

            // Stwórz wektor obrazu wejściowego (b)
            // Otwórz obraz i wpisz do Bitmap
            Bitmap orginalnyObraz = StrArtUtilities.OpenImageToBitmap();

            // Konwertuj do skali szarości
            orginalnyObraz = StrArtUtilities.ConvertToGrayscale(orginalnyObraz);

            // Przekształcenie bitmapy w skali szarości do jednowymiarowej tablicy wartości kolejnych pikseli (kolejność wierszowa) 
            byte[] bPixelArray = MathStrArt.GrayscaleBitmapToFlatArray(orginalnyObraz);

            // Tworzenie obiektu DenseVector na podstawie jednowymiarowej tablicy pixelArray
            var b = new DenseVector(bPixelArray.Select(p => (double)p).ToArray());
            
            
            // Stwórz listę punktów (x, y) i przypisz im przykładowe wartości

            //List<PointWithExclusions> pointsWithExclusions = new List<PointWithExclusions>

            //{
            //    new PointWithExclusions(new System.Drawing.Point(0, 0), new List<int> { /* indeksy punktów wykluczanych dla punktu (0, 0) */ }),
            //    new PointWithExclusions(new System.Drawing.Point(0, 4), new List<int> { /* indeksy punktów wykluczanych dla punktu (0, 1) */ }),
            //    new PointWithExclusions(new System.Drawing.Point(4, 0), new List<int> { /* indeksy punktów wykluczanych dla punktu (0, 2) */ }),
            //    new PointWithExclusions(new System.Drawing.Point(4, 4), new List<int> { /* indeksy punktów wykluczanych dla punktu (1, 0) */ }),

            //};

            

            // Dodaj do każdego punkty wykluczony indeks powodujący wykluczenie samego siebie?
            

            int lastLastPointIndex = 0;
            int lastPointIndex = 0;
            int widthImage = orginalnyObraz.Width;
            int heightImage = orginalnyObraz.Height;
            Log.Information("Odczytano rozmiar orginalnego obrazu: " + widthImage + ", " + heightImage + ".");
            // Utwórz biały obraz na którym będzie powstawał STRING ART
            Bitmap StringArt = ImageUtilities.CreateBitmap(widthImage, heightImage, System.Drawing.Color.White);
            for (int y = 0; y<6; y++)
            {
                // Lista do przechowywania obiektów ConnVector
                List<ConnVector> connVectors = new List<ConnVector>();

                for (int i = 0; i < pointsWithExclusions.Count; i++)
                {
                    
                    if (!pointsWithExclusions[lastPointIndex].ExcludedIndexes.Contains(i) && !(lastPointIndex==i))
                    {
                        Log.Information("Tworzenie połączenia pomiędzy punktem o indeksie: " + lastPointIndex + "a punktem o indeksie: " + i);
                        // Jeśli punkt nie należy do listy to utwórz bitmapę z linią pomiędzy poprzednim punktem a tym świeżo znalezionym
                        Bitmap obrazek = MathStrArt.DrawLineOnBitmap(pointsWithExclusions[lastPointIndex].Point, pointsWithExclusions[i].Point, widthImage, heightImage);
                        // Przekształcenie bitmapy w skali szarości do jednowymiarowej tablicy wartości kolejnych pikseli (kolejność wierszowa) 
                        byte[] pixelArray = MathStrArt.GrayscaleBitmapToFlatArray(obrazek);

                        // Zapisanie utworzonej bitmapy do plików - to potem do usunięcia
                        StrArtUtilities.SaveBitmapToPngWithPath(obrazek, $"D:\\Csharp_projekty\\STR_ART_VI\\STR_ART_VI\\bin\\bitmapy\\{y}bitmapa{i}.png");

                        // Tworzenie obiektu DenseVector na podstawie jednowymiarowej tablicy pixelArray
                        var denseVector = new DenseVector(pixelArray.Select(p => (double)p).ToArray());

                        for (int x = 0; x < pixelArray.Length; x++)
                        {
                            Log.Information("Tablica puntków dla połączenia z indeksem " + i + ": " + pixelArray[x] + " ");
                        }

                        // Dodanie obiektu ConnVector do listy wektorów połączeń
                        connVectors.Add(new ConnVector(pointsWithExclusions[lastPointIndex].Point, pointsWithExclusions[i].Point, denseVector, i));
                    
                    }
                    else
                    {
                        Log.Information("Punkt należy do tablicy excludedPoints");
                    }

                }

                foreach (var connVector in connVectors)
                {
                    Log.Information($"End Point X: {connVector.GetEndPointX()}");
                    Log.Information($"End Point Y: {connVector.GetEndPointY()}");
                }

                // Utwórz listę wektorów DenseVector
                List<DenseVector> denseVectorsList = connVectors.Select(connVector => connVector.Vector).ToList();

                // Utwórz macierz z listy wektorów
                DenseMatrix A = DenseMatrix.OfColumnVectors(denseVectorsList);

                // Wyświetlenie zawartości macierzy A
                Log.Information("Zawartość macierzy A:");
                for (int i = 0; i < A.ColumnCount; i++)
                {
                    for (int j = 0; j < A.RowCount; j++)
                    {
                        Log.Information(A[j, i] + " ");
                    }
                }

                // Wektor "b" -> Wektor orginalnego obrazu ------------- 0 to czarny, 255 to bialy
                // var b = new DenseVector(new double[] { 0, 0, 0, 0, 0, 255, 255, 255, 255, 0, 255, 255, 255, 255, 0, 255, 255, 255, 255, 0, 0, 0, 0, 0, 0 });
                // Krok 1: Transpozycja macierzy A
                var ATransposed = A.Transpose();

                // Krok 2: Iloczyn macierzy A i A^T
                var AAT = A * ATransposed;

                Vector<double> X;
                // Sprawdzenie, czy macierz AAT jest osobliwa
                if (AAT.Determinant() == 0)
                {
                    Log.Information("Macierz (A * A^T) jest osobliwa, nie ma odwrotności.");
                    // Krok 2: Iloczyn macierzy A i A^T
                    var ATA = ATransposed * A;
                    // Krok 3: Odwrócenie macierzy AAT
                    var ATAInverse = ATA.Inverse();
                    // Krok 4: Obliczenie X
                    X = ATAInverse * ATransposed * b;
                }
                else
                {
                    // Krok 3: Odwrócenie macierzy AAT
                    var AATInverse = AAT.Inverse();

                    // Krok 4: Obliczenie X
                    X = ATransposed * AATInverse * b;
                }
            

                // Wyświetlenie wyniku
                Log.Information("Wynik: " + X);

                // Znalezienie największej wartości
                double maxValue = X.Max();

                // Znalezienie indeksu największej wartości
                int maxIndex = X.MaximumIndex();

                Log.Information("Znaleziono największą wartość w wektorze X: " + maxValue + " o indeksie: " + maxIndex);

                int StartPointX = connVectors[maxIndex].GetStartPointX();
                int StartPointY = connVectors[maxIndex].GetStartPointY();
                int EndPointX = connVectors[maxIndex].GetEndPointX();
                int EndPointY = connVectors[maxIndex].GetEndPointY();

                Log.Information("Współrzędne punktu początkowego znalezionego połączenia X: " + StartPointX + ", Y: " + StartPointY + ", końcowego X: " + EndPointX + ", Y: " + EndPointY); ;

                System.Drawing.Point StartPointToDraw = connVectors[maxIndex].GetStartPoint();
                System.Drawing.Point EndPointToDraw = connVectors[maxIndex].GetEndPoint();
                // Jeśli punkt nie należy do listy to utwórz bitmapę z linią pomiędzy poprzednim punktem a tym świeżo znalezionym
                //Bitmap obrazek = MathStrArt.DrawLineOnBitmap(pointsList[lastPointIndex], pointsList[i], widthImage, heightImage);
                StringArt = MathStrArt.DrawLineOnBitmap(StringArt, StartPointToDraw, EndPointToDraw);
                // Zapisanie utworzonej bitmapy do plików - to potem do usunięcia
                StrArtUtilities.SaveBitmapToPngWithPath(StringArt, $"D:\\Csharp_projekty\\STR_ART_VI\\STR_ART_VI\\bin\\bitmapy\\stringart{y}.png");
                lastLastPointIndex = lastPointIndex;
                lastPointIndex = connVectors[maxIndex].GetPointIndex();
                Log.Information("Nowy lastPointIndex: " + lastPointIndex);
                pointsWithExclusions[lastLastPointIndex].ExcludedIndexes.Add(lastPointIndex);
                Log.Information("Dopisano do punktu o indeksie: " + lastLastPointIndex + " nowy wykluczony punkt o indeksie: " + lastPointIndex);
                pointsWithExclusions[lastPointIndex].ExcludedIndexes.Add(lastLastPointIndex);
                Log.Information("Dopisano do punktu o indeksie: " + lastPointIndex + " nowy wykluczony punkt o indeksie: " + lastLastPointIndex);

                // w tym miejscu musi nastąpić "osłabienie" orginalnego obrazu o wybrany wektor narysowany na obrazie strinart
                // mamy orginalny obraz w postaci wektoru b
                // potrzebujemy obraz z dorysowaną linią również w postaci wektoru
                DenseVector lineVector = connVectors[maxIndex].GetVector();
                // teraz trzeba ten wektor odwrócić to znaczy jak coś było 0 to musi być 255 itd
                // Odwracanie wartości wektora
                for (int i = 0; i < lineVector.Count; i++)
                {
                    lineVector[i] = 255 - lineVector[i];
                }

                // Sprawdź, czy oba wektory mają taki sam rozmiar
                DenseVector resultVector;
                if (b.Count == lineVector.Count)
                {
                    // Odjęcie wartości wektora B od wartości wektora A
                    resultVector = b + lineVector;

                    // Ograniczenie wartości wektora do zakresu od 0 do 255
                    for (int i = 0; i < resultVector.Count; i++)
                    {
                        resultVector[i] = (int)Math.Clamp(resultVector[i], 0, 255);
                    }
                }
                else
                {
                    // Wektory mają różne rozmiary, przypisz pusty wektor do resultVector
                    resultVector = new DenseVector(b.Count);
                    // Wektory mają różne rozmiary, obsłuż błąd
                    Log.Information("Wektory mają różne rozmiary!");
                }
                // Wypisanie zawartości wektora do konsoli
                Log.Information("Zawartość wektora b:");

                foreach (var value in b)
                {
                    Log.Information(value + " ");
                }
                // Wypisanie zawartości wektora do konsoli
                Log.Information("Zawartość wektora odnalezionego:");

                foreach (var value in lineVector)
                {
                    Log.Information(value + " ");
                }
                // Wypisanie zawartości wektora do konsoli
                Log.Information("Zawartość wektora po odjeciu od orginalu:");

                foreach (var value in resultVector)
                {
                    Log.Information(value + " ");
                }
                b = resultVector;
            }
        }




        public BitmapImage bitmap;
        [RelayCommand]
        public void OpenImage()
        {
            int x1 = 100;
            int y1 = 100;
            int x2 = 1000;
            int y2 = 1000;
            // Otwórz obraz i wpisz do Bitmap
            Bitmap obrazek = StrArtUtilities.OpenImageToBitmap();
            // Otwórz obraz i wpisz do Bitmap
            Bitmap blackwhitemask = StrArtUtilities.OpenImageToBitmap();
            // Zmierz wymiar dostępny pod wykonanie STRING ARTU (10px = 1mm)
            (PlywoodWidth, PlywoodHeight) = ImageUtilities.CalculateAreaSize(FirstPointX, SecondPointX, FirstPointY, SecondPointY);
            // Dotnij obraz do proporcji wymiaru pod STRING ART
            obrazek = ImageUtilities.CropImageBitmap(obrazek, PlywoodWidth, PlywoodHeight);
            // Dostosuj rozmiar obrazu do wymiaru pod STRING ART
            obrazek = ImageUtilities.ResizeImageBitmap(obrazek, PlywoodWidth, PlywoodHeight);
            // Konwertuj do skali szarości
            obrazek = StrArtUtilities.ConvertToGrayscale(obrazek);
            // Przesuń pozycje gwoździ do początku układu współrzędnych tak aby można było je nanieść na wczytany obraz
            gwozdzie = ImageUtilities.OffsetPoints(gwozdzie, FirstPointX, FirstPointY);
            // Utwórz biały obraz na którym będzie powstawał STRING ART
            StringArtImage = ImageUtilities.CreateBitmap(PlywoodWidth, PlywoodHeight, System.Drawing.Color.Azure);

            int lastIndexOfLowestBrightness = 0;
            int maxIteration = 3500;
            int[] kolejnePolaczenia = new int[maxIteration];
            kolejnePolaczenia[0] = 0;
            for (int j = 1; j < maxIteration; j++)
            {
                int indexOfLowestBrightness = -1;
                double lowestAverageBrightness = 2255;
                // Wczytanie punktu początku linii
                ImageUtilities.Punkt initialPunkt = gwozdzie[lastIndexOfLowestBrightness];
                int initialX = (int)initialPunkt.X;
                int initialY = (int)initialPunkt.Y;
                int finalX = 0;
                int finalY = 0;
                /*
                // Przeszukanie wszystkich kolejnych punktów i obliczenie średniej jasności linii pomiędzy punktem initial a final
                for (int i = 1; i < gwozdzie.Count; i++)
                {
                    ImageUtilities.Punkt finalPunkt = gwozdzie[i];
                    finalX = (int)finalPunkt.X;
                    finalY = (int)finalPunkt.Y;

                    //double averageBrightness = StrArtUtilities.CalculateAverageBrightness(obrazek, initialX, initialY, finalX, finalY, 4);
                    double averageBrightness = StrArtUtilities.CalculateAverageBrightness2(obrazek, initialX, initialY, finalX, finalY,PlywoodWidth, PlywoodHeight, 5);

                    if (averageBrightness < lowestAverageBrightness)
                    {
                        lowestAverageBrightness = averageBrightness;
                        indexOfLowestBrightness = i;
                    }
                }
                */
                // Ustaw ziarno dla generatora liczb losowych
                Random random = new Random();

                // Określ liczbę punktów do przeszukania (np. 10)
                int numberOfPointsToSearch = 10;

                // Przeszukaj losowe punkty
                for (int i = 0; i < numberOfPointsToSearch; i++)
                {
                    // Wybierz losowy indeks
                    int randomIndex = random.Next(gwozdzie.Count);

                    // Pobierz punkt o wybranym indeksie
                    ImageUtilities.Punkt finalPunkt = gwozdzie[randomIndex];
                    finalX = (int)finalPunkt.X;
                    finalY = (int)finalPunkt.Y;

                    // Oblicz średnią jasność dla wybranego punktu
                    (double averageBrightness, double whitePixelsFactor ) = StrArtUtilities.CalculateAverageBrightness2(obrazek, blackwhitemask, initialX, initialY, finalX, finalY, PlywoodWidth, PlywoodHeight, 3);
                    double averageBrightnessWithPenalty = averageBrightness + (averageBrightness * whitePixelsFactor);


                    if (averageBrightnessWithPenalty < lowestAverageBrightness)
                    {
                        lowestAverageBrightness = averageBrightness;
                        indexOfLowestBrightness = randomIndex;
                    }
                }

                kolejnePolaczenia[j] = indexOfLowestBrightness;
                Log.Information("Znaleziono połączenie: " + j + " / " + maxIteration);
                DriveInfo driveInfo = new DriveInfo(drivePath);
                long availableFreeSpaceInBytes = driveInfo.AvailableFreeSpace;
                double availableFreeSpaceInMegabytes = (double)availableFreeSpaceInBytes / (1024.0 * 1024.0);
                Log.Information($"Dysk: {driveInfo.Name}, Format: {driveInfo.DriveFormat}, Dostępne miejsce: {availableFreeSpaceInMegabytes:F2} MB");
                ImageUtilities.Punkt punktFinded = gwozdzie[indexOfLowestBrightness];
                int findedX = (int)punktFinded.X;
                int findedY = (int)punktFinded.Y;

                
                obrazek = StrArtUtilities.DrawLine(obrazek, initialX, initialY, findedX, findedY, System.Drawing.Color.White, 85);
                lastIndexOfLowestBrightness = indexOfLowestBrightness;
                Log.Information("Odjęto linię z orginalnego obrazu w połączeniu: " + j + " / " + maxIteration);
                Log.Information($"Dysk: {driveInfo.Name}, Format: {driveInfo.DriveFormat}, Dostępne miejsce: {availableFreeSpaceInMegabytes:F2} MB");
            }
            Log.Information("Rozpoczęto rysowanie czarnych linii");
            StringArtImage = StrArtUtilities.DrawLines(StringArtImage, gwozdzie, kolejnePolaczenia, System.Drawing.Color.Black, 85);
            Log.Information("Zakończono rysowanie czarnych linii");
            StrArtUtilities.SaveBitmapToPNG(StringArtImage);
            StrArtUtilities.SaveBitmapToPNG(obrazek);
        }



        [RelayCommand]
        private void ChangeView()
        {
            /*
            switch (ImageViewCounter)
            {
                case 0:
                    GeneratedBitmapImage = BlankView;
                    break;
                case 1:
                    GeneratedBitmapImage = LoadedImage;
                    break;
                case 2:
                    GeneratedBitmapImage = DistributedNailsView;
                    break;
            }
            ImageViewCounter += 1;
            if (ImageViewCounter == 3)
            {
                ImageViewCounter = 0;
            }
            */
        }

        [RelayCommand]
        public void DistributeNailsEqually()
        {
            string fileName = "punkty";
            NailsCount = int.Parse(NailsCountText);

            // Tworzenie listy punktów
            List<ImageUtilities.Punkt> listaPunktow = new List<ImageUtilities.Punkt>();

            (BitmapImage bitmapka, listaPunktow) = ImageUtilities.DodajPikseleNaBokach(BlankView, FirstPointX, FirstPointY, SecondPointX, SecondPointY, System.Drawing.Color.DarkRed, NailsCount);
            DistributedNailsView = bitmapka;
            // Tworzenie głębokiej kopii listy punktów
            List<ImageUtilities.Punkt> kopiaListaPunktow = new List<ImageUtilities.Punkt>();
            foreach (var punkt in listaPunktow)
            {
                kopiaListaPunktow.Add(punkt.DeepCopy());
            }
            gwozdzie = kopiaListaPunktow;

            GCodeDrillHole.GcodeNailsFromPointList(listaPunktow, 60, 5, fileName, 9, 4, 20);
            ImageUtilities.SaveBitmapImageToPng(DistributedNailsView, fileName);
            SystemUtilities.ZapiszListePunktowDoPliku(fileName, listaPunktow);
        }



        private void LoadFilePaths()
        {
            ImageFiles = new ObservableCollection<string>(SystemUtilities.GetImageFilesFromFolder("ImageCollection"));
        }

        [RelayCommand]
        public void GenBlankNailCircle()
        {
            string fileName = "blank";
            BlankView = ImageUtilities.GenerateBitmapImage(7200, 8200, System.Drawing.Color.LightSlateGray);
            BlankView = ImageUtilities.ZamalujObszar(BlankView, FirstPointX, FirstPointY, SecondPointX, SecondPointY, System.Drawing.Color.SandyBrown);
            


            
            ImageUtilities.SaveBitmapImageToPng(BlankView, fileName);


        }

        public class ConnVector
        {
            public System.Drawing.Point StartPoint { get; set; }
            public System.Drawing.Point EndPoint { get; set; }
            public DenseVector Vector { get; set; }
            public int PointIndex { get; set; }

            public ConnVector(System.Drawing.Point startPoint, System.Drawing.Point endPoint, DenseVector vector, int pointIndex)
            {
                StartPoint = startPoint;
                EndPoint = endPoint;
                Vector = vector;
                PointIndex = pointIndex;
            }

            // Metoda zwracająca współrzędną X punktu początkowego
            public int GetStartPointX()
            {
                int strPx = StartPoint.X;
                return strPx;
            }

            // Metoda zwracająca współrzędną Y punktu początkowego
            public int GetStartPointY()
            {
                int strPy = StartPoint.Y;
                return strPy;
            }

            // Metoda zwracająca współrzędną X punktu końcowego
            public int GetEndPointX()
            {
                return EndPoint.X;
            }

            // Metoda zwracająca współrzędną Y punktu końcowego
            public int GetEndPointY()
            {
                return EndPoint.Y;
            }

            // Metoda zwracająca pojedynczą wartość z obiektu Vector na podstawie indeksu
            public double GetVectorElement(int index)
            {
                return Vector[index];
            }

            // Metoda do odczytywania pojedynczego DenseVector z obiektu ConnVector
            public DenseVector GetVector()
            {
                return Vector;
            }

            public System.Drawing.Point GetStartPoint()
            {
                return StartPoint;
            }

            public System.Drawing.Point GetEndPoint()
            {
                return EndPoint;
            }

            public int GetPointIndex()
            { 
                return PointIndex;
            }
        }
    }
}
