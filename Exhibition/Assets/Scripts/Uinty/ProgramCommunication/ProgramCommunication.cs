using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

using Scanner.Communicate;

public class ProgramCommunication : MonoBehaviour
{
    private Client client;

    private IPEndPoint server_address;
    private IPEndPoint client_address;

    void Start(){
        /*
        server_address = new IPEndPoint(IPAddress.Parse(""), 0);
        client_address = new IPEndPoint(IPAddress.Any, 0);

        client = new Client();
        client.DataReceiveHandler += DataReceive;
        client.Connect(server_address, client_address, ProtocolType.Tcp);*/
    }

    public void DataReceive(byte[] buff, int offset, int length){

    }

    private void OnDestroy(){
        if (client != null) {
            client.DisConnect();
        }
    }

    public void SendData(string data) {
        if (client != null) {
            client.SendData(Encoding.Default.GetBytes(data));
        }
    }
}
