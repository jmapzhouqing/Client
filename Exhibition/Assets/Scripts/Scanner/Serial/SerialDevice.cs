using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO.Ports;
using Scanner.Util;
using System.Threading;
using Scanner.Struct;

namespace Scanner.Serial
{
    abstract class SerialDevice<T>
    {
        private SerialCorrespond correspond;

        protected DataBuffer data_buffer;

        private int used_index = 0;

        private Task task;

        protected byte address;

        protected CancellationTokenSource process_data_token_source;

        protected CancellationToken process_data_token;

        protected Task process_data_task;

        public delegate void DataDecodeCompleteHandle(T rays);
        public delegate void StatusChangedHandle(DeviceStatus status);
        public delegate void ErrorHandle(ExceptionHandler exception);

        public event DataDecodeCompleteHandle DataDecodeComplete;
        public event StatusChangedHandle StatusChanged;
        public event ErrorHandle Error;

        public SerialDevice(String name,Int32 baud_rate,Parity parity = Parity.None,Int32 data_bits = 8,StopBits stop_bits = StopBits.One){
            correspond = new SerialCorrespond(name, baud_rate, parity, data_bits, stop_bits);
            correspond.DataReceived += ReceiveData;
            correspond.Error += OnError;
            correspond.StatusChanged += OnStatusChanged;
            //buffer = new DataBuffer(102400, SocketType.Stream);
        }

        public void Open(){
            if (correspond != null){
                this.StartReceiveData(100);
                this.StartProcessData(100);
                correspond.Open();
            }
        }

        public virtual void Close() {
            this.StopProcessData();
            this.StopReceiveData();
            if (correspond != null){
                correspond.Close();
            }
        }

        public void SendData(byte[] data){
            if (correspond != null){
                correspond.SendData(data);
            }
        }
        public abstract void ProcessData(byte[] data);

        protected virtual void ReceiveData(byte[] buffer, int offset, int length){
            data_buffer.PushData(buffer, offset, length);
        }

        public void StartReceiveData(int delay){
            if (correspond != null){
                correspond.StartReceiveData(delay);
            }
        }

        public void StopReceiveData(){
            if (correspond != null){
                correspond.StopReceiveData();
            }
        }

        protected virtual void StartProcessData(int delay)
        {
            process_data_token_source = new CancellationTokenSource();
            process_data_token = process_data_token_source.Token;

            process_data_task = new Task(() => {
                while (true)
                {
                    if (process_data_token.IsCancellationRequested){
                        return;
                    }
                    byte[] data = data_buffer.SearchData();
                    if (data != null)
                    {
                       this.ProcessData(data);
                    }
                    //await Task.Delay(delay);
                }
            });
            process_data_task.Start();
        }

        protected virtual void StopProcessData()
        {
            if (process_data_token_source != null)
            {
                process_data_token_source.Cancel();
            }
        }

        protected void OnDataDecodeComplete(T data){
            if (this.DataDecodeComplete != null)
            {
                this.DataDecodeComplete(data);
            }
        }

        protected virtual void OnStatusChanged(DeviceStatus status)
        {
            if (this.StatusChanged != null)
            {
                this.StatusChanged(status);
            }
        }

        protected virtual void OnError(ExceptionHandler exception)
        {
            if (this.Error != null)
            {
                this.Error(exception);
            }
        }
    }
}
