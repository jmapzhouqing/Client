using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class BucketWheelStatusControl :DeviceStatusControl
{
    public Image status_icon;

    public Sprite error;
    public Sprite stop;
    public Sprite foward;
    public Sprite back;

    private void Awake()
    {
        this.Status = -1;
    }

    public override void SetStatus(string value){
        if (value.Equals("-1")) {
            status_icon.sprite = this.error;
        }else if (value.Equals("0")){
            status_icon.sprite = this.stop;
        }
        else if (value.Equals("1")){
            status_icon.sprite = this.foward;
        }
        else if (value.Equals("2")) {
            status_icon.sprite = this.back;
        }
    }
}