using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.Events;

public enum MouseControl {
    None,
    LeftClick,
    LeftUp,
    LeftDbclick,
    LeftDrag,
    RightClick,
    RightUp,
    RightDbClick,
    RightDrag,
    WheelClick,
    WheelUp,
    WheelDbClick,
    WheelDrag,
    WheelScroll
}
public class CameraControl : MonoBehaviour
{
    public float origin_distance = 100;

    public float pitch_min = 0;

    public float pitch_max = 80;

    public float distance_min = 10;

    public float distance_max = 200;

    public MouseControl control_state = MouseControl.None;

    public Transform target;

    private float speed = 30;
    
    private Matrix4x4 matrix;

    private Vector3 origin;

    public Vector2 origin_rotation;

    private Vector2 pre_rotation;

    private float pre_distance = 100;

    private Vector3 target_position;

    private bool rotation_control = true;

    private bool is_fllow = false;

    private Transform follow_target;

    private Transform origin_target;

    private float distance;

    private Vector2 rotation;
    // Start is called before the first frame update
    void Awake()
    {
        matrix = new Matrix4x4();

        this.recover_camera();
    }

    public void recover_camera() {
        pre_distance = distance;

        target_position = target.position;

        origin = this.distance * Vector3.forward;

        this.transform.rotation = Quaternion.Euler(origin_rotation.x, origin_rotation.y, 0);

        this.rotation = this.origin_rotation;

        this.distance = this.origin_distance;
    }

    // Update is called once per frame
    void Update(){
        if (!is_fllow)
        {
            if (rotation_control && control_state.Equals(MouseControl.LeftDrag))
            {
                rotation += new Vector2(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X")) * 1;

                rotation.x = Mathf.Clamp(rotation.x, pitch_min, pitch_max);
            }
            else if (control_state.Equals(MouseControl.WheelScroll))
            {
                distance += -1 * Input.GetAxis("Mouse ScrollWheel") * speed;

                distance = Mathf.Clamp(distance, distance_min, distance_max);
            }
            else if (control_state.Equals(MouseControl.WheelDrag))
            {
                //this.target.position += new Vector3(10,0,10);
                Vector3 direction = Vector3.Cross(this.transform.right, Vector3.up).normalized;
                target_position += -1 * this.transform.right * Input.GetAxis("Mouse X") - direction * Input.GetAxis("Mouse Y");
            }
            else if (rotation_control && control_state.Equals(MouseControl.LeftDbclick))
            {
                Coordinate coordinate = this.GetCoalYardPosition("CoalYard|CoalDump");
                if (coordinate != null){
                    Vector3 vertice = coordinate.vertice;
                    target_position = new Vector3(vertice.x, 0, vertice.z);
                    distance = 20;
                }
            }else if (rotation_control && control_state.Equals(MouseControl.RightDbClick))
            {
                string name = this.GetCoalDumpUUID("CoalYard|CoalDump");
            }
        }
        else {
            target_position = this.follow_target.position;
            this.rotation = new Vector2(30, 30);
        }
    }

    private void LateUpdate(){
        pre_rotation = Vector2.Lerp(pre_rotation, rotation, Time.deltaTime * 5);
        pre_distance = Mathf.Lerp(pre_distance, distance, Time.deltaTime * 5);

        target.position = Vector3.Lerp(target.position,target_position, Time.deltaTime * 5);

        origin = this.pre_distance *( -1 * Vector3.forward);

        Quaternion quaternion = Quaternion.Euler(pre_rotation.x, pre_rotation.y, 0);

        matrix.SetTRS(target.position, quaternion, new Vector3(1, 1, 1));

        this.transform.position = matrix.MultiplyPoint(origin);
        this.transform.rotation = quaternion;

        this.transform.LookAt(target);
    }

    void OnGUI(){
        Event mouse_event = Event.current;
        if (EventSystem.current.IsPointerOverGameObject()) {
            control_state = MouseControl.None;
            return;
        }
        if (mouse_event.isMouse && mouse_event.button == 0 && mouse_event.type == EventType.MouseDown && mouse_event.clickCount == 1) {
            control_state = MouseControl.LeftClick;
        } else if (mouse_event.isMouse && mouse_event.button == 0 && mouse_event.type == EventType.MouseUp ) {
            control_state = MouseControl.LeftUp;
        } else if (mouse_event.isMouse && mouse_event.button == 0 && mouse_event.type == EventType.MouseDown && mouse_event.clickCount == 2) {
            control_state = MouseControl.LeftDbclick;
        } else if (mouse_event.isMouse && mouse_event.button == 0 && mouse_event.type == EventType.MouseDrag) {
            control_state = MouseControl.LeftDrag;
        } else if (mouse_event.isMouse && mouse_event.button == 1 && mouse_event.type == EventType.MouseDown && mouse_event.clickCount == 1) {
            control_state = MouseControl.RightClick;
        }else if (mouse_event.isMouse && mouse_event.button == 1 && mouse_event.type == EventType.MouseUp){
            control_state = MouseControl.RightUp;
        }else if (mouse_event.isMouse && mouse_event.button == 1 && mouse_event.type == EventType.MouseDown && mouse_event.clickCount == 2) {
            control_state = MouseControl.RightDbClick;
        } else if (mouse_event.isMouse && mouse_event.button == 1 && mouse_event.type == EventType.MouseDrag) {
            control_state = MouseControl.RightDrag;
        } else if (mouse_event.isMouse && mouse_event.button == 2 && mouse_event.type == EventType.MouseDown && mouse_event.clickCount == 1) {
            control_state = MouseControl.WheelClick;
        } else if (mouse_event.isMouse && mouse_event.button == 2 && mouse_event.type == EventType.MouseDown && mouse_event.clickCount == 2) {
            control_state = MouseControl.WheelDbClick;
        } else if (mouse_event.isMouse && mouse_event.button == 2 && mouse_event.type == EventType.MouseDrag) {
            control_state = MouseControl.WheelDrag;
        } else if (mouse_event.isScrollWheel && mouse_event.type == EventType.ScrollWheel) {
            control_state = MouseControl.WheelScroll;
        }else {
            control_state = MouseControl.None;
        }
    }

    public void SetRotation(Vector2 rotation) {
        this.rotation = rotation;
    }

    public void SetRotationControl(bool station) {
        rotation_control = station;
    }

    public Coordinate GetCoalYardPosition(string layersName){
        /*
        string[] param = layersName.Split('|');

        LayerMask layer = LayerMask.GetMask(param);*/
        
        Coordinate result = null;

        RaycastHit hit;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit)){
            result = new Coordinate(hit.point);
        }

        return result;
    }

    public string GetCoalDumpUUID(string layerName) {
        string result = null;

        RaycastHit hit;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            result = hit.transform.parent.name;
        }

        return result;
    }

    public void SetCameraFollowing(Transform follow_target) {
        is_fllow = true;
        this.follow_target = follow_target;
        this.distance = 30;
        
    }

    public void CancleFollowing() {
        this.target = origin_target;
        is_fllow = false;
    }
}
