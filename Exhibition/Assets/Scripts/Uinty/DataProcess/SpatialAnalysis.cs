using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class SpatialAnalysis : MonoBehaviour
{

    private GridDataManager grid_data_manager;

    private List<Vector2> polygon_vertices;

    private Polygon check_forward_polygon;

    private Polygon check_polygon;

    private Polygon wheel_polygon;

    private Polygon check_left_boundary_polygon;

    private Polygon check_right_boundary_polygon;

    private Polygon check_left_polygon;

    private List<Vector3> update_vertices;

    private List<Vector3> rotation_vertices;

    private List<Vector3> exhibition_vertices;

    private bool isLeft = false;

    private bool isRight = false;

    private Vector3[,] vertices;

    private bool is_update = false;

    private float coal_height = 0;

    private float upper_level_height = 0;


    private HardWareDataMonitor hardware_monitor;

    private ProgramCommunication correspond;

    public Transform arm;
    public Transform foundation;

    private Vector3 arm_euler;
    private Vector3 foundation_euler;

    float step = -0.02f;

    public bool take_coal_status = false;
    // Start is called before the first frame update
    void Awake(){
        exhibition_vertices = new List<Vector3>();

        hardware_monitor = FindObjectOfType<HardWareDataMonitor>();

        correspond = FindObjectOfType<ProgramCommunication>();

        arm_euler = arm.localRotation.eulerAngles;
        foundation_euler = foundation.rotation.eulerAngles;

        grid_data_manager = FindObjectOfType<GridDataManager>();

        vertices = grid_data_manager.mesh_data;

        polygon_vertices = new List<Vector2>();

        wheel_polygon = this.CreateUpdateOrthogon();

        wheel_polygon.CreateInteriorPoint();

        update_vertices = this.CreateUpdateVertices();

        rotation_vertices = new List<Vector3>();


        StartCoroutine(MonitorBoundary());
    }



    private void StartName(){
        /*
        Grid grid = new Grid(0, 180, 100, 3000, 0.2f, 200);

        Polygon polygon = grid.CreatePolygon();
            
        List<Vector3> vertices = new List<Vector3>();

        foreach (Vector2 vector in polygon.polygon_vertices) {
            vertices.Add(new Vector3(vector.x, 0, vector.y));
        }*/

        //grid_data_manager.CreateCoalYard(vertices, "enter", false);

        //polygon_vertices = wheel_polygon.interior_vertices;

        //this.CaculateEntryPoint(grid,6.0f);

        //this.CaculateGridBoundary(grid,6.0f);

        //isLeft = true;
    }

    /*
    public void UpdateVertice() {
        Vector3 center = this.transform.position;

        float rotation = this.transform.rotation.eulerAngles.y - origin_angle.y;

        if (rotation > 90) {
            rotation = rotation - 180;
        }

        Polygon update_polygon = wheel_polygon.CreateRotationPolygon(new Vector2(center.x, center.z), rotation * Mathf.Deg2Rad);

        polygon_vertices = update_polygon.interior_vertices;

        List<Vector3> update_vertices = new List<Vector3>();

        foreach (Vector2 vertice in update_polygon.interior_vertices) {
            int x = Mathf.FloorToInt(vertice.x/0.2f);
            int y = Mathf.FloorToInt(vertice.y/0.2f);
            if (grid_data_manager.mesh_data[x, y].y > 4.0f) {
                update_vertices.Add(new Vector3(vertice.x, 4.0f, vertice.y));
            }
            
        }
        grid_data_manager.UpdateGridData(update_vertices);
    }*/

    public void UpdateVertice(){
        Vector3 center = this.transform.position;

        float rotation = foundation.rotation.eulerAngles.y - foundation_euler.y;

        float pitch = arm.rotation.eulerAngles.x - arm_euler.x;

        Quaternion quaternion = Quaternion.Euler(pitch,rotation,0);
        Matrix4x4 matrix = new Matrix4x4();
        matrix.SetTRS(center,quaternion,new Vector3(1,1,1));

        List<CoordinateIndex> need_update = new List<CoordinateIndex>();

        rotation_vertices = new List<Vector3>();

        foreach (Vector3 vertice in update_vertices) {
            Vector3 value = matrix.MultiplyPoint(vertice);

            rotation_vertices.Add(value);

            int x = Mathf.FloorToInt(value.x / 0.2f);
            int z = Mathf.FloorToInt(value.z / 0.2f);

            Vector3 grid_coordinate = Grandmaster.GetGridCoordinate(x,z);

            if (grid_coordinate.y > value.y) {
                Grandmaster.UpdateGridData(x, z, value);
                need_update.Add(new CoordinateIndex(x,z));
            }
        }

        grid_data_manager.UpdateCoalYard(need_update);
    }

    private bool arrive_left = false;
    private bool arrive_right = false;

    // Update is called once per frame
    void Update(){
        /*
        for (int i = 0, len = exhibition_vertices.Count; i < len; i++) {
            Vector3 _start = exhibition_vertices[i % len];
            Vector3 _end = exhibition_vertices[(i+1) % len];

            Vector3 start = new Vector3(_start.x, 0, _start.z);
            Vector3 end = new Vector3(_end.x, 0, _end.z);
            Debug.DrawLine(_start, _end, Color.red);
        }*/

        /*
        for (int i = 0, len = update_vertices.Count; i < len; i++)
        {
            Vector3 _start = update_vertices[i % len];
            Vector3 _end = update_vertices[(i + 1) % len];

            Vector3 start = new Vector3(_start.x, 0, _start.z);
            Vector3 end = new Vector3(_end.x, 0, _end.z);
            Debug.DrawLine(_start, _end, Color.red);

        }*/

        if (hardware_monitor.IsConnected && hardware_monitor.data!=null){
            float yaw = hardware_monitor.data.SlewAngle;
            float pitch = hardware_monitor.data.LuffAngle;
            foundation.eulerAngles = new Vector3(foundation_euler.x, foundation_euler.y + yaw, foundation_euler.z);
            arm.localEulerAngles = new Vector3(arm_euler.x + pitch, arm_euler.y, arm_euler.z);

            this.transform.position = new Vector3(this.transform.position.x,this.transform.position.y,hardware_monitor.data.CarPos);
        }

        if (hardware_monitor.IsConnected && hardware_monitor.data != null && take_coal_status) {
            DeviceData data = hardware_monitor.data;
            if (data.SlewStatus == 1)
            {
                this.arrive_right = false;
                if (CheckLeftBoundary()){
                    this.arrive_left = true;
                   
                }
            }
            else if (data.SlewStatus == 2)
            {
                this.arrive_left = false;
                if (CheckRightBoundary())
                {
                    this.arrive_right = true;
                }
            }
            else {
                this.arrive_left = false;
                this.arrive_right = false;
            }

            CheckFowardBoundary();

            UpdateVertice();
        }

        

        if (is_update) {
            UpdateVertice();
            if(isLeft || isRight){
                if (isLeft){
                    if (CheckLeftBoundary())
                    {
                        isLeft = false;
                        isRight = true;
                        step = 0.02f;
                        this.transform.position += new Vector3(0, 0, 1.0f);
                    }
                }
                else if (isRight)
                {
                    if (CheckRightBoundary())
                    {
                        isLeft = true;
                        isRight = false;
                        step = -0.02f;
                        this.transform.position += new Vector3(0, 0, 1.0f);
                    }
                }

                Vector3 angle = this.transform.rotation.eulerAngles;

                angle += new Vector3(0, step, 0);
                this.transform.rotation = Quaternion.Euler(angle);
            }
        }
    }

    private void FixedUpdate(){
        //UpdateVertice();
    }

    private IEnumerator MonitorBoundary() {
        while (true)
        {
            if (this.arrive_left) {
                correspond.SendData("Boundardy Left Arrive");
                Debug.Log("SendLeftBoundary");
            }

            if (this.arrive_right)
            {
                correspond.SendData("Boundardy Right Arrive");
                Debug.Log("SendRightBoundary");
            }

            if (this.arrive_left || this.arrive_right) {
                if (this.upper_level_total_number < 200)
                {
                    if (this.total_number < 100)
                    {
                        correspond.SendData("Boundardy Foward Arrive");
                    }
                }
                else {
                    correspond.SendData("Boundardy Foward Upper");
                }

                this.total_number = 0;
                this.upper_level_total_number = 0;
            }

            yield return new WaitForSeconds(1.0f);
        }
    }

    private Vector2 Vector2Rotate(Vector2 vertice,float rotation){
        return new Vector2(vertice.x * Mathf.Cos(rotation)+vertice.y * Mathf.Sin(rotation), vertice.y * Mathf.Cos(rotation) - vertice.x * Mathf.Sin(rotation));
    }

    private Polygon CreateCheckLeftBoundaryPolygon() {
        List<Vector2> vertices = new List<Vector2>();

        Polygon polygon = new Polygon(vertices);

        return polygon;
    }

    public bool CheckLeftBoundardyPolygon() {
        return true;
    }

    private void CaculateGridBoundary(Grid grid, int level){
        float height = (level - 1) * ConfigurationParameter.level_height;
    }

    public Dictionary<string, Vector3> CaculateGridBoundary(Grid grid, float height,float upper_level_height)
    {

        Dictionary<string, Vector3> param = null;

        coal_height = height;

        this.upper_level_height = upper_level_height;

        Vector2 entry_point = Vector2.zero;

        Vector3 rotation_center = ConfigurationParameter.rotation_center;

        BoundaryCoordinate<int> index_boundary = grid.index_boundary;

        int start_x = index_boundary.min_x;
        int end_x = index_boundary.max_x;
        int start_z = index_boundary.min_z;
        int end_z = index_boundary.max_z;

        if (end_x * grid.precision < rotation_center.x){
            isLeft = true;
        }else if (start_x * grid.precision > rotation_center.x){
            isLeft = false;
        }else {
            return null;
        }

        List<Vector3> boundary_vertices = new List<Vector3>();

        for (int i = start_x; i <= end_x; i++){
            for(int j = start_z; j <= end_z; j++){
                try
                {
                    if(vertices[i, j].y > height && vertices[i, j].y <= (height + ConfigurationParameter.level_height))
                    {
                        boundary_vertices.Add(vertices[i, j]);
                        break;
                    }
                }
                catch (Exception e) {
                    Debug.Log(i+"#"+j);
                }
            }
        }

        for (int i = start_x; i <= end_x; i++){
            for (int j = end_z; j >= start_z; j--){
                try
                {
                    if (vertices[i, j].y > height && vertices[i, j].y <= (height + ConfigurationParameter.level_height)) {
                        boundary_vertices.Add(vertices[i, j]);
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(i + "#" + j);
                }
            }
        }

        exhibition_vertices = boundary_vertices;

        if (boundary_vertices.Count == 0) {
            return null;
        }

        float radius = ConfigurationParameter.arm_length;

        float pitch = Mathf.Asin((height - ConfigurationParameter.bucket_wheel_bottom_coordinate.y) / radius);

        radius = Mathf.Cos(pitch) * radius;

        List<Vector2> validate_vertices = boundary_vertices.Select(vertice => new Vector2(vertice.x, vertice.z)).ToList();


        //exhibition_vertices = validate_vertices;

        float min_z = float.MaxValue;

        float max_z = float.MinValue;

        float entry_rotation = 0;
        Vector2 end_point = Vector2.zero;

        float max_rotation = float.MinValue;
        float min_rotation = float.MaxValue;

        float rotation_offset = Mathf.Atan(ConfigurationParameter.wheel_offset_center / ConfigurationParameter.arm_length);

        foreach(Vector2 vertice in validate_vertices){
            float rotation = Mathf.Asin((vertice.x - ConfigurationParameter.rotation_center.x)/ radius);

            float z = vertice.y - Mathf.Cos(rotation) * radius;

            if(rotation > max_rotation){
                max_rotation = rotation;
            }

            if (rotation < min_rotation) {
                min_rotation = rotation;
            }

            if (z < min_z) {
                min_z = z;
                entry_point = vertice;

                entry_rotation = rotation;
            }

            if (z > max_z) {
                max_z = z;
                end_point = vertice;
            }
        }

        Vector3 center = ConfigurationParameter.rotation_center + new Vector3(0, 0, min_z);
        entry_rotation -= rotation_offset;
       
        List<Vector2> arc_vertices = this.CreateArcLine(new Vector2(center.x, center.z), min_rotation * Mathf.Rad2Deg, 0, new Vector2(0, pitch));

        polygon_vertices = arc_vertices;

        Vector2 may_point = GetLeftRightVertice(arc_vertices,grid,height,3);

        Debug.Log(may_point);

        if (entry_point.x < may_point.x) {
            entry_point = may_point;
        }

        param  = this.CaculatePosture(entry_point,max_z,ConfigurationParameter.bucket_wheel_bottom_coordinate,pitch);

        check_left_boundary_polygon = this.CreateLeftBoundaryOrthogon(pitch);
        check_left_boundary_polygon.CreateInteriorPoint();

        check_right_boundary_polygon = this.CreateRightBoundaryOrthogon(pitch);
        check_right_boundary_polygon.CreateInteriorPoint();

        check_forward_polygon = this.CreateForwardBoundaryOrthogon(pitch);
        check_forward_polygon.CreateInteriorPoint();


        //this.SetTransform(param["rotation"],param["center"]);

        return param;
    }

    private Dictionary<string,Vector3> CaculatePosture(Vector2 target,float max_z,Vector3 origin,float pitch) {

        Dictionary<string, Vector3> dic = new Dictionary<string, Vector3>();

        Vector3 rotation_center = ConfigurationParameter.rotation_center;

        float wheel_rotation = Mathf.Atan((origin.x - rotation_center.x) / origin.z);

        //float pitch = Mathf.Asin((target.y - origin.y) / ConfigurationParameter.arm_length);

        float radius = Mathf.Cos(pitch) * ConfigurationParameter.arm_length;

        float rotation = Mathf.Asin((target.x - rotation_center.x) / radius);

        Vector3 center_coordinate = rotation_center + new Vector3(0, 0, target.y - radius * Mathf.Cos(rotation));

        Vector3 stop_coordinate = rotation_center + new Vector3(0, 0, max_z - radius * Mathf.Cos(rotation));

        rotation = rotation - wheel_rotation;

        dic.Add("rotation", new Vector3(-1 * pitch * Mathf.Rad2Deg, rotation * Mathf.Rad2Deg, 0));

        dic.Add("center", center_coordinate);

        dic.Add("stop", stop_coordinate);

        //Debug.Log(new Vector3(-1 * pitch * Mathf.Rad2Deg, rotation * Mathf.Rad2Deg, 0));

        return dic;
    }

    private void SetTransform(Vector3 rotation,Vector3 center) {

        Debug.Log(rotation);

        arm.localRotation = Quaternion.Euler(rotation.x, 0, 0);
        foundation.rotation = Quaternion.Euler(0, rotation.y, 0);

        this.transform.position = center;
    }

    public void SetUpdate(bool station) {
        this.is_update = station;
    }

    private List<Vector2> CreateArcLine(Vector2 center,float start, float end,Vector2 rotation){
        List<Vector2> vertices = new List<Vector2>();

        float r = ConfigurationParameter.arm_length * Mathf.Cos(rotation.y);

        Vector2 origin = new Vector2(0.0f, r);

        for (float i = start; i <= end; i += 0.1f){
            vertices.Add(Vector2Rotate(origin, i * Mathf.Deg2Rad));
        }

        vertices = vertices.Select(vertice => Vector2Rotate(vertice, rotation.x) + center).ToList();

        return vertices;
    }

    private Polygon CreateUpdateOrthogon(){
        Polygon polygon = null;
        List<Vector2> vertices = new List<Vector2>();

        Vector2 origin = new Vector2(ConfigurationParameter.wheel_offset_center,ConfigurationParameter.arm_length);

        float width = ConfigurationParameter.wheel_width/2.0f;
        float height = ConfigurationParameter.wheel_radius;

        Vector2 fir = origin + new Vector2(-width,-height);
        Vector2 sec = origin + new Vector2(width,-height);
        Vector2 third = origin + new Vector2(width,height);
        Vector2 four = origin + new Vector2(-width, height);

        vertices.Add(fir);
        vertices.Add(sec);
        vertices.Add(third);
        vertices.Add(four);

        polygon = new Polygon(vertices);
        return polygon;
    }

    private Polygon CreateOrthogon(Vector2 center,float width,float height)
    {
        Polygon polygon = null;
        List<Vector2> vertices = new List<Vector2>();

        Vector2 fir = center + new Vector2(-width, -height);
        Vector2 sec = center + new Vector2(width, -height);
        Vector2 third = center + new Vector2(width, height);
        Vector2 four = center + new Vector2(-width, height);

        vertices.Add(fir);
        vertices.Add(sec);
        vertices.Add(third);
        vertices.Add(four);

        polygon = new Polygon(vertices);
        return polygon;
    }

    private Polygon CreateLeftBoundaryOrthogon(float pitch) {

        float radius = Mathf.Sqrt(Mathf.Pow(ConfigurationParameter.arm_length, 2) + Mathf.Pow(ConfigurationParameter.wheel_center_offset_height, 2));

        float offset_pitch = Mathf.Asin(ConfigurationParameter.wheel_center_offset_height / radius);

        radius = Mathf.Cos(pitch + offset_pitch) * radius;

        Vector2 center = new Vector2(0, radius);

        float width = 1.0f;

        float height = ConfigurationParameter.wheel_radius;

        Polygon polygon = this.CreateOrthogon(center, width, height);

        return polygon;
    }

    private Polygon CreateRightBoundaryOrthogon(float pitch){
        float radius = Mathf.Sqrt(Mathf.Pow(ConfigurationParameter.arm_length, 2) + Mathf.Pow(ConfigurationParameter.wheel_center_offset_height, 2));

        float offset_pitch = Mathf.Asin(ConfigurationParameter.wheel_center_offset_height / radius);

        radius = Mathf.Cos(pitch + offset_pitch) * radius;

        Vector2 center = new Vector2(3.0f, radius);

        float width = 1.0f;

        float height = ConfigurationParameter.wheel_radius;

        Polygon polygon = this.CreateOrthogon(center, width, height);

        return polygon;
    }

    private Polygon CreateForwardBoundaryOrthogon(float pitch) {
        float radius = Mathf.Sqrt(Mathf.Pow(ConfigurationParameter.arm_length + ConfigurationParameter.wheel_radius, 2) + Mathf.Pow(ConfigurationParameter.wheel_center_offset_height, 2));

        float offset_pitch = Mathf.Asin(ConfigurationParameter.wheel_center_offset_height / radius);

        radius = Mathf.Cos(pitch + offset_pitch) * radius;

        Vector2 center = new Vector2(0.0f, radius + ConfigurationParameter.wheel_radius);

        float width = 1.0f;

        float height = ConfigurationParameter.wheel_radius;

        Polygon polygon = this.CreateOrthogon(center, width, height);

        return polygon;
    }

    private float ClampRotation(float value) {
        float param = -value / Mathf.Abs(value);
        if ((Mathf.Abs(value) - 180) > Mathf.Pow(10,-2)) {
            value = param * 360 + value;
        } else if ((Mathf.Abs(value) - 180) < Mathf.Pow(10,-2)) {
            value = param * 180 + value;
        }
        return value;
    }

    public bool CheckLeftBoundary(){

        Vector3 center = this.transform.position;

        float rotation = foundation.eulerAngles.y - foundation_euler.y;

        Polygon update_polygon = check_left_boundary_polygon.CreateRotationPolygon(new Vector2(center.x, center.z), rotation * Mathf.Deg2Rad);

        polygon_vertices = update_polygon.interior_vertices;

        int count = 0;

        foreach (Vector2 vertice in polygon_vertices)
        {
            int x = Mathf.FloorToInt(vertice.x / 0.2f);
            int y = Mathf.FloorToInt(vertice.y / 0.2f);
            try
            {
                if(grid_data_manager.mesh_data[x, y].y > coal_height){
                    count++;
                }
            }
            catch (Exception e) {
                //Debug.Log(x+"#"+y);
            }
            
        }

        if (count < 30){
            return true;
        }
        else {
            return false;
        }
    }

    public bool CheckRightBoundary(){
        Vector3 center = this.transform.position;

        float rotation = foundation.eulerAngles.y - foundation_euler.y;

        Polygon update_polygon = check_right_boundary_polygon.CreateRotationPolygon(new Vector2(center.x, center.z), rotation * Mathf.Deg2Rad);

        polygon_vertices = update_polygon.interior_vertices;

        int count = 0;

        foreach (Vector2 vertice in polygon_vertices)
        {
            int x = Mathf.FloorToInt(vertice.x / 0.2f);
            int y = Mathf.FloorToInt(vertice.y / 0.2f);

            try
            {
                if (grid_data_manager.mesh_data[x, y].y > coal_height)
                {
                    count++;
                }
            }
            catch (Exception e) {
                //Debug.Log(x+"#"+y);
            }
            
        }
        if (count < 30){
            return true;
        }else{
            return false;
        }
    }

    private int total_number = 0;

    private int upper_level_total_number = 0;

    public void CheckFowardBoundary()
    {
        Vector3 center = this.transform.position;

        float rotation = foundation.eulerAngles.y - foundation_euler.y;

        Polygon update_polygon = check_forward_polygon.CreateRotationPolygon(new Vector2(center.x, center.z), rotation * Mathf.Deg2Rad);

        polygon_vertices = update_polygon.interior_vertices;

        foreach (Vector2 vertice in polygon_vertices)
        {
            int x = Mathf.FloorToInt(vertice.x / 0.2f);
            int y = Mathf.FloorToInt(vertice.y / 0.2f);

            float height = grid_data_manager.mesh_data[x, y].y;

            try
            {
                if (height > coal_height && height < this.upper_level_height)
                {
                    total_number++;
                }

                if (height > this.upper_level_height){
                    upper_level_total_number++;
                }
            }
            catch (Exception e)
            {
                //Debug.Log(x+"#"+y);
            }

        }
    }


    public Vector2 GetLeftRightVertice(List<Vector2> arc_vertices,Grid grid,float height,int threshold){
        bool[] bool_value = new bool[arc_vertices.Count];
        int bool_index = 0;
        foreach (Vector2 vertice in arc_vertices){
            int x = Mathf.FloorToInt(vertice.x / grid.precision);
            int z = Mathf.FloorToInt(vertice.y / grid.precision);

            bool value = false;

            if (grid.CheckVerticeInGrid(x, z)){
                if (vertices[x, z].y > height && vertices[x, z].y <= (height + ConfigurationParameter.level_height)){
                    value = true;
                }
            }

            bool_value[bool_index++] = value;
        }

        List<Vector2> validate_vertices = new List<Vector2>();

        for(int i = 0; i < bool_value.Length; i++){
            if (bool_value[i]){
                int number = 0;
                int pre = (i - threshold) < 0 ? 0 : i - threshold;
                for (int j = i; j > pre; j--){
                    if(!bool_value[j]){
                        break;
                    }else{
                        number++;
                    }
                }

                int next = i + threshold > bool_value.Length ? bool_value.Length : i + threshold;
                for(int j = i + 1; j < next; j++){
                    if (!bool_value[j]){
                        break;
                    }else{
                        number++;
                    }
                }

                if (number >= threshold){
                    validate_vertices.Add(arc_vertices[i]);
                }
            }
        }

        Vector2 point = validate_vertices.OrderByDescending(vertice => vertice.x).First();

        return point;
    }

    public List<Vector3> CreateUpdateVertices() {
        List<Vector3> vertices = new List<Vector3>();

        Vector3 center = ConfigurationParameter.bucket_wheel_center_coordinate - ConfigurationParameter.rotation_center;

        float width = ConfigurationParameter.wheel_width/2.0f;

        float radius = ConfigurationParameter.wheel_radius;

        float interval = 0.5f * Mathf.Deg2Rad;

        for (float degree = -Mathf.PI; degree <= 0.0f; degree += interval) {
            Vector3 vertice = new Vector3(0,Mathf.Sin(degree), Mathf.Cos(degree)) * radius;

            Vector3 present = center + vertice;

            for (float dis = -width; dis <= width; dis += 0.1f) {
                Vector3 value = present + new Vector3(dis,0,0);
                vertices.Add(value);
            }
        }

        return vertices;
    }

    public void UpdateVerticesNoScanner() {

    }
}
