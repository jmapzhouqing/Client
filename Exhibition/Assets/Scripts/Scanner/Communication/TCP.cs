using System;
using System.Net;
using System.Net.Sockets;

namespace Scanner.Communicate
{
    class TCP : Communication
    {
        private byte[] buffer = new byte[2048];

        private BufferManager bufferManager;

        private int m_numConnections;

        private int receiveBufferSize = 2048;

        const int opsToPreAlloc = 2;

        private int cache_number = 5;

        SocketAsyncEventArgsPool readWritePool;


        public TCP(byte[] heartbeat_data) : base(heartbeat_data)
        {
            try
            {
                bufferManager = new BufferManager(cache_number * receiveBufferSize * 2, receiveBufferSize);

                readWritePool = new SocketAsyncEventArgsPool(cache_number);

                for (int i = 0; i < cache_number; i++)
                {
                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();

                    bufferManager.SetBuffer(args);

                    args.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);

                    readWritePool.Push(args);
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        public override void Connect(IPEndPoint endPoint,IPEndPoint self_end_point,ProtocolType protocol)
        {
            socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            socket.BeginConnect(endPoint, new AsyncCallback(ConnectCallback), null);
        }

        public void ConnectCallback(IAsyncResult result) {
            socket.EndConnect(result);

            Console.WriteLine(socket.Connected);

            //this.IsConnected = socket.Connected;

            socket.BeginReceive(buffer, 0, buffer.Length,SocketFlags.None,new AsyncCallback(ReceiveCallback),null);
        }

        public void ReceiveCallback(IAsyncResult result) {
            int length = socket.EndReceive(result);

            Console.WriteLine(length);

            for (int i = 0; i < length; i++) {
                //Console.WriteLine("{0:x}",buffer[i]);
            }

            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
        }


        public override void SendData(byte[] data)
        {
            if (this.IsConnected)
            {
                socket.Send(data);

                for (int i = 0; i < data.Length; i++) {
                    //Console.WriteLine("{0:x}", data[i]);
                }
            }
        }

        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    this.ProcessConnect(e);
                    break;
                case SocketAsyncOperation.Receive:
                    this.ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    this.ProcessSend(e);
                    break;
                case SocketAsyncOperation.Disconnect:
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        private void ProcessConnect(SocketAsyncEventArgs args)
        {
            this.IsConnected = socket.Connected;
            if (this.IsConnected)
            {
                bool willRaiseEvent = socket.ConnectAsync(args);
                if (!willRaiseEvent)
                {
                    this.ProcessReceive(args);
                }
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {

            }
            else
            {

            }

            if (args.BytesTransferred > 0)
            {
                this.OnDataReceived(args.Buffer, args.Offset, args.BytesTransferred,args.RemoteEndPoint as IPEndPoint);
            }

            bool willRaiseEvent = socket.ReceiveAsync(args);
            if (!willRaiseEvent)
            {
                this.ProcessReceive(args);
            }
        }

        private void ProcessSend(SocketAsyncEventArgs args)
        {
            readWritePool.Push(args);
        }

        private void ProcessDisConnect(SocketAsyncEventArgs args)
        {

        }

        public override void DisConnect()
        {
            if (IsConnected)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (Exception e)
                {

                }
            }
        }

        public override void Connect(Socket socket)
        {
            throw new NotImplementedException();
        }
    }
}
