using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Vectrosity;

public class Grandmaster : MonoBehaviour{
    private static GridDataManager grid_data;
    private static UIManager ui_manager;
    // Start is called before the first frame update
    void Awake(){
        ConfigurationParameter.init();
        grid_data = FindObjectOfType<GridDataManager>();
        ui_manager = FindObjectOfType<UIManager>();
    }

    private void Start(){
        UpdateCoalDumpData();

        //推送煤堆各层真实的边界
        foreach (CoalDumpManager manager in FindObjectsOfType<CoalDumpManager>()) {
            manager.grid.CaculateRealBoundary(grid_data.mesh_data,manager.info);
        }
    }

    public static void UpdateCoalDumpData() {
        List<CoalDumpInfo> dumps = DataRead.ReadCoalDumpData();
        grid_data.CreateCoalDump(dumps);
        ui_manager.CreateCoalDump(dumps);
    }

    public static void DrawCoalDumpArea(Transform container) {
        grid_data.DrawCoalDumpArea(container);
    }

    public static void DrawWholeCoalYard(){
        grid_data.CreateWholeCoalYard();
    }

    public static CoalDumpManager SearchCoalDump(string key) {
        return grid_data.SearchCoalDump(key);
    }

    public static void UpdateGridData(int x, int z, Vector3 vertice) {
        grid_data.UpdateGridData(x,z,vertice);
    }

    public static void UpdateGridData(List<Vector3> data) {
        grid_data.UpdateGridData(data);
    }

    public static void UpdateCoalYard(List<CoordinateIndex> data) {
        grid_data.UpdateCoalYard(data);
    }

    public static Vector3 GetGridCoordinate(int x, int z) {
        return grid_data.mesh_data[x, z];
    }

    /*
    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 60), "Click"))
        {
            GridDataPersistence.SaveData("", grid_data.precision, grid_data.mesh_data);
        }
    }*/

    private void OnApplicationQuit(){
        
    }
}
