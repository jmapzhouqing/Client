using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Collections.Generic;

using UnityEngine;

using Scanner.Serial;
using Scanner.Util;
using Scanner.Struct;

namespace Scanner.Holder
{
    class Holder: SerialDevice
    {
        private SerialCommunication communication;

        private Task send_task;
        private CancellationTokenSource cancel_source;
        private CancellationToken cancel_token;

        public float horizontal = -1;
        private float vertical;

        public Holder(String name, Int32 baud_rate, Parity parity = Parity.None, Int32 data_bits = 8, StopBits stop_bits = StopBits.One):base(name, baud_rate, parity, data_bits, stop_bits){

            this.StartReceiveData(10);
        }

        public override void DataProcess(byte[] data, int length){
            int crc = DataConvert.CaculateHolderChecksum(data, 1, length - 2);

            //Debug.Log(length+"#"+crc + "#" + data[length-1]);

            if (data[length-1] == crc){
                this.DecodeData(data, 2, length - 3);
            }
        }

        public override void Close(){
            if (cancel_source != null){
                cancel_source.Cancel();
            }
            this.Stop();
            base.Close();
        }

        private void DecodeData(byte[] data, int offset, int length){
            HolderData holder_data = DataConvert.ConvertByteStruct<HolderData>(data, offset, true);
            if (holder_data.control == 0x59) {
                horizontal = holder_data.data / 100.0f;
            }
        }

        public void SetDegree(float value){
            List<byte> list = new List<byte> {0xFF,0x01,0x00,0x4B};
            UInt16 degree = Convert.ToUInt16(value * 100);
            byte[] data = BitConverter.GetBytes(degree);

            list.Add(data[1]);
            list.Add(data[0]);

            list.Add(DataConvert.CaculateHolderChecksum(list,1));
            this.SendData(list.ToArray());
        }

        public void SetScanDegree(float start,float end){
            List<byte> list = new List<byte> { 0xFF,0x01,0x00,0x11,0x00,0x00};
            UInt16 start_value = Convert.ToUInt16(start * 100);
            byte[] data = BitConverter.GetBytes(start_value);
            list.Add(data[1]);
            list.Add(data[0]);
            list.Add(DataConvert.CaculateHolderChecksum(list, 1));

            this.SendData(list.ToArray());

            list = new List<byte> { 0xFF, 0x01, 0x00, 0x13, 0x00, 0x00 };
            UInt16 end_value = Convert.ToUInt16(end * 100);
            data = BitConverter.GetBytes(end_value);
            list.Add(data[1]);
            list.Add(data[0]);
            list.Add(DataConvert.CaculateHolderChecksum(list, 1));
            this.SendData(list.ToArray());
        }

        public void StartScan(float start, float end){
            Task start_task = null;
            Task center_task = null;
            Task end_task = null;
            float threshold = 0.1f;

            if(end - start > 180){
                float center = (start + end) / 2.0f;

                center_task = new Task(() =>{
                    this.SetDegree(center);
                    while (true) {
                        if (cancel_token.IsCancellationRequested){
                            return;
                        }

                        if (Mathf.Abs(horizontal + 1) > Mathf.Pow(10, -2) && (Mathf.Abs(horizontal - center) < threshold || Mathf.Abs(horizontal - center) > (360 - threshold))){
                            break;
                        }
                    }

                    if (end_task != null) {
                        end_task.Start();
                    }
                });
            }


            cancel_source = new CancellationTokenSource();
            cancel_token = cancel_source.Token;

            start_task = new Task(() => {
                this.horizontal = -1;
                this.ReadHorizontal();
                this.SetSpeed(64);
                this.SetDegree(start);
                while (true) {
                    if (cancel_token.IsCancellationRequested) {
                        return;
                    }

                    if (Mathf.Abs(horizontal + 1) > Mathf.Pow(10, -2) && (Mathf.Abs(horizontal - start) < threshold || Mathf.Abs(horizontal - start) >(360 - threshold))){
                        break;
                    }
                }

                this.SetSpeed(10);

                if (center_task != null){
                    center_task.Start();
                }else if (end_task != null) {
                    end_task.Start();
                }
            });

            end_task = new Task(()=>{
                this.SetDegree(end);
                while (true){
                    if (cancel_token.IsCancellationRequested){
                        return;
                    }

                    if (Mathf.Abs(horizontal + 1) > Mathf.Pow(10, -2) && (Mathf.Abs(horizontal - end) < threshold || Mathf.Abs(horizontal - end) > (360 - threshold)))
                    {
                        break;
                    }
                }

                //this.Stop();
            });

            start_task.Start();
        }

        public void SetSpeed(int value) {
            List<byte> list = new List<byte> { 0xFF, 0x01, 0x00, 0x5F};
            list.Add((byte)value);
            list.Add(0x00);
            list.Add(DataConvert.CaculateHolderChecksum(list, 1));
            this.SendData(list.ToArray());
        }

        public void StartScan(int speed) {
            List<byte> list = new List<byte> { 0xFF, 0x01, 0x00, 0x31 };
            list.Add(Convert.ToByte(speed));
            list.Add(0x00);
            list.Add(DataConvert.CaculateHolderChecksum(list, 1));
            this.SendData(list.ToArray());

            list = new List<byte> { 0xFF, 0x01, 0x00, 0x1B, 0x00, 0x00};
            list.Add(DataConvert.CaculateHolderChecksum(list, 1));
            this.SendData(list.ToArray());
        }

        public void StopScan(){
            List<byte> list = new List<byte> { 0xFF, 0x01, 0x00, 0x1D, 0x00, 0x00 };
            list.Add(DataConvert.CaculateHolderChecksum(list, 1));
            this.SendData(list.ToArray());
        }

        public void ReStart() {
            List<byte> list = new List<byte> { 0xFF, 0x01, 0x00, 0x0F, 0x00, 0x00 };
            list.Add(DataConvert.CaculateHolderChecksum(list, 1));
            this.SendData(list.ToArray());
        }

        public void Stop() {
            List<byte> list = new List<byte> { 0xFF, 0x01, 0x00, 0x00, 0x00, 0x00};
            list.Add(DataConvert.CaculateHolderChecksum(list, 1));
            this.SendData(list.ToArray());
        }

        public void ReadHorizontal() {
            List<byte> list = new List<byte> { 0xFF, 0x01, 0x00, 0x51, 0x00, 0x00};
            list.Add(DataConvert.CaculateHolderChecksum(list, 1));
            this.SendData(list.ToArray());
        }

        public void SetRotateRight() {
            List<byte> list = new List<byte> { 0xFF, 0x01, 0x00, 0x02, 0x20, 0x00 };
            list.Add(DataConvert.CaculateHolderChecksum(list, 1));
            this.SendData(list.ToArray());
        }
    }
}
