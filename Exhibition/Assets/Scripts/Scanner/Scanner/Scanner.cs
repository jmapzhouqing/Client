using System;
using System.Collections.Generic;
using Scanner.Communicate;
using System.Net;
using Scanner.Util;
using Scanner.Struct;
using System.Threading.Tasks;
using System.Timers;
using System.Threading;

namespace Scanner.Scanister
{
    abstract class Scanner
    {
        private string device_name;

        protected Dictionary<string, Action<string[]>> reply_process;

        protected CancellationTokenSource process_data_token_source;

        protected CancellationToken process_data_token;

        protected Task process_data_task;

        protected Correspond correspond;

        protected DataBuffer data_buffer;

        protected IPEndPoint server_address;

        protected IPEndPoint client_address = null;

        public DeviceStatus device_status;

        public delegate void DataDecodeCompleteHandle(SectorInfo rays);
        public delegate void StatusChangedHandle(DeviceStatus status);
        public delegate void ErrorHandle(ExceptionHandler exception);

        public event DataDecodeCompleteHandle DataDecodeComplete;
        public event StatusChangedHandle StatusChanged;
        public event ErrorHandle Error;

        protected virtual void OnTimedEvent(object source, ElapsedEventArgs e){

        }

        protected System.Timers.Timer timer;

        public Scanner(string device_name) {
            this.device_name = device_name;
        }

        public bool IsConnected{
            get { return this.correspond.IsConnected();}
        }

        public virtual void Connect(){
            /*
            communication = new Client(new byte[]{0x02});
            communication.DataReceived += ReceiveData;
            communication.StatusChanged += this.OnStatusChanged;
            communication.Error += this.OnError;

            communication.Connect(this.server_address,this.client_address,this.protocol);*/
            correspond.DataReceived += ReceiveData;
            correspond.Error += this.OnError;
            correspond.StatusChanged += this.OnStatusChanged;

            correspond.Connect(this.server_address,5000);
        }

        public virtual void DisConnect(){
            if (correspond != null){
                correspond.DisConnect();
            }
        }
        public virtual void SendData(byte[] data){
            if(this.IsConnected){
                this.correspond.SendData(data);
            }
            //this.correspond.SendData(data);
        }

        protected virtual void ReceiveData(byte[] buffer, int offset, int length){
            data_buffer.PushData(buffer, offset, length);
        }

        public virtual void Start(){
            this.start_scan();
        }

        public virtual void Stop(){
            this.stop_scan();
        }

        private void SearchData() {
            byte[] data = null;
            while ((data = data_buffer.SearchData()) != null){
                this.ProcessData(data);
            }
        }

        #region 设备参数
        public abstract void GetDeviceInfo();

        #endregion

        #region 盘煤仪操作
        protected abstract void scanner_login();

        protected abstract void start_scan();

        protected abstract void stop_scan();

        protected abstract void start_scan_data();

        protected abstract void stop_scan_data();

        protected virtual void get_latest_scan() {

        }
        public abstract byte[] CommandConstruct(string command);

        public abstract void ProcessData(byte[] data);

        #endregion

        protected virtual void StartProcessData(int delay){
            process_data_token_source = new CancellationTokenSource();
            process_data_token = process_data_token_source.Token;

            process_data_task = new Task(() => {
                while (true){
                    if (process_data_token.IsCancellationRequested){
                        return;
                    }
                    byte[] data = data_buffer.SearchData();
                    if (data != null){
                        this.ProcessData(data);
                    }
                    //await Task.Delay(delay);
                }
            });
            process_data_task.Start();
        }

        protected virtual void StopProcessData(){
            if (process_data_token_source != null) {
                process_data_token_source.Cancel();
            }
        }

        protected void OnDataDecodeComplete(SectorInfo data){
            if (this.DataDecodeComplete != null){
                this.DataDecodeComplete(data);
            }
        }

        protected virtual void OnStatusChanged(DeviceStatus status){
            if (this.StatusChanged != null){
                this.StatusChanged(status);
            }
        }

        protected virtual void OnError(ExceptionHandler exception){
            if (this.Error != null){
                this.Error(exception);
            }
        }

        public virtual void Close() {
            this.stop_scan_data();
            this.stop_scan();
            this.StopProcessData();
            this.DisConnect();
        }
    }
}
