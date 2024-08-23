using System.Text;

namespace HTTP;

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
        string[] fields = firstLine.Split(' ');
        if (fields.Length < 3)
        {
            throw new Exception($"invalid HTTP request: {firstLine}");
        }
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

