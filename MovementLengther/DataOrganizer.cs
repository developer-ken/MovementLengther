using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MovementLengther.Detector;

namespace MovementLengther
{
    class DataOrganizer
    {
        public const double Gravity=9.8;
        public struct Result3D
        {
            public double TimeSpan;
            public double DeltaA;
            public double DeltaB;
            public double Angle;
            public double LineLen;
        }

        private bool triggerA = false, triggerB = false;
        public event Action<Result3D> OnResultFrame;
        ResultPair RpA, RpB;

        public void TriggerA(ResultPair result)
        {
            RpA = result;
            triggerA = true;
            TriggerFrame();
        }

        public void TriggerB(ResultPair result)
        {
            RpB = result;
            triggerB = true;
            TriggerFrame();
        }

        public void TriggerFrame()
        {
            if (triggerA && triggerB)
            {
                triggerA = false;
                triggerB = false;

                //var error = Math.Abs(RpX.MiliDuration - RpY.MiliDuration);
                //if(error)
                var avgT = (RpA.MiliDuration + RpB.MiliDuration);
                var angle = Math.Atan(RpB.Movement / RpA.Movement);

                OnResultFrame?.Invoke(new Result3D
                {
                    Angle = angle,
                    DeltaA = RpA.Movement,
                    DeltaB = RpB.Movement,
                    TimeSpan = avgT,
                    LineLen = ((avgT/1000) * (avgT / 1000) * Gravity) / (4 * Math.PI * Math.PI)
                });
            }
        }
    }
}
