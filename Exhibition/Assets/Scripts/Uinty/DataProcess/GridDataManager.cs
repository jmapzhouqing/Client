using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading;

using UnityEngine;
using Vectrosity;



public class GridDataManager : MonoBehaviour{

    public Transform coaldump_container;

    public Transform coal_container;

    public Transform area_container;

    public float precision = 0.2f;

    public float width = 81;
    public float height = 650;

    private int width_segment_number;
    private int height_segment_number;
    private int mesh_segment_number = 200;

    public Vector3[,] mesh_data;

    private List<CoordinateIndex> update_vertices;

    private Dictionary<string, CoalDumpManager> coaldumps;

    private CoalDumpManager other;

    private List<Vector3> data_list;

    private Material material;

    void Awake(){

        data_list = new List<Vector3>();

        update_vertices = new List<CoordinateIndex>();

        coaldumps = new Dictionary<string, CoalDumpManager>();

        this.initialize();
        
        GridDataPersistence.ReadData(@"D:\CoalYard\data.txt",mesh_data);
    }

    /*
    private void Start(){
        //StartCoroutine(UpdateCoordinate());
    }*/

    public void initialize(){
        width_segment_number = Mathf.FloorToInt(width / precision);
        height_segment_number = Mathf.FloorToInt(height / precision);
        mesh_segment_number = 200;

        mesh_data = new Vector3[width_segment_number + 1, height_segment_number + 1];

        for (int i = 0; i <= width_segment_number; i++){
            for (int j = 0; j <= height_segment_number; j++){
                //mesh_data[i, j] =  new Vector3(i*precision,Mathf.Sin(i * Mathf.PI / width_segment_number) * 10,j*precision);
                mesh_data[i, j] = new Vector3(i * precision, 0, j * precision);
            }
        }
    }

    private void FixedUpdate(){
        List<CoordinateIndex> vertices = null;
        try{
            Monitor.Enter(update_vertices);
            vertices = new List<CoordinateIndex>(update_vertices);
            update_vertices.Clear();
            Monitor.Pulse(update_vertices);

            foreach (CoordinateIndex index in vertices)
            {
                UpdateCoalYard(index.x,index.z);
            }
            //this.UpdateCoalYard(vertices);
        }
        finally
        {
            Monitor.Exit(update_vertices);
        }
    }

    /*
    IEnumerator UpdateCoordinate() {
        while (true) {
            for (int i = 0; i < 1000; i++) {
                for (int j = 0; j < 300; j++) {
                    float x = precision * i;
                    float z = precision * j;
                    Vector3 value = new Vector3(x, 50, z);
                    data_list.Add(value);
                }
            }
            yield return new WaitForSeconds(1.0f);
            this.UpdateCoalYard(new List<Vector3>(data_list));

            data_list.Clear();
        }
    }*/

    public void CreateCoalDump(List<CoalDumpInfo> data) {
        this.ClearCoalDump();
        foreach (CoalDumpInfo info in data) {
            this.CreateCoalDump(info);
        }
    }

    public void CreateWholeCoalYard() {
        this.ClearCoalDump();
        int min_x = 0;
        int max_x = width_segment_number;
        int min_z = 0;
        int max_z = height_segment_number;

        int index_width = max_x - min_x;
        int index_height = max_z - min_z;

        int width_dir_number = index_width / mesh_segment_number + (index_width % mesh_segment_number == 0 ? 0 : 1);
        int height_dir_number = index_height / mesh_segment_number + (index_height % mesh_segment_number == 0 ? 0 : 1);

        Material material = this.CreateMaterial(ConfigurationParameter.level_number,ConfigurationParameter.level_height);

        GameObject coal_dump = new GameObject();
        coal_dump.transform.SetParent(coal_container);

        for (int i = 0; i < width_dir_number; i++)
        {
            for (int j = 0; j < height_dir_number; j++)
            {
                int start_x = min_x + i * mesh_segment_number;
                int end_x = (min_x + (i + 1) * mesh_segment_number) > max_x ? max_x : (min_x + (i + 1) * mesh_segment_number);

                int start_z = min_z + j * mesh_segment_number;
                int end_z = (min_z + (j + 1) * mesh_segment_number) > max_z ? max_z : (min_z + (j + 1) * mesh_segment_number);

                CreateMesh(start_x, end_x, start_z, end_z, coal_dump.transform, material);
            }
        }
    }

