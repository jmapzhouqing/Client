using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TakeCoalControl : MonoBehaviour
{
    private SpatialAnalysis analysis;

    private CoalDumpInfo dump_info;

    public Dropdown dropdown;
    // Start is called before the first frame update
    private ProgramCommunication communication;
    // Start is called before the first frame update
    void Awake(){
        analysis = FindObjectOfType<SpatialAnalysis>();
        communication = FindObjectOfType<ProgramCommunication>();
    }

    public void SetProperty(CoalDumpInfo info){
        this.dump_info = info;
        List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
        for (int i = 1; i <= info.level; i++) {
            list.Add(new Dropdown.OptionData(i+"层"));
        }
        dropdown.options = list;
        dropdown.value = (info.level - 1);
    }

    public void Confirm(){
        TakeCoalInfo info = new TakeCoalInfo();
        info.level = (dropdown.value + 1);
        info.dump_name = dump_info.dump_name;

        Grid grid = dump_info.CreateGrid();

        Dictionary<string, Vector3> param = analysis.CaculateGridBoundary(grid, ConfigurationParameter.level_height * (info.level-1),ConfigurationParameter.level_height * (info.level));

        if (param != null){
            info.euler = param["rotation"];
            info.position = param["center"].z;
            info.stop_position = param["stop"].z;

            Debug.Log(JsonUtility.ToJson(info));

            communication.SendData("TakeCoal Command " + JsonUtility.ToJson(info));
        }


        this.Cancel();
    }

    public void Cancel() {
        GameObject.DestroyImmediate(this.gameObject);
    }
}
