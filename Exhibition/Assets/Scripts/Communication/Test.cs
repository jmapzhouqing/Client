using System.Collections;
using System;
using System.Net;
using UnityEngine;

using Scanner.Communicate;
using Scanner.Struct;

public class Test : MonoBehaviour
{
    private Correspond correspond;

    IPEndPoint server_address = new IPEndPoint(IPAddress.Parse("192.168.0.101"), 8100);
    // Start is called before the first frame update
    void Start()
    {
        IPEndPoint client_address = new IPEndPoint(IPAddress.Any, 0);

        correspond = new Correspond_TCP("",client_address,new byte[]{0x02});

        correspond.Error += OnError;
        correspond.StatusChanged += OnStatusChanged;

        correspond.Connect(server_address,5000);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnError(ExceptionHandler exception) {
        Debug.Log(exception.Message+":"+exception.GetExceptionCode().ToString());
        ExceptionCode code = exception.GetExceptionCode();
        if (code == ExceptionCode.ConnectionRefused)
        {
            Debug.Log("远程服务器拒绝连接");
        }
        else if (code == ExceptionCode.TimedOut){
            Debug.Log("连接超时");
        }
        else if (code == ExceptionCode.ConnectionReset)
        {
            Debug.Log("远程服务器意外断开");
            correspond.DisConnect();
        }
        else if (code == ExceptionCode.Shutdown)
        {
            Debug.Log("连接正常断开");
        }
        else if (code == ExceptionCode.OperationAborted)
        {
            Debug.Log("操作终止");
        }
    }

    int index = 0;
    private void OnStatusChanged(DeviceStatus status){
        if (status.Equals(DeviceStatus.OffLine)){
            //correspond.Relink();
        }
        /*
        if (status.Equals(DeviceStatus.OnLine))
        {
            Debug.Log("设备上线");
        }
        else if (status.Equals(DeviceStatus.OffLine)) {
            Debug.Log("设备下线");
            //correspond.DisConnect();
            //correspond.Connect(server_address,5000);
        }*/
    }

    private void OnDisable()
    {
        correspond.DisConnect();
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 60), "Click"))
        {
            correspond.DisConnect();
        }

        if (GUI.Button(new Rect(200, 0, 100, 60), "Connect"))
        {
            correspond.Connect(server_address, 5000);
        }
    }
}
