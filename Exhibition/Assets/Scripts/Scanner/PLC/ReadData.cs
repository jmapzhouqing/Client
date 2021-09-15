using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unattended.PLC;

public class ReadData : MonoBehaviour
{
    private string data;

    private ABPLCTool plc;
    // Start is called before the first frame update
    void Awake()
    {
        plc = new ABPLCTool();
        plc.Init("192.168.0.144");
    }

    // Update is called once per frame
    void FixedUpdate(){
        data = plc.ReadData(4, "HS_Slew_Angle");
    }

    private void OnGUI()
    {
        GUI.TextField(new Rect(0, 0, 100, 60),data);
    }
}
