using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TakeCoalExhibition : MonoBehaviour
{
    public Text device_yaw;
    public Text device_pitch;
    public Text device_position;

    public Text coal_dump;
    public Text level;
    private HardWareDataMonitor hardware_data;

    private ScannerManager scanner_manager;
    // Start is called before the first frame update
    void Awake()
    {
        hardware_data = FindObjectOfType<HardWareDataMonitor>();

        scanner_manager = FindObjectOfType<ScannerManager>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (hardware_data.IsConnected){
            DeviceData data = hardware_data.data;
            device_pitch.text = data.LuffAngle.ToString();
            device_yaw.text = data.SlewAngle.ToString();
            device_position.text = data.CarPos.ToString();
        }else{
            device_yaw.text = "";
            device_pitch.text = "";
            device_position.text = "";
        }
    }

    public void SetInfo(TakeCoalInfo info){
        level.text = info.level.ToString();
    }

    public void ScannerDataUpdate(float value) {
        if (Mathf.Abs(value) < Mathf.Pow(10,-2)){
            scanner_manager.StopScanning();
        }else if(Mathf.Abs(value - 1)<Mathf.Pow(10,-2)){
            scanner_manager.StartScanning();
        }
    }

    private void OnEnable(){
        UIManager.instance.LockLeft(true);
    }

    private void OnDisable()
    {
        UIManager.instance.LockLeft(false);
    }
}
