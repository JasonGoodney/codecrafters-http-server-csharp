using System.Net;
using System.Net.Sockets;
using System.Text;

var server = new TcpListener(IPAddress.Any, 4221);
server.Start();

try {
    while (true) {
        var client = server.AcceptSocket();
        _ = Task.Run(() => HandleClient(client));
    }
} catch (Exception e) {
    Console.Error.WriteLine($"Error: {e.Message}");
} finally {
    server.Stop();
}

static void HandleClient(Socket client) {
    byte[] buffer = new byte[256];
    var bytesRecieved = client.Receive(buffer);
    HttpRequest request = HttpRequest.Parse(Encoding.UTF8.GetString(buffer, 0, bytesRecieved));
    StringBuilder sb = new StringBuilder(128);

    if (request.RequestTarget == "/") {
        sb.Append("HTTP/1.1 200 OK\r\n\r\n");
        sb.Append("Content-Type: text/plain\r\n");
    } else if (request.RequestTarget.StartsWith("/echo")) {
        string content = request.RequestTarget.Split('/').Last();
        sb.Append("HTTP/1.1 200 OK\r\n");
        sb.Append("Content-Type: text/plain\r\n");
        sb.Append($"Content-Length: {content.Length}\r\n");
        sb.Append("\r\n");
        sb.Append(content);
    } else if (request.RequestTarget.StartsWith("/user-agent")) {
        sb.Append("HTTP/1.1 200 OK\r\n");
        sb.Append("Content-Type: text/plain\r\n");
        sb.Append($"Content-Length: {request.Header.UserAgent.Length}\r\n");
        sb.Append("\r\n");
        sb.Append(request.Header.UserAgent);
    } else {
        sb.Append("HTTP/1.1 404 Not Found\r\n\r\n");
    }

    byte[] msg = Encoding.ASCII.GetBytes(sb.ToString());
    client.Send(msg);
    client.Close();
}

class HttpRequest
{
    public string HttpMethod;
    public string RequestTarget;
    public string Protocol;
    public HttpHeader Header;
    public string Body;

    public static HttpRequest Parse(string message)
    {
        HttpRequest request = new HttpRequest();
        string[] sections = message.Split("\r\n\r\n");
        if (sections.Length > 1) {
            request.Body = sections[1];
        }
        string[] lines = sections[0].Split("\r\n");
        string[] statusLine = lines[0].Split(' ');
        request.HttpMethod = statusLine[0];
        request.RequestTarget = statusLine[1];
        request.Protocol = statusLine[2];
        request.Header = HttpHeader.Parse(lines);
        return request;
    }
}

class HttpHeader {
    public string Host;
    public string UserAgent;
    public string ContentType;
    public string ContentLength;

    public static HttpHeader Parse(string[] headers) {
        HttpHeader header = new HttpHeader();
        foreach (var item in headers) {
            string[] components = item.Split([":", " "], StringSplitOptions.RemoveEmptyEntries);
            if (components[0].Equals("host", StringComparison.OrdinalIgnoreCase)) {
                header.Host = components[1];
            } else if (components[0].Equals("user-agent", StringComparison.OrdinalIgnoreCase)) {
                header.UserAgent = components[1];
            } else if (components[0].Equals("content-type", StringComparison.OrdinalIgnoreCase)) {
                header.ContentType = components[1];
            } else if (components[0].Equals("content-length", StringComparison.OrdinalIgnoreCase)) {
                header.ContentLength = components[1];
            }
        }
        return header;
    }

    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        if (UserAgent != null) sb.Append($"User-Agent: {UserAgent}\r\n");
        if (ContentType != null) sb.Append($"Content-Type: {ContentType}\r\n");
        if (ContentLength != null) sb.Append($"Content-Length: {ContentLength}\r\n");
        return sb.ToString();
    }
}