using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using UnityEngine;

namespace Scanner.Communicate
{
    class ClientTransmission : DataTransmission
    {

        public ClientTransmission(string ip, int port) : base(ip, port){
            byte[] heartbeat_data = this.CommandConstruct("Heart Request");
            correspond = new Correspond_TCP(new IPEndPoint(IPAddress.Any, 0), heartbeat_data);
        }

        protected override void CreateReplayProcess(){
            reply_process = new Dictionary<string, Action<string[]>>();

            reply_process.Add("Heart", this.ProcessHeart);
            reply_process.Add("StackCoal", this.ProcessStackCoal);
            reply_process.Add("TakeCoal", this.ProcessTakeCoal);
            reply_process.Add("Scanner", this.ProcessScanner);
            reply_process.Add("Info", this.ProcessInfo);
        }

        protected override void ProcessData(byte[] data)
        {
            string scan_data = Encoding.ASCII.GetString(data);

            scan_data = scan_data.Substring(1, scan_data.Length - 1);

            string[] fields = scan_data.Split(' ');

            //Debug.Log(fields.Length);

            if (fields.Length != 0)
            {
                string resCode = fields[0];
     
                if (resCode.Equals("Heart") || resCode.Equals("StackCoal") || resCode.Equals("TakeCoal") || resCode.Equals("Boundary") || resCode.Equals("Scanner") || resCode.Equals("Info"))
                {

                    Action<string[]> reply = null;

                    if (reply_process.TryGetValue(resCode, out reply))
                    {
                        reply(fields);
                    }
                }
                else
                {

                }
            }
        }

        private void ProcessHeart(string[] data)
        {
            //this.SendData("Heart");
            if (data[1].Equals("Request")){
                this.SendData("Heart Response");
            }
        }

        private void ProcessTakeCoal(string[] data)
        {
            if (data[1].Equals("Status"))
            {
                switch (data[2])
                {
                    case "Start":
                        UIManager.instance.Refresh(delegate () {
                            UIManager.instance.TakeCoalExhibition(data[3]);
                        });
                        break;
                    case "End":
                        UIManager.instance.Refresh(delegate () {
                            UIManager.instance.ClearInterface();
                        });
                        //UIManager.instance.ClearInterface();
                        break;
                    case "CommandReceive":
                        
                        break;
                    default:
                        break;
                }
            }
        }

        private void ProcessStackCoal(string[] data)
        {
            if (data[1].Equals("Status"))
            {
                switch (data[2])
                {
                    case "Start":
                        UIManager.instance.Refresh(delegate () {
                            UIManager.instance.StackCoalExhibition(data[3]);
                        });
                        break;
                    case "End":
                        UIManager.instance.Refresh(delegate (){
                            UIManager.instance.ClearInterface();
                        });
                        break;
                    case "CommandReceive":
                        
                        break;
                    default:
                        break;
                }
            }
        }

        private void ProcessScanner(string[] data)
        {

        }

        private void ProcessError(string[] data)
        {
           
        }

        private void ProcessInfo(string[] data)
        {
            Debug.Log(data[1]);
        }
    }
}
