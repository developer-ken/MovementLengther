using BrightJade.Serial;
using System;

namespace HardwareInterface.Motor
{
    public class UARTMotorChain : ISensor
    {
        public const byte Zx_Motor_FRAME_HEADER1 = 0XFA;
        public const byte Zx_Motor_FRAME_HEADER2 = 0XAF;
        public const byte Zx_Motor_MOVE_ANGLE = 0X01;
        public const byte Zx_Motor_LED = 0X04;
        public const byte Zx_Motor_READ_ANGLE = 0X02;
        public const byte Zx_Motor_ID_WRITE = 0XCD;
        public const byte Zx_Motor_SET_OFFSET = 0XD2;
        public const byte Zx_Motor_READ_OFFSET = 0XD4;
        public const byte Zx_Motor_VERSION = 0X01;
        public const byte Zx_Motor_FRAME_END = 0XED;
        public const byte Zx_Motor_RUNS = 0XFD;
        public const byte Zx_Motor_RUNN = 0XFE;

        private SerialPortManager _serialPortManager;

        /// <summary>
        /// 串口调速电机驱动程序  Driver for uart speed-controed motor
        /// </summary>
        /// <param name="Port">串口设备 UART device</param>
        public UARTMotorChain(string Port)
        {
            _serialPortManager = new SerialPortManager();
            _serialPortManager.CurrentSerialSettings.PortName = Port;
            _serialPortManager.CurrentSerialSettings.BaudRate = 115200;
        }

        static byte GET_LOW_BYTE(UInt16 A)
        {
            return (byte)A;
        }

        static byte GET_HIGH_BYTE(UInt16 A)
        {
            return (byte)(A >> 8);
        }

        private byte _checksum(byte[] buf)
        {
            byte i;
            int sum = 0;
            for (i = 2; i < 8; i++)
            {
                sum += buf[i];
            }
            if (sum > 255) sum &= 0x00FF;
            return (byte)sum;
        }

        /// <summary>
        /// 更改电机的ID  Modify motor id
        /// </summary>
        /// <param name="oldID">原ID，0选择所有电机  Original ID, 0 for all motors</param>
        /// <param name="newID">新ID，不可为0  New ID,do not use 0</param>
        public void ChangeMotorId(byte oldID, byte newID)
        {
            byte[] buf = new byte[10];
            buf[0] = Zx_Motor_FRAME_HEADER1;
            buf[1] = Zx_Motor_FRAME_HEADER2;
            buf[2] = oldID;
            buf[3] = Zx_Motor_ID_WRITE;
            buf[4] = 0x00;
            buf[5] = newID;
            buf[6] = 0x00;
            buf[7] = 0x00;
            buf[8] = _checksum(buf);
            buf[9] = Zx_Motor_FRAME_END;
            _serialPortManager.Write(buf, 0, 10);
        }

        /// <summary>
        /// 顺时针(编码器端)转动电机 Turn the motor clockwise
        /// </summary>
        /// <param name="id">电机ID  Motor ID</param>
        /// <param name="rpm">转速  Rotation speed</param>
        public void MotorRunS(byte id, ushort rpm)
        {
            byte[] buf = new byte[10];
            buf[0] = Zx_Motor_FRAME_HEADER1;
            buf[1] = Zx_Motor_FRAME_HEADER2;
            buf[2] = id;
            buf[3] = 0x01;
            buf[4] = Zx_Motor_RUNS;
            buf[5] = 0x00;
            buf[6] = GET_HIGH_BYTE(rpm);
            buf[7] = GET_LOW_BYTE(rpm);
            buf[8] = _checksum(buf);
            buf[9] = Zx_Motor_FRAME_END;
            _serialPortManager.Write(buf, 0, 10);
        }

        /// <summary>
        /// 逆时针(编码器端)转动电机  Turn the motor anti-clockwise
        /// </summary>
        /// <param name="id">电机ID  Motor ID</param>
        /// <param name="rpm">转速  Rotation speed</param>
        public void MotorRunN(byte id, ushort rpm)
        {
            byte[] buf = new byte[10];
            buf[0] = Zx_Motor_FRAME_HEADER1;
            buf[1] = Zx_Motor_FRAME_HEADER2;
            buf[2] = id;
            buf[3] = 0x01;
            buf[4] = Zx_Motor_RUNN;
            buf[5] = 0x00;
            buf[6] = GET_HIGH_BYTE(rpm);
            buf[7] = GET_LOW_BYTE(rpm);
            buf[8] = _checksum(buf);
            buf[9] = Zx_Motor_FRAME_END;
            _serialPortManager.Write(buf, 0, 10);
        }

        /// <summary>
        /// 停止所有电机  Stop all the motors
        /// </summary>
        public void StopAll()
        {
            MotorRunN(0, 0);
            MotorRunS(0, 0);
        }

        /// <summary>
        /// 初始化设备  Init the deivce
        /// </summary>
        public void Init()
        {
            _serialPortManager.StartListening();
        }

        /// <summary>
        /// 关闭设备  Disconnect the device
        /// </summary>
        public void Abort()
        {
            _serialPortManager.StopListening();
        }
    }
}
