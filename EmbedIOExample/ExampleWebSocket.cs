using EmbedIO.WebSockets;
using PublicInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TypeWebSocket;

namespace EmbedIOExample
{
    internal class ExampleWebSocket : WebSocketModule
    {
        public ExampleWebSocket(string urlPath, bool enableConnectionWatchdog) : base(urlPath, enableConnectionWatchdog)
        {
            RpcDispatcher.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        }

        protected override async Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result)
        {
            try
            {
                await RpcDispatcher.DispatchAsync(buffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        protected override Task OnClientConnectedAsync(IWebSocketContext context)
        {
            return base.OnClientConnectedAsync(context);
        }

        public async Task BroadcastAsync<T>(Expression<Action<T>> expression)
        {
           await RpcDispatcher.SendTextAsync(expression, async buffer =>
            {
                await BroadcastAsync(buffer);
            });
        }
    }
}
