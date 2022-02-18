using System;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;

namespace Scanner.Util
{
    public class RecvQueue<T> where T : struct
    {
        List<T> list;
        public RecvQueue()
        {
            this.list = new List<T>();
        }

        public void Push(T value)
        {
            try
            {
                Monitor.Enter(this);
                list.Add(value);
            }
            finally
            {
                //Monitor.Exit(this);
                Monitor.Pulse(this);
                Monitor.Exit(this);
            }
        }

        public bool WaitIncomingObject(int timeout)
        {
            try{
                Monitor.Enter(this);
                bool result = true;
                while(list.Count == 0 && result){
                    result = Monitor.Wait(this, timeout);
                }
                return result;
            }finally {
                Monitor.Exit(this);
            }
            
        }

        public T Pop()
        {
            try
            {
                Monitor.Enter(this);
                while (list.Count == 0)
                {
                    Monitor.Wait(this);
                }
                T value = list[0];
                list.RemoveAt(0);
                return value;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public int GetDataNumber()
        {
            int result = 0;
            try
            {
                Monitor.Enter(this);
                result = list.Count;
                return result;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        bool isQueueEmpty()
        {
            bool result = true;
            try
            {
                Monitor.Enter(this);
                result = (list.Count == 0 ? true : false);
                return result;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }
    }
}


