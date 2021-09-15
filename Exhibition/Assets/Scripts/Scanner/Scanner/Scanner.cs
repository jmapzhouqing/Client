using System;
using System.Collections.Generic;
using Scanner.Communicate;
using System.Net;
using Scanner.Util;
using Scanner.Struct;
using System.Threading.Tasks;
using System.Timers;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Scanner.Scanister
{
    abstract class Scanner
    {
        protected Task deal_data_task;

        protected ProtocolType protocol;

        protected Communication communication;

        protected DataBuffer data_buffer;

        protected IPEndPoint end_point;

        protected IPEndPoint self_end_point = null;

        protected CancellationTokenSource deal_data_token_source;

        protected CancellationToken deal_data_token;

        protected float rotation;

        protected bool encoder_run = false;

        public int receive_size = 0;

        public delegate void size_change(int data);

        public size_change datacheng;

        public float Rotation {
            get {
                return this.rotation;
            }
            set {
                if (this.rotation != value){
                    encoder_run = true;
                    timer.Stop();
                    timer.Start();
                }
                this.rotation = value;
            }
        }

        public delegate void data_transform_handle(List<RayInfo> rays);

        public event data_transform_handle DataTransform;

        protected virtual void OnTimedEvent(object source, ElapsedEventArgs e){
            encoder_run = false;
        }

        protected System.Timers.Timer timer;

        protected Task data_process_task;

        protected RecvQueue<ScannerSector> sectors;

        public virtual void Connect(){
            communication = new Client();
            communication.DataReceiveHandler += ReceiveData;
            communication.StatusHandler += StatusHandler;
            communication.Connect(this.end_point,this.self_end_point,this.protocol);
        }

        public virtual void DisConnect(){
            communication.DisConnect();
        }
        public virtual void SendData(byte[] data) {
            this.communication.SendData(data);
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

        protected abstract void StatusHandler(bool connected);

        public abstract void SearchData();

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

        protected virtual void StartDataDealTask(int delay){
            deal_data_token_source = new CancellationTokenSource();
            deal_data_token = deal_data_token_source.Token;

            deal_data_task = new Task(async () => {
                while (true){
                    if (deal_data_token.IsCancellationRequested){
                        return;
                    }
                    byte[] data = data_buffer.SearchData();
                    if (data != null){
                        this.ProcessData(data);
                    }
                    await Task.Delay(delay);
                }
            });
            deal_data_task.Start();
        }

        protected virtual void StopDataDealTask(){
            if (deal_data_token_source != null) {
                deal_data_token_source.Cancel();
            }
        }

        protected void OnDataTransform(List<RayInfo> rays){
            if (this.DataTransform != null){
                this.DataTransform(rays);
            }
        }

        public virtual void Close() {
            this.stop_scan_data();
            this.stop_scan();
            this.StopDataDealTask();
            if (communication != null) {
                communication.DisConnect();
            }
        }
    }
}
