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

    //扫描仪相对于斗轮机中心坐标
    public Vector3 scanner_coordinate = Vector3.zero;

    //扫描线初始方向
    public Vector3 scanner_line_dir = -1 * Vector3.right;

    //扫描线初始角度
    public float scanner_line_start_angle = 0;

    //扫描线旋转轴
    public Vector3 scanner_line_rotate_dir = Vector3.forward;

    //斗轮机大臂旋转轴
    public Vector3 pitch_axis = -1 * Vector3.right;

    //斗轮机初始俯仰角度
    public float start_pitch_angle = 0;

    //斗轮机水平旋转轴
    public Vector3 yaw_axis = Vector3.up;

    //斗轮机初始旋转角度
    public float start_yaw_angle = 0;

    //斗轮机行走方向
    public Vector3 forward_dir = Vector3.forward;


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
        //LoggerInfo.Log(typeof(ScannerPoint), "Error", this.transform);

        /*
        client = new Client();

        IPEndPoint server_address = new IPEndPoint(IPAddress.Parse("192.168.90.247"), 1024);
        IPEndPoint client_address = new IPEndPoint(IPAddress.Any,0);

        client.StatusChanged += StatusChanged;
        client.Error += OnError;

        client.Connect(server_address, client_address, ProtocolType.Udp);*/

        //盘煤仪 sick
        
        scanner = new LMS511("Sick", ip, port);
        scanner.DataDecodeComplete += SickTransform;
        scanner.StatusChanged += StatusChanged;
        scanner.Error += OnError;
        scanner.Connect();

        /*
        scanner = new Triple("Triple",ip,port);
        scanner.DataDecodeComplete += TripleTransform;
        
        scanner.StatusChanged += StatusChanged;
        scanner.Error += OnError;
        scanner.Connect();*/

        /*
        wit = new WIT("COM4",0x50, 9600);
        wit.Open();*/

        /*
        holder = new Holder("COM2", 4800);
        holder.Open();*/

        /*
        scanner = new KYLE("KYLE", ip, port);

        scanner.DataDecodeComplete += TripleTransform;

        scanner.StatusChanged += StatusChanged;
        scanner.Error += OnError;
        scanner.Connect();*/

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
                    matrix.SetTRS(scanner_coordinate, quaternion, new Vector3(1, 1, 1));
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

    public void SickTransform(SectorInfo sector_info) {
        try
        {
            if (hardware_monitor.IsConnected)
            {
                /*
                scanner_line_dir = -1 * Vector3.right;
                scanner_line_rotate_dir = Vector3.forward;

                pitch_axis = -1 * Vector3.right;
                yaw_axis = Vector3.up;
                forward_dir = Vector3.forward;*/

                float pitch = hardware_monitor.data.LuffAngle - start_pitch_angle;

                float yaw = hardware_monitor.data.SlewAngle - start_yaw_angle;
                float distance = hardware_monitor.data.CarPos;

                Matrix4x4 matrix = new Matrix4x4();
                Quaternion quaternion;

                Vector3 origin = Vector3.zero;

                List<Vector3> vertices = new List<Vector3>();

                foreach (RayInfo info in sector_info.rays){
                    quaternion = Quaternion.AngleAxis(info.degree - scanner_line_start_angle, scanner_line_rotate_dir);
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
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void TripleTransform(List<RayInfo> rays) {
        try{
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

                Vector3 rotation = Vector3.zero;

                quaternion = Quaternion.Euler(rotation.x, -1 * holder.horizontal, rotation.y);
                matrix.SetTRS(new Vector3(50, 2.5f, 50), quaternion, new Vector3(1, 1, 1));

                origin = matrix.MultiplyPoint(origin);

                vertices.Add(origin);
            }

            //grid_data_manager.UpdateGridData(vertices);
        }catch (Exception e){
            Debug.Log(e.Message);
        }
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
            scanner.Start();
        }
    }
    
    /*
    private void OnGUI(){
        if(GUI.Button(new Rect(0, 0, 100, 60), "Click")){
            this.StartDevice();
            //client.SendData(new byte[] {0x00});

            //triple.Start();
        }

        if (GUI.Button(new Rect(200, 0, 100, 60), "Click"))
        {
            this.StopDevice();
            //client.SendData(new byte[] {0x00});

            //triple.Start();
        }
    }*/

    public void StopDevice(){
        scanner.Stop();
    }

    private void OnDisable(){
        if (scanner != null) {
            scanner.Close();
        }
    }

    private void StatusChanged(DeviceStatus status){
        Loom.QueueOnMainThread((param) =>{
            scanner_status.Status = (short)status;
            },null);
    }

    private void OnError(ExceptionHandler handler) {
        Debug.Log(handler.Message);
        /*
        Loom.instance.QueueOnMainThread((param) =>{
            Debug.Log(handler.Message + "#" + handler.GetExceptionCode().ToString());
        }, null);*/
    }
}
