using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using UnityEngine;

using Scanner.Struct;


namespace Scanner.Communicate
{
    class Correspond_UDP : Correspond{
        
        public Correspond_UDP(IPEndPoint client_address,byte[] heartbeat_data):base(heartbeat_data){
            if (client_address != null){
                socket = new Socket(client_address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                socket.Bind(client_address);

                receive_async.RemoteEndPoint = new IPEndPoint(IPAddress.Any,0);
                bool willRaiseEvent = socket.ReceiveFromAsync(receive_async);
                if (!willRaiseEvent){
                    this.ProcessReceive(receive_async);
                }
            }else {
                throw new ArgumentNullException("client_address");
            }
        }

        public override void Connect(IPEndPoint server_address,int timeout){
            if (server_address != null){
                send_async.RemoteEndPoint = server_address;
                this.MonitorConnect(timeout);
            }else{
                throw new ArgumentNullException("server_address");
            }
        }

        public override void Connect(Socket socket){
            this.socket = socket;
        }

        public override void DisConnect(){
            this.StopHeart();
            if (socket != null){
                try{
                    this.Close();
                }catch (Exception e){
                    this.OnError(new ExceptionHandler(e.Message, ExceptionCode.InternalError));
                }
            }
        }

        public override void Close(){
            this.StatusMonitor = DeviceStatus.DisConnect;
            this.socket.Close();
        }

        public override void SendData(byte[] data){
            try{
                Buffer.BlockCopy(data, 0, send_async.Buffer, 0, data.Length);
                send_async.SetBuffer(0, data.Length);

                bool willRaiseEvent = socket.SendToAsync(send_async);

                if(!willRaiseEvent){
                    this.ProcessSend(send_async);
                }
            }
            catch (Exception e){
                this.OnError(new ExceptionHandler(e.Message, ExceptionCode.InternalError));
            }
        }

        protected override void MonitorConnect(int timeout){
            Task monitor_connect = new Task(async () =>{
                double ticks = DateTime.Now.Ticks * Math.Pow(10, -4);

                while ((DateTime.Now.Ticks * Math.Pow(10, -4) - ticks) < timeout && !this.IsConnected()){
                    this.SendData(heartbeat_data);
                    await Task.Delay(100);
                }

                if (!this.IsConnected()){
                    this.StatusMonitor = DeviceStatus.NotConnect;
                    this.OnError(new ExceptionHandler("设备连接超时", ExceptionCode.TimedOut));
                }else{
                    this.StartHeart(200);
                }
            });
            monitor_connect.Start();
        }

        protected override void ProcessConnect(SocketAsyncEventArgs args){
            
        }

        protected override void ProcessDisConnect(SocketAsyncEventArgs args){

        }

        protected override void ProcessReceive(SocketAsyncEventArgs args){
            try
            {
                
                if (args.SocketError == SocketError.Success){
                    if (args.BytesTransferred > 0){
                        this.UpdateReceiveTicks();
                        this.OnDataReceived(args.Buffer, args.Offset, args.BytesTransferred, args.RemoteEndPoint as IPEndPoint);

                        bool willRaiseEvent = socket.ReceiveFromAsync(args);

                        if (!willRaiseEvent){
                            this.ProcessReceive(args);
                        }
                    }else{
                        this.OnError(new ExceptionHandler("连接关闭", ExceptionCode.Shutdown));
                    }
                }else{
                    this.OnError(new ExceptionHandler("数据接收异常", this.HandlerError(args.SocketError)));
                }
            }catch (Exception e){
                this.OnError(new ExceptionHandler(e.Message, ExceptionCode.InternalError));
            }
        }

        protected override void ProcessSend(SocketAsyncEventArgs args){
            if (args.SocketError == SocketError.Success){
                
            }else{
                this.OnError(new ExceptionHandler("数据发送异常", this.HandlerError(args.SocketError)));
            }

        }

        public override void Relink()
        {
            throw new NotImplementedException();
        }
    }
}
