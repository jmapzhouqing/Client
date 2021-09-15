using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CoalDumpOperation : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler{

    public CoalDumpInfo dump_info;

    private Transform device;

    private Transform wheel;

    private CameraControl camera_control;

    private Vector2 delta;

    private CoalDumpManager coalDumpManager;
    // Start is called before the first frame update
    void Awake(){
        device = GameObject.Find("Circle001").transform;
        wheel = GameObject.Find("wheel").transform;

        camera_control = FindObjectOfType<CameraControl>();

        delta = new Vector2(5,5);
    }

    public void StackCoal(){
        Grid grid = dump_info.CreateGrid();

        int side = this.CheckCoalDumpSide(grid);

        if (side == -1) {
            return;
        }

        BoundaryCoordinate<float> boundary = grid.vertice_boundary;

        float max_z = boundary.max_z - delta.y;

        Vector3 center = ConfigurationParameter.rotation_center;
        float radius = ConfigurationParameter.arm_length;

        float rotation_offset = Mathf.Atan(ConfigurationParameter.wheel_offset_center / radius);

        float left_x = boundary.min_x + delta.x;
        float right_x = boundary.max_x - delta.x;

        float internal_rotation = 0;
        float external_rotation = 0;
        float z = 0;

        if (side == 0){
            internal_rotation = Mathf.Asin((right_x - center.x) / radius);
            external_rotation = Mathf.Asin((left_x - center.x) / radius);
        }
        else if (side == 1) {
            internal_rotation = Mathf.Asin((left_x - center.x) / radius);
            external_rotation = Mathf.Asin((right_x - center.x) / radius);
        }

        z = max_z - Mathf.Cos(internal_rotation) * radius;

        internal_rotation = (internal_rotation - rotation_offset) * Mathf.Rad2Deg;
        external_rotation = (external_rotation - rotation_offset) * Mathf.Rad2Deg;

        StackCoalInfo info = new StackCoalInfo();
        info.side = side;
        info.internal_rotation = internal_rotation;
        info.external_rotation = external_rotation;
        info.is_empty_dump = (dump_info.level == 0);
        info.device_position = z;
        info.use_config_corner = false;

        RectTransform rect = UIManager.LoadUserInterface("StackCoal");
        StackCoalControl stackCoalControl = rect.GetComponentInChildren<StackCoalControl>();
        stackCoalControl.SetProperty(info);
    }

    public void TakeCoal() {
        Grid grid = dump_info.CreateGrid();

        SpatialAnalysis analysis = device.GetComponent<SpatialAnalysis>()??device.gameObject.AddComponent<SpatialAnalysis>();

        Dictionary<string,Vector3> param = analysis.CaculateGridBoundary(grid, 1.9f);

        if (param != null){
            camera_control.SetCameraFollowing(wheel);
            analysis.SetUpdate(true);
        }else {
            GameObject.DestroyImmediate(analysis);
        }
    }

    private int CheckCoalDumpSide(Grid grid) {
        int result = -1;
        BoundaryCoordinate<float> boundary = grid.vertice_boundary;
        if (boundary.max_x < ConfigurationParameter.rotation_center.x){
            result = 0;
        }
        else if (boundary.min_x > ConfigurationParameter.rotation_center.x){
            result = 1;
        }

        return result;
    }

    public void OnPointerExit(PointerEventData eventData){
        if (coalDumpManager == null) {
            coalDumpManager = Grandmaster.SearchCoalDump(dump_info.uuid);
        }

        coalDumpManager?.material.SetColor("_Emission", new Color(0, 0, 0, 0));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (coalDumpManager == null)
        {
            coalDumpManager = Grandmaster.SearchCoalDump(dump_info.uuid);
        }

        coalDumpManager?.material.SetColor("_Emission", new Color(1, 0, 0, 0.3f));
    }
}
