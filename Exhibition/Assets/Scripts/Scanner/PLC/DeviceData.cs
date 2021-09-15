using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HslCommunication;
using HslCommunication.Reflection;

public class DeviceData{
    [HslDeviceAddress("HS_Car_Status")]
    public short CarStatus { get; set; }

    [HslDeviceAddress("HS_Slew_Status")]
    public short SlewStatus { get; set; }

    [HslDeviceAddress("HS_Luff_Status")]
    public short LuffStatus { get; set; }

    [HslDeviceAddress("HS_Car_POS_KU_R")]
    public float CarPos { get; set; }
}
