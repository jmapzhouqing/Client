using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardWareDataMonitor : MonoBehaviour
{
    private DataMonitor<DeviceData> data_monitor;
    private void Awake(){
        data_monitor = new DataMonitor<DeviceData>("192.168.0.144");

        data_monitor.OnDataUpdate += OnDataUpate;
        data_monitor.OnError += OnError;

        data_monitor.Connect();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update(){
        
    }

    void OnDataUpate(DeviceData data) {

    }

    void OnError(string data){
        Debug.Log(data);
    }

    private void OnDestroy(){
        if (data_monitor != null) {
            data_monitor.DisConnect();
        }
    }
}
