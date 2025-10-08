您好，我是Ouse，欢迎您查看该包。
以往使用WebSocket时，我们总是需要规范一套协议，以方便客户端和服务端进行通讯，对于小项目或者是试验项目，这很麻烦，并且是弱类型的。
于是便诞生了该包，该包提供了一套强类型编码，通过WebSocket通讯的方案😋😋😋。
最适合以下情况
1.客户端很少，几乎只有服务端和一个客户端
2.项目很小或者是敏捷开发
3.喜欢强类型

代码展示：
客户端连接服务端，接受和发送信息  (使用CLR中的ClientWebSockent)
```csharp
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
    //通知服务端速度变化
    await clientWebSocket.SendTextAsync<IRobotChange>(robot => robot.OnSpeedChanged(4));
}
else
{
    Console.WriteLine("连接失败！");
}
Console.ReadKey();
```
可见我们通过强类型的方式(基于接口规范)调用了服务端IRobotChange.OnSpeedChange(float speed)方法

分布教程：
正在写




