using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Scanner.Communicate;
using Scanner.Util;


namespace Scanner.Livox
{
    class DeviceManager
    {

        private readonly UInt16 ListenPort = 55000;

        private readonly UInt16 CmdPortOffset = 500;

        private readonly UInt16 DataPortOffset = 1000;

        private readonly UInt16 SensorPortOffset = 1000;


        private Communication client;
        private DataBuffer data_buffer;
        private IPEndPoint server_address;
        private IPEndPoint client_address;
        private ProtocolType protocol;

        private CancellationTokenSource deal_data_token_source;
        private CancellationToken deal_data_token;
        private Task deal_data_task;

        private LivoxDataUtil util;

        private Dictionary<string, Action<SdkPacket>> reply;

        public Dictionary<string, LivoxDevice> devices;

        public DeviceManager(IPEndPoint server_address,IPEndPoint client_address,ProtocolType protocol) {
            this.server_address = server_address;
            this.client_address = client_address;
            this.protocol = protocol;
            data_buffer = new DataBuffer(102400, SocketType.Dgram);

            util = new LivoxDataUtil();

            devices = new Dictionary<string, LivoxDevice>();

            reply = new Dictionary<string, Action<SdkPacket>>();

            reply.Add("00", OnBroadcast);
        }

        public void Connect() {
            this.client = new Client(new byte[]{0x00});
            this.client.DataReceived += this.ReceiveData;

            this.StartDataDealTask(100);
            this.client.Connect(this.server_address, this.client_address, this.protocol);
        }

        public void SendData(byte[] data) {
            this.client.SendData(data);
        }

        private void ReceiveData(byte[] buffer, int offset, int length){
            data_buffer.PushData(buffer, offset, length);
        }

        private void StartDataDealTask(int delay){
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

        private void StopDataDealTask()
        {
            deal_data_token_source.Cancel();
        }

        private void ProcessData(byte[] data) {
            if (util.CheckPreamble(data)&& util.CheckPacket(data)) {
                //SdkPreamble sdkPreamble = DataConvert.ConvertByteStruct<SdkPreamble>(data);
                SdkPacket packet = util.ParsePacket(data);

                Action<SdkPacket> process;
                string key = packet.cmd_id + "" + packet.cmd_set;
                if (reply.TryGetValue(key, out process)) {
                    process(packet);
                }

                //Console.WriteLine(key);
            }

            //Console.WriteLine(data.Length);
        }

        private void OnBroadcast(SdkPacket packet) {
            BroadcastDeviceInfo info = DataConvert.ConvertByteStruct<BroadcastDeviceInfo>(packet.data);

            LivoxDevice device;

            if (!devices.TryGetValue(info.broadcast_code,out device)){

                UInt16 deviceOffset = Convert.ToUInt16(devices.Count+1);

                /*
                HandshakeRequest handshake;
                handshake.ip_addr = 4217022656;
                handshake.cmd_port = Convert.ToUInt16(this.ListenPort + this.CmdPortOffset + deviceOffset);
                handshake.data_port = Convert.ToUInt16(this.ListenPort + this.DataPortOffset + deviceOffset);
                handshake.sensor_port = Convert.ToUInt16(this.ListenPort + this.DataPortOffset + deviceOffset);*/

                //util.GetGateway();

                IPAddress address = new IPAddress(4217022656);
                IPEndPoint remote = client.server_address;
                IPEndPoint command_address = new IPEndPoint(address,this.ListenPort + this.CmdPortOffset + deviceOffset);
                IPEndPoint data_address = new IPEndPoint(address, this.ListenPort + this.DataPortOffset + deviceOffset);

                device = new LivoxDevice("LvioxDevice",remote,command_address,data_address);
                device.Connect();

                devices.Add(info.broadcast_code, device);
                device.HandShake();

                device = null;
            }
        }


        public void DisConnect() {
            if (client != null) {
                client.DisConnect();
            }

            foreach (KeyValuePair<string, LivoxDevice> item in devices) {
                item.Value.DisConnect();
            }
        }
       
    }
}
