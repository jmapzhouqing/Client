using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Channels;
using System.Net.Sockets;
using System.Threading;

namespace Scanner.Communicate
{
    class SocketAsyncEventArgsPool
    {
        private Stack<SocketAsyncEventArgs> pool;

        public SocketAsyncEventArgsPool(int capacity){
            pool = new Stack<SocketAsyncEventArgs>(capacity);
        }

        public void Push(SocketAsyncEventArgs item){
            if (item == null) { throw new ArgumentNullException("SocketAsyncEventArgsPool cannot be null"); }
            try
            {
                Monitor.Enter(pool);
                pool.Push(item);
                Monitor.Pulse(pool);
            }
            finally {
                Monitor.Exit(pool);
            }
        }

        public bool WaitData(int timeout){
            bool result = true;
            try{
                Monitor.Enter(pool);
                while(pool.Count == 0 && result){
                    result = Monitor.Wait(pool,timeout,false);
                }
                return result;
            }finally {
                Monitor.Exit(pool);
            }
        }

        public SocketAsyncEventArgs Pop(){
            try {
                Monitor.Enter(pool);
                if (!Convert.ToBoolean(pool.Count)) {
                    Monitor.Wait(pool);
                }
                return pool.Pop();
            }
            finally
            {
                Monitor.Exit(pool);
            }
        }

        public int GetNumaber(){
            try{
                Monitor.Enter(pool);
                return pool.Count;
            }finally {
                Monitor.Exit(pool);
            }

        }
    }
}
