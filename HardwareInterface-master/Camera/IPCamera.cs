using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HardwareInterface
{

    public partial class IPCamera : ISensor
    {
        private Mat src;
        //private FrameSource cap;
        public VideoCapture cap;
        private Thread camthread;
        public readonly string StreamUrl;
        public IPCamera(string streamurl)
        {
            StreamUrl = streamurl;
            src = new Mat();
            cap = new VideoCapture(streamurl)
            {
                ConvertRgb = true
            };
            camthread = new Thread(new ThreadStart(() => { while (true) { try { CapNextFrame(); } catch (Exception ee) { Console.WriteLine(ee.Message + "\n" + ee.StackTrace); } } }));
        }

        public void Init()
        {
            while (!cap.IsOpened()) Thread.Sleep(0);
            camthread.Start();
            while (camthread.ThreadState != ThreadState.Running) Thread.Sleep(10);
            GetLatestFrame();
        }

        public Mat GetLatestFrame()
        {
            if (camthread == null || camthread.ThreadState != ThreadState.Running)
            {
                //throw new Exception("相机未初始化");
            }
            Mat latest;
            while (true)
            {
                lock (src)
                {
                    latest = src.Clone();
                }
                if (latest.Width > 0) { return latest; }
            }
        }

        public double QualityRate()
        {
            return QualityRate(GetLatestFrame());
        }

        public static bool IsGrayed(Mat frame)
        {
            return frame.Channels() == 1;
        }

        public static double QualityRate(Mat frame1)
        {
            Mat frame = frame1.Clone();
            if (!IsGrayed(frame)) frame = frame.CvtColor(ColorConversionCodes.RGB2GRAY);
            frame = frame.Sobel(frame.Type(), 1, 1);
            return frame.Mean().ToDouble();
        }

        public void Abort()
        {
            camthread.Interrupt();
            cap.Dispose();
        }

        private void CapNextFrame()
        {
            lock (src)
            {
                while (!cap.Read(src)) Thread.Sleep(0);//等待，直到获得帧
                //由于src被互斥锁定，这样的写法可以使GetLatestFrame在无数据时阻塞，直到获得图像。
            }
        }
    }
}
