using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareInterface.Exceptions
{
    class ProtocolException : SensorException
    {
        public ProtocolException(string message) : base(message) { }
    }
}
