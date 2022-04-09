using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using System.Threading;

public class UIManager : MonoBehaviour
{
    private RectTransform control_container;

    private string root_path = "UI/";

    private LeftControl left_control;

    private RectTransform info_container;

    public static UIManager instance;

    private List<Action> actions;

    // Start is called before the first frame update
    void Awake(){
        instance = this;

        actions = new List<Action>();
        DOTween.Init(true, true, null);
        DOTween.defaultAutoPlay = AutoPlay.None;
        DOTween.defaultAutoKill = false;

        left_control = FindObjectOfType<LeftControl>();

        control_container = GameObject.Find("right-container")?.GetComponent<RectTransform>();
        info_container = GameObject.Find("info")?.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update(){
        try
        {
            Monitor.Enter(actions);
            if (actions.Count != 0) {
                Action action = actions[0];
                action.Invoke();
                actions.RemoveAt(0);
            }
        }
        finally {
            Monitor.Exit(actions);
        }
    }

    public void EditCoalDump(string name) {
        LoadUserInterface(name);
    }

    public void CreateCoalDump(List<CoalDumpInfo> data) {
        left_control.CreateCoalDump(data);
    }

    public void UpdateCoalDump() {

    }

    public RectTransform LoadUserInterface(string name){
        RectTransform control = null;
        try{
            string path = Path.Combine(root_path, name);

            RectTransform prefab = Resources.Load<RectTransform>(path);

            control = GameObject.Instantiate<RectTransform>(prefab, control_container);
            control.localScale = new Vector3(1,1,1);
            control.anchoredPosition3D = Vector3.zero;
            control.rotation = Quaternion.identity;
        }catch (Exception e){
            Debug.Log(e.Message);
        }
        return control;
    }

    public void StackCoalExhibition(string data) {
        Debug.Log(data);
        StackCoalInfo info = JsonUtility.FromJson<StackCoalInfo>(data);

        string path = Path.Combine(root_path, "StackCoalExhibition");

        RectTransform prefab = Resources.Load<RectTransform>(path);

        RectTransform control = GameObject.Instantiate<RectTransform>(prefab, control_container);

        StackCoalExhibition stackCoalExhibition = control.GetComponent<StackCoalExhibition>();
        stackCoalExhibition.SetInfo(info);
    }

    public void TakeCoalExhibition(string data)
    {
        TakeCoalInfo info = JsonUtility.FromJson<TakeCoalInfo>(data);

        string path = Path.Combine(root_path, "TakeCoalExhibition");

        RectTransform prefab = Resources.Load<RectTransform>(path);

        RectTransform control = GameObject.Instantiate<RectTransform>(prefab, control_container);

        TakeCoalExhibition stackCoalExhibition = control.GetComponent<TakeCoalExhibition>();
        stackCoalExhibition.SetInfo(info);
    }

    public void ClearInterface() {
        for (int i = 0, number = control_container.childCount; i < number; i++) {
            Transform child = control_container.GetChild(i);
            GameObject.DestroyImmediate(child.gameObject);
        }
    }

    public void LockLeft(bool status) {
        left_control.Lock(status);
    }

    public void ExhibitionInfo(string data) {
        string path = Path.Combine(root_path, "Tip");

        RectTransform prefab = Resources.Load<RectTransform>(path);

        RectTransform control = GameObject.Instantiate<RectTransform>(prefab, info_container);

        TipControl tip = control.GetComponent<TipControl>();
        tip.SetInfo(data);
    }

    public void Refresh(Action action) {
        try
        {
            Monitor.Enter(actions);
            actions.Add(action);
        }
        finally {
            Monitor.Exit(actions);
        }
    }
}
