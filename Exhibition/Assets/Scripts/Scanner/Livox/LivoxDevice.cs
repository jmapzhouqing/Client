using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Scanner.Util;
using Scanner.Communicate;
using System.Threading;

using System.IO;
using UnityEngine;

namespace Scanner.Livox
{
    class LivoxDevice: Scanner.Scanister.Scanner
    {
        private LivoxDataUtil util;

        private bool connected;

        private IPEndPoint remote_address;
        private IPEndPoint command_address;
        private IPEndPoint data_address;
        private ProtocolType protocol;

        private Communication command_client;
        private Communication data_client;

        private DeviceInfo deviceInfo;

        private DataBuffer cmd_buffer;

        private DataBuffer pointcloud_buffer;

        private CancellationTokenSource deal_command_token_source;
        private CancellationToken deal_command_token;
        private Task deal_command_task;

        private CancellationTokenSource deal_pointcloud_token_source;
        private CancellationToken deal_pointcloud_token;
        private Task deal_pointcloud_task;

        private CancellationTokenSource deal_heartbeat_token_source;
        private CancellationToken deal_heartbeat_token;
        private Task deal_heartbeat_task;

        private Dictionary<string, Action<SdkPacket>> reply;

        private StreamWriter stream;

        public LivoxDevice(string name,IPEndPoint server_address,IPEndPoint command_address,IPEndPoint data_address):base(name)
        {
            try
            {
                this.remote_address = server_address;
                this.command_address = command_address;
                this.data_address = data_address;
                this.protocol = protocol;

                cmd_buffer = new DataBuffer(102400, SocketType.Dgram);

                pointcloud_buffer = new DataBuffer(10240000, SocketType.Dgram);

                util = new LivoxDataUtil();

                reply = new Dictionary<string, Action<SdkPacket>>();

                //设备握手协议
                reply.Add("01", HandShakeReply);
                //查询设备信息回调
                reply.Add("02", QueryDeviceInformationReply);
                //采样启停回调
                reply.Add("04", SampleReply);




            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        public LivoxDevice(string name,IPAddress ip_address, int port, ProtocolType protocol):base(name)
        {

        }

        public override byte[] CommandConstruct(string command)
        {
            throw new NotImplementedException();
        }

        public override void Connect(){
            command_client = new Client();
            command_client.DataReceived += ReceiveCommandData;
            command_client.Connect(this.remote_address, this.command_address, ProtocolType.Udp);


            data_client = new Client();
            data_client.DataReceived += ReceivePointCloud;
            data_client.Connect(this.remote_address, this.data_address, ProtocolType.Udp);

            this.StartCommandDealTask(100);
            //this.StartPointCloudDealTask(10);

            stream = new StreamWriter("D:\\PCD\\Point.pcd");

            stream.WriteLine("# .PCD v0.7 - Point Cloud Data file format");
            stream.WriteLine("VERSION 0.7");
            stream.WriteLine("FIELDS x y z");
            stream.WriteLine("SIZE 4 4 4");
            stream.WriteLine("TYPE F F F");
            stream.WriteLine("COUNT 1 1 1");
            stream.WriteLine("WIDTH {0}", 100000);
            stream.WriteLine("HEIGHT {0}", 1);
            stream.WriteLine("VIEWPOINT 0 0 0 1 0 0 0");
            stream.WriteLine("POINTS {0}", 100000);
            stream.WriteLine("DATA ascii");
        }

        private void ReceiveCommandData(byte[] buffer, int offset, int length)
        {
            cmd_buffer.PushData(buffer, offset, length);
        }

        int index = 0;
        private void ReceivePointCloud(byte[] buffer, int offset, int length)
        {
            ProcessPointCloud(buffer,offset,length);
            //pointcloud_buffer.PushData(buffer, offset, length);
        }

        #region 数据处理启/停方法
        public void StartHeartBeat(int delay)
        {
            deal_heartbeat_token_source = new CancellationTokenSource();
            deal_heartbeat_token = deal_heartbeat_token_source.Token;

            deal_heartbeat_task = new Task(async () => {
                while (true)
                {
                    if (deal_heartbeat_token.IsCancellationRequested)
                    {
                        return;
                    }
                    this.HeartBeat();
                    await Task.Delay(delay);
                }
            });
            deal_heartbeat_task.Start();
        }

        public void StopHeartBeat()
        {
            deal_heartbeat_token_source.Cancel();
        }


        private void StartCommandDealTask(int delay)
        {
            deal_command_token_source = new CancellationTokenSource();
            deal_command_token = deal_command_token_source.Token;

            deal_command_task = new Task(async () => {
                while (true)
                {
                    if (deal_command_token.IsCancellationRequested)
                    {
                        return;
                    }
                    byte[] data = cmd_buffer.SearchData();
                    if (data != null)
                    {
                        this.ProcessCommandData(data);
                    }
                    await Task.Delay(delay);
                }
            });
            deal_command_task.Start();
        }

        private void StopCommandDealTask() {
            deal_command_token_source.Cancel();
        }

        private void StartPointCloudDealTask(int delay)
        {
            deal_pointcloud_token_source = new CancellationTokenSource();
            deal_pointcloud_token = deal_pointcloud_token_source.Token;

            deal_pointcloud_task = new Task(async () => {
                while (true){
                    if (deal_pointcloud_token.IsCancellationRequested){
                        return;
                    }
                    byte[] data = pointcloud_buffer.SearchData();
                    if (data != null)
                    {
                        //this.ProcessPointCloud(data);
                    }
                    await Task.Delay(delay);
                }
            });
            deal_pointcloud_task.Start();
        }

        private void StopPointCloudDealTask()
        {
            deal_pointcloud_token_source.Cancel();
        }

        #endregion

        public override void SendData(byte[] data)
        {
            command_client.SendData(data);
        }

        public override void GetDeviceInfo()
        {
            throw new NotImplementedException();
        }

        private void ProcessCommandData(byte[] data){
            /*SdkPreamble info = DataConvert.ConvertValue<SdkPreamble>(data, 0);
            Console.WriteLine(info);*/
            SdkPacket packet = util.ParsePacket(data);

            Action<SdkPacket> action;

            string key = packet.cmd_set + "" + packet.cmd_id;

            //Console.WriteLine(key);

            if (reply.TryGetValue(key, out action)) {
                action(packet);
            }
        }

 
        private void ProcessPointCloud(byte[] data,int offset,int length) {
            LivoxEthPacket lidar_data = DataConvert.ConvertByteStruct<LivoxEthPacket>(data,offset);

            PointDataType data_type = (PointDataType)Enum.ToObject(typeof(PointDataType), lidar_data.data_type);

            int data_offset = DataConvert.GetLength<LivoxEthPacket>();

            switch (data_type)
            {
                case PointDataType.Cartesian:
                    //this.CoordinateTransformation<LivoxRawPoint>(data, offset);
                    break;
                case PointDataType.Spherical:
                    //this.CoordinateTransformation<LivoxSpherPoint>(data, offset);
                    break;
                case PointDataType.ExtendCartesian:
                    this.CoordinateTransformation<LivoxExtendRawPoint>(data,offset+data_offset,length - data_offset);
                    break;
                case PointDataType.ExtendSpherical:
                    //this.CoordinateTransformation<LivoxExtendSpherPoint>(data, offset);
                    break;
                case PointDataType.DualExtendCartesian:
                    //this.CoordinateTransformation<LivoxDualExtendRawPoint>(data, offset);
                    break;
                case PointDataType.DualExtendSpherical:
                    //this.CoordinateTransformation<LivoxDualExtendSpherPoint>(data, offset);
                    break;
                case PointDataType.Imu:
                    //this.CoordinateTransformation<LivoxImuPoint>(data, offset);
                    break;
                case PointDataType.TripleExtendCartesian:
                    //this.CoordinateTransformation<LivoxTripleExtendRawPoint>(data, offset);
                    break;
                case PointDataType.TripleExtendSpherical:
                    //this.CoordinateTransformation<LivoxTripleExtendSpherPoint>(data, offset);
                    break;
                default:
                    return;
            }

            //Console.WriteLine(data.Length);
           
        }

        private void CoordinateTransformation<T>(byte[] data, int offset,int length) where T:struct{
            int data_len = DataConvert.GetLength<T>();
            int number = length / data_len;
            try
            {
                //Monitor.Enter(DataLoad.update_list);

                for (int i = 0; i < number; i++)
                {
                    offset += data_len * i;
                    LivoxExtendRawPoint point = DataConvert.ConvertByteStruct<LivoxExtendRawPoint>(data, offset);

                    if (point.reflectivity != 0)
                    {
                        /*
                        if (index++ < 100000)
                        {
                            stream.WriteLine("{0} {1} {2}", point.x / 1000.0f, point.y / 1000.0f, point.z / 1000.0f);
                        }else {
                            stream.Flush();
                            stream.Close();
                        }*/

                        /*index++;
                        if (index % 100 == 0) {
                            Debug.Log(index);
                        }*/
                        //Debug.Log(index++);
                        //DataLoad.SetCoordinate(new Vector3(point.x/1000.0f, point.y/1000.0f, point.z/1000.0f));
                        
                        DataLoad.update_list.Add(new Vector3(point.x / 1000.0f,point.z / 1000.0f, point.y / 1000.0f));
                    }
                }
            }
            finally {
                //Monitor.Exit(DataLoad.update_list);
            }
        }

        /*
        public override void SearchData()
        {
            throw new NotImplementedException();
        }*/

        protected override void scanner_login()
        {
            throw new NotImplementedException();
        }

        protected override void start_scan()
        {
            throw new NotImplementedException();
        }

        protected override void start_scan_data()
        {
            throw new NotImplementedException();
        }

        protected override void stop_scan()
        {
            throw new NotImplementedException();
        }

        protected override void stop_scan_data()
        {
            throw new NotImplementedException();
        }

        public override void ProcessData(byte[] data)
        {
            throw new NotImplementedException();
        }

        #region 命令方法
        public void HandShake()
        {
            HandshakeRequest handshake;
            handshake.ip_addr = Convert.ToUInt32(this.command_address.Address.Address);
            handshake.cmd_port = Convert.ToUInt16(this.command_address.Port);
            handshake.data_port = Convert.ToUInt16(this.data_address.Port);
            handshake.sensor_port = Convert.ToUInt16(this.data_address.Port);

            byte[] data = DataConvert.ConvertStructByte<HandshakeRequest>(handshake);

            SdkPacket packet = util.PackSdkPacket(1,0,1,data);

            byte[] command_data = util.PackCommand(packet);

            this.SendData(command_data);
        }

        public void ConnectLivox() {

        }

        public void SetSamplingStation(bool station) {
            SdkPacket packet = util.PackSdkPacket(0, 0, 4, BitConverter.GetBytes(station));

            byte[] command_data = util.PackCommand(packet);

            this.SendData(command_data);
        }

        public void QueryDeviceInformation() {
            SdkPacket packet = util.PackSdkPacket(0, 0, 2, null);

            byte[] command_data = util.PackCommand(packet);

            this.SendData(command_data);
        }

        public void HeartBeat() {
            SdkPacket packet = util.PackSdkPacket(0,0,3,null);

            byte[] command_data = util.PackCommand(packet);

            this.SendData(command_data);
        }

        #endregion

        #region 命令回调方法
        public void HandShakeReply(SdkPacket packet){
            if(packet.data.Length == 1 && packet.data[0] == 0){
                this.connected = true;
                this.StartHeartBeat(100);
                //this.QueryDeviceInformation();
                this.SetSamplingStation(true);
            }else{

            }
        }

        public void QueryDeviceInformationReply(SdkPacket packet){
            DeviceInformationResponse response = DataConvert.ConvertByteStruct<DeviceInformationResponse>(packet.data);
        }

        public void SampleReply(SdkPacket packet) {
            Console.WriteLine("Enter");
        }

        

        public void RemoveDevice(){

        }


        #endregion

        #region 通讯方法
        public override void DisConnect()
        {
            if (command_client!=null) {
                command_client.DisConnect();
            }

            if (data_client != null) {
                data_client.DisConnect();
            }
        }
        #endregion
    }
}
