using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareInterface.Platform
{
    public class Platform
    {
        public static bool IsJetson()
        {
            return File.Exists("/etc/nv_tegra_release");
        }

        public static bool IsRaspberryPi()
        {
            return File.Exists("/usr/bin/raspi-config");
        }

        public static PlatformType GetPlatform()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT) return PlatformType.Windows;
            if (IsJetson()) return PlatformType.Jetson;
            if (IsRaspberryPi()) return PlatformType.RapberryPi;
            return PlatformType.OtherLinux;
        }
    }

    public enum PlatformType
    {
        Jetson,RapberryPi,OtherLinux,Windows
    }
}
