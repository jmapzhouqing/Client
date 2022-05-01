using System;
using System.Timers;
using System.Net;
using System.Net.Sockets;

using UnityEngine;

using Scanner.Struct;

namespace Scanner.Communicate
{
    abstract class Correspond{
        protected string device_name;

        private double receive_ticks;

        private bool connected = false;

        private Timer heart_timer;

        private Timer ticks_timer;

        private bool heart_work = false;

        private BufferManager bufferManager;

        private int receiveBufferSize = 10240;

        protected byte[] heartbeat_data;

        protected IPEndPoint server_address;

        protected IPEndPoint client_address;

        protected Socket socket;

        protected SocketAsyncEventArgs send_async;
        protected SocketAsyncEventArgs receive_async;
        protected SocketAsyncEventArgs connect_async;

        public delegate void DataReceiveHandle(byte[] buff, int offset, int length);
        public delegate void StatusChangedHandle(DeviceStatus status);
        public delegate void ErrorHandle(ExceptionHandler exception);

        public event DataReceiveHandle DataReceived;
        public event StatusChangedHandle StatusChanged;
        public event ErrorHandle Error;

        private DeviceStatus transfer_status;

        //private AutoResetEvent manual_reset;

        public bool IsConnected(){
            return (this.transfer_status.Equals(DeviceStatus.NotConnect) || this.transfer_status.Equals(DeviceStatus.DisConnect) || this.transfer_status.Equals(DeviceStatus.OffLine)) ? false : true;
        }

        protected DeviceStatus StatusMonitor {
            get { return this.transfer_status; }
            set {
                if(!this.transfer_status.Equals(value)){
                    this.transfer_status = value;
                    this.OnStatusChanged(transfer_status);
                }
            }
        }

        protected double LastReceiveTicks{
            get{
                return this.receive_ticks;
            }

            set{
                if(this.transfer_status.Equals(DeviceStatus.NotConnect)){
                    this.StatusMonitor = DeviceStatus.Connect;
                }else {
                    if (!this.transfer_status.Equals(DeviceStatus.Working)){
                        this.StatusMonitor = DeviceStatus.OnLine;
                    }
                }

                this.receive_ticks = value;

                this.StartMonitorTicks(1000);
            }
        }

        public Correspond(byte[] heartbeat_data){
            this.transfer_status = DeviceStatus.NotConnect;
            this.heartbeat_data = heartbeat_data;
            bufferManager = new BufferManager(2 * receiveBufferSize, receiveBufferSize);

            send_async = new SocketAsyncEventArgs();
            bufferManager.SetBuffer(send_async);
            send_async.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);

            receive_async = new SocketAsyncEventArgs();
            bufferManager.SetBuffer(receive_async);
            receive_async.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);

            connect_async = new SocketAsyncEventArgs();
            connect_async.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
        }

        public abstract void Connect(IPEndPoint server_address,int timeout);

        public abstract void Connect(Socket socket);

        public abstract void DisConnect();

        public abstract void Relink();

        public abstract void Close();


        public abstract void SendData(byte[] data);

        protected abstract void MonitorConnect(int timeout);

        protected void StartHeart(int interval){
            heart_timer = new Timer(interval);
            heart_timer.AutoReset = true;
            heart_timer.Enabled = true;
            heart_timer.Elapsed += new ElapsedEventHandler(HeartTimerUp);
        }

        public virtual void StopHeart(){
            if (heart_timer != null){
                heart_timer.Stop();
                heart_timer.Close();
            }
        }

        private void HeartTimerUp(object sender, ElapsedEventArgs e)
        {
            Timer timer = sender as Timer;
            try{
                double ticks = DateTime.Now.Ticks * Math.Pow(10, -4);

                if (Math.Abs(ticks - receive_ticks) > timer.Interval){
                    this.SendData(heartbeat_data);
                }
            }catch (Exception exception){
                Debug.Log("Enter HeartBeat Error");
                this.OnError(new ExceptionHandler(exception.Message, ExceptionCode.InternalError));
            }
        }
        private void StartMonitorTicks(int interval){
            if (ticks_timer != null){
                ticks_timer.Stop();
                ticks_timer.Close();
            }
            ticks_timer = new Timer(interval);
            ticks_timer.AutoReset = false;
            ticks_timer.Enabled = true;
            ticks_timer.Elapsed += new ElapsedEventHandler(TicksTimerUp);
        }

        private void TicksTimerUp(object sender, ElapsedEventArgs e){
            if (!this.transfer_status.Equals(DeviceStatus.NotConnect) && !this.transfer_status.Equals(DeviceStatus.DisConnect)) {
                this.StatusMonitor = DeviceStatus.OffLine;
            }
        }
        public void UpdateReceiveTicks(){
            this.LastReceiveTicks = DateTime.Now.Ticks * Math.Pow(10, -4);
        }

        protected void OnDataReceived(byte[] buffer, int offset, int length, IPEndPoint remote_address){
            if (this.DataReceived != null){
                this.DataReceived(buffer, offset, length);
            }
        }

        protected void OnStatusChanged(DeviceStatus status){
            if (this.StatusChanged != null){
                this.StatusChanged(status);
            }
        }

        protected void OnError(ExceptionHandler exception){
            if (this.Error != null){
                this.Error(exception);
            }
        }

        protected ExceptionCode HandlerError(SocketError error){
            switch (error){
                case SocketError.Success:
                    return ExceptionCode.Success;
                case SocketError.TimedOut:
                    return ExceptionCode.TimedOut;
                case SocketError.NetworkUnreachable:
                    return ExceptionCode.NetworkUnreachable;
                case SocketError.HostUnreachable:
                    return ExceptionCode.HostUnreachable;
                case SocketError.ConnectionRefused:
                    return ExceptionCode.ConnectionRefused;
                case SocketError.OperationAborted:
                    return ExceptionCode.OperationAborted;
                case SocketError.NotConnected:
                    return ExceptionCode.NotConnected;
                case SocketError.ConnectionAborted:
                    return ExceptionCode.ConnectionAborted;
                case SocketError.ConnectionReset:
                    return ExceptionCode.ConnectionReset;
                case SocketError.Shutdown:
                    return ExceptionCode.Shutdown;
                default:
                    return ExceptionCode.None;
            }
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    this.ProcessConnect(e);
                    break;
                case SocketAsyncOperation.Receive:
                case SocketAsyncOperation.ReceiveFrom:
                    this.ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                case SocketAsyncOperation.SendTo:
                    this.ProcessSend(e);
                    break;
                case SocketAsyncOperation.Disconnect:
                    this.ProcessDisConnect(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        protected abstract void ProcessConnect(SocketAsyncEventArgs args);

        protected abstract void ProcessReceive(SocketAsyncEventArgs args);

        protected abstract void ProcessSend(SocketAsyncEventArgs args);

        protected abstract void ProcessDisConnect(SocketAsyncEventArgs args);
        /*
        ~Client()
        {
            this.DisConnect();
            bufferManager.FreeBuffer(send_args);
            bufferManager.FreeBuffer(receive_args);

            readWritePool.Push(send_args);
            readWritePool.Push(receive_args);
            this.socket = null;
        }*/
    }
}
