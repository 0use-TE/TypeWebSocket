using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublicInterface.Server
{
    public interface IRobotChange
    {
        void OnSpeedChanged(float speed);
    }
}
