using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using HardwareInterface;
using OpenCvSharp;

namespace MovementLengther
{
    static class Program
    {
        public static Queue<Action> TaskList;
        static void Main(string[] args)
        {
            TaskList = new Queue<Action>();
            StreamWriter swA = new StreamWriter("resultA.csv")
                 , swB = new StreamWriter("resultB.csv"),
                swResult = new StreamWriter("resultFinal.csv");

            Console.WriteLine("Simple pendulum mesurement system");

            Console.WriteLine("Connecting to cam...");

            IPCamera camA = new IPCamera("http://192.168.66.3:8080/?action=stream");
            camA.Init();

            IPCamera camB = new IPCamera("http://192.168.66.2:8080/?action=stream");
            camB.Init();

            Console.WriteLine("Loading detectors...");

            Detector detA = new(),
                detB = new();
            bool preview = true;
            while (preview)
            {
                ResourcesTracker rt = new ResourcesTracker();
                Mat a = rt.T(camA.GetLatestFrame());
                Mat b = rt.T(camB.GetLatestFrame());
                Cv2.Line(a, new Point(0, a.Height / 2), new Point(a.Width, a.Height / 2), Scalar.Green, thickness: 2);
                Cv2.Line(b, new Point(0, a.Height / 2), new Point(a.Width, a.Height / 2), Scalar.Green, thickness: 2);
                Cv2.Line(a, new Point(a.Width / 2, 0), new Point(a.Width / 2, a.Height), Scalar.Green, thickness: 2);
                Cv2.Line(b, new Point(a.Width / 2, 0), new Point(a.Width / 2, a.Height), Scalar.Green, thickness: 2);
                Cv2.ImShow(camA.StreamUrl, a);
                Cv2.ImShow(camB.StreamUrl, b);
                preview = (Cv2.WaitKey(1) == -1);
            }
            new Thread(new ThreadStart(() => { detB.Alg(camB); })).Start();
            new Thread(new ThreadStart(() => { detA.Alg(camA, true); })).Start();

            Console.WriteLine("Loading DO...");

            DataOrganizer dato = new DataOrganizer();
            detA.OnNewResultArrive += dato.TriggerA;
            detB.OnNewResultArrive += dato.TriggerB;

            detA.OnLeftHit += dato.CountA;
            detB.OnLeftHit += dato.CountB;


            detA.OnMovement += (obj) =>
            {
                swA.WriteLine(obj.X + "," + obj.Y);
                swA.Flush();
            };
            detB.OnMovement += (obj) =>
            {
                swB.WriteLine(obj.X + "," + obj.Y);
                swB.Flush();
            };


            dato.OnResultFrame += Dato_OnResultFrame;
            dato.OnResultFrame += (obj) =>
            {
                swResult.WriteLine((180 * obj.Angle / Math.PI) + "," + obj.TimeSpan + "," + obj.LineLen * 100);
                swResult.Flush();
            };
            Console.WriteLine("Thread worker enabled");
            while (true)
            {
                if (TaskList.Count != 0)
                    TaskList.Dequeue()?.Invoke();
            }
        }

        private static void Dato_OnResultFrame(DataOrganizer.Result3D obj)
        {
            Console.WriteLine("Angle:" + 180 * obj.Angle / Math.PI + "\tLen:" + obj.LineLen * 100);
        }

        static Point Center(this Rect rect)
        {
            return rect.BottomRight - rect.TopLeft;
        }

        static double ConnerDist(this Size size)
        {
            return Math.Sqrt(size.Width * size.Width + size.Height * size.Height);
        }

        static double Distance(this Point point)
        {
            return Math.Sqrt(point.X * point.X + point.Y * point.Y);
        }

        static List<Rect> Clone(this List<Rect> list)
        {
            var result = new List<Rect>();
            result.AddRange(list);
            return result;
        }
    }
}