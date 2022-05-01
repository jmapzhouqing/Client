using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class LeftControl : MonoBehaviour
{

    #region

    public Transform container;

    public Sprite expand_image;

    public Sprite shrink_image;

    public Image expand;

    #endregion


    private RectTransform left_transform;

    private Tween left_expand;

    private bool is_expand = false;

    private float duration = 0.2f;

    private UIManager manager;

    private RectTransform coaldump_prefab;

    private bool is_lock = false;

    private void Awake(){
        manager = FindObjectOfType<UIManager>();

        coaldump_prefab = Resources.Load<RectTransform>("UI/CoalDump");
    }

    // Start is called before the first frame update
    void Start()
    {
        left_transform = this.GetComponent<RectTransform>();
        left_expand = left_transform.DOAnchorPos3DX(0, duration).OnStepComplete(ChangeIcon);
    }

    public void ClearCoalDump()
    {
        for (int i = 0, number = container.childCount; i < number; i++){
            Transform child = container.GetChild(0);
            GameObject.DestroyImmediate(child.gameObject);
        }
    }

    public void CreateCoalDump(List<CoalDumpInfo> data) {
        this.ClearCoalDump();
        foreach (CoalDumpInfo info in data) {
            this.CreateCoalDump(info);
        }
    }

    public void CreateCoalDump(CoalDumpInfo info) {
        RectTransform coal_dump = GameObject.Instantiate<RectTransform>(coaldump_prefab, container);
        coal_dump.name = info.uuid;
        CoalDumpOperation operation = coal_dump.GetComponent<CoalDumpOperation>();
        operation.SetInfo(info);
    } 

    public void UpdateCoalDump(List<CoalDumpInfo> data){
        foreach (CoalDumpInfo info in data) {
            this.UpdateCoalDump(info);
        }
    }

    public void UpdateCoalDump(CoalDumpInfo info) {

    }

    public void Expand() {
        if (!is_lock) {
            this.Expand(!is_expand);
        } 
    }

    private void Expand(bool station) {
        if (left_expand != null && left_expand.IsPlaying()) {
            left_expand.Pause();
        }
        if (station){
            left_expand.PlayForward();
        }else {
            left_expand.PlayBackwards();
        }
        this.is_expand = station;
    }

    public void Lock(bool status) {
        is_lock = status;
        if (is_lock) {
            this.Expand(false);
        }
    }

    private void ChangeIcon() {
        if (this.is_expand){
            this.expand.sprite = shrink_image;
        }else {
            this.expand.sprite = expand_image;
        }
    }
}
