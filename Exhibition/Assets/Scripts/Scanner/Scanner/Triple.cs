using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Scanner.Communicate;
using System.Net;
using Scanner.Util;
using Scanner.Struct;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Scanner.Scanister{
    class Triple : Scanner {
        private Dictionary<string, Action<List<UInt32>>> reply_process;

        private CancellationTokenSource get_scan_token_source;
        private CancellationToken get_scan_token;

        private Task get_data_task;

        private long start_time_stamp;

        public Triple(string name,string ip,int port):base(name) {
            try{
                reply_process = new Dictionary<string, Action<List<UInt32>>>();

                data_buffer = new DataBuffer(102400,SocketType.Dgram);

                this.server_address = new IPEndPoint(IPAddress.Parse(ip), port);

                reply_process.Add("SCAN", MeasurementStatusProcess);
                reply_process.Add("GSCN", ScandataProcess);
            }catch (Exception e) {

            }
            //this.communication.StatusHandler += this.StatusHandler;
        }

        ~Triple(){
            
        }

        public override void Connect() {
            this.StartProcessData(100);
            correspond = new Correspond_UDP(new IPEndPoint(IPAddress.Any, 0), new byte[]{0x00});
            base.Connect();
        }

        /*
        public override void SearchData(){
           
        }*/

        #region 设备参数
        public override void GetDeviceInfo() {

        }

        #endregion

        #region 盘煤仪操作
        protected override void scanner_login() {

        }

        protected override void start_scan() {
            List<byte> list = new List<byte>();
            DataConvert.AddStringToBuffer(list, "SCAN");
            DataConvert.AddNumberToBuffer<UInt32>(list, 4);
            DataConvert.AddNumberToBuffer<UInt32>(list, 1);

            CRC32 crc = new CRC32();
            DataConvert.AddNumberToBuffer<UInt32>(list, crc.get(list.ToArray(),list.Count));
            this.SendData(list.ToArray());
        }

        protected override void stop_scan() {
            List<byte> list = new List<byte>();
            DataConvert.AddStringToBuffer(list, "SCAN");
            DataConvert.AddNumberToBuffer<UInt32>(list, 4);
            DataConvert.AddNumberToBuffer<UInt32>(list, 0);

            CRC32 crc = new CRC32();
            DataConvert.AddNumberToBuffer<UInt32>(list, crc.get(list.ToArray(), list.Count));
            this.SendData(list.ToArray());
        }

        protected override void start_scan_data(){
            get_scan_token_source = new CancellationTokenSource();
            get_scan_token = get_scan_token_source.Token;
            get_data_task = new Task(async () => {
                while (true)
                {
                    if (get_scan_token.IsCancellationRequested)
                    {
                        return;
                    }
                    this.get_latest_scan();
                    await Task.Delay(10);
                }
            });
            get_data_task.Start();
        }

        protected override void stop_scan_data()
        {
            if (get_scan_token_source != null)
            {
                get_scan_token_source.Cancel();
            }
        }

        protected virtual void get_latest_scan(){
            List<byte> list = new List<byte>();
            DataConvert.AddStringToBuffer(list,"GSCN");
            DataConvert.AddNumberToBuffer<UInt32>(list, 4);
            DataConvert.AddNumberToBuffer<UInt32>(list, 0);

            CRC32 crc = new CRC32();
            DataConvert.AddNumberToBuffer<UInt32>(list, crc.get(list.ToArray(),list.Count));
            this.SendData(list.ToArray());
        }

        public override byte[] CommandConstruct(string command){
            return null;
        }

        public override void ProcessData(byte[] data){

            int pos = data.Length - 4;

            UInt32 crc_value = DataConvert.GetNumberFromBuffer<UInt32>(data, ref pos);

            CRC32 crc = new CRC32();
            UInt32 calc_value = crc.get(data, data.Length - 4);

            if (crc_value == calc_value){
                pos = 0;
                string resCode = DataConvert.GetStringFromBuffer(data, ref pos, 4);
                UInt32 data_length = DataConvert.GetNumberFromBuffer<UInt32>(data, ref pos);
                List<UInt32> values = new List<UInt32>();
                for (int i = 0; i < data_length / 4; i++){
                    values.Add(DataConvert.GetNumberFromBuffer<UInt32>(data, ref pos));
                }

                if(resCode.Equals("SCAN") || resCode.Equals("GSCN")){
                    string eventName = resCode;

                    Action<List<UInt32>> reply = null;

                    if (reply_process.TryGetValue(eventName, out reply)){
                        reply(values);
                    }
                }
            }
        }

        #endregion

        #region
        
        #endregion

        #region 数据处理方法
        public void MeasurementStatusProcess(List<UInt32> fields){
            bool status = Convert.ToBoolean(fields[0]);
            if (status){
                start_time_stamp = DateTime.UtcNow.Ticks/10000;
                this.start_scan_data();
            }
            else {
                this.stop_scan_data();
            }
        }

        public void ScandataProcess(List<UInt32> fields){
            int index = 0;
            int param_number = (int)fields[0];

            UInt32 scan_nmber = fields[1];

            if (scan_nmber == 0) {
                return;
            }

            UInt32 time_stamp = fields[2];

            float scan_start_dir = fields[3]/1000.0f;
            float scan_angle = fields[4]/1000.0f;

            UInt32 data_content = fields[9];

            UInt32 points_number = fields[param_number+1];

            int split = 1;

            if(data_content != 4){
                split = 2;
            }

            List<RayInfo> rays = new List<RayInfo>();

            for (int i = 0; i < points_number; i++){
                if (fields[param_number + 2 + split * i] > 400 * 10000) {
                    continue;
                }

                RayInfo info;
                info.distance = fields[param_number + 2 + split * i]/10000.0f;
                info.degree = scan_start_dir + i * 0.09f;
                rays.Add(info);
            }
            this.OnDataDecodeComplete(rays);
        }

        private void StartReceiveScanData(){
           
        }

        private void StopReceiveScanData() {
            
        }
        #endregion
    }
}