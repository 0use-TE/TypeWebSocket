using PublicInterface.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeWebSocket;

namespace EmbedIOExample
{
    [RpcService]
    internal class RobotMove:IRobotMove
    {
        public void Move(float dx, float dy, float speed)
        {
            Console.WriteLine($"机器人移动: dx={dx}, dy={dy}, speed={speed}");
        }
    }
}
