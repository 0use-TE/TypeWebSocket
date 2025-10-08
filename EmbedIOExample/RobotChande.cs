using PublicInterface.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeWebSocket;

namespace EmbedIOExample
{
    [RpcService]
    internal class RobotChande : IRobotChange
    {
        public void OnSpeedChanged(float speed)
        {
            Console.WriteLine($"机器人速度变为了{speed}");
        }
    }
}
