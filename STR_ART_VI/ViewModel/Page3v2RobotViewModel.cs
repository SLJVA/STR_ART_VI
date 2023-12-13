using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STR_ART_VI.View;
using System.Net.WebSockets;
using System.Threading;
using System.ComponentModel;
using System.Windows;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;


namespace STR_ART_VI.ViewModel
{
    public partial class Page3v2RobotViewModel : ObservableObject
    {
        private ClientWebSocket _clientWebSocket;
        public string GcodeToSend;
        private string _connectionStatus = "Not Connected";
        private string _receivedMessage;
        public Page3v2RobotViewModel()
        {
            ConnectCommand = new RelayCommand(Connect, CanConnect);
            MoveToSetpointCommand = new RelayCommand(MoveToSetpoint, CanMoveToSetpoint);
            AutoHomeCommand = new RelayCommand(AutoHome, CanAutoHome);
            GoToBaseCommand = new RelayCommand(GoToBase, CanGoToBase);
            ResetBasePosCommand = new RelayCommand(ResetBasePos, CanResetBasePos);
            DremelCommand = new RelayCommand(Dremel, CanDremel);
            _clientWebSocket = new ClientWebSocket();
            LoadFileAutoModeCommand = new RelayCommand(LoadFileAutoMode);
            StartProcessModeCommand = new RelayCommand(StartProcessMode);
            FirstPointRCFCommand = new RelayCommand(FirstPointRCF);
            SecondPointRCFCommand = new RelayCommand(SecondPointRCF);
            ThirdPointRCFCommand = new RelayCommand(ThirdPointRCF);
            CalculateCentreCommand = new RelayCommand(CalculateCentre);
            GenerateNailPositionCommand = new RelayCommand(GenerateNailPosition);

        }

        public IRelayCommand ConnectCommand { get; }
        public IRelayCommand MoveToSetpointCommand { get; }
        public IRelayCommand AutoHomeCommand { get; }
        public IRelayCommand GoToBaseCommand { get; }
        public IRelayCommand ResetBasePosCommand { get; }
        public IRelayCommand PlusYCommand { get; }
        public IRelayCommand DremelCommand { get; }
        public IRelayCommand LoadFileAutoModeCommand { get; }
        public IRelayCommand StartProcessModeCommand { get; }
        public IRelayCommand FirstPointRCFCommand { get; }
        public IRelayCommand SecondPointRCFCommand { get; }
        public IRelayCommand ThirdPointRCFCommand { get; }
        public IRelayCommand CalculateCentreCommand { get; }
        public IRelayCommand GenerateNailPositionCommand { get; }

        [ObservableProperty]
        private string _diameterSizeMessage = string.Empty;

        private string _fileContent;

        public string FileContent
        {
            get => _fileContent;
            set
            {
                SetProperty(ref _fileContent, value);
                LoadFileAutoModeCommand.NotifyCanExecuteChanged();
            }
        }
        //Pierwszy punkt
        private string _firstPointRCFvalue;
        public string FirstPointRCFvalue
        {
            get => _firstPointRCFvalue;
            set
            {
                SetProperty(ref _firstPointRCFvalue, value);
                FirstPointRCFCommand.NotifyCanExecuteChanged();
            }
        }
        public double firstRCFx;
        public double firstRCFy;
        private void FirstPointRCF()
        {
            FirstPointRCFvalue = ReceivedMessage;
            var result = ExtractCoordinates(FirstPointRCFvalue); // Użyj 'var' do deklaracji krotki
            firstRCFx = result.Item1; // Pobierz pierwszy element krotki
            firstRCFy = result.Item2; // Pobierz drugi element krotki
        }
        //Drugi punkt
        private string _secondPointRCFvalue;
        public string SecondPointRCFvalue
        {
            get => _secondPointRCFvalue;
            set
            {
                SetProperty(ref _secondPointRCFvalue, value);
                SecondPointRCFCommand.NotifyCanExecuteChanged();
            }
        }
        public double secondRCFx;
        public double secondRCFy;
        private void SecondPointRCF()
        {
            SecondPointRCFvalue = ReceivedMessage;
            var result = ExtractCoordinates(SecondPointRCFvalue); // Użyj 'var' do deklaracji krotki
            secondRCFx = result.Item1; // Pobierz pierwszy element krotki
            secondRCFy = result.Item2; // Pobierz drugi element krotki
        }
        //Trzeci punkt
        private string _thirdPointRCFvalue;
        public string ThirdPointRCFvalue
        {
            get => _thirdPointRCFvalue;
            set
            {
                SetProperty(ref _thirdPointRCFvalue, value);
                ThirdPointRCFCommand.NotifyCanExecuteChanged();
            }
        }
        public double thirdRCFx;
        public double thirdRCFy;
        private void ThirdPointRCF()
        {
            ThirdPointRCFvalue = ReceivedMessage;
            var result = ExtractCoordinates(ThirdPointRCFvalue); // Użyj 'var' do deklaracji krotki
            thirdRCFx = result.Item1; // Pobierz pierwszy element krotki
            thirdRCFy = result.Item2; // Pobierz drugi element krotki
        }


