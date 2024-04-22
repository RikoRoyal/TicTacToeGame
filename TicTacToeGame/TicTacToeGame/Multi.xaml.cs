using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TicTacToeGame
{
    public partial class Multi : Window
    {
        TcpClient client;
        StreamReader reader;
        StreamWriter writer;
        bool isMyTurn = false;

        public Multi()
        {
            InitializeComponent();
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            client = new TcpClient();
            client.BeginConnect("localhost", 12345, new AsyncCallback(ConnectCallback), client);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                client.EndConnect(ar);
                if (client.Connected)
                {
                    var stream = client.GetStream();
                    reader = new StreamReader(stream);
                    writer = new StreamWriter(stream) { AutoFlush = true };
                    Dispatcher.Invoke(() => MessageBox.Show("Connected to server"));
                    ReadMessages();
                }
                else
                {
                    MessageBox.Show("Failed to connect to server.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection error: " + ex.Message);
            }
        }

        private async void ReadMessages()
        {
            try
            {
                while (true)
                {
                    string message = await reader.ReadLineAsync();
                    if (message != null)
                        Dispatcher.Invoke(() => ProcessServerMessage(message));
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => MessageBox.Show("Failed to read from server: " + ex.Message));
            }
        }


        private void ProcessServerMessage(string message)
        {
            var parts = message.Split(',');
            switch (parts[0])
            {
                case "Your turn":
                    isMyTurn = true;
                    Dispatcher.Invoke(() => txtTimer.Text = "Your turn! You are " + parts[1]);
                    break;
                case "Move made":
                    Dispatcher.Invoke(() => {
                        if (parts.Length == 4)
                        {
                            int row = int.Parse(parts[1]);
                            int col = int.Parse(parts[2]);
                            string symbol = parts[3];
                            UpdateBoard(row, col, symbol);
                            txtTimer.Text = "Your turn!";
                            isMyTurn = true;
                        }
                    });
                    break;
                case "Winner":
                case "Draw":
                    Dispatcher.Invoke(() => {
                        MessageBox.Show(message);
                        ResetGame();
                    });
                    break;
                default:
                    Dispatcher.Invoke(() => txtTimer.Text = "Waiting for other player...");
                    isMyTurn = false;
                    break;
            }
        }

        private void UpdateBoard(int row, int col, string symbol)
        {
            Button button = FindButtonByCoordinates(row, col);
            if (button != null)
            {
                Dispatcher.Invoke(() => {
                    button.Content = symbol;
                    txtTimer.Text = "Waiting for other player...";
                });
            }
        }

        private Button FindButtonByCoordinates(int row, int col)
        {
            string buttonName = $"button{row}_{col}";
            return this.FindName(buttonName) as Button;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!isMyTurn) return;
            Button button = sender as Button;
            if (string.IsNullOrEmpty(button.Content as string))
            {
                string[] parts = button.Name.Substring(6).Split('_');
                if (parts.Length == 2 && int.TryParse(parts[0], out int row) && int.TryParse(parts[1], out int col))
                {
                    string playerSymbol = isMyTurn ? "X" : "O";
                    button.Content = playerSymbol;
                    try
                    {
                        writer.WriteLine($"{row},{col}");
                        isMyTurn = false;
                        txtTimer.Text = "Waiting for other player...";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to send move to server: " + ex.Message);
                    }
                }
            }
        }

        private void ResetGame()
        {
            foreach (var row in Enumerable.Range(0, 3))
            {
                foreach (var col in Enumerable.Range(0, 3))
                {
                    var buttonName = $"button{row}_{col}";
                    Button button = (Button)this.FindName(buttonName);
                    button.Content = null;
                }
            }
            isMyTurn = false;
            txtTimer.Text = "Waiting for other player...";
        }

        private void GoBackToMenu(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
