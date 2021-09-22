using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Scanner.Util;
using System.Threading.Tasks;
using Scanner.Struct;
using UnityEngine;
using System.Net.Sockets;

namespace Scanner.Serial
{
    abstract class SerialDevice
    {
        private SerialCommunication communication;

        private DataBuffer buffer;

        private int used_index = 0;

        private Task task;
        public SerialDevice(String name, Int32 baud_rate, Parity parity = Parity.None, Int32 data_bits = 8, StopBits stop_bits = StopBits.One)
        {
            communication = new SerialCommunication(name, baud_rate, parity, data_bits, stop_bits);

            communication.DataProcess += DataProcess;

            buffer = new DataBuffer(102400, SocketType.Stream);
        }

        public void Open(){
            if (communication != null){
                communication.Open();
            }
        }

        public virtual void Close() {
            if (communication != null){
                communication.Close();
            }
        }

        public void SendData(byte[] data)
        {
            if (communication != null){
                communication.SendData(data);
            }
        }

        public abstract void DataProcess(byte[] data, int length);

        public void StartReceiveData(int delay){
            if (communication != null) {
                communication.StartReceiveData(delay);
            }
        }

        public void StopReceiveData()
        {
            if (communication != null)
            {
                communication.StopReceiveData();
            }
        }
    }
}
