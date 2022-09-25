using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using UnityEngine;

public class ConfigurationParameter {

    [DllImport("kernel32")]
    public static extern long GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder retVal, int size, string filePath);

    //煤场精度
    public static readonly float precision = 0.2f;

    //煤场单体
    public static readonly int mesh_segment_number = 200;

    //煤场宽度(x方向)
    public static readonly float coalyard_width = 73;

    //煤场长度(Z方向)
    public static readonly float coalyard_height = 650;

    //斗轮机臂长(旋转中心点至斗轮中心点投影至Z方向长度)
    public static readonly float arm_length = 35.0f;

    //斗轮机斗轮半径
    public static readonly float bucket_wheel_radius = 3.05f;

    //斗轮机斗轮宽度
    public static readonly float bucket_wheel_thickness = 0.96f;

    //层高
    public static readonly float level_height = 2.0f;

    //最大层数
    public static readonly int level_number = 5;

    public static readonly float bucket_wheel_center_offset_height = 1.266f;

    public static readonly float bucket_wheel_center_offset_width = 1.4f;

    public static readonly Vector3 track_center = new Vector3(coalyard_width/ 2.0f,0,0);

    public static readonly Vector2 bucket_wheel_size = new Vector2(0.96f, 6.1f);

    public static readonly float center_height = 7.85f;

    //斗轮机旋转、俯仰中心点坐标
    public static readonly Vector3 rotation_center = new Vector3(coalyard_width / 2.0f, 7.85f, 0.0f);

    //斗轮机斗轮中心坐标
    public static readonly Vector3 bucket_wheel_center_coordinate = new Vector3(1.4f, 1.266f, arm_length) + rotation_center;

    //斗轮机斗轮底部坐标
    public static readonly Vector3 bucket_wheel_bottom_coordinate = bucket_wheel_center_coordinate - new Vector3(0, bucket_wheel_radius, 0);


    static ConfigurationParameter(){
        string file_path = Path.Combine(Application.dataPath,"config.ini");
        if (File.Exists(file_path)){
            precision = Convert.ToSingle(ReadConfig(file_path, "CoalYardParam", "precision"));

            mesh_segment_number = Convert.ToInt32(ReadConfig(file_path, "CoalYardParam", "mesh_segment_number"));

            coalyard_width = Convert.ToSingle(ReadConfig(file_path, "CoalYardParam", "coalyard_width"));

            coalyard_height = Convert.ToSingle(ReadConfig(file_path, "CoalYardParam", "coalyard_height"));

            arm_length = Convert.ToSingle(ReadConfig(file_path, "CoalYardParam", "arm_length"));

            bucket_wheel_radius = Convert.ToSingle(ReadConfig(file_path, "CoalYardParam", "bucket_wheel_radius"));

            bucket_wheel_thickness = Convert.ToSingle(ReadConfig(file_path, "CoalYardParam", "bucket_wheel_thickness"));

            bucket_wheel_center_offset_height = Convert.ToSingle(ReadConfig(file_path, "CoalYardParam", "bucket_wheel_center_offset_height"));

            bucket_wheel_center_offset_width = Convert.ToSingle(ReadConfig(file_path, "CoalYardParam", "bucket_wheel_center_offset_width"));

            level_height = Convert.ToSingle(ReadConfig(file_path, "CoalYardParam", "level_height"));

            level_number = Convert.ToInt32(ReadConfig(file_path, "CoalYardParam", "level_number"));

            center_height = Convert.ToSingle(ReadConfig(file_path, "CoalYardParam", "center_height"));

            track_center = new Vector3(coalyard_width / 2.0f, 0, 0);

            bucket_wheel_size = new Vector2(bucket_wheel_thickness, bucket_wheel_radius * 2);

            rotation_center = new Vector3(coalyard_width / 2.0f, center_height, 0.0f);

            bucket_wheel_center_coordinate = new Vector3(bucket_wheel_center_offset_width, bucket_wheel_center_offset_height, arm_length) + rotation_center;

            bucket_wheel_bottom_coordinate = bucket_wheel_center_coordinate - new Vector3(0, bucket_wheel_radius, 0);
        }
    }

    public static void init() {

    }

    private static string ReadConfig(string file_path,string section,string key) {
        StringBuilder buffer = new StringBuilder(255);
        GetPrivateProfileString(section, key, "配置文件不存在，读取未成功!", buffer, buffer.MaxCapacity, file_path);
        return buffer.ToString();
    }
}
