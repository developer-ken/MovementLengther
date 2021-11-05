﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HardwareInterface.UART
{
    public class UART : ISensor
    {
        public SerialPort port;
        Thread recv;
        bool runflag = true;
        event Action<string> LineReceivedEvent;
        string receive_tmp = "";

        public UART(string portname)
        {
            port = new SerialPort(portname);
            recv = new Thread(new ThreadStart(() =>
            {
                while ((!port.IsOpen) && runflag) Thread.Sleep(0);//等待端口打开
                while (runflag)
                {
                    var result = port.ReadLine();
                    receive_tmp = result;
                    LineReceivedEvent?.Invoke(result);
                }
            }));
        }

        public static string[] ListPorts()
        {
            return SerialPort.GetPortNames();
        }

        public string ReadLine()
        {
            while (receive_tmp.Length <= 0) Thread.Sleep(0);
            string rt = receive_tmp;
            receive_tmp = "";
            return rt;
        }

        public void SendLine(string data)
        {
            port.WriteLine(data);
        }

        public void Send(string data)
        {
            port.Write(data);
        }

        public void Abort()
        {
            runflag = false;
            port.Close();
        }

        public void Init()
        {
            runflag = true;
            port.Open();
            while ((!port.IsOpen) && runflag) Thread.Sleep(0);//等待端口打开
            recv.Start();
        }
    }
}
