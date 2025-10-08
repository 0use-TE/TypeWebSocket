using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublicInterface.Client
{
    public interface IRobotMove
    {
        public void Move(float dx,float dy,float speed);
    }
}
