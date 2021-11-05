using System;

namespace HardwareInterface
{
    public interface ISensor
    {
        /// <summary>
        /// 初始化硬件和相关资源
        /// </summary>
        public void Init();
        /// <summary>
        /// 关闭硬件设备，
        /// </summary>
        public void Abort();
    }
}
