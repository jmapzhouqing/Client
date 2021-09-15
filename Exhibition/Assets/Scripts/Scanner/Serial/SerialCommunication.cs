using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;

using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

namespace Scanner.Serial
{
    class SerialCommunication
    {
        private SerialPort port;
        private bool isOpen = false;
        byte[] recv_buffer = new byte[1024];

        public delegate void data_process(byte[] data,int length);
        public event data_process DataProcess;

        private Task read_task;

        private CancellationTokenSource cancel_source;
        CancellationToken cancel_token;

        private void OnData(byte[] data,int length) {
            if (DataProcess != null) {
                DataProcess(data,length);
            }
        }

        public SerialCommunication(String name, Int32 baud_rate, Parity parity = Parity.None, Int32 data_bits = 8, StopBits stop_bits = StopBits.One)
        {
            try{

                cancel_source = new CancellationTokenSource();
                cancel_token = cancel_source.Token;

                port = new SerialPort(name, baud_rate, parity, data_bits, stop_bits);
                port.ReadTimeout = 5000;
                port.ErrorReceived += SerialPort_ErrorReceived;
                //port.DataReceived += SerialPort_DataReceived;
                //port.ReceivedBytesThreshold = 10;
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        public void StartReceiveData(int delay) {
            read_task = new Task(() => {
                ReadDataAsync(delay);
            });
            read_task.Start();
        }

        public void StopReadData() {
            if (cancel_source != null) {
                cancel_source.Cancel();
            }
        }

        public void Open() {
            try{
                if (port != null){
                    port.Open();
                }
            }catch (Exception e){
                
            }
        }

        public void Close() {
            this.StopReadData();
            if (port != null && port.IsOpen) {
                port.Close();
                port.Dispose();
            }
        }

        public void SendData(byte[] data){
            if(port.IsOpen) {
                Thread.Sleep(100);
                port.Write(data, 0, data.Length);
            }
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e){
            Console.WriteLine("Error");
        }

        private void ReadDataAsync(int delay){
            while (true){
                if (cancel_token.IsCancellationRequested){
                    break;
                }
                
                if (port.IsOpen){
                    int length = 0;
                    while (port.BytesToRead != 0){
                        length = port.Read(recv_buffer, 0, recv_buffer.Length);
                        this.OnData(recv_buffer, length);
                    }
                }
                //await Task.Delay(delay);
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e){
            int length = 0;
            try{
                Debug.Log("Enter");
                while(port.BytesToRead!=0){
                    length = port.Read(recv_buffer, 0, recv_buffer.Length);
                    this.OnData(recv_buffer, length);
                }
            }
            catch (Exception error) {
                Console.WriteLine(error.Message);
            }

            /*
            byte[] byteTemp = new byte[1000];

            if (bClosing) return;
            try 
            {
                bListening = true;
                UInt16 usLength = 0;
                try
                {
                    usLength = (UInt16)spSerialPort.Read(RxBuffer, usRxLength, 700);
                }
                catch (Exception err)
                {
                    //MessageBox.Show(err.Message);
                    //return;
                }
                usRxLength += usLength;
                while (usRxLength >= 11)
                {
                    UpdateData Update = new UpdateData(DecodeData);
                    RxBuffer.CopyTo(byteTemp, 0);
                    if (!((byteTemp[0] == 0x55) & ((byteTemp[1] & 0x50) == 0x50)))
                    {
                        for (int i = 1; i < usRxLength; i++) RxBuffer[i - 1] = RxBuffer[i];
                        usRxLength--;
                        continue;
                    }
                    if (((byteTemp[0] + byteTemp[1] + byteTemp[2] + byteTemp[3] + byteTemp[4] + byteTemp[5] + byteTemp[6] + byteTemp[7] + byteTemp[8] + byteTemp[9]) & 0xff) == byteTemp[10])
                        this.Invoke(Update, byteTemp);
                    for (int i = 11; i < usRxLength; i++) RxBuffer[i - 11] = RxBuffer[i];
                    usRxLength -= 11;
                }

                Thread.Sleep(10);
            }
            finally
            {
                bListening = false;//我用完了，ui可以关闭串口了。   
            }*/
        }
    }

   
}