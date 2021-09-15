using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Scanner.Communicate
{
    class Client : Communication
    {
        private BufferManager bufferManager;

        private int m_numConnections;

        private int receiveBufferSize = 10240;

        const int opsToPreAlloc = 2;

        private int cache_number = 5;

        SocketAsyncEventArgsPool readWritePool;

        private SocketAsyncEventArgs receive_args;
        private SocketAsyncEventArgs send_args;
        private SocketAsyncEventArgs connect_args;
        public Client(){
            try{
                bufferManager = new BufferManager(cache_number * receiveBufferSize * 2, receiveBufferSize);

                readWritePool = new SocketAsyncEventArgsPool(cache_number);

                for (int i = 0; i < cache_number; i++){
                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();

                    bufferManager.SetBuffer(args);

                    args.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);

                    readWritePool.Push(args);
                }

                receive_args = readWritePool.Pop();
                send_args = readWritePool.Pop();

                connect_args = new SocketAsyncEventArgs();
                connect_args.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            }
            catch (Exception e) {
                throw (e);
            }
        }

        public override void Connect(IPEndPoint server_address, IPEndPoint client_address, ProtocolType protocol){
            try{
                if (client_address == null) {
                    client_address = new IPEndPoint(IPAddress.Any,0);
                }

                if (server_address == null) {
                    server_address = new IPEndPoint(IPAddress.Any, 0);
                }

                this.protocol = protocol;
                this.client_address = client_address;
                this.server_address = server_address;
                if (protocol.Equals(ProtocolType.Udp)){
                    socket = new Socket(client_address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                }else if (protocol.Equals(ProtocolType.Tcp)){
                    socket = new Socket(client_address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                }
                socket.Bind(client_address);

                /*
                connect_args.RemoteEndPoint = server_address;
                bool willRaiseEvent = socket.ConnectAsync(connect_args);
                if (!willRaiseEvent){
                    this.ProcessConnect(connect_args);
                }*/

                bool willRaiseEvent = false;

                if (this.protocol.Equals(ProtocolType.Udp))
                {
                    receive_args.RemoteEndPoint = server_address;
                    willRaiseEvent = socket.ReceiveFromAsync(receive_args);
                    if (!willRaiseEvent){
                        this.ProcessReceive(receive_args);
                    }
                }else{
                    connect_args.RemoteEndPoint = server_address;
                    willRaiseEvent = socket.ConnectAsync(connect_args);
                    if (!willRaiseEvent)
                    {
                        this.ProcessConnect(connect_args);
                    }
                }


            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        public override void SendData(byte[] data) {
            send_args.RemoteEndPoint = this.server_address;
            //if (this.IsConnected){
                //SocketAsyncEventArgs args = readWritePool.Pop();
                Buffer.BlockCopy(data, 0, send_args.Buffer, 0, data.Length);
                send_args.SetBuffer(0, data.Length);
                bool willRaiseEvent = false;
                if (this.protocol.Equals(ProtocolType.Udp)) {
                    willRaiseEvent = socket.SendToAsync(send_args);
                } else if (this.protocol.Equals(ProtocolType.Tcp)){
                    willRaiseEvent = socket.SendAsync(send_args);
                }

                if (!willRaiseEvent)
                {
                    this.ProcessSend(send_args);
                }
            //}
        }
    
        void IO_Completed(object sender, SocketAsyncEventArgs e)
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
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        private void ProcessConnect(SocketAsyncEventArgs args) {
            try
            {
                bool status = socket.Connected;

                receive_args.RemoteEndPoint = args.RemoteEndPoint;
                //if (status){
                bool willRaiseEvent = false;
                if (this.protocol.Equals(ProtocolType.Tcp))
                {
                    willRaiseEvent = socket.ReceiveAsync(receive_args);
                }
                else if (this.protocol.Equals(ProtocolType.Udp))
                {
                    willRaiseEvent = socket.ReceiveFromAsync(receive_args);
                }

                if (!willRaiseEvent)
                {
                    this.ProcessReceive(receive_args);
                }
                //}
                this.IsConnected = status;
            }
            catch (Exception e) {
                Debug.Log(e.Message);
            }
            
        }

        private void ProcessReceive(SocketAsyncEventArgs args){
            if (args.SocketError == SocketError.Success){

                if (args.BytesTransferred > 0)
                {
                    this.OnData(args.Buffer, args.Offset, args.BytesTransferred,args.RemoteEndPoint as IPEndPoint);
                }

                bool willRaiseEvent = false;

                if (this.protocol.Equals(ProtocolType.Tcp)){
                    willRaiseEvent = socket.ReceiveAsync(receive_args);
                }else if (this.protocol.Equals(ProtocolType.Udp)){
                    willRaiseEvent = socket.ReceiveFromAsync(receive_args);
                }

                if (!willRaiseEvent)
                {
                    this.ProcessReceive(args);
                }
            }else{

            }
        }

        private void ProcessSend(SocketAsyncEventArgs args){
            if (args.SocketError == SocketError.Success)
            {

            }else {

            }
        }

        private void ProcessDisConnect(SocketAsyncEventArgs args){
            
        }

        public override void DisConnect(){
            if(socket!=null){
                try{
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }catch (Exception e) {
                    Console.WriteLine(e.Message);
                }
            }
        }

        ~Client()
        {
            this.DisConnect();
            bufferManager.FreeBuffer(send_args);
            bufferManager.FreeBuffer(receive_args);

            readWritePool.Push(send_args);
            readWritePool.Push(send_args);
        }
    }
}
