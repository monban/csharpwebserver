using System.Text;

namespace HTTP;

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
        w.Write(ToString());
        w.Flush();
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
