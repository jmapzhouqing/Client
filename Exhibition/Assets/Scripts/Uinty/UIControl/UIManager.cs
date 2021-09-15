using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static RectTransform parent;

    private static string root_path = "UI/";

    private LeftControl left_control;
    // Start is called before the first frame update
    void Awake(){
        DOTween.Init(true, true, null);
        DOTween.defaultAutoPlay = AutoPlay.None;
        DOTween.defaultAutoKill = false;

        left_control = FindObjectOfType<LeftControl>();

        parent = GameObject.Find("container")?.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update(){
        
    }

    public void EditCoalDump(string name) {
        LoadUserInterface(name);
    }

   
    public void CreateCoalDump(List<CoalDumpInfo> data) {
        left_control.CreateCoalDump(data);
    }

    public void UpdateCoalDump() {

    }

    public static RectTransform LoadUserInterface(string name){
        string path = Path.Combine(root_path, name);

        RectTransform prefab = Resources.Load<RectTransform>(path);

        RectTransform control = GameObject.Instantiate<RectTransform>(prefab, parent);

        return control;
    }
}
