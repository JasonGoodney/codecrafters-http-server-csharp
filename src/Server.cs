using System.Net;
using System.Net.Sockets;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

Byte[] bytes = new Byte[256];
String data = null;

using TcpClient client = server.AcceptTcpClient(); // wait for client

data = null;

NetworkStream stream = client.GetStream();

data = "HTTP/1.1 200 OK\r\n\r\n";
byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

stream.Write(msg, 0, msg.Length);
