using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareInterface.Exceptions
{
    class SensorHardwareException : SensorException
    {
        public SensorHardwareException(string message) : base(message) { }
    }
}
