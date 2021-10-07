using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Scanner.Communicate
{
    class UDP : Communication
    {
        private EndPoint remote;
        private byte[] buffer = new byte[2048];

        public UDP(byte[] heartbeat_data) : base(heartbeat_data)
        {

        }

        public override void Connect(IPEndPoint server_point, IPEndPoint self_end_point, ProtocolType protocol)
        {
            socket = new Socket(server_point.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            this.client_address = new IPEndPoint(IPAddress.Any, 0);
            socket.Bind(client_address);

            socket.Connect(server_point);
        }

        public override void SendData(byte[] data)
        {
            /*
            Buffer.BlockCopy(data, 0, send_args.Buffer, 0, data.Length);
            send_args.SetBuffer(0, data.Length);
            bool value = socket.SendAsync(send_args);
            if (!value)
            {
                this.ProcessSend(send_args);
            }*/
        }

        public void Close()
        {
            socket.Shutdown(SocketShutdown.Both);
        }

        public void StartReceive()
        {
            remote = new IPEndPoint(0, 0);
            socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref remote, new AsyncCallback(ReceiveCallback), null);
        }

        private void ConnectCallback(IAsyncResult result)
        {
            socket.EndConnect(result);
            Console.WriteLine("enter");
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            int length = socket.EndReceiveFrom(result, ref remote);

            //socket.Close();

            IPEndPoint point = remote as IPEndPoint;

            byte[] buf = new byte[length];
            Array.Copy(buffer, buf, length);
            //Console.WriteLine(point.Port.ToString());

            //this.OnData(socket,buf,remote as IPEndPoint);

            socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref remote, new AsyncCallback(ReceiveCallback), null);
        }

        public override void DisConnect(){
        }

        public override void Connect(Socket socket)
        {
            throw new NotImplementedException();
        }
    }
}