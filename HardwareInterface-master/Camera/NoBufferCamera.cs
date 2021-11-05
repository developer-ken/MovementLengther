using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HardwareInterface
{

    public partial class NoBufferCamera
    {
        public VideoCapture cap;

        /// <summary>
        /// 无缓冲相机，在低性能系统上使用可大幅降低CPU占用。只支持V4L驱动。
        /// <para>Camera without buffer. On low-performance systems this can reduce cpu usage.V4L drivers only.</para>
        /// </summary>
        /// <param name="cam_id">相机设备ID camera device ID</param>
        /// <param name="vpis">底层驱动 Base api to use</param>
        public NoBufferCamera(int cam_id = 0, VideoCaptureAPIs vpis = VideoCaptureAPIs.V4L)
        {
            cap = new VideoCapture(cam_id, vpis)
            {
                ConvertRgb = true
            };
            cap.BufferSize = 1;
        }

        /// <summary>
        /// 等待相机就绪 Wait for the camera to get ready
        /// </summary>
        public new void Init()
        {
            while (!cap.IsOpened()) Thread.Sleep(0);
        }

        /// <summary>
        /// 获取一帧图像 Grab a frame
        /// </summary>
        /// <returns></returns>
        public new Mat GetLatestFrame()
        {
            Mat src = new Mat();
            while (!cap.Read(src)) Thread.Sleep(0);
            return src;
        }

        /// <summary>
        /// 关闭相机 Close the camera
        /// </summary>
        public void Abort()
        {
            cap.Dispose();
        }
    }
}
