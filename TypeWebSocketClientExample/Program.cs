using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using TypeWebSocket;

using var clientWebSocket = new ClientWebSocket();

RpcDispatcher.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

await clientWebSocket.ConnectAsync(new Uri("ws://0.0.0.0:8080/ws"), CancellationToken.None);


if (clientWebSocket.State == WebSocketState.Open)
{
    Console.WriteLine("连接成功！");


    // 启动监听任务
    _ = Task.Run(async () =>
    {
        var buffer = new byte[4096];

        while (clientWebSocket.State == WebSocketState.Open)
        {
            var result = await clientWebSocket.ReceiveAsync(buffer, CancellationToken.None);
            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

            try
            {
                await RpcDispatcher.DispatchAsync(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ DispatchAsync 出错: {ex.Message}");
            }
        }
    });

}
else
{
    Console.WriteLine("连接失败！");
}
while (true) ;
