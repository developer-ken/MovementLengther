using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareInterface
{
    public partial class Camera
    {
        public static class NvArgusCameraSrc
        {
            public struct NvCamSettings
            {
                public int CamNumber, Width, Height, Framerate;
                public NvCameraFlip Flip;
            }
            public enum NvCameraFlip
            {
                No = 0, UpSideDown = 2, LeftSideRight = 1
            }
            public static string GetDeviceGStream(NvCamSettings settings)
            {
                return "nvarguscamerasrc sensor-id=" +
                    settings.CamNumber +
                    " ! video/x-raw(memory:NVMM), width=" +
                    settings.Width +
                    ", height=" +
                    settings.Height +
                    ", format=(string)NV12, framerate=(fraction)" +
                    settings.Framerate +
                    " ! nvvidconv flip-method=" +
                    (int)settings.Flip +
                    " ! video/x-raw, width=" +
                    settings.Width +
                    ", height=" +
                    settings.Height +
                    ", format=(string)BGRx ! videoconvert" +
                    " ! video/x-raw, format=(string)BGR ! appsink";
            }
        }
    }
}
