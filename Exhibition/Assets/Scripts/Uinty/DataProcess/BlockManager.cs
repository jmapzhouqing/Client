using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BlockManager : MonoBehaviour
{
    public Polygon polygon;
    public Grid grid;
    private bool need_update = false;

    GridDataManager gridDataManager;


    private void Awake()
    {
        BoxCollider collider = this.gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
    }
    // Start is called before the first frame update
    void Start(){
        gridDataManager = FindObjectOfType<GridDataManager>();
        StartCoroutine(run());
    }

    IEnumerator run() {
        while (true) {
            if (need_update) {
                this.UpdateVertices();
                need_update = false;
            }
            yield return new WaitForSeconds(1.0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateBlock(int x,int z) {
        //int vertice_index = (x - grid.min_x_index) * grid.height + z - grid.min_z_index;
        need_update = true;
        /*
        Mesh mesh = this.transform.GetComponent<MeshFilter>().mesh;
       
        Vector3[] origin = mesh.vertices;

        origin[vertice_index] = vertice;

        mesh.vertices = origin;*/
    }

    public void UpdateBlock() {
        need_update = true;
    }

    public void ErasureGridData() {
        Mesh mesh = this.transform.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Color[] colors = mesh.colors;
        for (int i = 0; i < vertices.Length; i++) {
            vertices[i].y = 0;
            colors[i].a = 0;
        }
        mesh.vertices = vertices;
    }

    public bool CheckVerticeInGrid(int x, int z){
        return this.grid.CheckVerticeInGrid(x,z);
    }

    public void UpdateVertices() {
        int row = grid.index_width + 1;
        int col = grid.index_height + 1;

        Mesh mesh = this.transform.GetComponent<MeshFilter>().mesh;

        Vector3[] vertices = mesh.vertices;

        int index = 0;

        for (int m = 0; m < row; m++){
            for (int n = 0; n < col; n++){
                int value = index++;

                Vector3 vertice = gridDataManager.mesh_data[m + grid.index_boundary.min_x, n + grid.index_boundary.min_z];
                try{
                    vertices[value] = vertice;
                }catch (Exception e){
                    //Debug.Log(start_z + "#" + (m + start_x) + "#" + (n + start_z));
                }
            }
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
}
