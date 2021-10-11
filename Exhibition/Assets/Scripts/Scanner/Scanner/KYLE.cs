using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;

using UnityEngine;

using Scanner.Util;
using Scanner.Struct;
using Scanner.Communicate;

namespace Scanner.Scanister
{
    class KYLE : Scanner{

        public KYLE(string name, string ip, int port):base(name){
            try{
                reply_process = new Dictionary<string, Action<string[]>>();

                data_buffer = new DataBuffer(1024000, SocketType.Stream,new byte[] {0x02}, new byte[] { 0x03});

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
            this.StartProcessData(100);
            correspond = new Correspond_TCP(new IPEndPoint(IPAddress.Any, 0),this.CommandConstruct("sRN LMPscancfg"));
            base.Connect();
        }

        public override byte[] CommandConstruct(string command){
            List<byte> data = new List<byte>();
            data.Add(0x02);

            byte[] command_data = Encoding.ASCII.GetBytes(command);
            data.AddRange(command_data);

            data.Add(0x03);
            return data.ToArray();
        }

        public override void GetDeviceInfo()
        {
            throw new NotImplementedException();
        }

        public override void ProcessData(byte[] data)
        {
            string scan_data = Encoding.ASCII.GetString(data);

            string[] fields = scan_data.Split(' ');

            if (fields.Length != 0){
                string resCode = fields[0];

                Debug.Log(resCode);

                if (resCode.Equals("sRA") || resCode.Equals("sSN") || resCode.Equals("sAN") || resCode.Equals("sEA"))
                {
                    string eventName = resCode + fields[1];

                    Action<string[]> reply = null;

                    if (reply_process.TryGetValue(eventName, out reply))
                    {
                        reply(fields);
                    }
                }
                else if (resCode.Equals("sFA"))
                {
                    Action<string[]> reply = null;

                    if (reply_process.TryGetValue(resCode, out reply))
                    {
                        reply(fields);
                    }
                }
            }
        }

        protected override void scanner_login()
        {
            throw new NotImplementedException();
        }

        protected override void start_scan(){
            /*
            Debug.Log("Enter StartScan");

            byte[] data = this.CommandConstruct("sRN LMDscandata");



            string value = "";
            foreach (byte item in data) {
                value += item.ToString("X")+" ";
            }

            Debug.Log(value);

            this.SendData(data);*/
            this.SendData(this.CommandConstruct("sEN LMDscandata 1"));
        }

       

        protected override void stop_scan(){
            this.SendData(this.CommandConstruct("sEN LMDscandata 0"));
        }

        protected override void start_scan_data(){
            //this.SendData(this.CommandConstruct("sEN LMDscandata 1"));
        }

        protected override void stop_scan_data(){
            //this.SendData(this.CommandConstruct("sEN LMDscandata 0"));
        }

        public void AccessModeProcess(string[] fields){
            int index = 0;
            int length = fields.Length;
            //isLogin = Convert.ToBoolean(Convert.ToInt16(fields[2]));
        }

        public void StartmeasProcess(string[] fields){
            int index = 0;
            int length = fields.Length;

            bool isError = Convert.ToBoolean(Convert.ToInt16(fields[2]));
            if (!isError){
                this.start_scan_data();
            }
        }

        public void StopProcess(string[] fields)
        {
            int index = 0;
            int length = fields.Length;

            bool isError = Convert.ToBoolean(Convert.ToInt16(fields[2]));
            if (!isError){

            }
        }

        public void LMDscandataEventProcess(string[] fields){
            int index = 0;
            int length = fields.Length;

            bool isStart = Convert.ToBoolean(Convert.ToInt16(fields[2]));

            if (isStart){
                this.OnStatusChanged(DeviceStatus.Working);
            }else{
                this.OnStatusChanged(DeviceStatus.OnLine);
            }
        }

        public void LMDscandataProcess(string[] fields){
            try{
                bool echo_intensity = Convert.ToBoolean(Convert.ToUInt16(fields[15]));

                int factor = 0;

                if (Convert.ToUInt32(fields[21], 16) == 0x3F800000){
                    factor = 1;
                }

                float start_angle = Convert.ToInt32(fields[23],16) / 10000.0f;

                float angle_step = Convert.ToInt32(fields[24],16) / 10000.0f;

                UInt16 data_number = Convert.ToUInt16(fields[25],16);

                int index = 26;

                int step = 1;

                if (echo_intensity){
                    step += 2;
                }

                List<RayInfo> rays = new List<RayInfo>();

                for (int i = 0; i < data_number; i+=step){
                    RayInfo info;
                    info.distance = Convert.ToUInt32(fields[index++],16) * factor / 1000.0f;
                    info.degree = start_angle + angle_step * i;
                    rays.Add(info);
                }

                this.OnDataDecodeComplete(rays);
            }catch (Exception e){
                this.OnError(new ExceptionHandler(e.Message, ExceptionCode.InternalError));
            }
        }
    }
}