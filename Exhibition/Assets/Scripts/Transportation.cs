using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using Scanner.Communicate;
using Scanner.Livox;

public class Transportation : MonoBehaviour
{
    private DeviceManager deviceManager;
    // Start is called before the first frame update
    void Start()
    {
        /*
        Client client = new Client();
        IPEndPoint client_address = new IPEndPoint(IPAddress.Any, 8000);
        client.Connect(null, client_address, ProtocolType.Udp);

        client.DataReceiveHandler += ReceiveData;*/

        deviceManager = new DeviceManager(new IPEndPoint(IPAddress.Parse("192.168.90.250"), 0), new IPEndPoint(IPAddress.Any, 55000), ProtocolType.Udp);

        deviceManager.Connect();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ReceiveData(byte[] data,int offset,int length) {
        Debug.Log(data.Length);
    }


    private void OnApplicationQuit()
    {
        /*
        if (client != null) {
            client.DisConnect();
        }*/

        if (deviceManager != null) {
            deviceManager.DisConnect();
        }
    }
}