        //Generowanie tablicy zawierającej pozycje gwoździ (współrzędne x,y) rozmieszczonych po okręgu o znanej średnicy i pozycji środka okręgu.
        //Ilość gwoździ również jest znana

        private string _pointsXYPosArray;

        public string PointsXYPosArray
        {
            get => _pointsXYPosArray;
            set
            {
                SetProperty(ref _pointsXYPosArray, value);
            }
        }
        public double centerX;
        public double centerY;

        public void GenerateNailPosition()
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
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < pointsX.Length; i++)
            {
                string pointString = string.Format("x{0:0.00},y{1:0.00};", pointsX[i], pointsY[i]);
                stringBuilder.Append(pointString);
            }

            PointsXYPosArray = stringBuilder.ToString();



        }
        //Wyznaczenie środka okręgu
        public (double centerX, double centerY, double radius) CalculateCircleCenter(double firstX, double firstY, double secondX, double secondY, double thirdX, double thirdY)
        {

            NumberFormatInfo setPrecision = new NumberFormatInfo();
            setPrecision.NumberDecimalDigits = 2;

            double x12 = firstX - secondX;
            double x13 = firstX - thirdX;

            double y12 = firstY - secondY;
            double y13 = firstY - thirdY;

            double y31 = thirdY - firstY;
            double y21 = secondY - firstY;

            double x31 = thirdX - firstX;
            double x21 = secondX - firstX;

            double sx13 = (double)(Math.Pow(firstX, 2) -
                            Math.Pow(thirdX, 2));

            double sy13 = (double)(Math.Pow(firstY, 2) -
                            Math.Pow(thirdY, 2));

            double sx21 = (double)(Math.Pow(secondX, 2) -
                            Math.Pow(firstX, 2));

            double sy21 = (double)(Math.Pow(secondY, 2) -
                            Math.Pow(firstY, 2));

            double f = ((sx13) * (x12)
                    + (sy13) * (x12)
                    + (sx21) * (x13)
                    + (sy21) * (x13))
                    / (2 * ((y31) * (x12) - (y21) * (x13)));
            double g = ((sx13) * (y12)
                    + (sy13) * (y12)
                    + (sx21) * (y13)
                    + (sy21) * (y13))
                    / (2 * ((x31) * (y12) - (x21) * (y13)));

            double c = -(double)Math.Pow(firstX, 2) - (double)Math.Pow(firstY, 2) -
                                        2 * g * firstX - 2 * f * firstY;
            double centerX = -g;
            double centerY = -f;





            // Promień okręgu to odległość od środka środkowego okręgu do dowolnego z punktów trójkąta.
            double radius = Math.Sqrt(Math.Pow(centerX - firstX, 2) + Math.Pow(centerY - firstY, 2));

            return (centerX, centerY, radius);
        }
        private void CalculateCentre()
        {

            var result = CalculateCircleCenter(firstRCFx, firstRCFy, secondRCFx, secondRCFy, thirdRCFx, thirdRCFy);

            CalculatedCenterRCFvalue = $"Środek okręgu: X={result.Item1:F2}, Y={result.Item2:F2}, Promień={result.Item3:F2}";
            centerX = result.Item1;
            centerY = result.Item2;

        }
        private string _calculatedCenterRCFvalue;
        public string CalculatedCenterRCFvalue
        {
            get => _calculatedCenterRCFvalue;
            set
            {
                SetProperty(ref _calculatedCenterRCFvalue, value);
                CalculateCentreCommand.NotifyCanExecuteChanged();
            }
        }

        // auto mode DOKOŃCZYĆ
        async private void LoadFileAutoMode()
        {
            //Wczytaj plik i podziel na części
            FileContent = OpenAndReadTextFile();
            string[] stringArray = FileContent.Split(',');

            foreach (string command in stringArray)
            {
                await SendCommandAsync(command);

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

        private async void Connect()
        {
            try
            {
                Uri uri = new Uri($"ws://192.168.5.106:8080");
                await _clientWebSocket.ConnectAsync(uri, CancellationToken.None);
                ConnectionStatus = "Connected";

                await StartReceivingAsync();
            }
            catch (Exception ex)
            {
                ConnectionStatus = "Connection Failed";

            }
        }

        private bool CanConnect()
        {
            return ConnectionStatus != "Connected";
        }

        private async void StartProcessMode()
        {
            await SendCommandAsync(FileContent);
            await SendCommandAsync("on");
        }

        private async void Dremel()
        {
            await SendCommandAsync("dremel");
        }

        private bool CanDremel()
        {
            return true;
        }


        public string ConnectionStatus
        {
            get => _connectionStatus;
            set
            {
                SetProperty(ref _connectionStatus, value);
                ConnectCommand.NotifyCanExecuteChanged();
            }
        }

        public async Task SendCommandAsync(string command)
        {
            ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(command));
            await _clientWebSocket.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async void MoveToSetpoint()
        {
            string combinedPosition;

            if (absX != null && absY != null && absZ != null)
            {
                combinedPosition = $"G1 X{absX} Y{absY} Z{absZ} F{Speed}";
            }
            else
            {
                combinedPosition = $"G1 X0 Y0 Z0 F0";
            }
            await SendCommandAsync(combinedPosition);

            await SendCommandAsync("on");
        }

        private bool CanMoveToSetpoint()
        {
            return true;
        }

        private async void AutoHome()
        {
            await SendCommandAsync("home");
        }

        private bool CanAutoHome()
        {
            return true;
        }

        private async void GoToBase()
        {
            await SendCommandAsync("base");
        }

        private bool CanGoToBase()
        {
            return true;
        }

        private async void ResetBasePos()
        {
            await SendCommandAsync("resetbase");
        }

        private bool CanResetBasePos()
        {
            return true;
        }


        public new event PropertyChangedEventHandler? PropertyChanged;

        public string SpeedSetpointMessage { get => Speed; set { Speed = value; OnPropertyChanged(); } }
        public string Speed = "";

        public string AbsXPosMessage { get => absX; set { absX = value; OnPropertyChanged(); } }
        public string absX = "";

        public string AbsYPosMessage { get => absY; set { absY = value; OnPropertyChanged(); } }
        public string absY = "";

        public string AbsZPosMessage { get => absZ; set { absZ = value; OnPropertyChanged(); } }
        public string absZ = "";



        public async Task StartReceivingAsync()
        {
            byte[] buffer = new byte[1024]; // Rozmiar bufora do odbierania wiadomości
            while (_clientWebSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    HandleReceivedMessage(receivedMessage);
                }
            }
            ConnectionStatus = "Connection Closed";
        }

        private void HandleReceivedMessage(string message)
        {
            ReceivedMessage = message;
            Console.WriteLine("Received message: " + message);
        }
        public string ReceivedMessage
        {
            get { return _receivedMessage; }
            set
            {
                _receivedMessage = value;
                OnPropertyChanged(nameof(ReceivedMessage));
            }
        }


        static (double x, double y) ExtractCoordinates(string input)
        {
            double x = 0.0;
            double y = 0.0;

            // Użyj wyrażenia regularnego do znalezienia wartości X i Y.
            Match matchX = Regex.Match(input, @"X=(-?\d+\.\d+)");
            Match matchY = Regex.Match(input, @"Y=(-?\d+\.\d+)");

            if (matchX.Success)
            {
                // Jeśli znaleziono wartość X, parsuj ją na double.
                x = double.Parse(matchX.Groups[1].Value, CultureInfo.InvariantCulture);
            }

            if (matchY.Success)
            {
                // Jeśli znaleziono wartość Y, parsuj ją na double.
                y = double.Parse(matchY.Groups[1].Value, CultureInfo.InvariantCulture);
            }

            return (x, y);
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