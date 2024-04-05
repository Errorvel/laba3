using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Server
{
    private const int BUFFER_SIZE = 2048;
    private static readonly byte[] buffer = new byte[BUFFER_SIZE];
    private static readonly string serverPath = @"..\..\server\data\";

    public static void StartServer()
    {
        try
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 8888);
            tcpListener.Start();
            Console.WriteLine("Server started. Waiting for connections...");

            while (true)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                Console.WriteLine("Client connected!");

                HandleClient(client);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private static void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();

        try
        {
            while (true)
            {
                int receivedBytes = stream.Read(buffer, 0, buffer.Length);
                if (receivedBytes == 0)
                {
                    Console.WriteLine("Client disconnected.");
                    break;
                }

                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);

                // Process received command
                ProcessCommand(stream, receivedMessage);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        finally
        {
            stream.Close();
            client.Close();
        }
    }

    private static void ProcessCommand(NetworkStream stream, string message)
    {
        try
        {
            string[] parts = message.Split(' ');
            if (parts.Length < 2)
            {
                SendResponse(stream, "Invalid command format.");
                return;
            }

            string command = parts[0];
            string filename = parts[1];

            switch (command)
            {
                case "PUT":
                    HandlePut(stream, filename);
                    break;
                case "GET":
                    HandleGet(stream, filename);
                    break;
                case "DELETE":
                    HandleDelete(stream, filename);
                    break;
                default:
                    SendResponse(stream, "Unknown command.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            SendResponse(stream, "Error processing command.");
        }
    }

    private static void SendResponse(NetworkStream stream, string response)
    {
        byte[] responseData = Encoding.UTF8.GetBytes(response);
        stream.Write(responseData, 0, responseData.Length);
    }

    private static void HandlePut(NetworkStream stream, string filename)
    {
        string filePath = Path.Combine(serverPath, filename);

        if (File.Exists(filePath))
        {
            SendResponse(stream, "File already exists.");
            return;
        }

        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        {
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fs.Write(buffer, 0, bytesRead);
            }
        }

        SendResponse(stream, "File uploaded successfully.");
    }

    private static void HandleGet(NetworkStream stream, string filename)
    {
        string filePath = Path.Combine(serverPath, filename);

        if (!File.Exists(filePath))
        {
            SendResponse(stream, "File not found.");
            return;
        }

        byte[] fileData = File.ReadAllBytes(filePath);
        stream.Write(fileData, 0, fileData.Length);
    }

    private static void HandleDelete(NetworkStream stream, string filename)
    {
        string filePath = Path.Combine(serverPath, filename);

        if (!File.Exists(filePath))
        {
            SendResponse(stream, "File not found.");
            return;
        }

        File.Delete(filePath);
        SendResponse(stream, "File deleted successfully.");
    }
}

class Program
{
    static void Main(string[] args)
    {
        Server.StartServer();
    }
}
