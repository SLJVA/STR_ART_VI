using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using STR_ART_VI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Net.WebSockets;
using System.Threading;
using System.ComponentModel;


namespace STR_ART_VI.ViewModel
{
    public partial class Page3RobotViewModel : INotifyPropertyChanged
    {
        private ClientWebSocket _clientWebSocket;
        private string _receivedMessage;


        private string _connectionStatus = "Not Connected";



        public Page3RobotViewModel()
        {

            _clientWebSocket = new ClientWebSocket();
            
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


        public string ConnectionStatus
        {
            get { return _connectionStatus; }
            set
            {
                _connectionStatus = value;
                OnPropertyChanged(nameof(ConnectionStatus));
            }
        }

        public async Task ConnectWebSocketAsync()
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

        public async Task SendCommandAsync(string command)
        {
            ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(command));
            await _clientWebSocket.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
        }



        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
            // Obsługa otrzymanej wiadomości od ESP32
            // W tym miejscu możesz przetworzyć wiadomość i zaktualizować stan interfejsu, jeśli to konieczne.
            // Przykład:

            ReceivedMessage = message;
            Console.WriteLine("Received message: " + message);
        }

    }
}
