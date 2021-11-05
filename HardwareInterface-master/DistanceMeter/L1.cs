using HardwareInterface.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HardwareInterface.DistanceMeter
{
    public class L1 : ISensor
    {
        UART.UART uart;

        public struct SensorReadout
        {
            public double Distance;
            public int Reflection;
        }

        public L1(string portname)
        {
            uart = new UART.UART(portname);
            uart.port.BaudRate = 38400;
        }

        public SensorReadout MeasureOnce()
        {
            uart.Send("iSM");//发送测量指令
            string data = uart.ReadLine();
            var match1 = Regex.Match(data, "([A-Z])=([\\.0-9]*)");
            var match2 = Regex.Match(data, "([A-Z])=([\\.0-9]*)m,([0-9]*)#");
            double distance = -1;
            int reflection = 1;
            int err = 0;
            char stat = '_';
            if (match2.Success)
            {
                stat = match2.Groups[1].Value[0];
                distance = float.Parse(match2.Groups[2].Value);
                reflection = int.Parse(match2.Groups[3].Value);
            }
            else if (match1.Success)
            {
                stat = match1.Groups[1].Value[0];
                err = int.Parse(match1.Groups[2].Value);
            }
            switch (stat)
            {
                case 'E':
                    GenerateException(err);
                    return new SensorReadout();
                case 'D':
                    return new SensorReadout
                    {
                        Distance = distance,
                        Reflection = reflection
                    };
                default:
                    throw new ProtocolException("协议错误: 无法解析的数据包");
            }
        }

        private void GenerateException(int code)
        {
            switch (code)
            {
                case 0:
                    throw new ProtocolException("协议错误：传感器未返回读数");
                    break;
                case 140:
                case 141:
                case 142:
                    throw new ProtocolException("Custom Hex 协议发生传输时错误");
                case 252:
                    throw new WrongWorkingConditionException("传感器温度过高");
                case 253:
                    throw new WrongWorkingConditionException("传感器温度过低");
                case 255:
                    throw new WrongWorkingConditionException("反射追踪失败：反射过弱或运算错误");
                case 256:
                    throw new WrongWorkingConditionException("反射过强");
                case 258:
                    throw new WrongWorkingConditionException("目标过于接近");
                case 286:
                    throw new SensorHardwareException("激光管故障");
                case 290:
                    throw new SensorHardwareException("未知硬件错误");
                default:
                    throw new ProtocolException("协议错误：未能捕获传感器错误");
            }
        }

        public void Abort()
        {
            uart.Abort();
        }

        public void Init()
        {
            uart.Init();
        }
    }
}
