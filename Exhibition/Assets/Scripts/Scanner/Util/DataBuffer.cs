using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

using UnityEngine;

namespace Scanner.Util
{
    class DataBuffer
    {
        private byte[] buffer;
        private int used;

        private SocketType data_type;

        private readonly System.Object locker = new System.Object();

        private KMPSearch<byte> start_pattern;
        private KMPSearch<byte> end_pattern;

        private Queue<int> data_queue;

        public DataBuffer(int capacity, SocketType type) {
            used = 0;
            buffer = new byte[capacity];

            this.data_type = type;
            data_queue = new Queue<int>();

            /*start_pattern = new KMPSearch<byte>(new byte[] {0x02});
            end_pattern = new KMPSearch<byte>(new byte[] {0x03 });*/
        }

        public DataBuffer(int capacity, byte[] start) {
            used = 0;
            buffer = new byte[capacity];
            start_pattern = new KMPSearch<byte>(start);
            data_queue = new Queue<int>();
        }

        public DataBuffer(int capacity, byte[] start, byte[] end) {
            used = 0;
            buffer = new byte[capacity];
            start_pattern = new KMPSearch<byte>(start);
            end_pattern = new KMPSearch<byte>(end);
            data_queue = new Queue<int>();
        }

        public void PushData(byte[] src, int offset, int length) {
            //Debug.Log("PushData");
            if (this.data_type.Equals(SocketType.Stream))
            {
                this.PushStreamData(src, offset, length);
            } else if (this.data_type.Equals(SocketType.Dgram)) {
                this.PushDgramData(src, offset, length);
            }
        }

        private void PushDgramData(byte[] src, int offset, int length) {
            try
            {
                //Debug.Log(DateTime.Now.Ticks + ":push:" + used);
                Monitor.Enter(locker);

                length = src.Length - offset < length ? src.Length - offset : length;
                length = (buffer.Length - used) < length ? (buffer.Length - used) : length;

                while (buffer.Length - used <= 0) { 
                    Monitor.Wait(locker);
                }

                Buffer.BlockCopy(src, offset, buffer, used, length);
                data_queue.Enqueue(length);
                used += length;

                
            }
            finally
            {
                Monitor.PulseAll(locker);
                Monitor.Exit(locker);
            }
        }
        private void PushStreamData(byte[] src, int offset, int length){
            try
            {
                Monitor.Enter(locker);

                while (buffer.Length - used <= 0){
                    Monitor.Wait(locker);
                }

                length = src.Length < length ? src.Length : length;
                length = (buffer.Length - used) < length ? (buffer.Length - used) : length;

                Buffer.BlockCopy(src, offset, buffer, used, length);
                used += length;
            }
            finally
            {
                Monitor.PulseAll(locker);
                Monitor.Exit(locker);
            }
        }

        public byte[] PopData(int offset, int length) {
            try{
                //Monitor.Enter(locker);

                byte[] data = new byte[length];
                Buffer.BlockCopy(buffer, offset, data, 0, length);

                int translation_start = offset + length;
                int translation_length = used - translation_start;

                Buffer.BlockCopy(buffer, translation_start, buffer, 0, buffer.Length - translation_start);

                used -= translation_start;

                return data;
            }finally{
                //Monitor.PulseAll(locker);
                //Monitor.Exit(locker);
            }
        }

        public byte[] SearchData() {
            if(this.data_type.Equals(SocketType.Stream)){
                return this.SearchStreamData();
            }else if (this.data_type.Equals(SocketType.Dgram)){
                return this.SearchDgramData();
            }

            return null;
        }

        private byte[] SearchStreamData(){
            try{
                Monitor.Enter(locker);
                byte[] result = null;

                while(used == 0) {
                    Monitor.Wait(locker);
                }

                if (used > 0) {
                    int start = start_pattern.Search(buffer, 0,used);
                    if (start != -1){
                        int end = end_pattern.Search(buffer, start, used);
                        if (end != -1 && end < used){
                            result = this.PopData(start + 1, end - start - 1);
                        }
                    }
                }
                return result;
            }
            finally
            {
                Monitor.PulseAll(locker);
                Monitor.Exit(locker);
            }
        }

        private byte[] SearchDgramData() {
            try
            {
                Monitor.Enter(locker);
                byte[] result = null;
                //Debug.Log(DateTime.Now.Ticks + ":search:" + used);
                while (data_queue.Count == 0){
                    //Debug.Log("Enter Zero");
                    Monitor.Wait(locker);
                }

                if(data_queue.Count != 0){
                    int length = data_queue.Dequeue();
                    if (length != 0){
                        result = this.PopData(0, length);
                    }
                }

                return result;
            }
            finally{
                Monitor.PulseAll(locker);
                Monitor.Exit(locker);
            }
        }

        public byte[] GetData(){
            return this.buffer;
        }
    }
}