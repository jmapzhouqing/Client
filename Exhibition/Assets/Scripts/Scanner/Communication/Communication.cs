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

        public delegate void data_receive_handle(byte[] buff,int offset,int length);
        public delegate void status_handle(bool status);

        public event data_receive_handle DataReceiveHandler;
        public event status_handle StatusHandler;

        public bool IsConnected {
            get {
                return this.connected;
            }
            set {
                if (this.connected != value) {
                    this.connected = value;
                    this.OnStatus(value);
                }
            }
        }

        public abstract void Connect(IPEndPoint end_point,IPEndPoint self_end_point,ProtocolType protocol);

        public abstract void DisConnect();


        public abstract void SendData(byte[] data);

        public virtual void ReceiveData() { }

        public virtual void StartHeart() { }

        protected void OnData(byte[] buffer,int offset,int length,IPEndPoint remote_address)
        {
            this.server_address = remote_address;
            if (this.DataReceiveHandler != null) {
                this.DataReceiveHandler(buffer,offset,length);
            }
        }

        protected void OnStatus(bool status) {
            if (this.StatusHandler != null) {
                this.StatusHandler(status);
            }
        }
    }
}