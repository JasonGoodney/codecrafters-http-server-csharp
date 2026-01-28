using System.Net;
using System.Net.Sockets;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

while (true) {
    using TcpClient client = server.AcceptTcpClient();
    NetworkStream stream = client.GetStream();

    Byte[] buffer = new Byte[256];
    stream.Read(buffer, 0, buffer.Length);
    String data = System.Text.Encoding.ASCII.GetString(buffer);
    if (data == "") {
        continue;
    }

    String[] fields = data.Split("\r\n");
    if (fields.Length == 0) {
        continue;
    }

    String[] requestLine = fields[0].Split(' ');
    String response = "HTTP/1.1 404 Not Found\r\n\r\n";
    if (requestLine.Length == 3) {
        if (requestLine[1] == "/") {
            response = "HTTP/1.1 200 OK\r\n\r\n";
        }
    }

    byte[] msg = System.Text.Encoding.ASCII.GetBytes(response);
    stream.Write(msg, 0, msg.Length);
}
