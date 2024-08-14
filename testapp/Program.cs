using System.Net;
using HTTP;

class Program
{
    public static void Main()
    {
        IPAddress localAddr = IPAddress.Parse("127.0.0.1");
        IPEndPoint endpoint = new(localAddr, 13000);
        HTTP.Server s = new(endpoint);
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