    private CoalDumpManager CreateCoalDump(CoalDumpInfo info){
        Grid grid = new Grid(info.vertices, ConfigurationParameter.precision, ConfigurationParameter.mesh_segment_number);

        BoundaryCoordinate<int> index_boundary = grid.index_boundary;

        int min_x = index_boundary.min_x;
        int max_x = index_boundary.max_x;
        int min_z = index_boundary.min_z;
        int max_z = index_boundary.max_z;

        if (string.IsNullOrEmpty(info.uuid)){
            info.uuid = Guid.NewGuid().ToString("N");
        }

        int total_level = CaculateCoalDumpLevelInfo(index_boundary);
        info.level = total_level;

        int index_width = max_x - min_x;
        int index_height = max_z - min_z;

        int width_dir_number = index_width / mesh_segment_number + (index_width % mesh_segment_number == 0 ? 0 : 1);
        int height_dir_number = index_height / mesh_segment_number + (index_height % mesh_segment_number == 0 ? 0 : 1);

        GameObject coal_dump = new GameObject();
        coal_dump.name = info.uuid;
        coal_dump.transform.SetParent(coaldump_container);

        Material material = this.CreateMaterial(ConfigurationParameter.level_number,ConfigurationParameter.level_height);

        CoalDumpManager coalDumpManager = coal_dump.AddComponent<CoalDumpManager>();
        coalDumpManager.info = info;
        coalDumpManager.grid = grid;
        coalDumpManager.material = material;

        coaldumps.Add(info.uuid, coalDumpManager);

        for (int i = 0; i < width_dir_number; i++){
            for (int j = 0; j < height_dir_number; j++){
                int start_x = min_x + i * mesh_segment_number;
                int end_x = (min_x + (i + 1) * mesh_segment_number) > max_x ? max_x : (min_x + (i + 1) * mesh_segment_number);

                int start_z = min_z + j * mesh_segment_number;
                int end_z = (min_z + (j + 1) * mesh_segment_number) > max_z ? max_z : (min_z + (j + 1) * mesh_segment_number);

                CreateMesh(start_x, end_x, start_z, end_z, i + "_" + j, coalDumpManager, material);
            }
        }

        return coalDumpManager;
    }

    private Transform CreateMesh(int start_x, int end_x, int start_z, int end_z,Transform parent,Material material) {
        Vector3[] vertices = new Vector3[(end_x - start_x + 1) * (end_z - start_z + 1)];
        int[] triangles = new int[(end_x - start_x) * (end_z - start_z) * 2 * 3];

        int index = 0;
        int triangle_index = 0;

        int row = end_x - start_x + 1;
        int col = end_z - start_z + 1;

        for (int m = 0; m < row; m++)
        {
            for (int n = 0; n < col; n++)
            {
                int value = index++;
                try
                {
                    vertices[value] = mesh_data[m + start_x, n + start_z];
                }
                catch (Exception e)
                {
                    //Debug.Log(start_z + "#" + (m + start_x) + "#" + (n + start_z));
                }

                if ((m < (row - 1)) && (n < (col - 1)))
                {
                    triangles[triangle_index++] = m * col + n;
                    triangles[triangle_index++] = (m + 1) * col + n + 1;
                    triangles[triangle_index++] = (m + 1) * col + n;

                    triangles[triangle_index++] = m * col + n;
                    triangles[triangle_index++] = m * col + n + 1;
                    triangles[triangle_index++] = (m + 1) * col + n + 1;
                }
            }
        }

        GameObject child = new GameObject();
        child.layer = LayerMask.NameToLayer("CoalDump");
        child.transform.SetParent(parent);

        MeshFilter meshfilter = child.AddComponent<MeshFilter>();
        MeshRenderer render = child.AddComponent<MeshRenderer>();

        render.material = material;

        Mesh mesh = meshfilter.mesh;
        mesh.MarkDynamic();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        child.transform.position = Vector3.zero;

        return child.transform;
    }
    private Transform CreateMesh(int start_x, int end_x, int start_z, int end_z,string name, CoalDumpManager coalDumpManager,Material material){
        Vector3[] vertices = new Vector3[(end_x - start_x + 1) * (end_z - start_z + 1)];
        int[] triangles = new int[(end_x - start_x) * (end_z - start_z) * 2 * 3];

        int index = 0;
        int triangle_index = 0;

        int row = end_x - start_x + 1;
        int col = end_z - start_z + 1;

        for (int m = 0; m < row; m++)
        {
            for (int n = 0; n < col; n++)
            {
                int value = index++;
                try{
                    vertices[value] = mesh_data[m + start_x, n + start_z];
                }catch (Exception e){
                    //Debug.Log(start_z + "#" + (m + start_x) + "#" + (n + start_z));
                }

                if ((m < (row - 1)) && (n < (col - 1)))
                {
                    triangles[triangle_index++] = m * col + n;
                    triangles[triangle_index++] = (m + 1) * col + n + 1;
                    triangles[triangle_index++] = (m + 1) * col + n;

                    triangles[triangle_index++] = m * col + n;
                    triangles[triangle_index++] = m * col + n + 1;
                    triangles[triangle_index++] = (m + 1) * col + n + 1;
                }
            }
        }

        GameObject child = new GameObject();
        child.layer = LayerMask.NameToLayer("CoalDump");
        child.transform.SetParent(coalDumpManager.transform);
        child.name = name;

        MeshFilter meshfilter = child.AddComponent<MeshFilter>();
        MeshRenderer render = child.AddComponent<MeshRenderer>();

        render.material = material; 

        Mesh mesh = meshfilter.mesh;
        mesh.MarkDynamic();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        child.transform.position = Vector3.zero;

        BlockManager blockManager = child.AddComponent<BlockManager>();
        blockManager.grid = new Grid(start_x, end_x, start_z, end_z,precision,mesh_segment_number);
        blockManager.polygon = blockManager.grid.CreatePolygon();

        coalDumpManager.AddChild(name, blockManager);

        return child.transform;
    }

