using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

while (true) {
    using TcpClient client = server.AcceptTcpClient();
    NetworkStream stream = client.GetStream();

    Byte[] buffer = new Byte[256];
    stream.Read(buffer, 0, buffer.Length);
    String request = System.Text.Encoding.ASCII.GetString(buffer);
    if (request == "") {
        continue;
    }

    String[] fields = request.Split("\r\n");
    if (fields.Length == 0) {
        continue;
    }

    String[] requestLine = fields[0].Split(' ');
    String response = "HTTP/1.1 404 Not Found\r\n\r\n";
    if (requestLine.Length == 3) {
        String method = requestLine[0];
        String requestTarget = requestLine[1];
        String protocol = requestLine[2];

        if (requestTarget == "/") {
            response = "HTTP/1.1 200 OK\r\n\r\n";
        }
        else if (requestTarget.StartsWith("/echo")) {
            String[] components = requestTarget.Split('/');
            String content = components[components.Length-1];
            StringBuilder sb = new StringBuilder(128);
            sb.Append("HTTP/1.1 200 OK\r\n");
            sb.Append("Content-Type: text/plain\r\n");
            sb.Append($"Content-Length: {content.Length}\r\n");
            sb.Append("\r\n");
            sb.Append(content);
            response = sb.ToString();
        }
    }

    byte[] msg = System.Text.Encoding.ASCII.GetBytes(response);
    stream.Write(msg, 0, msg.Length);
}
