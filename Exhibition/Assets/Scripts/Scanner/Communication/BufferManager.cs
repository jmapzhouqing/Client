using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Scanner.Communicate
{
    class BufferManager
    {
        int totalBytes;                 
        byte[] buffer;
        Stack<int> freeIndexPool;
        int index;
        int bufferSize;

        public BufferManager(int totalBytes, int bufferSize)
        {
            this.totalBytes = totalBytes;
            this.index = 0;
            this.bufferSize = bufferSize;
            freeIndexPool = new Stack<int>();

            this.buffer = new byte[totalBytes];
        }

        public bool SetBuffer(SocketAsyncEventArgs args){
            if (freeIndexPool.Count > 0){
                args.SetBuffer(buffer, freeIndexPool.Pop(),bufferSize);
            }else{
                if((totalBytes - bufferSize) < index){
                    return false;
                }
                args.SetBuffer(buffer, index, bufferSize);
                index += bufferSize;
            }
            return true;
        }

        public void FreeBuffer(SocketAsyncEventArgs args){
            freeIndexPool.Push(args.Offset);
            args.SetBuffer(null, 0, 0);
        }
    }
}
