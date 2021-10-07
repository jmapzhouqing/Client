using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using UnityEngine;

using Scanner.Struct;

namespace Scanner.Communicate
{
    class Correspond_TCP : Correspond{
        public Correspond_TCP(IPEndPoint client_address,byte[] heartbeat_data):base(heartbeat_data){
            if (client_address != null){
                this.client_address = client_address;
                socket = new Socket(client_address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Blocking = false;
                socket.Bind(client_address);
            }else {
                throw new ArgumentNullException("client_address");
            }
        }

        public override void Connect(IPEndPoint server_address,int timeout){
            if (server_address != null){
                this.server_address = server_address;

                this.MonitorConnect(timeout);

                connect_async.RemoteEndPoint = server_address;
                bool willRaiseEvent = socket.ConnectAsync(connect_async);
                if (!willRaiseEvent){
                    this.ProcessConnect(connect_async);
                }
            }else {
                throw new ArgumentNullException("server_address");
            }
        }

        public override void Connect(Socket socket){
            
        }

        public override void Relink(){
            this.Close();
            this.socket = new Socket(client_address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.socket.Blocking = false;
            this.socket.Bind(client_address);

            this.Connect(this.server_address,5000);
        }

        public override void DisConnect(){
            this.StopHeart();
            if (socket != null){
                try{
                    if (this.IsConnected()){
                        connect_async.DisconnectReuseSocket = true;
                        bool willRaiseEvent = socket.DisconnectAsync(connect_async);
                        if (!willRaiseEvent){
                            this.ProcessDisConnect(connect_async);
                        }
                    }else {
                        this.Close();
                    }
                }catch (Exception e){
                    this.OnError(new ExceptionHandler(e.Message, ExceptionCode.InternalError));
                }
            }
        }

        public override void Close(){
            try{
                this.StopHeart();
                if (this.socket != null) {
                    this.socket.Shutdown(SocketShutdown.Both);
                    this.socket.Close();
                    this.socket = null;
                    this.StatusMonitor = DeviceStatus.DisConnect;

                    Debug.Log(this.StatusMonitor);
                }
            }
            catch (Exception e) {
                Debug.Log(e.Message);
            }
        }

        public override void SendData(byte[] data){
            try{
                if (data != null){
                    Buffer.BlockCopy(data, 0, send_async.Buffer, 0, data.Length);
                    send_async.SetBuffer(0, data.Length);

                    bool willRaiseEvent = socket.SendAsync(send_async);

                    if (!willRaiseEvent){
                        this.ProcessSend(send_async);
                    }
                }
                else {
                    throw new ArgumentNullException("data");
                }
            }catch (Exception e){
                this.OnError(new ExceptionHandler(e.Message, ExceptionCode.InternalError));
            }
        }

        protected override void MonitorConnect(int timeout){
            Task monitor_connect = new Task(async () => {
                double ticks = DateTime.Now.Ticks * Math.Pow(10, -4);

                while ((DateTime.Now.Ticks * Math.Pow(10, -4) - ticks) < timeout && !this.IsConnected()){
                    await Task.Delay(100);
                }

                if (!this.IsConnected()){
                    this.OnError(new ExceptionHandler("设备连接超时", ExceptionCode.TimedOut));
                }else{
                    this.UpdateReceiveTicks();
                    this.StartHeart(500);
                }
            });
            monitor_connect.Start();
        }

        protected override void ProcessConnect(SocketAsyncEventArgs args){
            try{
                if (args.SocketError == SocketError.IsConnected) {
                    this.StatusMonitor = DeviceStatus.Connect;
                    this.OnError(new ExceptionHandler("设备已连接，无需再次连接", this.HandlerError(args.SocketError)));
                }else if (args.SocketError == SocketError.Success){
                    if (args.ConnectSocket.Connected){
                        this.StatusMonitor = DeviceStatus.Connect;

                        this.OnError(new ExceptionHandler("设备连接成功", this.HandlerError(args.SocketError)));

                        bool willRaiseEvent = socket.ReceiveAsync(receive_async);

                        if (!willRaiseEvent){
                            this.ProcessReceive(receive_async);
                        }
                    }
                }else{
                    this.OnError(new ExceptionHandler("设备无法连接", this.HandlerError(args.SocketError)));
                }
            }catch (Exception e){
                Debug.Log(e.Message);
                this.OnError(new ExceptionHandler(e.Message, ExceptionCode.InternalError));
            }
        }

        protected override void ProcessDisConnect(SocketAsyncEventArgs args){
            try{
                if (args.SocketError == SocketError.Success){
                    this.Close();
                    this.OnError(new ExceptionHandler("设备断开连接", this.HandlerError(args.SocketError)));
                }
                else{
                    this.OnError(new ExceptionHandler("通讯连接断开异常", this.HandlerError(args.SocketError)));
                }
            }
            catch (Exception e) {
                Debug.Log(e.Message);
            }
           
        }

        protected override void ProcessReceive(SocketAsyncEventArgs args){
            try{
                if (args.SocketError == SocketError.Success){
                    if (args.BytesTransferred > 0){
                        this.UpdateReceiveTicks();
                        this.OnDataReceived(args.Buffer, args.Offset, args.BytesTransferred, args.RemoteEndPoint as IPEndPoint);

                        bool willRaiseEvent = socket.ReceiveAsync(args);

                        if(!willRaiseEvent){
                            this.ProcessReceive(args);
                        }
                    }else{
                        this.OnError(new ExceptionHandler("连接关闭", ExceptionCode.Shutdown));
                    }
                }else{
                    this.OnError(new ExceptionHandler("数据接收异常", this.HandlerError(args.SocketError)));
                }
            }catch (Exception e){
                Debug.Log("Enter Receive Error");
                this.OnError(new ExceptionHandler(e.Message, ExceptionCode.InternalError));
            }
        }

        protected override void ProcessSend(SocketAsyncEventArgs args){
            if (args.SocketError == SocketError.Success){

            }else{
                this.OnError(new ExceptionHandler("数据发送异常", this.HandlerError(args.SocketError)));
            }
        }
    }
}
