using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareInterface.MPUSensor
{
    public class MPU6050 : ISensor
    {
        public const byte ADDRESS = 0x68;
        private const byte PWR_MGMT_1 = 0x6B;
        private const byte SMPLRT_DIV = 0x19;
        private const byte CONFIG = 0x1A;
        private const byte GYRO_CONFIG = 0x1B;
        private const byte ACCEL_CONFIG = 0x1C;
        private const byte FIFO_EN = 0x23;
        private const byte INT_ENABLE = 0x38;
        private const byte INT_STATUS = 0x3A;
        private const byte USER_CTRL = 0x6A;
        private const byte FIFO_COUNT = 0x72;
        private const byte FIFO_R_W = 0x74;

        private const double GyRate = 360d / 65535d;

        private double accrate = 0;
        private double gyorate = 0;
        private int accoffset = 0;
        private int gyooffset = 0;

        I2cDevice device;

        public MPU6050(int busid = 1, byte addr = ADDRESS)
        {
            I2cConnectionSettings cs = new I2cConnectionSettings(busid, addr);
            device = I2cDevice.Create(cs);
        }

        public void Abort()
        {
            device.Dispose();
        }

        public void Init()
        {
            Init(AccRange.MP2g,GyoRange.MP250dps);
        }

        public void Init(AccRange acc_range,GyoRange gyo_range)
        {
            WriteByte(PWR_MGMT_1, 0x01);
            Task.Delay(100).Wait();
            WriteByte(ACCEL_CONFIG, (byte)acc_range);
            switch (acc_range)
            {
                case AccRange.MP2g:
                    accrate = 4d / 65535d;
                    accoffset = 2;
                    break;
                case AccRange.MP4g:
                    accrate = 8d / 65535d;
                    accoffset = 4;
                    break;
                case AccRange.MP8g:
                    accrate = 16d / 65535d;
                    accoffset = 8;
                    break;
                case AccRange.MP16g:
                    accrate = 32d / 65535d;
                    accoffset = 16;
                    break;
            }

            switch (gyo_range)
            {
                case GyoRange.MP250dps:
                    gyorate = 500d / 65535d;
                    gyooffset = 250;
                    break;
                case GyoRange.MP500dps:
                    gyorate = 1000d / 65535d;
                    gyooffset = 500;
                    break;
                case GyoRange.MP1000dps:
                    gyorate = 2000d / 65535d;
                    gyooffset = 1000;
                    break;
                case GyoRange.MP2000dps:
                    gyorate = 4000d / 65535d;
                    gyooffset = 2000;
                    break;
            }
        }

        public Vect3Result ReadAccelerometer()
        {
            var raw = new Vect3Raw
            {
                X = ReadWord(0x3B),//3B 3C
                Y = ReadWord(0x3D),//3D 3E
                Z = ReadWord(0x3F) //3F 40
            };
            return new Vect3Result
            {
                X = raw.X * accrate,
                Y = raw.Y * accrate,
                Z = raw.Z * accrate,
                Raw = raw
            };
        }

        public Vect3Result ReadGyroscope()
        {
            var raw = new Vect3Raw
            {
                X = ReadWord(0x43),//43 44
                Y = ReadWord(0x45),//45 46
                Z = ReadWord(0x47) //47 48
            };
            return new Vect3Result
            {
                X = raw.X * gyorate,
                Y = raw.Y * gyorate,
                Z = raw.Z * gyorate,
                Raw = raw
            };
        }

        private double dist(double a,double b)
        {
            return Math.Sqrt((a * a) + (b * b));
        }

        private double arc2deg(double a)
        {
            return (a/(2*Math.PI))*360;
        }

        public Vect3Result GetRotation()
        {
            var acc = ReadAccelerometer();
            var rotX = Math.Atan2(acc.X, dist(acc.Y, acc.Z));
            var rotY = Math.Atan2(acc.Y, dist(acc.X, acc.Z));
            var rotZ = Math.Atan2(acc.Z, dist(acc.X, acc.Y));
            return new Vect3Result
            {
                X = arc2deg(rotX),
                Y = arc2deg(rotY),
                Z = arc2deg(rotZ),
                Raw = new Vect3Raw
                {
                    X = rotX,
                    Y = rotY,
                    Z = rotZ
                }
            };
        }

        private byte[] ReadBytes(byte regAddr, int length)
        {
            byte[] values = new byte[length];
            byte[] buffer = new byte[1];
            buffer[0] = regAddr;
            device.WriteRead(buffer, values);
            return values;
        }

        private short ReadWord(byte address)
        {
            byte[] buffer = ReadBytes(address, 2);
            return (short)((buffer[0] << 8) | buffer[1]);
        }

        private void WriteByte(byte regAddr, byte data)
        {
            byte[] buffer = new byte[2];
            buffer[0] = regAddr;
            buffer[1] = data;
            device.Write(buffer);
        }
    }
}
