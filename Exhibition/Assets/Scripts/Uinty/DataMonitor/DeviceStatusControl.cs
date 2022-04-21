using UnityEngine;

using Scanner.Struct;

public abstract class DeviceStatusControl : MonoBehaviour
{

    protected short status = -1;
    public short Status
    {
        get { return this.status; }
        set
        {
            if (this.status != value){
                this.SetStatus(value.ToString());
            }
        }
    }
    public abstract void SetStatus(string value);
}
