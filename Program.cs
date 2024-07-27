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
        try
        {
            s.Start();
        }
        catch (Exception e)
        {
            Console.WriteLine("Something terrible has happened: {0}", e);
        }
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
                Console.WriteLine("Listening on {0}", listener.LocalEndpoint);

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Task.Run(() => ServeHTTP(client));
                }
            }
            catch
            {
                Console.WriteLine("unable to start server");
                throw;
            }
        }

        public static void ServeHTTP(TcpClient client)
        {
            try
            {
                // Wait for the client to finish sending its
                // request (we don't care what it is)
                var stream = client.GetStream();
                Request req = new Request(stream);
                Console.WriteLine(req);

                var res = new HTTP.Response("Hello, world");
                byte[] rBody = res.GetBytes();
                stream.Write(rBody, 0, rBody.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                // Do I need to dispose the client, or does it happen
                // automatically when it goes out scope?
                client.Dispose();
            }
        }
        TcpListener listener;
    }

    class Request
    {
        public Request()
        {
            uri = "";
            headers = new Dictionary<string, string> { };
        }

        public Request(Stream s)
        {
            var reader = new StreamReader(s);

            string? firstLine = reader.ReadLine();
            if (firstLine is not string)
            {
                throw new Exception("unable to read from socket");
            }
            Console.WriteLine(firstLine);
            string[] fields = firstLine.Split(' ');
            if (fields.Length < 3)
            {
                throw new Exception($"invalid HTTP request: {firstLine}");
            }
            Console.WriteLine(fields);
            if (Stuff.methodFromString(fields[0]) is Method meth)
            {
                method = meth;
            }
            uri = fields[1];

            headers = new Dictionary<string, string> { };
            string? line = reader.ReadLine();
            while (line is string && line.Length > 0)
            {
                string[] l = line.Split(": ");
                if (l.Length == 2)
                {
                    headers.Add(l[0], l[1]);
                }
                line = reader.ReadLine();
            }
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.AppendLine($"{method} {uri}");
            foreach (var header in headers)
            {
                s.AppendLine(header.ToString());
            }
            return s.ToString();
        }
        public Method method;
        public string uri;
        public Dictionary<string, string> headers;
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
            // Use our own AppendLine to make sure we get CRLF
            Action<string> AppendLine = (line) =>
            {
                s.Append(line);
                s.Append("\r\n");
            };
            AppendLine($"HTTP/1.1 {responseCode}");
            AppendLine("Content-Type: text/plain; charset=UTF-8");
            AppendLine($"Content-Length: {body.Length}");
            AppendLine("");
            AppendLine(body);
            return Encoding.ASCII.GetBytes(s.ToString());
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

    class Stuff // Come up with a better name later
    {
        public static Method? methodFromString(string s)
        {
            switch (s.ToUpper())
            {
                case "GET":
                    return Method.GET;
                case "POST":
                    return Method.POST;
                case "PUT":
                    return Method.PUT;
                case "PATCH":
                    return Method.PATCH;
                case "DELETE":
                    return Method.DELETE;
                default:
                    return null;
            }
        }

    }
}
