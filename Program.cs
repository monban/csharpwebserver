using System.Net;
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
        byte[] buf = new byte[128];
        int n = stream.Read(buf);
        var req = Encoding.UTF8.GetString(buf);

        var res = new HTTP.Response("Hello, world");
        byte[] rBody = res.GetBytes();
        stream.Write(rBody, 0, rBody.Length);

        // Do I need to dispose the client, or does it happen
        // automatically when it goes out scope?
        client.Dispose();
    }
}

namespace HTTP
{
    class Request
    {
        Request()
        {
            uri = "";
            headers = new Dictionary<string, string> { };
        }
        Method method;
        string uri;
        Dictionary<string, string> headers;
    }

    class Response
    {
        public Response(string body)
        {
            this.responseCode = 200;
            headers = new Dictionary<string, string> { };
            this.body = body;
        }

        public byte[] GetBytes()
        {
            var s = new StringBuilder();
            s.Append($"HTTP/1.1 {responseCode}\r\n");
            s.Append("Content-Type: text/plain; charset=UTF-8\r\n");
            s.Append($"Content-Length: {body.Length}\r\n");
            s.Append($"\r\n");
            s.Append(body);
            return Encoding.UTF8.GetBytes(s.ToString());
        }

        int responseCode;
        Dictionary<string, string> headers;
        string body;
    }

    enum Method
    {
        GET,
        POST,
        PUT,
        PATCH,
        DELETE,
    }
}
