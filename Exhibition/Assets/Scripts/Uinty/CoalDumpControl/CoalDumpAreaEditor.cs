using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

using System.Linq;

public class CoalDumpAreaEditor : MonoBehaviour
{
    public VectorLine Line;

    public string sign;

    private Vector3 origin;

    private Vector3 origin_vertice;

    private List<Vector3> origin_vertices;

    private CoalDumpEditorControl editor_control;

    private CoalDumpInfo info;

    // Start is called before the first frame update

    // Update is called once per frame
    void Awake(){
        editor_control = FindObjectOfType<CoalDumpEditorControl>();
        origin_vertice = this.transform.position;

        info = this.GetComponentInParent<CoalDumpEditor>().info;
        this.gameObject.layer = LayerMask.NameToLayer("Subsidiary");
    }

    private void OnMouseDown()
    {
        origin = Camera.main.WorldToScreenPoint(this.transform.position);

        if (Line != null) {
            BoxCollider collider = this.Line.rectTransform.gameObject.GetComponent<BoxCollider>();
            if (collider != null) {
                GameObject.DestroyImmediate(collider);
            }
        } 
    }

    private void OnMouseDrag()
    {
        Vector3 present = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,origin.z));
        present.y = 0;
        UpdateCoordinate(present);
    }

    private void UpdateCoordinate(Vector3 present) {
        this.transform.position = present;
        if (this.sign.Equals("start")){
            Line.points3[0] = present;
            Line.points3[4] = present;

            Line.points3[1] = new Vector3(Line.points3[0].x, Line.points3[1].y, Line.points3[2].z);
            Line.points3[3] = new Vector3(Line.points3[2].x, Line.points3[3].y, Line.points3[0].z);
        }
        else if (this.sign.Equals("end"))
        {
            Line.points3[2] = present;
            Line.points3[1] = new Vector3(Line.points3[0].x, Line.points3[1].y, Line.points3[2].z);
            Line.points3[3] = new Vector3(Line.points3[2].x, Line.points3[3].y, Line.points3[0].z);
        }
    }

    private void OnMouseUp(){
        StopAllCoroutines();
        if (Line != null){
            Grid grid = new Grid(Line.points3,ConfigurationParameter.precision,ConfigurationParameter.mesh_segment_number);
            if (editor_control.CheckCoalDumpSuperposition(grid,info.uuid)){
                this.UpdateCoordinate(origin_vertice);
            }
            info.vertices = Line.points3.Select(vertice=>new Vector2(vertice.x,vertice.z)).ToList();
            StartCoroutine(UpdateBoxCollider());
        }
    }

    IEnumerator UpdateBoxCollider() {
        yield return new WaitForEndOfFrame();
        if (Line != null){
            BoxCollider collider = this.Line.rectTransform.gameObject.AddComponent<BoxCollider>();
            collider.center = new Vector3(collider.center.x, 0, collider.center.z);
            collider.size = new Vector3(collider.size.x, 0, collider.size.z);
            collider.isTrigger = true;
        }
    }
}
