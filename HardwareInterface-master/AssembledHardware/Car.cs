using HardwareInterface.Motor;
using HardwareInterface.Servo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareInterface.AssembledHardware
{
    public class Car
    {
        const float WidthOfDrivingWheel = 16.5f;//后驱动轮宽度
        const int L = 2, R = 1;//左右轮电机ID
        const string ServoDev = "/dev/ttyCH9344USB0";//转向舵机设备
        const string MotorDev = "/dev/ttyCH9344USB1";//闭环电机设备
        const double TurnConst = 62.5 / 30.0;//转动角系数
        const double WheelSize = 6.725 * 3.141593;//轮胎直径cm * 3.141593
        const double CenterOffset = -17.5;//转向舵机中点偏移

        FSServoChain dservo;
        UARTMotorChain motors;

        /// <summary>
        /// 当前设定转速 Current rotation speed
        /// </summary>
        public ushort CurrentTurnRate { private set; get; }

        struct SpeedSet { public ushort L, R; }

        public enum TurnDirection { Left, Right };

        /// <summary>
        /// 小车底盘系统  The car
        /// <para>包含：两个串口磁编码电机，一个总线转向舵机</para>
        /// <para>Includes 2 uart motor and a uart servo for turning</para>
        /// </summary>
        public Car()
        {
            dservo = new FSServoChain(ServoDev);
            motors = new UARTMotorChain(MotorDev);
            dservo.Init();
            motors.Init();
        }

        private static SpeedSet GetTurnSpeed(TurnDirection dir, double turnangle, ushort avgTurnRate)
        {
            double delta = Math.Tan(turnangle) * WidthOfDrivingWheel;
            SpeedSet result = new SpeedSet();
            if (dir == TurnDirection.Left)
            {
                result.L = (ushort)(avgTurnRate - (delta / 2));
                result.R = (ushort)(avgTurnRate + (delta / 2));
            }
            else
            {
                result.L = (ushort)(avgTurnRate + (delta / 2));
                result.R = (ushort)(avgTurnRate - (delta / 2));
            }
            return result;
        }

        /// <summary>
        /// 定速前进  Go forward at a certain speed
        /// </summary>
        /// <param name="speed">速度 Speed (cm/s)</param>
        public void GoForward(double speed)
        {
            dservo.SetAngle(0, CenterOffset);
            CurrentTurnRate = (ushort)(speed * 60 / WheelSize);
            motors.MotorRunS(L, CurrentTurnRate);
            motors.MotorRunN(R, CurrentTurnRate);
        }

        /// <summary>
        /// 定速后退 Go backward at a certain speed
        /// </summary>
        /// <param name="speed">速度 Speed (cm/s)</param>
        public void GoBackward(double speed)
        {
            dservo.SetAngle(0, CenterOffset);
            CurrentTurnRate = (ushort)(speed * 60 / WheelSize);
            motors.MotorRunN(L, CurrentTurnRate);
            motors.MotorRunS(R, CurrentTurnRate);
        }

        /// <summary>
        /// 原地停下 Break
        /// </summary>
        public void Stop()
        {
            dservo.SetAngle(0, CenterOffset);
            motors.StopAll();
        }

        /// <summary>
        /// 保持当前速率定速转弯 Turn at current speed
        /// </summary>
        /// <param name="dir">转弯方向 Which direction to turn to</param>
        /// <param name="angle">转动角度 The angle to turn</param>
        public void Turn(TurnDirection dir, double angle)
        {
            var ts = GetTurnSpeed(dir, angle, CurrentTurnRate);
            angle = dir == TurnDirection.Right ? -angle : angle;
            dservo.SetAngle(0, (double)(CenterOffset + (angle * TurnConst)));
            motors.MotorRunS(L, ts.L);
            motors.MotorRunN(R, ts.R);
        }
    }
}
