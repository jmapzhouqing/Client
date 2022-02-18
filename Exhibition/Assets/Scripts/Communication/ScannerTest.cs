using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System;

using Scanner.Serial;
using Scanner.Scanister;
using Scanner.Struct;
using Scanner.Holder;
using Scanner.Util;

public class ScannerTest : MonoBehaviour
{
    public string ip;

    public int port;

    private WIT wit;

    private Scanner.Scanister.Scanner scanner;

    private Holder holder;

    GridDataManager grid_data_manager;

    private HardWareDataMonitor hardware_monitor;

    public DeviceStatusControl scanner_status;

    public DeviceStatusControl wit_status;

    public DeviceStatusControl holder_status;

    RecvQueue<DegreeInfo> wit_data;
    RecvQueue<DegreeInfo> holder_data;
    RecvQueue<SectorInfo> scanner_data;

    private Task process_task;

    private CancellationTokenSource process_cancle_token_source;
    private CancellationToken process_cancle_token;

    private Vector3 wit_degree;
    private Vector3 holder_degree;

    private void Awake()
    {
        grid_data_manager = FindObjectOfType<GridDataManager>();

        hardware_monitor = FindObjectOfType<HardWareDataMonitor>();

        wit_data = new RecvQueue<DegreeInfo>();
        holder_data = new RecvQueue<DegreeInfo>();
        scanner_data = new RecvQueue<SectorInfo>();

        //process_cancle_token = 
    }
    // Start is called before the first frame update
    void Start(){
        CoalDumpInfo info = new CoalDumpInfo();
        info.vertices = new List<Vector2> { new Vector2(0, 0), new Vector2(100, 0), new Vector2(100, 100), new Vector2(0, 100) };

        grid_data_manager.CreateCoalDump(new List<CoalDumpInfo> { info });

        scanner = new Triple("Triple-IN",ip, port);
        scanner.DataDecodeComplete += TripleTransform;
        scanner.StatusChanged += ScannerStatusChanged;
        scanner.Error += ScannerOnError;
        scanner.Connect();

        wit = new WIT("COM4",0x50,9600);
        wit.DataDecodeComplete += WitDataComplete;
        wit.StatusChanged += WitStatusChanged;
        wit.Error += WitOnError;
        wit.Open();

        
        holder = new Holder("COM2", 0x01,4800);
        holder.DataDecodeComplete += HolderDataComplete;
        holder.StatusChanged += HolderStatusChanged;
        holder.Error += HolderOnError;
        holder.Open();

        /*
        scanner = new KYLE("KYLE", ip, port);

        scanner.DataDecodeComplete += TripleTransform;

        scanner.StatusChanged += StatusChanged;
        scanner.Error += OnError;
        scanner.Connect();*/
    }

    public void StartDevice()
    {
        /*
        CoalDumpInfo info = new CoalDumpInfo();
        info.vertices = new List<Vector2>{new Vector2(0, 0), new Vector2(100, 0), new Vector2(100, 100), new Vector2(0, 100) };

        grid_data_manager.CreateCoalDump(new List<CoalDumpInfo>{info});*/

        wit.StartReadData(100);
        holder.StartScan(1,359);
        scanner.Start();

        //this.StartProcess();
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 60), "Click")){
            this.StartDevice();
        }

        if (GUI.Button(new Rect(200, 0, 100, 60), "Click")){
            this.StopDevice();
        }
    }

    public void StopDevice(){
        scanner.Stop();
    }

    private void StartProcess(){
        process_task = new Task(()=>{
            while (true) {
                if (scanner_data.WaitIncomingObject(5000) && wit_data.WaitIncomingObject(5000) && holder_data.WaitIncomingObject(5000)){
                    Debug.Log("Enter");
                }else{
                    break;
                }
            }
            
        });
        process_task.Start();
    }

    private void StopProcess() {

    }

    private void OnDisable(){
        if (scanner != null){
            scanner.Close();
        }

        if (wit != null){
            wit.Close();
        }

        if(holder != null) {
            holder.Close();
        }

        if(scanner != null){
            scanner.Close();
        }
    }

    public void DataTransform(List<RayInfo> rays)
    {
        try
        {
            if (hardware_monitor.IsConnected)
            {
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

                foreach (RayInfo info in rays)
                {
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
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void TripleTransform(SectorInfo sector)
    {
        try
        {
            Vector3 scanner_line_dir = -1 * Vector3.right;
            Vector3 scanner_rotate_dir = -1 * Vector3.up;
            Vector3 scanner_line_rotate_dir = Vector3.forward;

            Matrix4x4 matrix = new Matrix4x4();
            Quaternion quaternion;

            Vector3 origin = Vector3.zero;

            List<Vector3> vertices = new List<Vector3>();

            List<RayInfo> rays = sector.rays;

            foreach (RayInfo info in rays)
            {
                quaternion = Quaternion.AngleAxis(info.degree, scanner_line_rotate_dir);
                matrix.SetTRS(Vector3.zero, quaternion, new Vector3(1, 1, 1));

                origin = matrix.MultiplyPoint(scanner_line_dir * info.distance);

                quaternion = Quaternion.Euler(wit_degree.z, -1 * holder_degree.y, wit_degree.x);
                matrix.SetTRS(new Vector3(50, 2.5f, 50), quaternion, new Vector3(1, 1, 1));

                origin = matrix.MultiplyPoint(origin);

                vertices.Add(origin);
            }

            grid_data_manager.UpdateGridData(vertices);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void WitDataComplete(DegreeInfo info){
        //wit_data.Push(info);

        this.wit_degree = info.degree;
        //Debug.Log(info.degree);
    }

    public void HolderDataComplete(DegreeInfo info){
        //holder_data.Push(info);
        this.holder_degree = info.degree;
    }

    public void ScannerDataComplete(SectorInfo info){
        scanner_data.Push(info);
    }

    private void WitStatusChanged(DeviceStatus status)
    {
        Loom.QueueOnMainThread((param) => {
            wit_status.Status = (short)status;
        }, null);
    }

    private void WitOnError(ExceptionHandler handler){
        Debug.Log(handler.Message);
    }

    private void HolderStatusChanged(DeviceStatus status){
        Loom.QueueOnMainThread((param) => {
            holder_status.Status = (short)status;
        }, null);
    }

    private void HolderOnError(ExceptionHandler handler)
    {
        Debug.Log(handler.Message);

    }

    private void ScannerStatusChanged(DeviceStatus status)
    {
        Loom.QueueOnMainThread((param) => {
            scanner_status.Status = (short)status;
        }, null);
    }

    private void ScannerOnError(ExceptionHandler handler)
    {
        Debug.Log(handler.Message);
        /*
        Loom.instance.QueueOnMainThread((param) =>{
            Debug.Log(handler.Message + "#" + handler.GetExceptionCode().ToString());
        }, null);*/
    }
}
