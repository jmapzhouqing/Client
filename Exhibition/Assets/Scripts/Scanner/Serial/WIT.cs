using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Scanner.Serial;
using System.IO.Ports;
using Scanner.Util;
using System.Threading.Tasks;
using System.Threading;
using Scanner.Struct;
using UnityEngine;
using System.Net.Sockets;

namespace Scanner.Serial
{
    
    class WIT:SerialDevice<DegreeInfo>{
        private DataBuffer buffer;

        private int used_index = 0;

        private Task send_task;

        private Vector3 rotation;

        private CancellationTokenSource cancel_source;
        private CancellationToken cancel_token;

        public WIT(String name, byte address, Int32 baud_rate,Parity parity = Parity.None, Int32 data_bits = 8, StopBits stop_bits = StopBits.One):base(name,baud_rate,parity,data_bits,stop_bits) {
            this.address = address;
            data_buffer = new DataBuffer(102400, SocketType.Stream,start_pattern:new byte[]{address, 0x03});
        }

        public override void ProcessData(byte[] data){
            int pos = data.Length - 2;
            UInt16 value = DataConvert.GetNumberFromBuffer<UInt16>(data,ref pos);
            int crc = DataConvert.CaculateModbusCRC(data, 0, data.Length - 2);
            if (value == crc){
                this.DecodeData(data, 3, data.Length - 5);
            }
        }

        public override void Close() {
            if (cancel_source != null){
                cancel_source.Cancel();
            }
            base.Close();
        }

        public void StartReadData(int delay){
            cancel_source = new CancellationTokenSource();
            cancel_token = cancel_source.Token;

            send_task = new Task(async()=>{
                while (true) {
                    if(cancel_token.IsCancellationRequested){
                        break;
                    }
                    this.ReadData();
                    await Task.Delay(delay);
                }
            });
            send_task.Start();
        }

        public void StopReadData() {
            if(cancel_source != null){
                cancel_source.Cancel();
            }
        }

        public void ReadData(){
            this.SendData(new byte[] {address,0x03,0x00, 0x30, 0x00, 0x13, 0x09, 0x89});
        }

        private void DecodeData(byte[] data, int offset, int length){
            WitData wit_data = DataConvert.ConvertByteStruct<WitData>(data, offset,true);

            float roll = (wit_data.rollH << 16 | wit_data.rollL) / 1000.0f;
            float pitch = (wit_data.pitchH << 16 | wit_data.pitchL) / 1000.0f;
            float yaw = (wit_data.yawH << 16 | wit_data.yawL) / 1000.0f;

            DegreeInfo info;
            info.degree = new Vector3(pitch,yaw,roll);
            info.ticks = 0;//Convert.ToUInt64(DateTime.Now.Ticks * Math.Pow(10, -4));

            this.OnDataDecodeComplete(info);
            /*
            float ax = ((Int16)wit_data.ax) / 32768.0f * 16;

            Console.WriteLine("时间:{0}/{1}/{2} {3}:{4}:{5}.{6}", wit_data.year, wit_data.month, wit_data.day,wit_data.hour,wit_data.minute,wit_data.second,wit_data.msecond);

            Console.WriteLine("轴加速度:{0},{1},{2}", wit_data.ax / 32768.0f * 16, wit_data.ay / 32768.0f * 16, wit_data.az / 32768.0f * 16);
            Console.WriteLine("角加速度:{0},{1},{2}", wit_data.wx, wit_data.wy, wit_data.wz);
            Console.WriteLine("磁场:{0},{1},{2}", wit_data.hx, wit_data.hy, wit_data.hz);
            Console.WriteLine("角度:{0},{1},{2}",roll,pitch,yaw);*/

            //this.OnDataDecodeComplete()

            //Debug.Log(this.rotation);

            //Debug.Log(Rotation);
        } 
    }
}