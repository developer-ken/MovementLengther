using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareInterface.Exceptions
{
    class SensorException : Exception
    {
        public SensorException(string message) : base(message) { }
    }
}
