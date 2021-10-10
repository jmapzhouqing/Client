using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Threading;

using System.IO.Ports;

using System.Net;

using Scanner.Serial;
using Scanner.Scanister;
using Scanner.Struct;
using Scanner.Holder;
using Scanner.Communicate;

using System.Net.Sockets;

public class ScannerPoint : MonoBehaviour
{
    public string ip;

    public int port;

    private WIT wit;
    //private 
    private Scanner.Scanister.Scanner scanner;

    private Holder holder;

    private Client client;

    GridDataManager grid_data_manager;

    private HardWareDataMonitor hardware_monitor;

    public DeviceStatusControl scanner_status;

    private void Awake(){
        grid_data_manager = this.GetComponent<GridDataManager>();

        hardware_monitor = FindObjectOfType<HardWareDataMonitor>();
    }
    // Start is called before the first frame update
    void Start(){
        //wit = new WIT("COM4", 9600);
        //wit.Open();

        //LoggerInfo.Log(typeof(ScannerPoint), "Error", this.transform);


        /*
        client = new Client();

        IPEndPoint server_address = new IPEndPoint(IPAddress.Parse("192.168.90.247"), 1024);
        IPEndPoint client_address = new IPEndPoint(IPAddress.Any,0);

        client.StatusChanged += StatusChanged;
        client.Error += OnError;

        client.Connect(server_address, client_address, ProtocolType.Udp);*/

        //盘煤仪 sick
        /*scanner = new LMS511("Sick", ip, port);
        scanner.DataDecodeComplete += DataTransform;*/

        /*
        scanner = new Triple("Triple",ip,port);
        scanner.DataDecodeComplete += TripleTransform;
        
        scanner.StatusChanged += StatusChanged;
        scanner.Error += OnError;
        scanner.Connect();*/


        scanner = new KYLE("KYLE", ip, port);

        scanner.DataDecodeComplete += TripleTransform;

        scanner.StatusChanged += StatusChanged;
        scanner.Error += OnError;
        scanner.Connect();

        //holder = new Holder("COM2", 4800);
        //holder.Open();

        //holder.HorizontalScan();
    }

    public void DataTransform(List<RayInfo> rays){
        try{

            if (hardware_monitor.IsConnected) {
                Vector3 scanner_line_dir = -1 * Vector3.right;
                Vector3 scanner_line_rotate_dir = Vector3.forward;

                Vector3 pitch_axis = -1 * Vector3.right;
                Vector3 yaw_axis = Vector3.up;
                Vector3 forward_dir = Vector3.forward;

                float pitch = hardware_monitor.data.LuffAngle;
                float yaw = hardware_monitor.data.SlewAngle;
                float distance = hardware_monitor.data.CarPos;

                Matrix4x4 matrix = new Matrix4x4();
                Quaternion quaternion;

                Vector3 origin = Vector3.zero;

                List<Vector3> vertices = new List<Vector3>();

                foreach (RayInfo info in rays){
                    quaternion = Quaternion.AngleAxis(info.degree, scanner_line_rotate_dir);
                    matrix.SetTRS(Vector3.zero, quaternion, new Vector3(1, 1, 1));
                    origin = matrix.MultiplyPoint(scanner_line_dir * info.distance);

                    quaternion = Quaternion.AngleAxis(pitch, pitch_axis);
                    matrix.SetTRS(Vector3.zero, quaternion, new Vector3(1, 1, 1));
                    origin = matrix.MultiplyPoint(origin);

                    quaternion = Quaternion.AngleAxis(yaw, yaw_axis);
                    matrix.SetTRS(Vector3.zero, quaternion, new Vector3(1, 1, 1));
                    origin = matrix.MultiplyPoint(origin);

                    origin += forward_dir * distance;

                    vertices.Add(origin);
                }

                grid_data_manager.UpdateGridData(vertices);
            }
        }
        catch (Exception e) {
            Debug.Log(e.Message);
        }
    }

    public void TripleTransform(List<RayInfo> rays) {
        Debug.Log(rays.Count);
    }

    public void StartDevice() {
        /*
        CoalDumpInfo info = new CoalDumpInfo();
        info.vertices = new List<Vector2>{new Vector2(0, 0), new Vector2(100, 0), new Vector2(100, 100), new Vector2(0, 100) };

        grid_data_manager.CreateCoalDump(new List<CoalDumpInfo>{info});

        wit.StartReadData(10);
        triple.Start();
        holder.StartScan(1, 359);*/


        if (scanner.IsConnected){
            //scanner.Start();
        }

        scanner.Start();
    }
    private void OnGUI(){
        if(GUI.Button(new Rect(0, 0, 100, 60), "Click")){
            this.StartDevice();
            //client.SendData(new byte[] {0x00});

            //triple.Start();
        }
    }

    public void StopDevice(){
        scanner.Stop();
    }

    private void OnDisable(){
        if (scanner != null) {
            scanner.Close();
        }
    }

    private void StatusChanged(DeviceStatus status)
    {

        Loom.QueueOnMainThread((param) =>{
            //scanner_status.Status = (short)status;
            Debug.Log(status);
            },null);
    }

    private void OnError(ExceptionHandler handler) {
        //Debug.Log(handler.GetExceptionCode());
        /*
        Loom.instance.QueueOnMainThread((param) =>{
            Debug.Log(handler.Message + "#" + handler.GetExceptionCode().ToString());
        }, null);*/
    }
}
