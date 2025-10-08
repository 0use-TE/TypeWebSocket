using System;
using System.Linq.Expressions;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TypeWebSocket;

namespace TypeWebSocketClientExample
{
    public static class Extensions
    {
        public static async Task SendTextAsync<T>(this ClientWebSocket clientWebSocket, Expression<Action<T>> expression)
        {
           await  RpcDispatcher.SendTextAsync(expression, async buffer =>
            {
                await clientWebSocket.SendAsync(buffer,WebSocketMessageType.Text,true,CancellationToken.None);
            });
        }
    }
}

