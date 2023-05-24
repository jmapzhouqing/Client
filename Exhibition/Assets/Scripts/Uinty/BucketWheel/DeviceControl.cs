using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceControl : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform arm;
    public Transform foundation;

    private Vector3 foundation_euelr;
    private Vector3 arm_euler;

    void Start()
    {
        foundation_euelr = foundation.eulerAngles;
        arm_euler = arm.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetRotation(float rotation) {
        Debug.Log(foundation_euelr);
        foundation.rotation = Quaternion.Euler(foundation_euelr + new Vector3(0,0,rotation));
    }

    public void SetPosition(Vector3 position) {
        this.transform.position = position;
    }

    public void SetPitch(float pitch) {
        arm.rotation = Quaternion.Euler(arm_euler + new Vector3(pitch,0,0));
    }
}
