using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigurationParameter {
    //煤场精度
    public static readonly float precision = 0.2f;

    //煤场单体
    public static readonly int mesh_segment_number = 200;

    //煤场宽度(x方向)
    public static readonly float coal_width = 73;

    //煤场长度(Z方向)
    public static readonly float coal_height = 650;

    //斗轮机臂长(旋转中心点至斗轮中心点投影至Z方向长度)
    public static readonly float arm_length = 35.0f;

    //斗轮机斗轮半径
    public static readonly float wheel_radius = 3.05f;

    //斗轮机斗轮宽度
    public static readonly float wheel_width = 0.96f;

    //层高
    public static readonly float level_height = 2.0f;

    //最大层数
    public static readonly int level_number = 5;

    public static readonly float wheel_center_offset_height = 1.266f;

    //左侧盘煤仪坐标
    public static readonly Vector3 left_scanner_coordinate = new Vector3(-4.2f, 2.8f, 29);

    //右侧盘煤仪坐标
    public static readonly Vector3 right_scanner_coordinate = new Vector3(2.28f, 2.8f, 29);

    public static readonly Vector3 track_center = new Vector3(coal_width/2.0f,0,0);


    public static readonly Vector2 bucket_wheel_size = new Vector2(0.96f, 6.1f);

    public static readonly float center_height = 7.85f;

    public static readonly float wheel_offset_center = 1.4f;


    //斗轮机旋转、俯仰中心点坐标
    public static readonly Vector3 rotation_center = new Vector3(coal_width / 2.0f, 7.85f, 0.0f);

    //斗轮机斗轮中心坐标
    public static readonly Vector3 bucket_wheel_center_coordinate = new Vector3(1.4f, 1.266f, arm_length) + rotation_center;

    //斗轮机斗轮底部坐标
    public static readonly Vector3 bucket_wheel_bottom_coordinate = bucket_wheel_center_coordinate - new Vector3(0, wheel_radius, 0);


    static ConfigurationParameter(){
        precision = 0.2f;
    }

    public static void init() {

    }
}
