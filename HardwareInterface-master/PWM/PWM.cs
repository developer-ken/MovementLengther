using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Pwm;

namespace HardwareInterface.PWM
{
    public class PWM : ISensor
    {
        PwmChannel pchannel;
        public PWM(int chip = 0, int channel = 0)
        {
            pchannel = PwmChannel.Create(0, 0, 1, 0.5);
        }

        public void Set(int freq,double duty)
        {
            pchannel.Frequency = freq;
            pchannel.DutyCycle = duty;
        }

        void ISensor.Init()
        {
            pchannel.Start();
        }

        void ISensor.Abort()
        {
            pchannel.Stop();
        }
    }
}
