using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using System.Threading;

public class DataLoad : MonoBehaviour
{
    private float width = 650;
    private float height = 66;
    private float precision = 0.1f;

    private int width_segment_number;
    private int height_segment_number;
    private int mesh_segment_number = 200;

    private Dictionary<string, GameObject> mesh_dic;

    private HashSet<string> update_mesh;

    public float[,] mesh_data;

    public static List<Vector3> update_list;

    private void Awake()
    {
        update_list = new List<Vector3>();

        width_segment_number = Mathf.FloorToInt(width / precision);
        height_segment_number = Mathf.FloorToInt(height / precision);
        mesh_segment_number = 200;

        update_mesh = new HashSet<string>();

        mesh_dic = new Dictionary<string, GameObject>();

        mesh_data = new float[width_segment_number + 1, height_segment_number + 1];

        for (int i = 0; i <= width_segment_number; i++)
        {
            for (int j = 0; j <= height_segment_number; j++)
            {
                mesh_data[i, j] = Mathf.Sin(j * Mathf.PI / height_segment_number) * 10;
            }
        }

        this.CreateMesh();

        StopAllCoroutines();
        StartCoroutine(MeshUpdate(3.0f));
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void CreateMesh(){
        GameObject parent = new GameObject();

        parent.name = "CoalYard";

        int width_number = Mathf.CeilToInt(width_segment_number / mesh_segment_number) + (width_segment_number%mesh_segment_number == 0 ? 0 : 1);
        int height_number = Mathf.CeilToInt(height_segment_number / mesh_segment_number) + (height_segment_number % mesh_segment_number == 0 ? 0 : 1);

        for(int i = 0; i < width_number; i++){
            for (int j = 0; j < height_number; j++){
                GameObject child = this.CreateMesh(i,j);
                child.transform.SetParent(parent.transform);
            }
        }
    }

    private IEnumerator MeshUpdate(float interval) {
        while (true) {
            try
            {
                //Debug.Log(update_list.Count);
                //List<Vector3> datas = update_list;
                int length = update_list.Count;
                for (int i=0;i<length;i++) {
                    this.SetCoordinate(update_list[i]);
                }

                update_list.Clear();

                //Debug.Log(update_mesh.Count);

                if (update_mesh.Count != 0){
                    foreach (string key in update_mesh)
                    {
                        this.UpdateMesh(key);
                        //Debug.Log(key);
                    }
                    update_mesh.Clear();
                }
                yield return new WaitForSeconds(interval);
            }
            finally {
                //Monitor.Exit(DataLoad.update_list);
            }
            /*
            Monitor.Enter(update_mesh);
            if (update_mesh.Count == 0){
                foreach (string key in update_mesh) {
                    //this.UpdateMesh(key);
                    Debug.Log(key);
                }
                update_mesh.Clear();
            }*/

            
        }  
    }

    private GameObject CreateMesh(int i,int j){
        int start_x = i * mesh_segment_number;
        int end_x = (i + 1) * mesh_segment_number > width_segment_number ? width_segment_number : (i + 1) * mesh_segment_number;

        int start_z = j * mesh_segment_number;
        int end_z = (j + 1) * mesh_segment_number > height_segment_number ? height_segment_number : (j + 1) * mesh_segment_number;

        Vector3[] vertices = new Vector3[(end_x - start_x+1)*(end_z - start_z+1)];
        Color[] colors = new Color[(end_x - start_x + 1) * (end_z - start_z + 1)];


        int[] triangles = new int[(end_x - start_x) * (end_z - start_z) * 2 * 3];

        int index = 0;
        int triangle_index = 0;

        int row = end_x - start_x + 1;
        int col = end_z - start_z + 1;

        for (int m = 0; m < row; m++) {
            for (int n = 0; n < col; n++) {
                int value = index++;
                try
                {
                    vertices[value] = new Vector3((m + start_x) * precision, mesh_data[m + start_x, n + start_z], (n + start_z) * precision);
                }
                catch (Exception e) {
                    Debug.Log(start_z+"#"+(m + start_x)+"#"+(n + start_z));
                }
                
                if (mesh_data[m + start_x, n + start_z] < 2)
                {
                    colors[value] = Color.red;
                    colors[value].a = 0;
                }
                else if (mesh_data[m + start_x, n + start_z] > 2 && mesh_data[m + start_x, n + start_z] < 4) {
                    colors[value] = Color.green;
                }
                else if (mesh_data[m + start_x, n + start_z] > 4 && mesh_data[m + start_x, n + start_z] < 6)
                {
                    colors[value] = Color.yellow;
                }
                else if (mesh_data[m + start_x, n + start_z] > 6 && mesh_data[m + start_x, n + start_z] <8)
                {
                    colors[value] = Color.gray;
                }
                else if (mesh_data[m + start_x, n + start_z] > 8)
                {
                    colors[value] = Color.white;
                }

                if((m < (row-1)) && (n < (col-1))){
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
        child.name = i + "_" + j;

        MeshFilter meshfilter = child.AddComponent<MeshFilter>();
        MeshRenderer render = child.AddComponent<MeshRenderer>();

        render.material = new Material(Shader.Find("Custom/vertexcolor"));
        render.material.SetColor("_Color", Color.gray);

        Mesh mesh = meshfilter.mesh;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
       
        child.transform.position = Vector3.zero;

        mesh_dic.Add(child.name,child);

        return child;
    }

    private void UpdateMesh(string key) {
        GameObject child;
        if (mesh_dic.TryGetValue(key, out child)) {
            string[] param = key.Split('_');

            int i = Convert.ToInt32(param[0]);
            int j = Convert.ToInt32(param[1]);

            Mesh mesh = child.GetComponent<MeshFilter>().mesh;


            int start_x = i * mesh_segment_number;
            int end_x = (i + 1) * mesh_segment_number > width_segment_number ? width_segment_number : (i + 1) * mesh_segment_number;

            int start_z = j * mesh_segment_number;
            int end_z = (j + 1) * mesh_segment_number > height_segment_number ? height_segment_number : (j + 1) * mesh_segment_number;

            Vector3[] vertices = new Vector3[(end_x - start_x + 1) * (end_z - start_z + 1)];
            Color[] colors = new Color[(end_x - start_x + 1) * (end_z - start_z + 1)];

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
                        vertices[value] = new Vector3((m + start_x) * precision, mesh_data[m + start_x, n + start_z], (n + start_z) * precision);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(start_z + "#" + (m + start_x) + "#" + (n + start_z));
                    }

                    if (mesh_data[m + start_x, n + start_z] < 2)
                    {
                        colors[value] = Color.red;
                    }
                    else if (mesh_data[m + start_x, n + start_z] > 2 && mesh_data[m + start_x, n + start_z] < 4)
                    {
                        colors[value] = Color.green;
                    }
                    else if (mesh_data[m + start_x, n + start_z] > 4 && mesh_data[m + start_x, n + start_z] < 6)
                    {
                        colors[value] = Color.yellow;
                    }
                    else if (mesh_data[m + start_x, n + start_z] > 6 && mesh_data[m + start_x, n + start_z] < 8)
                    {
                        colors[value] = Color.gray;
                    }
                    else if (mesh_data[m + start_x, n + start_z] > 8)
                    {
                        colors[value] = Color.white;
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

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors = colors;
            mesh.RecalculateNormals();
            //mesh.UploadMeshData(false);
        }
    }

    public void SetCoordinate(Vector3 position){
        int x = Mathf.RoundToInt((position.x+50) / precision);
        int z = Mathf.RoundToInt((position.z+25) / precision);

        if (x<0 || x > width_segment_number ||z<0 || z > height_segment_number) {
            return;
        }

        mesh_data[x, z] = position.y;

        string key = (x/mesh_segment_number) + "_" + (z / mesh_segment_number);

        update_mesh.Add(key);
    }

    private void OnApplicationQuit()
    {
        StopAllCoroutines();
    }
}