    private int CaculateCoalDumpLevelInfo(BoundaryCoordinate<int> index_boundary,int threshold = 200) {
        int min_x = index_boundary.min_x;
        int max_x = index_boundary.max_x;
        int min_z = index_boundary.min_z;
        int max_z = index_boundary.max_z;

        float level_height = ConfigurationParameter.level_height;
        int level_number = ConfigurationParameter.level_number;

        Dictionary<int, int> level_info = new Dictionary<int, int>();

        for (int i = 0; i < level_number; i++){
            level_info.Add(i, 0);
        }

        for (int i = min_x; i <= max_x; i++){
            for (int j = min_z; j <= max_z; j++){
                Vector3 vertice = mesh_data[i, j];
                if (vertice.y < Mathf.Pow(10, -2)){
                    continue;
                }

                int level = Mathf.FloorToInt(vertice.y / level_height);
                level = level < 0 ? 0 : level;
                level = level > (level_number - 1) ? level_number - 1 : level;
                level_info[level]++;
            }
        }

        List<int> level_list = level_info.Where(item => item.Value > threshold).OrderByDescending(item => item.Key).Select(item=>item.Key).ToList();

        if (level_list.Count != 0){
            return level_list[0] + 1;
        }else {
            return 0;
        }
    }

    public void UpdateGridData(int x,int z,Vector3 vertice){
        mesh_data[x, z] = vertice;
    }

    public void UpdateGridData(List<Vector3> vertices) {
        List<CoordinateIndex> need_update = new List<CoordinateIndex>();
        foreach (Vector3 vertice in vertices) {
            int x = Mathf.FloorToInt(vertice.x / precision);
            int z = Mathf.FloorToInt(vertice.z / precision);

            mesh_data[x, z] = vertice;

            need_update.Add(new CoordinateIndex(x,z));
        }

        this.UpdateCoalYard(need_update);
    }

    public void UpdateCoalYard(List<CoordinateIndex> data){
        try{
            Monitor.Enter(update_vertices);
            update_vertices.AddRange(data);
        }finally{
            Monitor.Exit(update_vertices);
        }
    }

    private void UpdateCoalYard(int x, int z) {
        this.CheckVertices(x, z);
    }

