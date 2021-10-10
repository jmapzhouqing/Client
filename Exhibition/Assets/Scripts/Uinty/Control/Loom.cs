using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;



public class Loom : MonoBehaviour
{
    public static int maxThreads = 8;
    static int numThreads;

    List<NoDelayedQueueItem> nodelay_execute_actions;

    List<DelayedQueueItem> delay_execute_actions;

    private static List<NoDelayedQueueItem> no_delayed;

    private static List<DelayedQueueItem> delayed;

    void Awake(){
        nodelay_execute_actions = new List<NoDelayedQueueItem>();

        delay_execute_actions = new List<DelayedQueueItem>();

        no_delayed = new List<NoDelayedQueueItem>();

        delayed = new List<DelayedQueueItem>();
    }

    /*
    public static void Initialize(){
        if (!initialized){
            if (!Application.isPlaying)
                return;
            initialized = true;
            var loom = new GameObject("Loom");
            _current = loom.AddComponent<Loom>();
            #if !ARTIST_BUILD
                        UnityEngine.Object.DontDestroyOnLoad(loom);
            #endif
        }
    }*/

    public struct NoDelayedQueueItem{
        public Action<object> action;
        public object param;
    }

    public struct DelayedQueueItem{
        public double time;
        public Action<object> action;
        public object param;
    }
    

    public static void QueueOnMainThread(Action<object> taction, object tparam)
    {
        QueueOnMainThread(taction, tparam, 0f);
    }
    public static void QueueOnMainThread(Action<object> taction, object tparam, double time){
        if(time > Mathf.Pow(10,-2)){
            long times = DateTime.Now.Ticks;
            try{
                Monitor.Enter(delayed);
                delayed.Add(new DelayedQueueItem{time = TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds + time, action = taction, param = tparam});
            }finally {
                Monitor.Exit(delayed);
            }
        }else{
            try{
                Monitor.Enter(no_delayed);
                no_delayed.Add(new NoDelayedQueueItem{action = taction, param = tparam});
            }finally{
                Monitor.Exit(no_delayed);
            }
        }
    }

    public static Thread RunAsync(Action a){
        //Initialize();
        while (numThreads >= maxThreads){
            Thread.Sleep(100);
        }
        Interlocked.Increment(ref numThreads);
        ThreadPool.QueueUserWorkItem(RunAction, a);
        return null;
    }

    private static void RunAction(object action){
        try{
            ((Action)action)();
        }catch{

        }finally{
            Interlocked.Decrement(ref numThreads);
        }
    }

    void Update(){
        try{
            Monitor.Enter(no_delayed);
            if (no_delayed.Count > 0){
                nodelay_execute_actions.Clear();
                nodelay_execute_actions.AddRange(no_delayed);
                no_delayed.Clear();
            }

            foreach (NoDelayedQueueItem item in nodelay_execute_actions){
                item.action(item.param);
            }
            nodelay_execute_actions.Clear();
        }finally {
            Monitor.Exit(no_delayed);
        }

        try{
            Monitor.Enter(delayed);
            if (delayed.Count > 0){
                delay_execute_actions.Clear();
                delay_execute_actions.AddRange(delayed.Where(d => d.time <= TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds));
                foreach (DelayedQueueItem item in delay_execute_actions){
                    delayed.Remove(item);
                }
            }

            foreach(DelayedQueueItem item in delay_execute_actions){
                item.action(item.param);
            }
            delay_execute_actions.Clear();
        }finally {
            Monitor.Exit(delayed);
        }
    }
}

