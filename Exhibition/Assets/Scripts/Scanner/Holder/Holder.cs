using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Collections.Generic;
using System.Net.Sockets;

using UnityEngine;

using Scanner.Serial;
using Scanner.Util;
using Scanner.Struct;

namespace Scanner.Holder
{
    class Holder : SerialDevice<DegreeInfo>
    {
        private SerialCorrespond communication;

        private Task send_task;
        private CancellationTokenSource cancel_source;
        private CancellationToken cancel_token;

        public float horizontal = -1;
        private float vertical;

        public Holder(String name,byte address,Int32 baud_rate, Parity parity = Parity.None, Int32 data_bits = 8, StopBits stop_bits = StopBits.One) : base(name, baud_rate, parity, data_bits, stop_bits) {
            this.address = address;
            data_buffer = new DataBuffer(102400, SocketType.Stream, start_pattern: new byte[] {0xFF,address});
        }

        public override void ProcessData(byte[] data) {
            int length = data.Length;
            int crc = DataConvert.CaculateHolderChecksum(data, 1, length - 2);

            if (data[length - 1] == crc){
                this.DecodeData(data, 2, length - 3);
            }
        }

        public override void Close() {
            if(cancel_source != null){
                cancel_source.Cancel();
            }
            this.Stop();
            base.Close();
        }

        private void DecodeData(byte[] data, int offset, int length) {
            HolderData holder_data = DataConvert.ConvertByteStruct<HolderData>(data, offset, true);
            if(holder_data.control == 0x59){
                DegreeInfo info;
                info.ticks = 0;//Convert.ToUInt64(DateTime.Now.Ticks * Math.Pow(10,-4));
                horizontal = holder_data.data / 100.0f;

                info.degree = new Vector3(0,horizontal,0);
                this.OnDataDecodeComplete(info);
            }
        }

        public void SetDegree(float value) {
            List<byte> list = new List<byte> { 0xFF, address, 0x00, 0x4B };
            UInt16 degree = Convert.ToUInt16(value * 100);
            byte[] data = BitConverter.GetBytes(degree);

            list.Add(data[1]);
            list.Add(data[0]);

            list.Add(DataConvert.CaculateHolderChecksum(list, 1));
            this.SendData(list.ToArray());
        }

        public void StartScan(float start, float end) {
            Task start_task = null;
            Task center_task = null;
            Task end_task = null;
            float threshold = 0.1f;

            cancel_source = new CancellationTokenSource();
            cancel_token = cancel_source.Token;

            if (end - start > 180) {
                float center = (start + end) / 2.0f;
                center_task = new Task(() => {
                    this.SetDegree(center);
                    while (true) {
                        if (cancel_token.IsCancellationRequested) {
                            return;
                        }


                        if (Mathf.Abs(horizontal + 1) > Mathf.Pow(10, -2) && (Mathf.Abs(horizontal - center) < threshold || Mathf.Abs(horizontal - center) > (360 - threshold))) {
                            break;
                        }
                    }

                    if (end_task != null) {
                        end_task.Start();
                    }
                });
            }

            start_task = new Task(() => {
                this.horizontal = -1;
                this.ReadHorizontal();
                this.SetSpeed(64);
                this.SetDegree(start);
                while (true) {
                    if (cancel_token.IsCancellationRequested) {
                        return;
                    }

                    if (Mathf.Abs(horizontal + 1) > Mathf.Pow(10, -2) && (Mathf.Abs(horizontal - start) < threshold || Mathf.Abs(horizontal - start) > (360 - threshold))) {
                        break;
                    }
                }

                this.SetSpeed(10);

                if (center_task != null) {
                    center_task.Start();
                } else if (end_task != null) {
                    end_task.Start();
                }
            });

            end_task = new Task(() => {
                this.SetDegree(end);
                while (true) {
                    if (cancel_token.IsCancellationRequested) {
                        return;
                    }

                    if (Mathf.Abs(horizontal + 1) > Mathf.Pow(10, -2) && (Mathf.Abs(horizontal - end) < threshold || Mathf.Abs(horizontal - end) > (360 - threshold))){
                        break;
                    }
                }
                //this.Stop();
            });
            start_task.Start();
        }

        public void StopScan(){
            if(cancel_source != null){
                cancel_source.Cancel();
            }
        }

        public void SetSpeed(int value) {
            List<byte> list = new List<byte> { 0xFF, address, 0x00, 0x5F};
            list.Add((byte)value);
            list.Add(0x00);
            list.Add(DataConvert.CaculateHolderChecksum(list, 1));
            this.SendData(list.ToArray());
        }

        public void ReStart() {
            List<byte> list = new List<byte> { 0xFF, address, 0x00, 0x0F, 0x00, 0x00};
            list.Add(DataConvert.CaculateHolderChecksum(list, 1));
            this.SendData(list.ToArray());
        }

        public void Stop() {
            List<byte> list = new List<byte> { 0xFF, address, 0x00, 0x00, 0x00, 0x00};
            list.Add(DataConvert.CaculateHolderChecksum(list, 1));
            this.SendData(list.ToArray());
        }

        public void ReadHorizontal() {
            List<byte> list = new List<byte> { 0xFF, address, 0x00, 0x51, 0x00, 0x00};
            list.Add(DataConvert.CaculateHolderChecksum(list, 1));
            this.SendData(list.ToArray());
        }

        public void SetRotateRight() {
            List<byte> list = new List<byte> { 0xFF, address, 0x00, 0x02, 0x20, 0x00 };
            list.Add(DataConvert.CaculateHolderChecksum(list, 1));
            this.SendData(list.ToArray());
        }
    }
}
