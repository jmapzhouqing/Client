using System;
using UnityEngine;
using UnityEngine.UI;

using Scanner.Struct;

public class ScannerStatusControl : DeviceStatusControl {
    public Image status_icon;

    public Sprite disconnect;
    public Sprite connect;
    public Sprite working;

    private void Awake()
    {
        this.Status = (short)DeviceStatus.NotConnect;
    }

    public override void SetStatus(string value){
        DeviceStatus status = (DeviceStatus)Enum.ToObject(typeof(DeviceStatus),Convert.ToInt16(value));
        if (status.Equals(DeviceStatus.DisConnect)|| status.Equals(DeviceStatus.NotConnect) || status.Equals(DeviceStatus.OffLine)) {
            status_icon.sprite = this.disconnect;
        }else if (status.Equals(DeviceStatus.Connect)|| status.Equals(DeviceStatus.OnLine)){
            status_icon.sprite = this.connect;
        }else if (status.Equals(DeviceStatus.Working)){
            status_icon.sprite = this.working;
        }
    }
}
