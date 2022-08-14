using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using Vectrosity;
using LitJson;

public class CoalDumpEditorControl : MonoBehaviour
{
    #region

    #endregion


    private CameraControl camera_control;

    private VectorLine rect_line;

    private Transform container;

    private bool is_drag;

    private ProgramCommunication communication;
    // Start is called before the first frame update
    void Awake()
    {
        communication = FindObjectOfType<ProgramCommunication>();

    }

        // Start is called before the first frame update
    void Start() {
        container = new GameObject().transform;
        container.name = "LineContainer";
        //container.hideFlags = HideFlags.HideInHierarchy;
        camera_control = FindObjectOfType<CameraControl>();

        this.CameraOrientation();

        Grandmaster.DrawCoalDumpArea(container);

        Grandmaster.DrawWholeCoalYard();
    }

    public void CameraOrientation() {
        camera_control.SetRotation(new Vector2(90, 0));
        camera_control.SetRotationControl(false);

        if (rect_line != null) {
            VectorLine.Destroy(ref rect_line);
        }
    }

    Coordinate GetCoalYardPosition(Vector3 input) {
        Coordinate result = null;

        RaycastHit hit;

        Ray ray = Camera.main.ScreenPointToRay(input);

        if (Physics.Raycast(ray, out hit)) {
            result = new Coordinate(hit.point);
        }
        return result;
    }

    public void Save() {
        List<CoalDumpInfo> list = new List<CoalDumpInfo>();
        List<CoalPosition> list_position = new List<CoalPosition>();
        foreach (CoalDumpEditor dump_editor in container.GetComponentsInChildren<CoalDumpEditor>(true)) {
            //string data = JsonMapper.ToJson(dump_editor.info);
            //Debug.Log(data);
            list.Add(dump_editor.info);

            Grid grid = dump_editor.info.CreateGrid();
            BoundaryCoordinate<float>  vertice_boundary = grid.vertice_boundary;

            Vector2 size = grid.GetSize();
            CoalPosition coal_position = new CoalPosition(vertice_boundary.min_x, vertice_boundary.min_z, size.x, size.y);

            list_position.Add(coal_position);
        }
        DataRead.SaveCoalDumpData(list);

        communication.SendData("CoalManager Position " + JsonMapper.ToJson(list_position));

        this.Cancel();
    }

    public void Cancel() {
        Grandmaster.UpdateCoalDumpData();
        camera_control.SetRotationControl(true);
        GameObject.Destroy(this.gameObject);
    }

    public void SetAddStation() {
        CoalDumpEditorAddControl editor_add = this.gameObject.GetComponent<CoalDumpEditorAddControl>() ?? this.gameObject.AddComponent<CoalDumpEditorAddControl>();
        editor_add.container = container;
    }

    public void AddCoalDump(VectorLine line, CoalDumpInfo info) {
        line.rectTransform.SetParent(container);
        CoalDumpEditor dump_editor = line.rectTransform.gameObject.AddComponent<CoalDumpEditor>();

        dump_editor.relink_line = line;
        dump_editor.info = info;

        //dump_editor.CreateEditor();
    }

    public bool CheckCoalDumpSuperposition(Grid grid, string key = "") {
        bool result = false;
        foreach (CoalDumpEditor dump_editor in container.GetComponentsInChildren<CoalDumpEditor>(true)) {
            Grid check_grid = new Grid(dump_editor.relink_line.points3, ConfigurationParameter.precision, ConfigurationParameter.mesh_segment_number);
            if (!key.Equals(dump_editor.info.uuid) && grid.CheckPolygonIntersection(check_grid)) {
                result = true;
            }
        }
        return result;
    }

    public bool CheckCoalDumpSize(Grid grid, float value = 5)
    {
        bool result = false;

        Vector2 size = grid.GetSize();

        if (size.x < value || size.y < value)
        {
            result = true;
        }

        return result;
    }

    private void OnDestroy(){
        if (container != null){
            GameObject.DestroyImmediate(container.gameObject);
        }
    }
}
