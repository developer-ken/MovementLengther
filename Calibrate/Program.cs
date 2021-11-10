using System;
using System.IO;

namespace Calibrate
{
    class Program
    {
        static void Main(string[] args)
        {
            var data = File.ReadAllText("resultFinal.csv").Split('\n');
            double average_time = 0, count = 0;
            foreach (var line in data)
            {
                var linedata = line.Split(',');
                if (linedata.Length != 3) continue;
                count++;
                average_time += double.Parse(linedata[1]);
            }
            average_time /= count;
            Console.WriteLine("Average T(ms)=" + average_time);
            average_time /= 1000;
            string t;
        reinput:
            Console.Write("True Lenth(cm)=");
            t = Console.ReadLine();
            if (!double.TryParse(t, out double Len)) goto reinput;
            Len += 5.45;
            Len /= 100;
            double gravity = (4 * Len * Math.PI * Math.PI) / (average_time * average_time);
            Console.Write("Gravity result=" + gravity);
            File.WriteAllText("GravityConst.txt", gravity.ToString());
        }
    }
}
