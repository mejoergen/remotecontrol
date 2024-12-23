using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class RemoteControlServer
{
    private const int Port = 9000;

    static void Main()
    {
        TcpListener server = null;

        try
        {
            server = new TcpListener(IPAddress.Any, Port);
            server.Start();

            Console.WriteLine($"Server đang lắng nghe trên cổng {Port}...");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Máy khách đã kết nối.");

                NetworkStream stream = client.GetStream();

                bool isConnected = true;
                while (isConnected)
                {
                    ShowMenu();

                    string choice = Console.ReadLine();

                    if (choice == "1")
                    {
                        SendCommand(stream, "Shutdown");
                        Console.WriteLine("Lệnh tắt máy khách đã được gửi.");
                    }
                    else if (choice == "2")
                    {
                        SendCommand(stream, "Restart");
                        Console.WriteLine("Lệnh khởi động lại máy khách đã được gửi.");
                    }
                    else if (choice == "3")
                    {
                        Console.Write("Nhập đường dẫn file: ");
                        string filePath = Console.ReadLine();

                        if (File.Exists(filePath))
                        {
                            SendFile(stream, filePath);
                        }
                        else
                        {
                            SendCommand(stream, "File not found.");
                            Console.WriteLine("File không tồn tại.");
                        }
                    }
                    else if (choice == "4")
                    {
                        SendCommand(stream, "Disconnect");
                        Console.WriteLine("Đã ngắt kết nối với máy khách.");
                        isConnected = false;
                    }
                    else
                    {
                        Console.WriteLine("Lựa chọn không hợp lệ.");
                    }
                }

                client.Close();
                Console.WriteLine("Máy khách đã ngắt kết nối.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi server: {ex.Message}");
        }
        finally
        {
            server?.Stop();
        }
    }

    private static void ShowMenu()
    {
        Console.WriteLine("\n=== Menu Server ===");
        Console.WriteLine("1. Tắt máy khách");
        Console.WriteLine("2. Khởi động lại máy khách");
        Console.WriteLine("3. Gửi file đến máy khách");
        Console.WriteLine("4. Ngắt kết nối máy khách");
        Console.Write("===================");
        Console.Write("Chọn một lựa chọn: ");
    }

    private static void SendCommand(NetworkStream stream, string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Gửi lệnh thất bại: {ex.Message}");
        }
    }

    private static void SendFile(NetworkStream stream, string filePath)
    {
        try
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            SendCommand(stream, "FileTransferStart");

            Thread.Sleep(100);  // Delay để máy khách sẵn sàng nhận file

            stream.Write(fileData, 0, fileData.Length);
            Console.WriteLine($"File {filePath} đã được gửi.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Gửi file thất bại: {ex.Message}");
        }
    }
}
