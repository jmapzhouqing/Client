using System;
using UnityEngine;

using UnityEngine.UI;

public class StackCoalControl : MonoBehaviour
{
    private StackCoalInfo info;

    public InputField interior_edge;
    public InputField external_edge;

    public Slider use_config_corner;

    private ProgramCommunication communication;
    // Start is called before the first frame update
    void Awake()
    {
        communication = FindObjectOfType<ProgramCommunication>();
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

        communication.SendData("StackCoal Command " + JsonUtility.ToJson(info));

        this.Cancel();
    }

    public void Cancel() {
        GameObject.DestroyImmediate(this.gameObject);
    }
}
