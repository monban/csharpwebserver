using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    public static void Main()
    {
        IPAddress localAddr = IPAddress.Parse("127.0.0.1");
        var endpoint = new IPEndPoint(localAddr, 13000);
        var s = new HTTP.Server(endpoint);
        s.Start();
    }
}

namespace HTTP
{
    class Server
    {
        public Server(IPEndPoint endpoint)
        {
            listener = new TcpListener(endpoint.Address, endpoint.Port);
        }

        public void Start()
        {
            try
            {
                listener.Start();
                Console.WriteLine($"Listening on {listener.LocalEndpoint}");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
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
        TcpListener listener;
    }

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
            Action<string> addline = (line) =>
            {
                s.Append(line);
                s.Append("\r\n");
            };
            addline($"HTTP/1.1 {responseCode}");
            addline("Content-Type: text/plain; charset=UTF-8");
            addline($"Content-Length: {body.Length}");
            addline("");
            addline(body);
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
