using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using BrightJade;
using BrightJade.Serial;
using FashionStar.Servo.Uart;
using FashionStar.Servo.Uart.Protocol;

namespace HardwareInterface.Servo
{
    /// <summary>
    /// FashionStar UART 舵机链
    /// </summary>
    public class FSServoChain : ISensor
    {
        // Serial Port 管理器。
        private SerialPortManager _serialPortManager;
        // 舵机控制器。
        private ServoController _servoController;
        private Dictionary<byte, float> _servoAngle;
        private List<byte> _servos;

        public FSServoChain(string Port)
        {
            _servoAngle = new Dictionary<byte, float>();
            _serialPortManager = new SerialPortManager();
            _serialPortManager.CurrentSerialSettings.PortName = Port;
            _serialPortManager.CurrentSerialSettings.BaudRate = 115200;
            _servoController = new ServoController(_serialPortManager);
            _servoController.ReadAngleResponsed += _servoController_ReadAngleResponsed;
        }

        private void _servoController_ReadAngleResponsed(object sender, DataEventArgs<ReadAngleResponse> e)
        {
            if (_servoAngle.ContainsKey(e.Data.ID)) _servoAngle[e.Data.ID] = e.Data.Angle / 10f;
            else _servoAngle.Add(e.Data.ID, e.Data.Angle / 10f);
        }

        public float GetCurrentAngle(byte id)
        {
            
            _servoController.ReadAngle(id);
            while (!_servoAngle.ContainsKey(id)) Thread.Sleep(0);//等待串口返回
            float ang = _servoAngle[id];
            _servoAngle.Remove(id);
            return ang;
        }

        public void SetAngle(byte id, double angle, ushort interval = 5)
        {
            _servoController.MoveOnAngleMode(id, (short)(angle * 10), interval);
        }

        public void SetAngle(byte id, short eangle, ushort interval = 5)
        {
            _servoController.MoveOnAngleMode(id, eangle, interval);
        }

        public byte[] ListServos()
        {
            byte i = 0;
            while (true)
            {
                bool hit = false;
                EventHandler<DataEventArgs<PingResponse>> eventhandler = (sender, e) =>
                {
                    hit = true;
                };
                _servoController.PingResponsed += eventhandler;
                DateTime dt = DateTime.Now;
                _servoController.Ping(i);
                while (!hit)
                {
                    if ((DateTime.Now - dt).TotalSeconds > 2)
                    {

                    }
                }
            }
        }

        public void Abort()
        {
            _servoController.StopListening();
        }

        public void Init()
        {
            _servoController.StartListening();
        }
    }
}
