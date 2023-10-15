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

        }

        public IRelayCommand ConnectCommand { get; }
        public IRelayCommand MoveToSetpointCommand { get; }
        public IRelayCommand AutoHomeCommand { get; }
        public IRelayCommand GoToBaseCommand { get; }
        public IRelayCommand ResetBasePosCommand { get; }
        public IRelayCommand PlusYCommand { get; }
        public IRelayCommand DremelCommand { get; }


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

    }
}