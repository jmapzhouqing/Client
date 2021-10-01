using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Timers;

namespace Scanner.Communicate
{
    abstract class Communication{

        System.Timers.Timer timer;

        protected ProtocolType protocol;

        protected Socket socket;

        protected bool connected = false;

        protected double receive_ticks = 0;

        public IPEndPoint server_address;

        protected IPEndPoint client_address;

        public delegate void DataReceiveHandle(byte[] buff,int offset,int length);
        public delegate void StatusChangedHandle(DeviceStatus status);
        public delegate void ErrorHandle(ExceptionHandler exception);

        public event DataReceiveHandle DataReceived;
        public event StatusChangedHandle StatusChanged;
        public event ErrorHandle Error;

        private CancellationTokenSource cancel_token_source;
        private CancellationToken cancel_token;

        private Task heart_task;

        public bool IsConnected{
            get {
                return this.connected;
            }
            protected set{
                if (this.connected != value) {
                    this.connected = value;
                    this.OnStatusChanged(this.connected ? DeviceStatus.OnLine : DeviceStatus.OffLine);
                }
            }
        }

        public abstract void Connect(IPEndPoint end_point,IPEndPoint self_end_point,ProtocolType protocol);

        public abstract void DisConnect();

        public abstract void SendData(byte[] data);

        public virtual void ReceiveData() { }

        public virtual void StartHeart(int delay){
            cancel_token_source = new CancellationTokenSource();
            cancel_token = cancel_token_source.Token;

            heart_task = new Task(async() =>{
                while (true){
                    if (cancel_token.IsCancellationRequested) {
                        break;
                    }
                    this.SendData(new byte[]{0x00});
                    await Task.Delay(delay);
                }
            });
            heart_task.Start();

            this.OnTimer(delay);
        }

        private void OnTimer(int interval) {
            timer = new System.Timers.Timer(interval);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += new ElapsedEventHandler(TimerUp);
        }

        private void TimerUp(object sender,ElapsedEventArgs e){
            System.Timers.Timer timer = sender as System.Timers.Timer;
            try{
                double ticks = DateTime.Now.Ticks * Math.Pow(10,-4);

                if (Math.Abs(receive_ticks) < Math.Pow(10, -2)){
                    this.IsConnected = false;
                }else if (Math.Abs(ticks - receive_ticks) < timer.Interval){
                    this.IsConnected = true;
                }else{
                    this.IsConnected = false;
                }
            }catch (Exception exception){
                this.OnError(new ExceptionHandler(exception.Message, ExceptionCode.InternalError));
            }
        }

        protected void UpdateReceiveTicks(){
            this.receive_ticks = DateTime.Now.Ticks * Math.Pow(10, -4);
        }

        public virtual void StopHeart() {
            if (cancel_token_source != null) {
                cancel_token_source.Cancel();
            }
            if (timer != null) {
                timer.Stop();
            }
        }

        protected void OnDataReceived(byte[] buffer,int offset,int length,IPEndPoint remote_address)
        {
            this.server_address = remote_address;
            if(this.DataReceived != null){
                this.DataReceived(buffer,offset,length);
            }
        }

        protected void OnStatusChanged(DeviceStatus status){
            if (this.StatusChanged != null) {
                this.StatusChanged(status);
            }
        }

        protected void OnError(ExceptionHandler exception){
            if (this.Error != null) {
                this.Error(exception);
            }
        }

        protected ExceptionCode HandlerError(SocketError error){
            switch (error) {
                case SocketError.NetworkUnreachable:
                case SocketError.HostUnreachable:
                    return ExceptionCode.NetworkUnreachable;
                case SocketError.ConnectionRefused:
                    return ExceptionCode.DeviceConnectionRefused;
                default:
                    return ExceptionCode.None;
            }
            return ExceptionCode.None;
        }
    }
}