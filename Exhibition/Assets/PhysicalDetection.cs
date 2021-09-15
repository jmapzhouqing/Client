using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class PhysicalDetection:MonoBehaviour
{
    private float length;
    private float width;
    private float height;

    Quaternion rotation;
    Matrix4x4 matrix;

    private List<Vector3> polygon_vertices;

    private List<Vector3> vertices;

    private List<Vector2> project_vertices;

    public PhysicalDetection(float length,float width,float height){
        this.length = length;
        this.width = width;
        this.height = height;

        matrix = new Matrix4x4();

        polygon_vertices = new List<Vector3>();

        Vector3 vertice = new Vector3(0, -1 * height, this.width/2.0f);
        vertice = matrix.MultiplyPoint(vertice);
        polygon_vertices.Add(vertice);

        vertice = new Vector3(0, -1 * height, -this.width / 2.0f);
        vertice = matrix.MultiplyPoint(vertice);
        polygon_vertices.Add(vertice);

        vertice = new Vector3(length, -1 * height, -this.width / 2.0f);
        vertice = matrix.MultiplyPoint(vertice);
        polygon_vertices.Add(vertice);

        vertice = new Vector3(length, -1 * height, this.width / 2.0f);
        vertice = matrix.MultiplyPoint(vertice);
        polygon_vertices.Add(vertice);
    }

    public PhysicalDetection() {
        float length = 35.5f;
        float width = 5;
        float height = 5;

        matrix = new Matrix4x4();

        polygon_vertices = new List<Vector3>();

        Vector3 vertice = new Vector3(0, -1 * height, this.width);
        polygon_vertices.Add(vertice);

        vertice = new Vector3(0, -1 * height, -this.width);
        polygon_vertices.Add(vertice);

        vertice = new Vector3(length, -1 * height, -this.width);
        polygon_vertices.Add(vertice);

        vertice = new Vector3(length, -1 * height, this.width);
        polygon_vertices.Add(vertice);
    }

    public bool CheckCollision(float[,] mesh_data,float precision,Vector3 uwb_position,float rotate,float pitch,int threshold = 100)
    {
        bool result = false;

        int number = 0;
         
        vertices = new List<Vector3>();

        project_vertices = new List<Vector2>();

        Quaternion rotation = Quaternion.Euler(0, rotate, pitch);

        Matrix4x4 uwb_matrix = new Matrix4x4();

        uwb_matrix.SetTRS(Vector3.zero, rotation, new Vector3(1, 1, 1));

        Vector3 uwb = new Vector3(9.6f,0,0);

        Vector3 uwb_rotation = uwb_matrix.MultiplyPoint(uwb);

        Vector3 center = uwb_position - uwb_rotation;

        center.y = uwb_position.y;

        matrix = new Matrix4x4();

        matrix.SetTRS(center, rotation, new Vector3(1, 1, 1));

        foreach (Vector3 origin_vertice in polygon_vertices) {
            Vector3 vertice = matrix.MultiplyPoint(origin_vertice);
            vertices.Add(vertice);
            project_vertices.Add(new Vector2(vertice.x, vertice.z));
        }

        float max_x = vertices.OrderByDescending(s => s.x).FirstOrDefault().x;
        float min_x = vertices.OrderBy(s => s.x).FirstOrDefault().x;
        float max_z = vertices.OrderByDescending(s => s.z).FirstOrDefault().z;
        float min_z = vertices.OrderBy(s => s.z).FirstOrDefault().z;

        Vector3 normal = Vector3.Cross((vertices[0] - vertices[1]), vertices[2] - vertices[1]).normalized;

        if(Vector3.Dot(normal, Vector3.up) < 0){
            normal *= -1;
        }

        Vector3 point = vertices[3];

        float d = -1 * Vector3.Dot(normal, point);

        int start_x = Mathf.RoundToInt(min_x / precision);
        int end_x = Mathf.RoundToInt(max_x / precision);
        int start_z = Mathf.RoundToInt(min_z / precision);
        int end_z = Mathf.RoundToInt(max_z / precision);

        int x_length = mesh_data.GetLength(0);
        int z_length = mesh_data.GetLength(1);

        try{
            for (int m = start_x; m < end_x; m++){
                for (int n = start_z; n < end_z; n++)
                {
                    if (m < 0 || m >= x_length || n < 0 || n >= z_length){
                        continue;
                    }

                    if (!CheckInPolygon(project_vertices, new Vector2(precision * m, precision * n))){
                        continue;
                    }

                    Vector3 cur_point = new Vector3(precision * m, mesh_data[m, n], precision * n);

                    float value = Vector3.Dot(cur_point, normal) + d;
                    if (value / normal.y > 0){
                        number++;
                    }
                }
            }
        }catch (Exception e){
            Console.WriteLine(e.Message);
        }

        if (number < threshold){
            return false;
        }else {
            return true;
        }
    }

    private bool CheckInPolygon(List<Vector2> polygon_vertices, Vector2 checkPoint){
        bool inside = false;
        int pointCount = polygon_vertices.Count;
        Vector2 p1, p2;
        for (int i = 0, j = pointCount - 1; i < pointCount; j = i, i++){
            p1 = polygon_vertices[i];
            p2 = polygon_vertices[j];
            if (checkPoint.y < p2.y){
                if (p1.y <= checkPoint.y){
                    if ((checkPoint.y - p1.y) * (p2.x - p1.x) > (checkPoint.x - p1.x) * (p2.y - p1.y)){
                        inside = !inside;
                    }
                }
            }else if (checkPoint.y < p1.y){
                if ((checkPoint.y - p1.y) * (p2.x - p1.x) < (checkPoint.x - p1.x) * (p2.y - p1.y)){
                    inside = !inside;
                }
            }
        }
        return inside;
    }

    /*
    private Material mat;
    private float[,] mesh_data;

    private Vector3 origin;
    public void Awake()
    {
        origin = this.transform.position;

        Shader shader = Shader.Find("Hidden/Internal-Colored");
        mat = new Material(shader);
        mat.hideFlags = HideFlags.HideAndDontSave;
        // Turn on alpha blending
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        // Turn backface culling off
        mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        // Turn off depth writes
        mat.SetInt("_ZWrite", 0);
    }

    private void Start()
    {
        mesh_data = FindObjectOfType<DataLoad>().mesh_data;

        StopAllCoroutines();
        StartCoroutine(run());
    }

    private IEnumerator run()
    {
        float index = 0;
        while (true)
        {
            index -= 5f;
            this.CheckCollision(new Vector3(30, 0, -40), index, 0);
            yield return new WaitForSeconds(1.0f);
        }
    }


    void Update()
    {
        int length = vertices.Count;
        
        for (int i = 0; i <= length; i++)
        {
            Debug.DrawLine(vertices[i % length], vertices[(i + 1) % length], Color.red);
        }
    }

    void OnRenderObject() {

        mat.SetPass(0);

        GL.PushMatrix();
        
        GL.Begin(GL.LINES);
        GL.Vertex(Vector3.zero);
        GL.Vertex(new Vector3(100, 0, 0));
        GL.End();
        GL.PopMatrix();
    }*/
}
