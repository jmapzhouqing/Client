using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Scanner.Communicate
{
    class Server
    {

        internal class AsyncUserToken{
            public Socket Socket { get; set; }
        }

        private Socket server;

        private SocketAsyncEventArgs accept_args;

        private BufferManager bufferManager;

        private int m_numConnections;

        private int receiveBufferSize = 10240;

        const int opsToPreAlloc = 2;

        private int cache_number = 5;

        SocketAsyncEventArgsPool readWritePool;

        private ProtocolType protocol;

        private IPEndPoint server_address;


        Dictionary<string, Socket> socket_dic;
        public Server(){
            try
            {
                bufferManager = new BufferManager(cache_number * receiveBufferSize * 2, receiveBufferSize);

                readWritePool = new SocketAsyncEventArgsPool(cache_number);

                for (int i = 0; i < cache_number; i++)
                {
                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();

                    bufferManager.SetBuffer(args);

                    args.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                    args.UserToken = new AsyncUserToken();

                    readWritePool.Push(args);
                } 
            }
            catch (Exception e) {

            }
           
        }

        public void Start(int port,ProtocolType protocol)
        {
            this.protocol = protocol;

            if (protocol.Equals(ProtocolType.Tcp)){
                server_address = new IPEndPoint(IPAddress.Any, port);
                server = new Socket(server_address.AddressFamily, SocketType.Stream, protocol);
                this.protocol = protocol;

                server.Bind(server_address);
                server.Listen(0);

                accept_args = new SocketAsyncEventArgs();
                accept_args.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                StartAccept(accept_args);
            }else if (protocol.Equals(ProtocolType.Udp)){
                server_address = new IPEndPoint(IPAddress.Parse("192.168.90.255"), 8000);
                server = new Socket(server_address.AddressFamily, SocketType.Dgram, protocol);
                server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            }
        }

        public void StartAccept(SocketAsyncEventArgs accept_args){
            accept_args.AcceptSocket = null;
            bool willRaiseEvent = server.AcceptAsync(accept_args);
            if (!willRaiseEvent){
                ProcessAccept(accept_args);
            }
        }

        public void SendData(byte[] data) {
            if (this.protocol.Equals(ProtocolType.Tcp)){
                foreach (KeyValuePair<string, Socket> item in socket_dic)
                {
                    item.Value.Send(data, data.Length, SocketFlags.None);
                }
            }else if (this.protocol.Equals(ProtocolType.Udp)){
                server.SendTo(data, server_address);
            }
        }

        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    ProcessAccept(e);
                    break;
                case SocketAsyncOperation.Receive:
                case SocketAsyncOperation.ReceiveFrom:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                case SocketAsyncOperation.SendTo:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }


        private void ProcessAccept(SocketAsyncEventArgs e){
            /*
            Interlocked.Increment(ref m_numConnectedSockets);
            Console.WriteLine("Client connection accepted. There are {0} clients connected to the server",
                m_numConnectedSockets);*/

            SocketAsyncEventArgs readEventArgs = readWritePool.Pop();
            
            ((AsyncUserToken)readEventArgs.UserToken).Socket = e.AcceptSocket;

            socket_dic.Add("1",e.AcceptSocket);

            bool willRaiseEvent = e.AcceptSocket.ReceiveAsync(readEventArgs);
            if (!willRaiseEvent){
                ProcessReceive(readEventArgs);
            }

            StartAccept(e);
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success){
                bool willRaiseEvent = ((AsyncUserToken)e.UserToken).Socket.ReceiveAsync(e);
                if (!willRaiseEvent){
                    this.ProcessReceive(e);
                }
            }else{
                //CloseClientSocket(e);
            }   
        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                bool willRaiseEvent = e.ConnectSocket.ReceiveAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessReceive(e);
                }
            }
            else
            {
                //CloseClientSocket(e);
            }
        }

    }
}
