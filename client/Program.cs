using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

class Client
{
    static void Main(string[] args)
    {
        TcpClient client = new TcpClient("localhost", 8888);
        NetworkStream stream = client.GetStream();

        try
        {
            while (true)
            {
                Console.WriteLine("Enter action (1 - get a file, 2 - save a file, 3 - delete a file):");
                string action = Console.ReadLine();

                if (action.ToLower() == "exit")
                {
                    break;
                }

                byte[] actionBytes = Encoding.UTF8.GetBytes(action);
                stream.Write(actionBytes, 0, actionBytes.Length);

                if (action == "2")
                {
                    Console.WriteLine("Enter name of the file:");
                    string filename = Console.ReadLine();
                    Console.WriteLine("Enter name of the file to be saved on server:");
                    string saveFilename = Console.ReadLine();

                    byte[] filenameBytes = Encoding.UTF8.GetBytes(filename);
                    byte[] saveFilenameBytes = Encoding.UTF8.GetBytes(saveFilename);

                    stream.Write(filenameBytes, 0, filenameBytes.Length);
                    stream.Write(saveFilenameBytes, 0, saveFilenameBytes.Length);
                }
                else
                {
                    Console.WriteLine("Do you want to {0} the file by name or by id (1 - name, 2 - id):", action.ToLower() == "delete" ? "delete" : "get");
                    string option = Console.ReadLine();

                    byte[] optionBytes = Encoding.UTF8.GetBytes(option);
                    stream.Write(optionBytes, 0, optionBytes.Length);

                    if (option == "2")
                    {
                        Console.WriteLine("Enter id:");
                        string fileId = Console.ReadLine();
                        byte[] fileIdBytes = Encoding.UTF8.GetBytes(fileId);
                        stream.Write(fileIdBytes, 0, fileIdBytes.Length);
                    }
                    else
                    {
                        Console.WriteLine("Enter name of the file:");
                        string filename = Console.ReadLine();
                        byte[] filenameBytes = Encoding.UTF8.GetBytes(filename);
                        stream.Write(filenameBytes, 0, filenameBytes.Length);
                    }
                }

                byte[] responseBytes = new byte[1024];
                int bytesRead = stream.Read(responseBytes, 0, responseBytes.Length);
                string response = Encoding.UTF8.GetString(responseBytes, 0, bytesRead);
                Console.WriteLine(response);
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
}
