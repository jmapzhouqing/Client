using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scanner.Communicate
{
    class ServerTransmission : DataTransmission
    {
        public ServerTransmission() : base()
        {

        }

        protected override void CreateReplayProcess(){
            reply_process = new Dictionary<string, Action<string[]>>();

            reply_process.Add("Heart", this.ProcessHeart);
            reply_process.Add("StackCoal", this.ProcessStackCoal);
            reply_process.Add("TakeCoal", this.ProcessTakeCoal);
            reply_process.Add("Scanner", this.ProcessScanner);
        }

        protected override void ProcessData(byte[] data)
        {
            string scan_data = Encoding.ASCII.GetString(data);

            string[] fields = scan_data.Split(' ');

            if (fields.Length != 0)
            {
                string resCode = fields[0];
                if (resCode.Equals("Heart") || resCode.Equals("StackCoal") || resCode.Equals("TakeCoal") || resCode.Equals("Boundary") || resCode.Equals("Scanner"))
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
        }

        private void ProcessTakeCoal(string[] data)
        {

        }

        private void ProcessStackCoal(string[] data)
        {

        }

        private void ProcessScanner(string[] data)
        {

        }

        private void ProcessError(string[] data)
        {

        }
    }
}
