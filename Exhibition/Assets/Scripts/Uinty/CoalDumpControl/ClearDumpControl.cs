using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ClearDumpControl:MonoBehaviour{

    private CoalDumpInfo dump_info;

    private GridDataManager grid_data_manager;
    private void Awake()
    {
        grid_data_manager = FindObjectOfType<GridDataManager>();
    }

    public void SetProperty(CoalDumpInfo info)
    {
        this.dump_info = info;
    }

    public void Confirm()
    {
        Grid grid = dump_info.CreateGrid();

        BoundaryCoordinate<int> boundary = grid.index_boundary;

        grid_data_manager?.SetRegionData(boundary, 0);

        this.Cancel();
    }

    public void Cancel()
    {
        GameObject.DestroyImmediate(this.gameObject);
    }
}