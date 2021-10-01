using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using Scanner.Util;
using Scanner.Struct;

namespace Scanner.Scanister
{

    class Tele:Scanner{
        public Tele(string name,IPEndPoint remote,IPEndPoint self, ProtocolType protocol):base(name){
            try{
                this.end_point = remote;
                this.self_end_point = self;
                this.protocol = protocol;

                data_buffer = new DataBuffer(102400, SocketType.Dgram);

                //reply_process.Add("SCAN", MeasurementStatusProcess);
                //reply_process.Add("GSCN", ScandataProcess);

            }catch (Exception e){

            }
        }

        public Tele(string name,IPAddress ip_address, int port, ProtocolType protocol):base(name){
            
        }

        public override void Connect()
        {
            base.Connect();
            this.StartProcessData(100);
        }

        //public override void SearchData() { }

        #region 设备参数
        public override void GetDeviceInfo() { }

        #endregion

        #region 盘煤仪操作
        protected override void scanner_login() { }

        protected override void start_scan() { }

        protected override void stop_scan() { }

        protected override void start_scan_data() { }

        protected override void stop_scan_data() { }

        protected override void get_latest_scan()
        {

        }

        public override byte[] CommandConstruct(string command)
        {
            return null;
        }

        public override void ProcessData(byte[] data) {
            /*SdkPreamble info = DataConvert.ConvertValue<SdkPreamble>(data, 0);
            Console.WriteLine(info);*/
            Console.WriteLine(data.Length);
        }
        #endregion
    }
}
