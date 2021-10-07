using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

public class CoalDumpEditor : MonoBehaviour
{
    public VectorLine relink_line;
    public CoalDumpInfo info;

    private bool isHover = false;
    private CameraControl camera_control;

    private RaycastHit hit;

    private Transform prefab;
    // Start is called before the first frame update
    void Awake(){
        camera_control = FindObjectOfType<CameraControl>();
        this.gameObject.layer = LayerMask.NameToLayer("Subsidiary");

        prefab = Resources.Load<Transform>("prefab/PointEditor");
    }

    private IEnumerator Start(){
        yield return new WaitForEndOfFrame();
        if (relink_line != null) {
            BoxCollider collider = relink_line.rectTransform.gameObject.AddComponent<BoxCollider>();
            collider.center = new Vector3(collider.center.x, 0, collider.center.z);
            collider.size = new Vector3(collider.size.x,0,collider.size.z);
            collider.isTrigger = true;

            yield return new WaitForEndOfFrame();
            this.CreateEditor();
        }
    }

    // Update is called once per frame
    void Update(){
        if (isHover&&camera_control.control_state.Equals(MouseControl.LeftDbclick)){
            RectTransform confirm = UIManager.instance.LoadUserInterface("CoalDumpInfo");
            CoalDumpInfoEditorControl control = confirm?.GetComponent<CoalDumpInfoEditorControl>();
            control?.SetProperty(relink_line, info, true);
        }
    }

    void OnMouseEnter(){
        isHover = true;
    }

    void OnMouseExit(){
        isHover = false;
    }

    public void CreateEditor() {
        if (relink_line != null) {
            List<Vector3> points = relink_line.points3;

            Transform child = GameObject.Instantiate<Transform>(prefab, relink_line.rectTransform);
            child.transform.position = points[0];
            CoalDumpAreaEditor area_editor = child.gameObject.AddComponent<CoalDumpAreaEditor>();
            area_editor.Line = relink_line;
            area_editor.sign = "start";

            child = GameObject.Instantiate<Transform>(prefab, relink_line.rectTransform);
            child.transform.SetParent(relink_line.rectTransform);
            child.transform.position = points[2];
            area_editor = child.gameObject.AddComponent<CoalDumpAreaEditor>();
            area_editor.Line = relink_line;
            area_editor.sign = "end";
        }
    }
}
