using System;
using UnityEngine;

using UnityEngine.UI;

public class StackCoalControl : MonoBehaviour
{
    public StackCoalInfo info;

    public InputField interior_edge;
    public InputField external_edge;

    public Slider use_config_corner;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetProperty(StackCoalInfo info){
        this.info = info;

        interior_edge.text = info.internal_rotation.ToString();
        external_edge.text = info.external_rotation.ToString();

        use_config_corner.value = Convert.ToInt16(info.use_config_corner);
    }

    public void Confirm() {
        info.internal_rotation = Convert.ToSingle(interior_edge.text);
        info.external_rotation = Convert.ToSingle(external_edge.text);

        info.use_config_corner = Convert.ToBoolean(use_config_corner.value);

        Debug.Log(info.use_config_corner);

        this.Cancel();
    }

    public void Cancel() {
        GameObject.DestroyImmediate(this.gameObject);
    }
}
