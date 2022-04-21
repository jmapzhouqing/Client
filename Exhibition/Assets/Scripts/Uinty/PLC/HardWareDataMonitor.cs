using System;
using UnityEngine;


public class HardWareDataMonitor : MonoBehaviour
{
    public DeviceStatusControl device_yaw;

    public DeviceStatusControl device_pitch;

    public DeviceStatusControl device_forward;

    private DataMonitor<DeviceData> data_monitor;

    public DeviceData data;

    public bool IsConnected = false;

    public string ip;

    private void Awake(){
        data_monitor = new DataMonitor<DeviceData>(ip);

        data_monitor.OnDataUpdate += OnDataUpate;
        data_monitor.OnError += OnError;
        data_monitor.OnStatusChange += OnStatusChanged;
       
        data_monitor.Connect();
    }

    void Start(){

    }

    // Update is called once per frame
    void Update(){
        
    }

    void OnDataUpate(DeviceData data){
        Debug.Log(data.SlewStatus+":"+data.LuffStatus+":"+data.CarStatus);
        device_yaw.Status = data.SlewStatus;
        device_pitch.Status = data.LuffStatus;
        device_forward.Status = data.CarStatus;
        this.data = data;
    }

    void OnError(Exception data){
        Debug.Log(data);
    }

    void OnStatusChanged(bool status) {
        this.IsConnected = status;
    }

    private void OnDestroy(){
        if (data_monitor != null) {
            data_monitor.DisConnect();
        }
    }
}
