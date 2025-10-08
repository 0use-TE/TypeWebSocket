using EmbedIO;
using EmbedIOExample;
using PublicInterface.Client;
using TypeWebSocket;
var url = "http://0.0.0.0:8080/";

var webServer =new WebServer(options =>
{
     options.WithUrlPrefix(url)
            .WithMode(HttpListenerMode.EmbedIO);
});
var webSocket= new ExampleWebSocket("/ws", true);
webServer
    .WithModule(webSocket);

_ = Task.Run(async () =>
{
    // ✅ 每隔 3 秒广播一条消息
    while (true)
    {
        await Task.Delay(1000);
        Console.WriteLine("服务端发送消息!");
        await webSocket.BroadcastAsync<IRobotMove>(x=>x.Move(3,3,3));
    }
});

await webServer.RunAsync();

Console.WriteLine("服务端正常运行");

Console.ReadLine();
