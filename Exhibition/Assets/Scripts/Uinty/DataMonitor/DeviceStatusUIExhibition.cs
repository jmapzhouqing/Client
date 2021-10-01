using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class DeviceStatusUIExhibition:DeviceStatusControl
{
    public Image status;

    private Sprite stop;
    private Sprite error;
    private Sprite foward;
    private Sprite back;
    public override void SetStatus(string value){
        if (value.Equals("0")){

        }else if (value.Equals("1")){

        }else if (value.Equals("2")) {

        }
    }
}