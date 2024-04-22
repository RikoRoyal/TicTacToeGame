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
using System.Windows.Shapes;
using System.Windows.Threading;
using TicTacToe;

namespace TicTacToeGame
{
    public partial class GameWindow : Window
    {
        private int playerWins = 0;
        private int computerWins = 0;
        private bool isPlayerTurn = true;
        private Button[,] buttons;
        private DispatcherTimer timer;
        private TimeSpan time;

        public GameWindow()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            buttons = new Button[,] {
                { button0_0, button0_1, button0_2 },
                { button1_0, button1_1, button1_2 },
                { button2_0, button2_1, button2_2 }
            };

            timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                txtTimer.Text = "Timer: " + time.ToString("mm':'ss");
                time = time.Add(TimeSpan.FromSeconds(1));
            }, Application.Current.Dispatcher);
            timer.Start();
        }

        private void ResetGame()
        {
            foreach (Button btn in buttons.Cast<Button>())
            {
                btn.Content = null;
                btn.IsEnabled = true;
            }
            time = TimeSpan.Zero;
            timer.Stop();
            timer.Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button.Content == null)
            {
                button.Content = isPlayerTurn ? "X" : "O";
                if (CheckForVictory((string)button.Content))
                {
                    MessageBox.Show($"Победил {(isPlayerTurn ? "игрок" : "компьютер")}!");
                    UpdateScore(isPlayerTurn);
                }
                else if (!AnyMovesLeft())
                {
                    MessageBox.Show("Ничья!");
                    UpdateScore(false);
                }
                else
                {
                    isPlayerTurn = !isPlayerTurn;
                    if (!isPlayerTurn)
                    {
                        ComputerMove();
                    }
                }
            }
        }

        private bool CheckForVictory(string player)
        {
            for (int i = 0; i < 3; i++)
            {
                if ((buttons[i, 0].Content as string == player && buttons[i, 1].Content as string == player && buttons[i, 2].Content as string == player) ||
                    (buttons[0, i].Content as string == player && buttons[1, i].Content as string == player && buttons[2, i].Content as string == player))
                    return true;
            }
            if ((buttons[0, 0].Content as string == player && buttons[1, 1].Content as string == player && buttons[2, 2].Content as string == player) ||
                (buttons[0, 2].Content as string == player && buttons[1, 1].Content as string == player && buttons[2, 0].Content as string == player))
                return true;

            return false;
        }

        private bool AnyMovesLeft()
        {
            return buttons.Cast<Button>().Any(b => b.Content == null);
        }

        private void ComputerMove()
        {
            var emptyButtons = buttons.Cast<Button>().Where(b => b.Content == null).ToArray();
            if (emptyButtons.Length > 0)
            {
                var random = new Random();
                var button = emptyButtons[random.Next(emptyButtons.Length)];
                button.Content = "O";
                if (CheckForVictory("O"))
                {
                    MessageBox.Show("Победил компьютер!");
                    DisableButtons();
                }
                else if (!AnyMovesLeft())
                {
                    MessageBox.Show("Ничья!");
                    DisableButtons();
                }
                else
                {
                    isPlayerTurn = true;
                }
            }
        }

        private void DisableButtons()
        {
            foreach (var button in buttons)
            {
                button.IsEnabled = false;
            }
        }
        private void UpdateScore(bool playerWon)
        {
            if (playerWon)
                playerWins++;
            else
                computerWins++;
            txtScore.Text = $"Счет: Игрок {playerWins} - Компьютер {computerWins}";
            ResetGame();
        }

        private void GoBackToMenu(object sender, RoutedEventArgs e)
        {
            this.Close();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
