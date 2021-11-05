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
            Mat diff = rt.T(gray2.Clone()).MedianBlur(99);
            Cv2.Absdiff(gray1, gray2, diff);
            Mat diff_thresh = diff.Clone();
            Cv2.Threshold(diff, diff_thresh, 28, 255, ThresholdTypes.Binary);
            Cv2.Blur(diff_thresh, diff_thresh, new Size(8, 8));
            Cv2.Threshold(diff_thresh, diff_thresh, 28, 255, ThresholdTypes.Binary);
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
            Cv2.Threshold(diff, diff_thresh, 28, 255, ThresholdTypes.Binary);
            Cv2.Blur(diff_thresh, diff_thresh, new Size(8, 8));
            Cv2.Threshold(diff_thresh, diff_thresh, 28, 255, ThresholdTypes.Binary);
            return diff_thresh;
        }

        public void Alg(IPCamera cam)
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

            bool RLock = false;

            while (true)
            {
                ResourcesTracker rt = new ResourcesTracker();
                Mat frame = cam.GetLatestFrame();
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
                //diff = Detector.MovementDetectGray(diff, lastdiff);
                //diff = Detector.MovementDetectGray(diff, lastdiff);
                lastdiff.Dispose();
                lastdiff = diff;

                Mat kernel = rt.T(Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3), new Point(-1, -1)));
                Cv2.MorphologyEx(diff, diff, MorphTypes.Close, kernel, new Point(-1, -1));
                //kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3), new Point(-1, -1));
                //Cv2.MorphologyEx(diff, diff, MorphTypes.Open, kernel, new Point(-1, -1));
                Cv2.FindContours(diff, out Point[][] conts, out HierarchyIndex[] h, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                List<Rect> targets = new();

                using (Mat f = frame.Clone())
                {
                    List<Point> p = new List<Point>();
                    foreach (var obj in conts)
                    {
                        var recb = Cv2.BoundingRect(obj);
                        if (recb.Width * recb.Height >= frame.Width * frame.Height / 3000)
                            p.AddRange(obj);
                    }
                    var rect = Cv2.BoundingRect(p.ToArray());
                    //if (rect.Contains(lastrect.BottomRight - lastrect.TopLeft)) continue;
                    var csize = (rect & lastrect).Size;
                    var coll = csize.Height * csize.Width;
                    if (coll > (rect.Width * rect.Height / 2) ||
                        coll > (lastrect.Width * lastrect.Height / 2))
                        continue;
                    if (rect.X == 0 || rect.Y == 0) continue;
                    //Cv2.Rectangle(f, rect, Scalar.Red);

                    /*
                    bool changed = false;
                    do
                    {
                        List<Rect> ftargets = new();
                        int i = 0, j = 0;
                        changed = false;
                        foreach (var target1 in targets)
                        {
                            for (j = i; j < targets.Count; j++)
                            {
                                var target2 = targets[j];
                                if (!target1.Equals(target2))
                                {
                                    var dist = (target1.Center() - target2.Center()).Distance();
                                    if (dist < target1.Size.ConnerDist() ||
                                        dist < target2.Size.ConnerDist())
                                    {
                                        ftargets.Add(target1 | target2);
                                        changed = true;
                                    }
                                    else
                                    {
                                        //ftargets.Add(target1);
                                        //changed = false;
                                    }
                                }
                                //if (j == targets.Count - 1)
                                //{
                                //    ftargets.Add(target1);
                                //    changed = false;
                                //}
                            }
                            i++;
                        }
                        targets = ftargets.Clone();
                    } while (changed);
                    */
                    Rect target = rect;
                    var refpoint = Center(target);
                    Cv2.Rectangle(f, target, Scalar.Red, 2);
                    Cv2.DrawMarker(f, refpoint, Scalar.Red);
                    //if (Leftend.Hit)
                    //{
                    Cv2.DrawMarker(f, Leftend.Position, Scalar.Blue, thickness: 2);
                    //}
                    //if (Rightend.Hit)
                    //{
                    Cv2.DrawMarker(f, Rightend.Position, Scalar.Green, thickness: 2);
                    //}
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
                        watchdog = 0;
                        RLock = false;
                    }
                    else
                    if (refpoint.X >= Rightend.Position.X)
                    {
                        if (!RLock)
                        {
                            Rightend.Hit = true;
                            Rightend.Position = refpoint;
                            Rightend.Timestamp = DateTime.Now;
                            watchdog = 0;
                        }
                    }
                    else
                    {
                        if (Leftend.Hit && Rightend.Hit)//半周期测量完成
                        {
                            RLock = true;
                            var item = new ResultPair()
                            {
                                MiliDuration = Math.Abs((Leftend.Timestamp - Rightend.Timestamp).TotalMilliseconds),
                                Movement = Leftend.Position.DistanceTo(Rightend.Position)
                            };
                            Results.Enqueue(item);
                            OnNewResultArrive?.Invoke(item);
                            Leftend.Hit = false;
                            Leftend.Position = refpoint;
                            Leftend.Timestamp = DateTime.Now;
                            Rightend.Hit = false;
                            Rightend.Position = refpoint;
                            Rightend.Timestamp = DateTime.Now;
                        }
                    }

                    Cv2.ImShow(cam.StreamUrl, diff);
                    Cv2.WaitKey(1);
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
