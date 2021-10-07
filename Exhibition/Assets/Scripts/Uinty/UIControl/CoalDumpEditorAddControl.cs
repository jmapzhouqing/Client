using System;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

public class CoalDumpEditorAddControl : MonoBehaviour
{
    #region
    public Transform container;
    #endregion

    private CameraControl camera_control;

    private VectorLine rect_line;

    private bool is_drag = false;

    private CoalDumpEditorControl editor_control;
    // Start is called before the first frame update
    void Awake(){
        rect_line = null;

        camera_control = FindObjectOfType<CameraControl>();
        editor_control = FindObjectOfType<CoalDumpEditorControl>();
    }

    // Update is called once per frame
    void Update()
    {
        if (camera_control.control_state.Equals(MouseControl.LeftClick)){
            Coordinate point = camera_control.GetCoalYardPosition("CoalYard");
            if (point != null){
                point.vertice.y = 0.0f;
                if (rect_line != null){
                    VectorLine.Destroy(ref rect_line);
                    rect_line = null;
                }

                var points = new List<Vector3> { point.vertice, Vector3.zero, Vector3.zero, Vector3.zero, point.vertice };
                rect_line = new VectorLine("Line", points, 2.0f, LineType.Continuous, Joins.Weld);
                rect_line.layer = LayerMask.NameToLayer("Subsidiary");
                rect_line.SetColor(new Color(0, 1, 0));
            }
        }else if (camera_control.control_state.Equals(MouseControl.LeftDrag)){
            is_drag = true;
            Coordinate point = camera_control.GetCoalYardPosition("CoalYard");
            if (rect_line != null && point != null){
                point.vertice.y = 0.0f;
                rect_line.points3[2] = point.vertice;
                rect_line.points3[1] = new Vector3(rect_line.points3[0].x, 0, rect_line.points3[2].z);
                rect_line.points3[3] = new Vector3(rect_line.points3[2].x, 0, rect_line.points3[0].z);

                rect_line.Draw3DAuto();
            }
        }else if (camera_control.control_state.Equals(MouseControl.LeftUp)){
            if (is_drag && rect_line != null){
                Grid grid = new Grid(rect_line.points3,ConfigurationParameter.precision,ConfigurationParameter.mesh_segment_number);
                if (editor_control.CheckCoalDumpSuperposition(grid)){
                    if (rect_line != null){
                        VectorLine.Destroy(ref rect_line);
                        rect_line = null;
                    }
                    return;
                }

                CoalDumpInfo info = new CoalDumpInfo(Guid.NewGuid().ToString("N"));

                RectTransform confirm = UIManager.instance.LoadUserInterface("CoalDumpInfo");
                CoalDumpInfoEditorControl control = confirm?.GetComponent<CoalDumpInfoEditorControl>();
                control?.SetProperty(rect_line, info);
            }
            is_drag = false;

            GameObject.DestroyImmediate(this);
        }
    }

    private void OnDisable()
    {
        if (rect_line != null){
            //VectorLine.Destroy(ref rect_line);
            //rect_line = null;
        }
    }
}
