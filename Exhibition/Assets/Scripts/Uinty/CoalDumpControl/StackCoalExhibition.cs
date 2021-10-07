using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class StackCoalExhibition : MonoBehaviour
{
    public Text device_yaw;
    public Text device_pitch;
    public Text device_position;

    public Text coal_dump;
    public Text interior_edge;
    public Text external_edge;

    private HardWareDataMonitor hardware_data;
    // Start is called before the first frame update
    void Awake(){
        hardware_data = FindObjectOfType<HardWareDataMonitor>();
    }

    // Update is called once per frame
    void FixedUpdate(){
        if (hardware_data.IsConnected){
            DeviceData data = hardware_data.data;

            device_pitch.text = data.LuffAngle.ToString();
            device_yaw.text = data.SlewAngle.ToString();
            device_position.text = data.CarPos.ToString();
        }else {
            device_yaw.text = "";
            device_pitch.text = "";
            device_position.text = "";
        }
    }

    public void SetInfo(StackCoalInfo info) {
        interior_edge.text = info.internal_rotation.ToString();
        external_edge.text = info.external_rotation.ToString();
    }

    private void OnEnable(){
        UIManager.instance.LockLeft(true);
    }

    private void OnDisable()
    {
        UIManager.instance.LockLeft(false);
    }
}
