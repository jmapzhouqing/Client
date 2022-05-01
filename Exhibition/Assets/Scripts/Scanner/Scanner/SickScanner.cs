using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using Scanner.Communicate;
using Scanner.Util;
using Scanner.Struct;
using System.Reflection;
using System.Globalization;
using System.Threading.Tasks;
using System.Timers;
using System.Net.Sockets;
using UnityEngine;

namespace Scanner.Scanister
{
    class SickScanner:Scanner
    {
        private bool isLogin = false;

        public SickScanner(string name,string ip, int port):base(name)
        {
            try{
                reply_process = new Dictionary<string, Action<string[]>>();
                data_buffer = new DataBuffer(1024000,SocketType.Stream,new byte[] { 0x02},new byte[] { 0x03});

                this.server_address = new IPEndPoint(IPAddress.Parse(ip), port);
               
                reply_process.Add("sANSetAccessMode", AccessModeProcess);
                reply_process.Add("sANLMCstartmeas", StartmeasProcess);
                reply_process.Add("sANLMCstopmeas", StopProcess);

                reply_process.Add("sRALMDscandata", LMDscandataProcess);
                reply_process.Add("sSNLMDscandata", LMDscandataProcess);

                reply_process.Add("sEALMDscandata", LMDscandataEventProcess);
            }
            catch (Exception e){

            }
        }

        public override void Connect(){
            correspond = new Correspond_TCP(this.device_name, new IPEndPoint(IPAddress.Any, 0),this.CommandConstruct("sRN LMPscancfg"));
            base.Connect();
        }

        /*
        protected override void StatusHandler(bool connected){
            if(connected){
                System.Threading.Thread.Sleep(100);
                this.scanner_login();
            }else{

            }
        }*/

        /*
        public override void SearchData(){
            byte[] data = null;
            while((data = data_buffer.SearchData()) != null){
                this.ProcessData(data);
            }
        }*/

        public override void ProcessData(byte[] data) {
            string scan_data = Encoding.ASCII.GetString(data);

            string[] fields = scan_data.Split(' ');

            if (fields.Length != 0) {
                string resCode = fields[0];
                if (resCode.Equals("sRA") || resCode.Equals("sSN")||resCode.Equals("sAN")||resCode.Equals("sEA")){
                    string eventName = resCode + fields[1];

                    Action<string[]> reply = null;

                    if(reply_process.TryGetValue(eventName, out reply)){
                        reply(fields);
                    }
                }else if (resCode.Equals("sFA")) {
                    Action<string[]> reply = null;

                    if(reply_process.TryGetValue(resCode, out reply)){
                        reply(fields);
                    }
                }
            }
        }

        protected override void scanner_login() {
            //this.communication.SendData(this.CommandConstruct("sMN SetAccessMode ", 0x03, 0xF4, 0x72, 0x47, 0x44));
            this.SendData(this.CommandConstruct("sMN SetAccessMode 03 F4724744"));
        }

        protected override void start_scan(){
            this.SendData(this.CommandConstruct("sMN LMCstartmeas"));
        }

        protected override void stop_scan()
        {
            this.SendData(this.CommandConstruct("sMN LMCstopmeas"));
        }

        protected override void start_scan_data(){
            //this.communication.SendData(this.CommandConstruct("sEN LMDscandata ",0x01));
            this.SendData(this.CommandConstruct("sEN LMDscandata 1"));
        }

        protected override void stop_scan_data() {
            this.correspond.SendData(this.CommandConstruct("sEN LMDscandata 0"));
        }

        public override void GetDeviceInfo(){
            throw new NotImplementedException();
        }

        /*
        public  byte[] CommandConstruct(string command, params byte[] numbers) {
            List<byte> data = new List<byte>();
            data.AddRange(new byte[] { 0x2, 0x2, 0x2, 0x2 });

            DataConvert.AddStringToBuffer(data, command);

            foreach (byte number in numbers)
            {
                DataConvert.AddNumberToBuffer<byte>(data, number);
            }

            int length = data.Count - 4;

            DataConvert.InsertNumberToBuffer<int>(data, 4, length);
            
            DataConvert.CaculateSickChecksum(data);

            return data.ToArray();
        }*/

        public override byte[] CommandConstruct(string command){
            List<byte> data = new List<byte>();
            data.Add(0x02);
            byte[] command_data = Encoding.ASCII.GetBytes(command);

            data.AddRange(command_data);

            data.Add(0x03);
            return data.ToArray();
        }

        public override void Start(){
            if (isLogin) {
                this.start_scan();
            }
        }

        public override void Stop(){
            if (isLogin) {
                this.stop_scan();
            }
        }

