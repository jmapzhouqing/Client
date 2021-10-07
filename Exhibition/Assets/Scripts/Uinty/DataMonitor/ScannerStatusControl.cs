using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScannerStatusControl : DeviceStatusControl{
    public Image status_icon;

    public Sprite error;
    public Sprite stop;
    public Sprite working;

    private void Awake()
    {
        this.Status = -1;
    }

    public override void SetStatus(string value)
    {
        if (value.Equals("-1")) {
            status_icon.sprite = this.error;
        }else if (value.Equals("3")){
            status_icon.sprite = this.stop;
        }else if (value.Equals("4")){
            status_icon.sprite = this.working;
        }else if (value.Equals("2")){

        }
    }
}
