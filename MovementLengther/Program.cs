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
        static void Main(string[] args)
        {
            StreamWriter swA = new StreamWriter("resultA.csv")
                , swB = new StreamWriter("resultB.csv"),
                swResult = new StreamWriter("resultFinal.csv");
            IPCamera camB = new IPCamera("http://192.168.66.2:8080/?action=stream");
            camB.Init();
            IPCamera camA = new IPCamera("http://192.168.66.3:8080/?action=stream");
            camA.Init();

            Detector detA = new(),
                detB = new();
            new Thread(new ThreadStart(() => { detA.Alg(camA); })).Start();
            new Thread(new ThreadStart(() => { detB.Alg(camB); })).Start();
            DataOrganizer dato = new DataOrganizer();
            detA.OnNewResultArrive += dato.TriggerA;
            detB.OnNewResultArrive += dato.TriggerB;

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
                swResult.WriteLine((180 * obj.Angle / Math.PI) + "," + obj.TimeSpan + "," + obj.LineLen);
                swResult.Flush();
            };
        }

        private static void Dato_OnResultFrame(DataOrganizer.Result3D obj)
        {
            Console.WriteLine("DA:" + obj.DeltaA + "\tDB:" + obj.DeltaB + "\tAngle:" + 180 * obj.Angle / Math.PI + "\tT:" + obj.TimeSpan);
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