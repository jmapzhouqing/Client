﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CoalDumpOperation : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler{

    public Text coal_type;

    private CoalDumpInfo dump_info;

    private Transform wheel;

    private CameraControl camera_control;

    private Vector2 delta;

    private CoalDumpManager coalDumpManager;

    private GridDataManager grid_data_manager;
    // Start is called before the first frame update
    void Awake(){

        grid_data_manager = FindObjectOfType<GridDataManager>();

        wheel = GameObject.Find("wheel").transform;

        camera_control = FindObjectOfType<CameraControl>();

        delta = new Vector2(5,5);
    }

    public void SetInfo(CoalDumpInfo info) {
        this.dump_info = info;
        coal_type.text = info.dump_name;
    }

    public void StackCoal(){
        Grid grid = dump_info.CreateGrid();

        int side = this.CheckCoalDumpSide(grid);

        if (side == -1) {
            return;
        }

        BoundaryCoordinate<float> boundary = grid.vertice_boundary;

        float max_z = boundary.max_z - delta.y;
        float min_z = boundary.min_z + delta.y;

       

        Vector3 center = ConfigurationParameter.rotation_center;
        float radius = ConfigurationParameter.arm_length;

        float rotation_offset = Mathf.Atan(ConfigurationParameter.wheel_offset_center / radius);

        float left_x = boundary.min_x + delta.x;
        float right_x = boundary.max_x - delta.x;

        float internal_rotation = 0;
        float external_rotation = 0;

        if (side == 0){
            internal_rotation = Mathf.Asin((right_x - center.x) / radius);
            external_rotation = Mathf.Asin((left_x - center.x) / radius);
        }
        else if (side == 1) {
            internal_rotation = Mathf.Asin((left_x - center.x) / radius);
            external_rotation = Mathf.Asin((right_x - center.x) / radius);
        }

        max_z = max_z - Mathf.Cos(internal_rotation) * radius;
        min_z = min_z - Mathf.Cos(internal_rotation) * radius;

        internal_rotation = (internal_rotation - rotation_offset) * Mathf.Rad2Deg;
        external_rotation = (external_rotation - rotation_offset) * Mathf.Rad2Deg;

        max_z = max_z > 0 ? max_z : 0;
        min_z = min_z > 0 ? min_z : 0;

        StackCoalInfo info = new StackCoalInfo();
        info.dump_name = dump_info.dump_name;
        info.side = side;
        info.internal_rotation = internal_rotation;
        info.external_rotation = external_rotation;
        info.is_empty_dump = (dump_info.level == 0);
        info.max_z = max_z;
        info.min_z = min_z;
        info.use_config_corner = false;

        RectTransform rect = UIManager.instance.LoadUserInterface("StackCoal");
        StackCoalControl stackCoalControl = rect.GetComponentInChildren<StackCoalControl>();
        stackCoalControl.SetProperty(info);
    }

    public void TakeCoal() {

        if(dump_info.level == 0){
            return;
        }

        RectTransform rect = UIManager.instance.LoadUserInterface("TakeCoal");
        TakeCoalControl takeCoalControl = rect.GetComponentInChildren<TakeCoalControl>();
        takeCoalControl.SetProperty(dump_info);

        /*
        if (param != null){
            camera_control.SetCameraFollowing(wheel);
            analysis.SetUpdate(true);
        }else {
            GameObject.DestroyImmediate(analysis);
        }*/
    }

    public void ClearCoal() {
        Grid grid = dump_info.CreateGrid();

        BoundaryCoordinate<int> boundary = grid.index_boundary;

        grid_data_manager?.SetRegionData(boundary,0);
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
