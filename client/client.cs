using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

class RemoteControlClient
{
    private const string ServerAddress = "127.0.0.1";
    private const int Port = 9000;

    static void Main()
    {
        try
        {
            TcpClient client = new TcpClient(ServerAddress, Port);
            Console.WriteLine("Đã kết nối với server.");

            NetworkStream stream = client.GetStream();

            while (true)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    Console.WriteLine("Server đã ngắt kết nối.");
                    break;
                }

                string command = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Lệnh nhận được từ server: {command}");

                if (command == "Shutdown")
                {
                    ExecuteShutdown();
                }
                else if (command == "Restart")
                {
                    ExecuteRestart();
                }
                else if (command == "SendFileRequest")
                {
                    ReceiveFile(stream);
                }
                else
                {
                    Console.WriteLine("Lệnh không được nhận diện.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi kết nối server: {ex.Message}");
        }
    }

    // Tắt máy khách
    private static void ExecuteShutdown()
    {
        Console.WriteLine("Đang tắt máy khách...");
        Process.Start("shutdown", "/s /t 0");
    }

    // Khởi động lại máy khách
    private static void ExecuteRestart()
    {
        Console.WriteLine("Đang khởi động lại máy khách...");
        Process.Start("shutdown", "/r /t 0");
    }

    // Nhận file từ server
    private static void ReceiveFile(NetworkStream stream)
    {
        try
        {
            string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ReceivedFile.txt");

            using (FileStream fs = new FileStream(savePath, FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = new byte[4096];
                int bytesRead;

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fs.Write(buffer, 0, bytesRead);
                }

                Console.WriteLine($"File đã được lưu ở {savePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Nhận file thất bại: {ex.Message}");
        }
    }
}
