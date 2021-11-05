using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareInterface.Exceptions
{
    class WrongWorkingConditionException : SensorException
    {
        public WrongWorkingConditionException(string message) : base(message) { }
    }
}
