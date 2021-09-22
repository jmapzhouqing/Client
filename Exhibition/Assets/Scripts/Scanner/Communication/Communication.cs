using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Scanner.Communicate
{
    abstract class Communication{

        protected ProtocolType protocol;

        protected Socket socket;

        protected bool connected = false;

        public IPEndPoint server_address;

        protected IPEndPoint client_address;

        public delegate void DataReceiveHandle(byte[] buff,int offset,int length);
        public delegate void StatusChangedHandle(int status);
        public delegate void ErrorHandle(Exception exception);

        public event DataReceiveHandle DataReceived;
        public event StatusChangedHandle StatusChanged;
        public event ErrorHandle Error;

        public bool IsConnected {
            get {
                return this.connected;
            }
            set {
                if (this.connected != value) {
                    this.connected = value;
                    //this.OnStatus(value);
                }
            }
        }

        public abstract void Connect(IPEndPoint end_point,IPEndPoint self_end_point,ProtocolType protocol);

        public abstract void DisConnect();

        public abstract void SendData(byte[] data);

        public virtual void ReceiveData() { }

        public virtual void StartHeart() {

        }

        public virtual void StopHeart() {

        }

        protected void OnDataReceived(byte[] buffer,int offset,int length,IPEndPoint remote_address)
        {
            this.server_address = remote_address;
            if(this.DataReceived != null){
                this.DataReceived(buffer,offset,length);
            }
        }

        protected void OnStatusChanged(int status) {
            if (this.StatusChanged != null) {
                this.StatusChanged(status);
            }
        }

        protected void OnError(Exception exception){
            if (this.Error != null) {
                this.Error(exception);
            }
        }
    }
}