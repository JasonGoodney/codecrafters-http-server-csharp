using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

while (true)
{
    using TcpClient client = server.AcceptTcpClient();
    NetworkStream stream = client.GetStream();

    Byte[] buffer = new Byte[256];
    stream.Read(buffer, 0, buffer.Length);
    String request = System.Text.Encoding.ASCII.GetString(buffer);

    String response = "HTTP/1.1 404 Not Found\r\n\r\n";
    String[] lines = request.Split("\r\n");
    if (lines.Length == 0)
        continue;

    String[] requestLines = lines[0].Split(' ');
    String requestTarget = requestLines[1];
    if (requestTarget == "/")
    {
        response = "HTTP/1.1 200 OK\r\n\r\n";
    }
    else if (requestTarget.StartsWith("/echo"))
    {
        String[] components = requestTarget.Split('/');
        String content = components[components.Length - 1];
        StringBuilder sb = new StringBuilder(128);
        sb.Append("HTTP/1.1 200 OK\r\n");
        sb.Append("Content-Type: text/plain\r\n");
        sb.Append($"Content-Length: {content.Length}\r\n");
        sb.Append("\r\n");
        sb.Append(content);
        response = sb.ToString();
    }
    else if (requestTarget.StartsWith("/user-agent"))
    {
        for (int j = 1; j < lines.Length; j++)
        {
            if (lines[j].StartsWith("User-Agent"))
            {
                String[] components = lines[j].Split(':');
                String content = components[components.Length - 1];
                if (content.StartsWith(' '))
                {
                    content = content.Split(' ')[1];
                }
                StringBuilder sb = new StringBuilder(128);
                sb.Append("HTTP/1.1 200 OK\r\n");
                sb.Append("Content-Type: text/plain\r\n");
                sb.Append($"Content-Length: {content.Length}\r\n");
                sb.Append("\r\n");
                sb.Append(content);
                response = sb.ToString();
                break;
            }
        }
    }

    byte[] msg = System.Text.Encoding.ASCII.GetBytes(response);
    stream.Write(msg, 0, msg.Length);
}
