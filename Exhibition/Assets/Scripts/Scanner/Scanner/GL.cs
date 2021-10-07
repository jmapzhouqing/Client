using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Scanner.Struct;
using Scanner.Util;
using System.Net.Sockets;
using System.Net;

namespace Scanner.Scanister
{
    class GL: Scanner
    {
        private Dictionary<int, Action<List<UInt32>>> reply_process;
        public GL(string name,string ip, int port):base(name){
            try
            {
                reply_process = new Dictionary<int, Action<List<UInt32>>>();
                data_buffer = new DataBuffer(102400, SocketType.Dgram);
                this.server_address = new IPEndPoint(IPAddress.Parse(ip), port);

                //reply_process.Add("SCAN", MeasurementStatusProcess);
                //reply_process.Add("GSCN", ScandataProcess);

            }
            catch (Exception e)
            {

            }
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

        protected override void get_latest_scan(){

        }

        public override byte[] CommandConstruct(string command) {
            return null;
        }

        public override void ProcessData(byte[] data) { }

        #endregion

    }
}
