using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using Scanner.Util;
using Scanner.Struct;

namespace Scanner.Communicate
{
    abstract class DataTransmission
    {
        private ProtocolType protocol;

        protected Correspond correspond;

        protected Dictionary<string, Action<string[]>> reply_process;

        protected CancellationTokenSource process_data_token_source;

        protected CancellationToken process_data_token;

        protected Task process_data_task;

        protected DataBuffer data_buffer;

        protected IPEndPoint server_address;

        protected IPEndPoint client_address = null;

        public delegate void StatusChangedHandle(DeviceStatus status);
        public delegate void ErrorHandle(ExceptionHandler exception);

        public event StatusChangedHandle StatusChanged;
        public event ErrorHandle Error;

        private int timeout;

        public DataTransmission(string ip, int port){
            try
            {
                this.server_address = new IPEndPoint(IPAddress.Parse(ip), port);
                data_buffer = new DataBuffer(1024000, SocketType.Stream, new byte[] { 0x02 }, new byte[] { 0x03 });
                this.CreateReplayProcess();
            }
            catch (Exception e)
            {

            }
        }

        public DataTransmission()
        {
            data_buffer = new DataBuffer(1024000, SocketType.Stream, new byte[] { 0x02 }, new byte[] { 0x03 });
            this.CreateReplayProcess();
        }

        protected abstract void CreateReplayProcess();


        public virtual void Connect(int timeout){
            this.timeout = timeout;
            this.StartProcessData(100);
            
            correspond.DataReceived += ReceiveData;
            correspond.StatusChanged += this.OnStatusChanged;
            correspond.Error += this.OnError;

            correspond.Connect(this.server_address,timeout);
        }

        public void ReLink() {
            correspond?.Connect(this.server_address,this.timeout);
        }

        public void Connect(Socket socket){
            /*
            this.StartProcessData(100);
            byte[] heartbeat_data = this.CommandConstruct("Heart Request");
            communication = new Client(heartbeat_data);
            communication.DataReceived += ReceiveData;
            communication.StatusChanged += this.OnStatusChanged;
            communication.Error += this.OnError;

            communication.Connect(socket);*/
        }

        public void DisConnect()
        {
            this.StopProcessData();
            if (correspond != null){
                correspond.DisConnect();
            }
        }

        public void SendData(string command)
        {
            if (correspond != null && this.correspond.IsConnected())
            {
                this.correspond.SendData(this.CommandConstruct(command));
            }
        }

        public byte[] CommandConstruct(string command)
        {
            List<byte> data = new List<byte>();
            data.Add(0x02);
            byte[] command_data = Encoding.ASCII.GetBytes(command);

            data.AddRange(command_data);

            data.Add(0x03);
            return data.ToArray();
        }

        private void ReceiveData(byte[] buffer, int offset, int length)
        {
            data_buffer.PushData(buffer, offset, length);
        }

        protected abstract void ProcessData(byte[] data);


        public void StartProcessData(int delay)
        {
            process_data_token_source = new CancellationTokenSource();
            process_data_token = process_data_token_source.Token;

            process_data_task = new Task(async () => {
                while (true){
                    if (process_data_token.IsCancellationRequested){
                        return;
                    }
                    byte[] data = data_buffer.SearchData();
                    if (data != null){
                        this.ProcessData(data);
                    }
                    await Task.Delay(delay);
                }
            });
            process_data_task.Start();
        }

        public void StopProcessData()
        {
            if (process_data_token_source != null)
            {
                process_data_token_source.Cancel();
            }
        }

        protected void OnStatusChanged(DeviceStatus status)
        {
            if (this.StatusChanged != null)
            {
                this.StatusChanged(status);
            }
        }

        protected void OnError(ExceptionHandler exception){
            if (this.Error != null){
                this.Error(exception);
            }
        }
    }
}
