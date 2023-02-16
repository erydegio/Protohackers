using ProtoHackers;

var server = new TcpEchoServer("127.0.0.1", 8001);
await server.Run();