using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HTTP
{
    public class Server
    {
        public Server(IPEndPoint endpoint)
        {
            listener = new(endpoint.Address, endpoint.Port);
        }

        public void Start()
        {
            try
            {
                listener.Start();
                Console.WriteLine("Listening on {0}", listener.LocalEndpoint);

                while (true)
                {
                    Task.Run(() => acceptClient(listener.AcceptTcpClient()));
                }
            }
            catch
            {
                Console.WriteLine("unable to start server");
                throw;
            }
        }

        private void acceptClient(TcpClient c)
        {
            using Stream s = c.GetStream();
            StreamReader r = new(s, Encoding.ASCII, false, 1024, true);
            StreamWriter w = new(s);
            ServeHTTP(w, r);
        }

        public void ServeHTTP(StreamWriter w, StreamReader r)
        {
            try
            {
                // Wait for the client to finish sending its
                // request (we don't care what it is)
                Request req = new(r);
                Console.WriteLine("REQUEST:");
                Console.WriteLine(req);

                HTTP.Response res = new("Hello, world");
                res.WriteResponse(w);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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

        public Request(StreamReader r)
        {
            string? firstLine = r.ReadLine();
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
            string? line = r.ReadLine();
            while (line is string && line.Length > 0)
            {
                string[] l = line.Split(": ");
                if (l.Length == 2)
                {
                    headers.Add(l[0], l[1]);
                }
                line = r.ReadLine();
            }
        }

        public override string ToString()
        {
            StringBuilder s = new();
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

        public override string ToString()
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

            // Add 2 to body.Length to account for the CRLF
            AppendLine($"Content-Length: {body.Length + 2}");
            AppendLine("");
            AppendLine(body);
            return s.ToString();
        }

        public void WriteResponse(StreamWriter w)
        {
            string str = ToString();
            w.Write(str);
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
