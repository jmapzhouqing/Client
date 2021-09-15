using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Threading;

using System.IO.Ports;

using Scanner.Serial;
using Scanner.Scanister;
using Scanner.Struct;
using Scanner.Holder;

using System.Net.Sockets;


public class ScannerPoint : MonoBehaviour
{
    private WIT wit;
    //private 
    private Triple triple;

    private Holder holder;

    GridDataManager grid_data_manager;

    private void Awake()
    {
        grid_data_manager = this.GetComponent<GridDataManager>();

       
    }
    // Start is called before the first frame update
    void Start(){
        wit = new WIT("COM4", 9600);
        wit.Open();

        LoggerInfo.Log(typeof(ScannerPoint), "Error", this.transform);

        triple = new Triple("192.168.90.247", 1024, ProtocolType.Udp);
        triple.DataTransform += DataTransform;
        triple.Connect();

        holder = new Holder("COM2", 4800);
        holder.Open();

        //holder.HorizontalScan();
    }

    public void DataTransform(List<RayInfo> rays){
        Vector3 scanner_line_dir = -1 * Vector3.right;
        Vector3 scanner_rotate_dir = -1 * Vector3.up;
        Vector3 scanner_line_rotate_dir = Vector3.forward;

        Matrix4x4 matrix = new Matrix4x4();
        Quaternion quaternion;

        Vector3 origin = Vector3.zero;

        List<Vector3> vertices = new List<Vector3>();

        foreach (RayInfo info in rays){

            quaternion = Quaternion.AngleAxis(info.degree, scanner_line_rotate_dir);
            matrix.SetTRS(Vector3.zero, quaternion, new Vector3(1, 1, 1));

            origin = matrix.MultiplyPoint(scanner_line_dir * info.distance);

            Debug.Log(wit.Rotation);

            Vector3 rotation = wit.Rotation;

            quaternion = Quaternion.Euler(rotation.x,-1 * holder.horizontal,rotation.y);
            matrix.SetTRS(new Vector3(50,2.5f,50),quaternion,new Vector3(1,1,1));

            origin = matrix.MultiplyPoint(origin);

            vertices.Add(origin);
        }

        grid_data_manager.UpdateGridData(vertices);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartDevice() {
        CoalDumpInfo info = new CoalDumpInfo();
        info.vertices = new List<Vector2> { new Vector2(0, 0), new Vector2(100, 0), new Vector2(100, 100), new Vector2(0, 100) };

        grid_data_manager.CreateCoalDump(new List<CoalDumpInfo> { info });


        wit.StartReadData(10);
        triple.Start();
        holder.StartScan(1, 359);
    }

    private void OnGUI()
    {
        if(GUI.Button(new Rect(0, 0, 100, 60), "Click")){
            this.StartDevice();
        }
    }

    private void StopDevice(){

    }

    private void OnDisable()
    {
        if (wit != null){
            Debug.Log("Disable");
            wit.Close();
        }

        if (triple != null) {
            triple.Close();
        }

        if (holder != null) {
            holder.Close();
        }
    }

    void OnApplicationQuit(){
        
        if (wit != null) {
            Debug.Log("Quit");
            wit.Close();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (wit != null)
        {
            wit.Close();
        }
    }
}
