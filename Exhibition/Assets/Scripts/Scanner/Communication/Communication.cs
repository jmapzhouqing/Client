using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Timers;

using Scanner.Struct;

namespace Scanner.Communicate
{
    abstract class Communication
    {
        private System.Timers.Timer heart_timer;

        private System.Timers.Timer ticks_timer;

        protected byte[] heartbeat_data;

        protected ProtocolType protocol;

        protected Socket socket;

        protected bool connected = false;

        private double receive_ticks;

        public IPEndPoint server_address;

        protected IPEndPoint client_address;

        private bool heart_work = false;


        public delegate void DataReceiveHandle(byte[] buff, int offset, int length);
        public delegate void StatusChangedHandle(DeviceStatus status);
        public delegate void ErrorHandle(ExceptionHandler exception);

        public event DataReceiveHandle DataReceived;
        public event StatusChangedHandle StatusChanged;
        public event ErrorHandle Error;


        public bool IsConnected
        {
            get
            {
                return this.connected;
            }
            protected set
            {
                if (this.connected != value){
                    this.connected = value;
                    this.OnStatusChanged(this.connected ? DeviceStatus.OnLine : DeviceStatus.OffLine);
                }
            }
        }

        protected double LastReceiveTicks
        {
            get
            {
                return this.receive_ticks;
            }

            set
            {
                this.IsConnected = true;
                this.receive_ticks = value;
                this.StartMonitorTicks(1000);
            }
        }

        public Communication(byte[] heartbeat_data)
        {
            this.heartbeat_data = heartbeat_data;
        }

        public abstract void Connect(IPEndPoint end_point, IPEndPoint self_end_point, ProtocolType protocol);

        public abstract void Connect(Socket socket);

        public abstract void DisConnect();

        public abstract void SendData(byte[] data);

        public virtual void ReceiveData() { }


        protected void StartHeart(int interval)
        {
            heart_timer = new System.Timers.Timer(interval);
            heart_timer.AutoReset = true;
            heart_timer.Enabled = true;
            heart_timer.Elapsed += new ElapsedEventHandler(HeartTimerUp);
        }

        public virtual void StopHeart()
        {
            if (heart_timer != null)
            {
                heart_timer.Stop();
                heart_timer.Close();
            }
        }

        private void HeartTimerUp(object sender, ElapsedEventArgs e)
        {
            System.Timers.Timer timer = sender as System.Timers.Timer;
            try
            {
                double ticks = DateTime.Now.Ticks * Math.Pow(10, -4);

                if (Math.Abs(ticks - receive_ticks) > timer.Interval)
                {
                    this.SendData(heartbeat_data);
                }
            }
            catch (Exception exception)
            {
                Debug.Log("Enter HeartBeat Error");
                this.OnError(new ExceptionHandler(exception.Message, ExceptionCode.InternalError));
            }
        }

        private void StartMonitorTicks(int interval)
        {
            if (ticks_timer != null)
            {
                ticks_timer.Stop();
                ticks_timer.Close();
            }
            ticks_timer = new System.Timers.Timer(interval);
            ticks_timer.Enabled = true;
            ticks_timer.Elapsed += new ElapsedEventHandler(TicksTimerUp);
        }

        private void TicksTimerUp(object sender, ElapsedEventArgs e)
        {
            this.IsConnected = false;
        }
        public void UpdateReceiveTicks()
        {
            this.LastReceiveTicks = DateTime.Now.Ticks * Math.Pow(10, -4);
        }

        protected void OnDataReceived(byte[] buffer, int offset, int length, IPEndPoint remote_address)
        {
            this.server_address = remote_address;
            if (this.DataReceived != null)
            {
                this.DataReceived(buffer, offset, length);
            }
        }

        protected void OnStatusChanged(DeviceStatus status)
        {
            if (this.StatusChanged != null)
            {
                this.StatusChanged(status);
            }
        }

        protected void OnError(ExceptionHandler exception)
        {
            if (this.Error != null)
            {
                this.Error(exception);
            }
        }

        protected ExceptionCode HandlerError(SocketError error)
        {
            Console.WriteLine(error);
            switch (error)
            {
                case SocketError.NetworkUnreachable:
                case SocketError.HostUnreachable:
                    return ExceptionCode.NetworkUnreachable;
                case SocketError.ConnectionRefused:
                    return ExceptionCode.ConnectionRefused;
                case SocketError.OperationAborted:
                    return ExceptionCode.OperationAborted;
                case SocketError.NotConnected:
                    return ExceptionCode.NotConnected;
                case SocketError.ConnectionAborted:
                    return ExceptionCode.ConnectionAborted;
                default:
                    return ExceptionCode.None;
            }
        }
    }
}