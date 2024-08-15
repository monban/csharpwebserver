namespace test;
using System.Net;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        IPAddress localAddr = IPAddress.Parse("127.0.0.1");
        IPEndPoint endpoint = new(localAddr, 13000);
        HTTP.Server s = new(endpoint);
        MemoryStream mws = new(1024);
        StreamWriter w = new(mws, System.Text.Encoding.ASCII, 1024, true);
        string req = "GET / HTTP/1.1\r\nHost: example.com\r\n\r\n";
        var ascii = new System.Text.ASCIIEncoding();
        byte[] reqBytes = ascii.GetBytes(req);
        MemoryStream mrs = new(reqBytes);
        StreamReader r = new(mrs);
        s.ServeHTTP(w, r);
        mws.Seek(0, 0);
        StreamReader mwsr = new(mws);
        string responseString = mwsr.ReadToEnd();
        Assert.Contains("HTTP/1.1 200 OK", responseString);
    }
}
