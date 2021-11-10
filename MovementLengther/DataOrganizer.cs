using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MovementLengther.Detector;

namespace MovementLengther
{
    class DataOrganizer
    {
        public double Gravity = 12.00336;

        public DataOrganizer()
        {
            if (File.Exists("GravityConst.txt"))
            {
                Gravity = double.Parse(File.ReadAllText("GravityConst.txt"));
                Console.WriteLine("Using calibrated GravityConst:" + Gravity);
            }
        }

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
        bool isFixed = false;

        public void CountA(Point p)
        {
            countA++;
            if (countA == 1)
            {
                StartA = DateTime.Now;
            }
        }

        public void CountB(Point p)
        {
            countB++;
            if (countB == 1)
            {
                StartB = DateTime.Now;
            }
        }

        double movementA = 0, movementB = 0;

        public void TriggerA(ResultPair result)
        {
            RpA = result;
            triggerA = true;
            movementA += result.Movement;
            if (RpB.Movement > result.Movement && !isFixed)
            {
                Console.Title = "Trust B";
                trustA = false;
                isFixed = true;
            }
            TriggerFrame(true);
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
            movementB += result.Movement;
            if (RpA.Movement > result.Movement && !isFixed)
            {
                trustA = true;
                Console.Title = "Trust A";
                isFixed = true;
            }
            TriggerFrame();
        }

        int num = 1;
        int anum = 1;
        double totalang = 0;
        bool trustA = false;
        DateTime Start;
        DateTime LastTrigger;
        bool firsttrigger = false;

        //bool ignoreA, igonreB;

        public void TriggerFrame(bool isA = false)
        {
            if (!isFixed) return;
            if ((trustA && isA) || (!trustA && !isA))
            {
                if (firsttrigger || (DateTime.Now - LastTrigger).TotalSeconds > 0.8)
                {
                    LastTrigger = DateTime.Now;
                    triggerA = false;
                    triggerB = false;
                    if (num == 1)
                    {
                        Start = DateTime.Now;
                        Console.Write("? ? ? ? ? ? ? ? ? ? ? ? ? ? ?\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b");
                    }
                    //var error = Math.Abs(RpA.MiliDuration - RpB.MiliDuration);
                    //var avgT = (RpA.MiliDuration + RpB.MiliDuration);
                    /*if (error > avgT / 10)
                    {
                        Console.WriteLine("error=" + error + "\t Invalid Result");
                        return;
                    }*/

                    Console.Write(isA ? "A " : "B ");
                    if (num >= 15)
                    {
                        Console.WriteLine();
                        var angle = Math.Atan(movementB / movementA);
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
                }
            }
            else
            {
                Console.Write("! ");
            }
        }
    }
}
