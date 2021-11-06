using OpenCvSharp;
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
        public const double Gravity = 12.00336;
        public struct Result3D
        {
            public double TimeSpan;
            public double Angle;
            public double LineLen;
        }

        private bool triggerA = false, triggerB = false;
        public int countA = 0, countB = 0;
        DateTime StartA, StartB;
        public event Action<Result3D> OnResultFrame;
        ResultPair RpA, RpB;
        double lenfA = -1, lenfB = -1;

        public void CountA(Point p)
        {
            countA++;
            if (countA == 1)
            {
                StartA = DateTime.Now;
            }
            if (countA >= 10)
            {
                var dur = DateTime.Now - StartA;
                var avgT = dur.TotalMilliseconds / countA;
                lenfA = ((avgT / 1000) * (avgT / 1000) * Gravity) / (4 * Math.PI * Math.PI);
                countA = 0;
            }
        }

        public void CountB(Point p)
        {
            countB++;
            if (countB == 1)
            {
                StartB = DateTime.Now;
            }
            if (countB >= 10)
            {
                var dur = DateTime.Now - StartB;
                var avgT = dur.TotalMilliseconds / countA;
                lenfB = ((avgT / 1000) * (avgT / 1000) * Gravity) / (4 * Math.PI * Math.PI);
                countB = 0;
            }
        }

        public void TriggerA(ResultPair result)
        {
            RpA = result;
            triggerA = true;
            if (RpB.Movement > result.Movement && trustA == null)
            {
                Console.Title = "Trust B";
                trustA = false;
            }
            TriggerFrame();
        }

        public double Lenf()
        {
            Console.WriteLine("Waiting for length reads...");
            while (lenfA < 0 || lenfB < 0) ;
            var result = lenfA < 0 ? lenfB : lenfA;
            lenfA = lenfB = -1;
            return result;
        }

        public void TriggerB(ResultPair result)
        {
            RpB = result;
            triggerB = true;
            if (RpA.Movement > result.Movement && trustA == null)
            {
                trustA = true;
                Console.Title = "Trust A";
            }
            TriggerFrame();
        }

        int num = 1;
        int anum = 1;
        double totalang = 0;
        bool? trustA = null;
        DateTime Start;
        DateTime LastTrigger;
        bool firsttrigger = false;

        //bool ignoreA, igonreB;

        public void TriggerFrame()
        {
            //if (!(triggerA||) || (!triggerB)) return;
            if (firsttrigger || (DateTime.Now - LastTrigger).TotalSeconds > 0.5)
            {
                LastTrigger = DateTime.Now;
                triggerA = false;
                triggerB = false;
                if (num == 1)
                {
                    Start = DateTime.Now;
                    Console.Write("□□□□□□□□□□\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b");
                }
                //var error = Math.Abs(RpA.MiliDuration - RpB.MiliDuration);
                //var avgT = (RpA.MiliDuration + RpB.MiliDuration);
                /*if (error > avgT / 10)
                {
                    Console.WriteLine("error=" + error + "\t Invalid Result");
                    return;
                }*/
                totalang += Math.Atan(RpB.Movement / RpA.Movement);

                Console.Write("■");
                if (num >= 10)
                {
                    Console.WriteLine();
                    var angle = totalang / anum;
                    var avgT = (DateTime.Now - Start).TotalMilliseconds / num;
                    OnResultFrame?.Invoke(new Result3D
                    {
                        Angle = angle,
                        TimeSpan = avgT,
                        LineLen = (((avgT / 1000) * (avgT / 1000) * Gravity) / (4 * Math.PI * Math.PI)) - 0.0545//Lenf()
                    });
                    totalang = 0;
                    Start = DateTime.Now;
                    num = 0;
                }
                num++;
                anum++;
            }
        }
    }
}
