using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Calc : MonoBehaviour
{
    private Material mat;

    Vector3 data = new Vector3(2.5f,Mathf.Sqrt(3)*5/-2.0f - 0.001f,0);

    private List<Vector3> list;

    private List<Vector2> project_vertices;

    private Vector3 origin;

    private float[,] mesh_data;

    private bool CheckInPolygon(List<Vector2> polygon_vertices,Vector2 checkPoint) {
        bool inside = false;
        int pointCount = polygon_vertices.Count;
        Vector2 p1, p2;
        for (int i = 0, j = pointCount - 1; i < pointCount; j = i, i++)
        {
            p1 = polygon_vertices[i];
            p2 = polygon_vertices[j];
            if (checkPoint.y < p2.y)
            {
                if (p1.y <= checkPoint.y)
                {
                    if ((checkPoint.y - p1.y) * (p2.x - p1.x) > (checkPoint.x - p1.x) * (p2.y - p1.y))
                    {
                        inside = !inside;
                    }
                }
            }
            else if (checkPoint.y < p1.y)
            {
                if ((checkPoint.y - p1.y) * (p2.x - p1.x) < (checkPoint.x - p1.x) * (p2.y - p1.y))
                {
                    inside = !inside;
                }
            }
        }
        return inside;
    }

    private bool CheckCollider(float rotate) {

        bool result = false;

        int count = 0;

        float x = 50;
        float y = 5;
        float z = 5;

        list = new List<Vector3>();

        project_vertices = new List<Vector2>();

        Vector3 center = new Vector3(30, 0, -40);

        Quaternion rotation = Quaternion.Euler(0,rotate,0);

        Matrix4x4 matrix = new Matrix4x4();
        matrix.SetTRS(center, rotation, new Vector3(1, 1, 1));

        this.transform.position = matrix.MultiplyPoint(origin-center);
        this.transform.rotation = rotation;


        Vector3 vertice = origin + new Vector3(x, -1 * y, z) - center;
        vertice = matrix.MultiplyPoint(vertice);
        list.Add(vertice);
        project_vertices.Add(new Vector2(vertice.x, vertice.z));

        vertice = origin + new Vector3(x , -1 * y, -z) - center;
        vertice = matrix.MultiplyPoint(vertice);
        list.Add(vertice);
        project_vertices.Add(new Vector2(vertice.x, vertice.z));

        vertice = origin + new Vector3(-x, -1 * y, -z) - center;
        vertice = matrix.MultiplyPoint(vertice);
        list.Add(vertice);
        project_vertices.Add(new Vector2(vertice.x, vertice.z));

        vertice = origin + new Vector3(x, -1 * y, z) - center;
        vertice = matrix.MultiplyPoint(vertice);
        list.Add(vertice);
        project_vertices.Add(new Vector2(vertice.x, vertice.z));

        float max_x = list.OrderByDescending(s =>s.x).FirstOrDefault().x;
        float min_x = list.OrderBy(s => s.x).FirstOrDefault().x;
        float max_z = list.OrderByDescending(s => s.z).FirstOrDefault().z;
        float min_z = list.OrderBy(s => s.z).FirstOrDefault().z;

        Vector3 normal = Vector3.Cross((list[0] - list[1]), list[2] - list[1]).normalized;

        if (Vector3.Dot(normal, Vector3.up) < 0){
            normal *= -1;
        }

        Vector3 point = list[3];

        float d = -1 * Vector3.Dot(normal, point);

        int start_x = Mathf.RoundToInt(min_x / 0.1f);
        int end_x = Mathf.RoundToInt(max_x / 0.1f);
        int start_z = Mathf.RoundToInt(min_z / 0.1f);
        int end_z = Mathf.RoundToInt(max_z / 0.1f);

        int x_length = mesh_data.GetLength(0);
        int z_length = mesh_data.GetLength(1);

        try{
            for (int m = start_x; m < end_x; m++)
            {
                for (int n = start_z; n < end_z; n++)
                {
                    if (m < 0 || m >= x_length || n < 0 || n >= z_length)
                    {
                        continue;
                    }

                    if (!CheckInPolygon(project_vertices, new Vector2(0.1f * m, 0.1f * n))) {
                        continue;
                    }

                    Vector3 cur_point = new Vector3(0.1f * m, mesh_data[m, n], 0.1f * n);

                    float value = Vector3.Dot(cur_point, normal) + d;
                    if (value / normal.y > 0){
                        count++;
                    }
                }
            }
        }catch(Exception e) {

        }
        
        return result;
    }

    private void Awake(){
        origin = this.transform.position;

        //CheckCollider();
       

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

    private IEnumerator run() {
        float index = 0;
        while (true) {
            index -= 5f;
            CheckCollider(index);
            yield return new WaitForSeconds(1.0f);
        }
    }

    /*
    private void OnPostRender()
    {
        mat.SetPass(0);

        GL.PushMatrix();
        //GL.MultMatrix(transform.localToWorldMatrix);

        // Draw lines
        GL.Begin(GL.LINES);
        foreach(Vector3 item in list){
         
            GL.Color(Color.red);
            GL.Vertex(Vector3.zero);
            GL.Vertex(item);
        }
        GL.End();
        GL.PopMatrix();
    }*/


    // Start is called before the first frame update
    void Start(){
        mesh_data = FindObjectOfType<DataLoad>().mesh_data;

        StopAllCoroutines();
        StartCoroutine(run());
    }

    // Update is called once per frame
    void Update(){
        int length = project_vertices.Count;
        for (int i=0;i<=length;i++)
        {
            Vector2 fir = project_vertices[i % length];
            Vector2 sec = project_vertices[(i + 2) % length];
            Debug.DrawLine(new Vector3(fir.x,0,fir.y),new Vector3(sec.x,0,sec.y),Color.red);
        }

        //Debug.DrawLine(origin, data, Color.green);
    }

    private void OnApplicationQuit()
    {
        StopAllCoroutines();
    }
}
