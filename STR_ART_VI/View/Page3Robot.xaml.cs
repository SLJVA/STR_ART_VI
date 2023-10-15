using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using STR_ART_VI.ViewModel;
using System.Net.WebSockets;
using System.Threading;


namespace STR_ART_VI.View
{
    public partial class Page3Robot : UserControl
    {
        private Page3RobotViewModel _viewModel;
        private ClientWebSocket _clientWebSocket;

        public Page3Robot()
        {
            InitializeComponent();
            _viewModel = new Page3RobotViewModel();
            this.DataContext = _viewModel;
            _clientWebSocket = new ClientWebSocket();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.ConnectWebSocketAsync();
        }

        private async void TurnOnButton_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.SendCommandAsync("on");
        }

        private async void TurnOffButton_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.SendCommandAsync("off");
        }

        private async void BaseButton_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.SendCommandAsync("base");
        }
        private async void ResetBaseButton_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.SendCommandAsync("resetbase");
        }

        private async void DremelButton_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.SendCommandAsync("dremel");
        }

        private async void Homing_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.SendCommandAsync("home");
        }

        private async void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text;
            await _viewModel.SendCommandAsync(message);
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string messageToSend = MessageTextBox.Text;
            await SendWebSocketMessage(messageToSend);
        }

        private async Task SendWebSocketMessage(string message)
        {
            if (_clientWebSocket.State != WebSocketState.Open)
            {
                Uri uri = new Uri("ws://192.168.5.106:8080");
                await _clientWebSocket.ConnectAsync(uri, CancellationToken.None);
            }

            ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            await _clientWebSocket.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private void MessageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Foreground = Brushes.Gray;
                textBox.Text = "Enter message to send";
            }
            else
            {
                textBox.Foreground = Brushes.Black;
            }
        }

    }
}
