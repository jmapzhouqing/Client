using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HslCommunication;
using HslCommunication.Reflection;

public class DeviceData{
    [HslDeviceAddress("HS_Car_FaultS")]
    public short CarStatus { get; set; }

    [HslDeviceAddress("HS_Slew_FaultS")]
    public short SlewStatus { get; set; }

    [HslDeviceAddress("HS_Luff_FaultS")]
    public short LuffStatus { get; set; }

    [HslDeviceAddress("HS_Car_POS_KU_R")]
    public float CarPos { get; set; }

    [HslDeviceAddress("HS_Luff_Angle")]
    public float LuffAngle { get; set; }


    [HslDeviceAddress("HS_Slew_Angle")]
    public float SlewAngle { get; set; }


}
