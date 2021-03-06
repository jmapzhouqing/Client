using UnityEngine;
using UnityEngine.UI;

using Scanner.Struct;

public class BucketArmStatusControl : DeviceStatusControl
{
    public Image status_icon;

    public Sprite disconnect;
    public Sprite connect;
    public Sprite working;

    private void Awake()
    {
        this.Status = (short)DeviceStatus.NotConnect;
    }

    public override void SetStatus(string value)
    {
        Loom.QueueOnMainThread((param) =>
        {
            if (value.Equals("1"))
            {
                status_icon.sprite = this.connect;
            }
            else if (value.Equals("2"))
            {
                status_icon.sprite = this.disconnect;
            }
        }, null);
    }
}
