using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

class Program
{
    static TcpListener listener;
    static List<TcpClient> clients = new List<TcpClient>();
    static char[,] gameBoard = new char[3, 3];
    static int currentPlayer = 0;

    static void Main(string[] args)
    {
        listener = new TcpListener(IPAddress.Any, 12345);
        listener.Start();
        Console.WriteLine("Server started on port 12345.");
        AcceptClients();
        Console.ReadLine();
    }

    static async void AcceptClients()
    {
        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            clients.Add(client);
            Console.WriteLine($"Client connected from {client.Client.RemoteEndPoint}.");
            if (clients.Count == 2)
            {
                Console.WriteLine("Both players connected. Starting game.");
                StartGame();
                // Starting a separate task for each client to handle their communication
                Task.Run(() => HandleClient(client, clients.IndexOf(client)));
            }
        }
    }

    static void StartGame()
    {
        currentPlayer = 0;
        Array.Clear(gameBoard, 0, gameBoard.Length);
        Broadcast("Game started.");
        UpdateClientsAfterMove();
        Console.WriteLine("Game initialized and clients notified.");
    }

    static async Task HandleClient(TcpClient client, int playerIndex)
    {
        var stream = client.GetStream();
        var reader = new StreamReader(stream);
        var writer = new StreamWriter(stream) { AutoFlush = true };

        while (true)
        {
            string move = await reader.ReadLineAsync();
            if (move == null) break;
            Console.WriteLine($"Received move from player {playerIndex + 1}: {move}");
            if (currentPlayer == playerIndex && ProcessMove(move, playerIndex))
            {
                if (CheckForWin())
                {
                    Broadcast($"Player {playerIndex + 1} wins!");
                    Console.WriteLine($"Game over. Player {playerIndex + 1} wins.");
                    break;
                }
                if (IsDraw())
                {
                    Broadcast("Draw!");
                    Console.WriteLine("Game over with a draw.");
                    break;
                }
                currentPlayer = 1 - currentPlayer;
                UpdateClientsAfterMove();
            }
        }
        Console.WriteLine($"Ending session for player {playerIndex + 1}");
    }



    static bool ProcessMove(string move, int playerIndex)
    {
        string[] parts = move.Split(',');
        if (parts.Length == 2 && int.TryParse(parts[0], out int row) && int.TryParse(parts[1], out int col))
        {
            if (gameBoard[row, col] == '\0')
            {
                gameBoard[row, col] = playerIndex == 0 ? 'X' : 'O';
                Console.WriteLine($"Move made by player {playerIndex + 1}: {row},{col},{gameBoard[row, col]}");
                Broadcast($"Move made,{row},{col},{gameBoard[row, col]}");
                return true;
            }
            else
            {
                Console.WriteLine($"Invalid move by player {playerIndex + 1} at {row},{col}. Cell already occupied.");
            }
        }
        else
        {
            Console.WriteLine($"Invalid move format by player {playerIndex + 1}: {move}");
        }
        return false;
    }

    static void UpdateClientsAfterMove()
    {
        for (int i = 0; i < clients.Count; i++)
        {
            if (i == currentPlayer)
            {
                SendToClient(clients[i], "Your turn," + (currentPlayer == 0 ? "X" : "O"));
                Console.WriteLine($"Notifying player {i + 1} of their turn.");
            }
            else
            {
                SendToClient(clients[i], "Waiting for other player," + (currentPlayer == 0 ? "X" : "O"));
                Console.WriteLine($"Notifying player {i + 1} to wait.");
            }
        }
    }


    static bool IsDraw()
    {
        bool isDraw = !gameBoard.Cast<char>().Any(c => c == '\0');
        if (isDraw) Console.WriteLine("Board full, game is a draw.");
        return isDraw;
    }

    static bool CheckForWin()
    {
        for (int i = 0; i < 3; i++)
        {
            if ((gameBoard[i, 0] == gameBoard[i, 1] && gameBoard[i, 1] == gameBoard[i, 2] && gameBoard[i, 0] != '\0') ||
                (gameBoard[0, i] == gameBoard[1, i] && gameBoard[1, i] == gameBoard[2, i] && gameBoard[0, i] != '\0'))
            {
                return true;
            }
        }

        if ((gameBoard[0, 0] == gameBoard[1, 1] && gameBoard[1, 1] == gameBoard[2, 2] && gameBoard[0, 0] != '\0') ||
            (gameBoard[0, 2] == gameBoard[1, 1] && gameBoard[1, 1] == gameBoard[2, 0] && gameBoard[0, 2] != '\0'))
        {
            return true;
        }

        return false;
    }

    static void Broadcast(string message)
    {
        Console.WriteLine($"Broadcasting: {message}");
        foreach (var client in clients)
        {
            SendToClient(client, message);
        }
    }

    static void SendToClient(TcpClient client, string message)
    {
        StreamWriter writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
        writer.WriteLine(message);
        Console.WriteLine($"Sent to {client.Client.RemoteEndPoint}: {message}");
    }
}