        public void LMDscandataProcess(string[] fields){
            int index = 0;
            int length = fields.Length;

            UInt32 scan_frequency = Convert.ToUInt32(fields[16], 16) / 100;
            UInt32 measurement_frequency = Convert.ToUInt32(fields[17], 16);
            UInt16 amount_channels = Convert.ToUInt16(fields[19], 16);

            index = 19;

            SectorInfo sector;
            sector.ticks = Convert.ToUInt64(DateTime.Now.Ticks * Math.Pow(10,-4));

            for (int i = 0; i < amount_channels; i++){
                string content = fields[++index];
                UInt32 scale_factor = Convert.ToUInt32(fields[++index], 16);

                if (scale_factor == 0x3F800000){
                    scale_factor = 1;
                }else if (scale_factor == 0x40000000){
                    scale_factor = 2;
                }else if (scale_factor == 0x40800000){
                    scale_factor = 4;
                }

                UInt32 scale_factor_offset = Convert.ToUInt32(fields[++index], 16);
                float start_angle = Convert.ToUInt32(fields[++index], 16) / 10000.0f;
                float angular_step = Convert.ToUInt16(fields[++index], 16) / 10000.0f;
                UInt16 amount_data = Convert.ToUInt16(fields[++index], 16);

                List<RayInfo> rays = new List<RayInfo>();

                for (int j = 0; j < amount_data; j++){
                    RayInfo info;
                    info.distance = Convert.ToInt16(fields[++index], 16) / 1000.0f * scale_factor;
                    info.degree = start_angle + i * angular_step;
                    rays.Add(info);
                }

                sector.rays = rays;
                sector.rotation = Vector3.zero;

                this.OnDataDecodeComplete(sector);
            }

            /*
            scandata.version_number = DataConvert.GetNumberFromBuffer<UInt16>(data, ref pos);
            scandata.device_number = DataConvert.GetNumberFromBuffer<UInt16>(data, ref pos);
            scandata.serial_number = DataConvert.GetNumberFromBuffer<UInt32>(data, ref pos);
            scandata.device_status = DataConvert.GetNumberFromBuffer<UInt16>(data, ref pos);
            scandata.telegram_counter = DataConvert.GetNumberFromBuffer<UInt16>(data, ref pos);
            scandata.scan_counter = DataConvert.GetNumberFromBuffer<UInt16>(data, ref pos);
            scandata.time_since_start = DataConvert.GetNumberFromBuffer<UInt32>(data, ref pos);
            scandata.time_transmision = DataConvert.GetNumberFromBuffer<UInt32>(data, ref pos);

            scandata.status_digital_inputs = DataConvert.GetNumberFromBuffer<UInt16>(data, ref pos);

            scandata.status_digital_outputs = DataConvert.GetNumberFromBuffer<UInt16>(data, ref pos);
            scandata.layer_angle = DataConvert.GetNumberFromBuffer<UInt16>(data, ref pos);

            scandata.scan_frequency = DataConvert.GetNumberFromBuffer<UInt32>(data, ref pos)/100;

            scandata.measurement_frequency = DataConvert.GetNumberFromBuffer<UInt32>(data, ref pos);

            scandata.amount_encoder = DataConvert.GetNumberFromBuffer<UInt16>(data, ref pos);

            if (scandata.amount_encoder != 0) {
                scandata.encoder_position = DataConvert.GetNumberFromBuffer<UInt32>(data, ref pos);
                scandata.encoder_speed = DataConvert.GetNumberFromBuffer<UInt16>(data, ref pos);
            }

            scandata.amount_channels = DataConvert.GetNumberFromBuffer<UInt16>(data, ref pos);

            for (int i = 0; i < scandata.amount_channels;i++) {
                scandata.content = DataConvert.GetStringFromBuffer(data, ref pos, 5);

                scandata.scale_factor = DataConvert.GetNumberFromBuffer<UInt32>(data, ref pos);
                scandata.scale_factor_offset = DataConvert.GetNumberFromBuffer<UInt32>(data, ref pos);
                scandata.start_angle = DataConvert.GetNumberFromBuffer<UInt32>(data, ref pos) / 10000.0f;
                scandata.angular_step = DataConvert.GetNumberFromBuffer<UInt16>(data, ref pos) / 10000.0f;
                scandata.amount_data = DataConvert.GetNumberFromBuffer<UInt16>(data, ref pos);

                for (int j = 0; j < scandata.amount_data; j++){
                    //Console.Write("{0},",DataConvert.GetNumberFromBuffer<UInt16>(data, ref pos)/1000.0f);
                    DataConvert.GetNumberFromBuffer<UInt16>(data, ref pos);
                }



                Console.WriteLine(scandata.content+"#"+scandata.amount_data);
            }*/

        }

        public void AccessModeProcess(string[] fields) {
            int index = 0;
            int length = fields.Length;
            isLogin = Convert.ToBoolean(Convert.ToInt16(fields[2]));
        }

        public void StartmeasProcess(string[] fields){
            int index = 0;
            int length = fields.Length;

            bool isError = Convert.ToBoolean(Convert.ToInt16(fields[2]));
            if (!isError) {
                this.OnStatusChanged(DeviceStatus.Working);
                this.start_scan_data();
            }
        }

        public void StopProcess(string[] fields){
            int index = 0;
            int length = fields.Length;

            bool isError = Convert.ToBoolean(Convert.ToInt16(fields[2]));
            if (!isError){
                this.OnStatusChanged(DeviceStatus.OnLine);
            }
        }

        public void LMDscandataEventProcess(string[] fields){
            int index = 0;
            int length = fields.Length;

            bool isStart = Convert.ToBoolean(Convert.ToInt16(fields[2]));
            if(isStart){
                
            }else {
                
            }
        }

        private void data_transform(){
            
        }
    }
}