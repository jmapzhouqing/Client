using System.Threading;
using System.Threading.Tasks;

using Scanner.Struct;

namespace Scanner.Scanister
{
    class LMS511:SickScanner
    {
        private CancellationTokenSource get_scan_token_source;
        private CancellationToken get_scan_token;

        private Task get_data_task;
        public LMS511(string name,string ip, int port) :base(name,ip,port){

        }

        protected override void OnStatusChanged(DeviceStatus status){
            if (status.Equals(DeviceStatus.Connect)) {
                this.scanner_login();
            }
            base.OnStatusChanged(status);
        }

        public override void Connect(){
            this.StartProcessData(100);
            base.Connect();
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

        protected override void get_latest_scan(){
            this.SendData(this.CommandConstruct("sRN LMDscandata"));
        }
    }
}