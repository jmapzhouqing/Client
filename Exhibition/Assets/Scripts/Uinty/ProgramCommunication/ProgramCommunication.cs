using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

using Scanner.Communicate;
using Scanner.Scanister;
using Scanner.Struct;

public class ProgramCommunication : MonoBehaviour
{
    //private Correspond client;
    public string ip;
    public int port;

    public int timeout;

    private Triple triple;

    private ClientTransmission client;


    public DeviceStatusControl client_server;

    void Start(){
        //server_address = new IPEndPoint(IPAddress.Parse("192.168.90.252"), 8100);
        //client_address = new IPEndPoint(IPAddress.Any,0);

        client = new ClientTransmission("控制端",ip,port);
        client.StatusChanged += StatusChanged;
        client.Error += OnError;
        client.Connect(timeout);

        //client.Connect(server_address, client_address, ProtocolType.Tcp);

        /*
        Triple triple = new Triple("", "192.168.90.247", 1024, ProtocolType.Udp);
        triple.StatusChanged += StatusChanged;
        triple.Error += OnError;

        triple.Connect();*/
    }

    private void OnDestroy(){
        if (client != null) {
            client.DisConnect();
        }
    }

    /*
    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 60), "Click"))
        {
            if (client != null)
            {
                client.DisConnect();
            }
        }
    }*/

    public void SendData(string data) {
        if (client != null) {
            client.SendData(data);
        }
    }

    private void StatusChanged(DeviceStatus status){
        Debug.Log(status);
        if (status.Equals(DeviceStatus.OnLine)){
            //client.StartProcessData(100);
            client_server.Status = 1;
        }
        else {
            client_server.Status = 2;
            //UIManager.instance.ReLinkServer("控制端连接断开,请检查控制端并重新连接", client.ReLink);
            //client.StopProcessData();
        }
    }

    private void OnError(ExceptionHandler exception) {
        //Debug.Log(exception.Message+"#"+exception.GetExceptionCode().ToString());

        //exception.GetExceptionCode().Equals(ExceptionCode.ConnectionRefused)

        if (exception.GetExceptionCode().Equals(ExceptionCode.ConnectionRefused))
        {
            //UIManager.instance.ReLinkServer("连接控制端被拒,请检查控制端是否启动并重新连接", client.ReLink);
        }
        else {
            //UIManager.instance.ExhibitionInfo(exception.Message);
        }
    }
}