    /*
    private void UpdateCoalYard(List<Vector3> vertices){
        int x, z;
        try{
            foreach (Vector3 vertice in vertices){
                x = Mathf.RoundToInt(vertice.x / precision);
                z = Mathf.RoundToInt(vertice.z / precision);
                if (mesh_data[x, z].y > vertice.y) {
                    
                    mesh_data[x, z] = vertice;
                }
                

                //this.CheckVertices(x,z,vertice);
            }
        }catch (Exception e) {
            Debug.Log(e.Message);
        }
        

        Thread thread = new Thread(delegate(){
            CheckVerticesInBlocks(vertices);
        });
        thread.Start();
        //vertices.Clear();
    }*/

    private string CheckVertices(int x,int z) {
        string key = "";
        foreach (KeyValuePair<string, CoalDumpManager> item in coaldumps)
        {
            if (item.Value.CheckVerticeInGrid(x,z)) {
                item.Value.UpdateCoalYard(x,z);
                break;
            }
        }
        return key;
    }

    private void CheckVerticesInBlocks(List<Vector3> vertices) {
        List<Vector2> convex_hull = ConvexHull.convexHull(vertices);
        Polygon polygon = new Polygon(convex_hull);

        foreach (KeyValuePair<string, CoalDumpManager> coalyard_item in coaldumps){
            foreach (KeyValuePair<string, BlockManager> block_item in coalyard_item.Value.blockManagers) {
                if (polygon.CheckPolygonIntersection(block_item.Value.polygon)) {
                    //Debug.Log("Enter");
                    block_item.Value.UpdateBlock();
                }
            }
        }
        vertices.Clear();
    }

    /*
    public bool CheckCoalYardSuperposition(Polygon polygon) {
        bool result = false;
        foreach (KeyValuePair<string, CoalDumpManager> item in this.coaldumps) {
            if (polygon.CheckPolygonIntersection(item.Value.polygon)) {
                result = true;
            }
        }

        return result;
    }*/

    //获取所有煤场
    public Dictionary<string, CoalDumpManager> GetCoalYardsInfo() {
        return this.coaldumps;
    }

    public Material CreateMaterial(int level,float height) {
        Color[] colors = new Color[] { Color.red, Color.green, Color.blue,Color.yellow,Color.white};

        Material material = new Material(Shader.Find("Custom/CoalYardShader"));

        Texture2D texture = new Texture2D(level,level,TextureFormat.ARGB32, true);
        Color[] cols = new Color[level * level];

        // Populate the array so that the x, y, and z values of the texture will map to red, blue, and green colors
        for (int x = 0; x < level; x++){
            for (int y = 0; y < level; y++){
                texture.SetPixel(x, y, colors[x]);
            }
        }

        texture.Apply();

        material.SetTexture("_MainTex", texture);
        material.SetInt("_Level", level);
        material.SetFloat("_Height", height);

        return material;
    }

    public void ClearCoalDump() {
        for (int i = 0, number = coal_container.childCount; i < number; i++){
            Transform child = coal_container.GetChild(0);
            GameObject.DestroyImmediate(child.gameObject);
        }

        for (int i = 0, number = coaldump_container.childCount; i < number; i++) {
            Transform child = coaldump_container.GetChild(0);
            GameObject.DestroyImmediate(child.gameObject);
        }
        this.coaldumps.Clear();
    }

    public void DrawCoalDumpArea(Transform container){
        foreach (KeyValuePair<string, CoalDumpManager> item in coaldumps){
            List<Vector2> vertices = item.Value.grid.vertices;

            List<Vector3> points = vertices.Select(vertice => new Vector3(vertice.x, 0.0f, vertice.y)).ToList();

            points.Add(points[0]);

            VectorLine area = new VectorLine("Line", points, 2.0f, LineType.Continuous, Joins.Weld);
            area.Draw3DAuto();
            area.name = item.Value.name;
            area.rectTransform.SetParent(container);

            CoalDumpEditor dump_editor = area.rectTransform.gameObject.AddComponent<CoalDumpEditor>();
            dump_editor.relink_line = area;
            dump_editor.info = item.Value.info;

            foreach (BoxCollider collider in item.Value.GetComponentsInChildren<BoxCollider>()) {
                GameObject.DestroyImmediate(collider);
            }
        }
    }

    public CoalDumpManager SearchCoalDump(string name) {
        CoalDumpManager coalDumpManager = coaldump_container.Find(name)?.GetComponent<CoalDumpManager>();
        return coalDumpManager;
    }
}
