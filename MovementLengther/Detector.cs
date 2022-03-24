using HardwareInterface;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MovementLengther
{
    class Detector
    {
        public Detector()
        {

        }

        public event Action<Point> OnMovement;
        public event Action<Point> OnLeftHit;
        public event Action<Point> OnRightHit;

        public double MoveDist = 0;
        public bool Acceptable = false;
        public struct ResultItem
        {
            public DateTime Timestamp;
            public Point Position;
            public bool Hit;
        }

        public struct ResultPair
        {
            public double MiliDuration;
            public double Movement;
        }

        public ResultItem Leftend, Rightend;
        public Queue<ResultPair> Results = new Queue<ResultPair>();

        public event Action<ResultPair> OnNewResultArrive;

        public Mat MovementDetectRGB(Mat background, Mat frame)
        {
            ResourcesTracker rt = new ResourcesTracker();
            Mat gray1 = rt.T(frame.Clone()), gray2 = rt.T(frame.Clone());
            Cv2.CvtColor(background, gray1, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(frame, gray2, ColorConversionCodes.BGR2GRAY);
            Mat diff = rt.T(gray2.Clone());
            Cv2.Absdiff(gray1, gray2, diff);
            Cv2.MedianBlur(diff, diff, 5);
            Mat diff_thresh = diff.Clone();
            Cv2.Threshold(diff, diff_thresh, 35, 255, ThresholdTypes.Binary);
            Cv2.Blur(diff_thresh, diff_thresh, new Size(8, 8));
            Cv2.Threshold(diff_thresh, diff_thresh, 35, 255, ThresholdTypes.Binary);
            //diff_thresh
            rt.Dispose();
            return diff_thresh;
        }

        public Mat MovementDetectGray(Mat background, Mat frame)
        {
            ResourcesTracker rt = new ResourcesTracker();
            Mat diff = rt.T(background.Clone());
            Cv2.Absdiff(background, frame, diff);
            Mat diff_thresh = diff.Clone();
            Cv2.Threshold(diff, diff_thresh, 35, 255, ThresholdTypes.Binary);
            Cv2.Blur(diff_thresh, diff_thresh, new Size(8, 8));
            Cv2.Threshold(diff_thresh, diff_thresh, 35, 255, ThresholdTypes.Binary);
            return diff_thresh;
        }

        public void Alg(IPCamera cam, bool flip = false)
        {
            Mat last = null;
            Mat lastdiff = null;
            Rect lastrect = new Rect(0, 0, 0, 0);
            bool inited = false;
            int watchdog = 0;

            new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    Thread.Sleep(100);
                    watchdog++;
                    if (watchdog > 20)
                    {
                        inited = false;
                        watchdog = 0;
                    }
                }
            })).Start();


            while (true)
            {
                ResourcesTracker rt = new ResourcesTracker();
                Mat frame = cam.GetLatestFrame();
                if (flip)
                    Cv2.Flip(frame, frame, FlipMode.Y);
                //Cv2.Resize(frame, frame, new Size(400, 400));
                if (last == null)
                {
                    last = frame;
                    continue;
                }

                var diff = rt.T(MovementDetectRGB(last, frame));
                last.Dispose();
                last = frame;

                if (lastdiff == null)
                {
                    lastdiff = diff;
                    continue;
                }
                lastdiff.Dispose();
                lastdiff = diff;



                Mat kernel = rt.T(Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3), new Point(-1, -1)));
                Cv2.MorphologyEx(diff, diff, MorphTypes.Close, kernel, new Point(-1, -1));
                kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3), new Point(-1, -1));
                Cv2.MorphologyEx(diff, diff, MorphTypes.Open, kernel, new Point(-1, -1));
                Cv2.FindContours(diff, out Point[][] conts, out HierarchyIndex[] h,
                    RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                kernel.Dispose();

                List<Rect> targets = new();

                Mat f = frame.Clone();
                {
                    if (conts.Length == 0) continue;
                    var rect = Cv2.BoundingRect(conts.Last());
                    var csize = (rect & lastrect).Size;
                    var coll = csize.Height * csize.Width;
                    if (coll > (rect.Width * rect.Height / 2) ||
                        coll > (lastrect.Width * lastrect.Height / 2))
                        continue;
                    if (rect.X == 0 || rect.Y == 0) continue;
                    Rect target = rect;
                    var refpoint = Center(target);
                    Cv2.Rectangle(f, target, Scalar.Red, 2);
                    Cv2.DrawMarker(f, refpoint, Scalar.Red);
                    Cv2.DrawMarker(f, Leftend.Position, Scalar.Blue, thickness: 2);
                    Cv2.DrawMarker(f, Rightend.Position, Scalar.Green, thickness: 2);
                    OnMovement?.Invoke(Center(rect));
                    if (!inited)
                    {
                        Leftend.Hit = false;
                        Leftend.Position = refpoint;
                        Leftend.Timestamp = DateTime.Now;

                        Rightend.Hit = false;
                        Rightend.Position = refpoint;
                        Rightend.Timestamp = DateTime.Now;
                        inited = true;
                    }
                    if (refpoint.X <= Leftend.Position.X)
                    {
                        Leftend.Hit = true;
                        Leftend.Position = refpoint;
                        Leftend.Timestamp = DateTime.Now;
                        OnLeftHit?.Invoke(refpoint);
                        watchdog = 0;
                    }
                    else
                    if (refpoint.X >= Rightend.Position.X)
                    {
                        Rightend.Hit = true;
                        Rightend.Position = refpoint;
                        Rightend.Timestamp = DateTime.Now;
                        OnRightHit?.Invoke(refpoint);
                        watchdog = 0;
                    }
                    else
                    {
                        if (Leftend.Hit && Rightend.Hit)//半周期测量完成
                        {
                            var item = new ResultPair()
                            {
                                MiliDuration = Math.Abs((Leftend.Timestamp - Rightend.Timestamp).TotalMilliseconds),
                                Movement = Math.Abs(Leftend.Position.X - Rightend.Position.X)
                            };
                            Results.Enqueue(item);
                            OnNewResultArrive?.Invoke(item);
                            Leftend.Hit = false;
                            var pos = Leftend.Position;
                            Leftend.Position = Rightend.Position;
                            Leftend.Timestamp = DateTime.Now;
                            Rightend.Hit = false;
                            Rightend.Position = pos;
                            Rightend.Timestamp = DateTime.Now;
                        }
                    }
                    Program.TaskList.Enqueue(() =>
                    {
                        Cv2.ImShow(cam.StreamUrl, f);
                        Cv2.WaitKey(1);
                        f.Dispose();
                    });
                }
                rt.Dispose();
            }
        }

        public static Point Center(Rect rect)
        {
            return rect.TopLeft.Add(new Point(rect.Width / 2, rect.Height / 2));
        }
    }
}
