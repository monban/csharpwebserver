﻿using System.Net;
using System.Net.Sockets;
using System.Text;

class MyTcpListener
{
    public static void Main()
    {
        // Set the TcpListener on port 13000.
        Int32 port = 13000;
        IPAddress localAddr = IPAddress.Parse("127.0.0.1");
        var server = new TcpListener(localAddr, port);

        try
        {
            server.Start();
            Console.WriteLine($"Listening on {localAddr}:{port}");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Task.Run(() => ServeHTTP(client));
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        }
    }

    public static void ServeHTTP(TcpClient client)
    {
        // Wait for the client to finish sending its
        // request (we don't care what it is)
        var stream = client.GetStream();
        int lfs = 0;
        while (lfs < 4)
        {
            var b = stream.ReadByte();
            if (b == 10 || b == 13)
            {
                lfs++;
            }
            else
            {
                lfs = 0;
            }
        }
        Thread.Sleep(10);
        byte[] rBody = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\nContent-Type: text/plain; charset=UTF-8\n\nHello, world.\n");
        stream.Write(rBody, 0, rBody.Length);

        // Do I need to dispose the client, or does it happen
        // automatically when it goes out scope?
        client.Dispose();
    }
}

