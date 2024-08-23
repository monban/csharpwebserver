using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HTTP;

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
            Response res = new("Hello, world");
            res.WriteResponse(w);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    TcpListener listener;
}
