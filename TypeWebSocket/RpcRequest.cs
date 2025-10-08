using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeWebSocket
{
    public class RpcRequest
    {
        public string Interface { get; set; } = "";
        public string Method { get; set; } = "";
        public object?[] Args { get; set; } = Array.Empty<object>();
    }
}
