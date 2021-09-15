using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vectrosity;

using System.Linq;

public class CoalDumpInfoEditorControl : MonoBehaviour {

    public CoalDumpInfo info;

    public VectorLine Line;

    public InputField coal_dump_name;

    private CoalDumpEditorControl editor_control;

    private bool is_edit = false;

    // Start is called before the first frame update
    void Awake() {
        editor_control = FindObjectOfType<CoalDumpEditorControl>();
    }

    public void Confirm() {
        info.dump_name = coal_dump_name.text;

        info.vertices = Line.points3.Select(vertice=>new Vector2(vertice.x,vertice.z)).ToList();
        if (!is_edit) {
            editor_control.AddCoalDump(Line, info);
        }

        GameObject.DestroyImmediate(this.gameObject);
    }

    public void Cancle() {
        if(!is_edit && Line != null){
            VectorLine.Destroy(ref Line);
        }
        GameObject.DestroyImmediate(this.gameObject);
    }

    public void SetProperty(VectorLine Line, CoalDumpInfo info,bool is_edit = false) {
        this.Line = Line;
        this.info = info;
        this.is_edit = is_edit;

        this.SetExhibitionProperty(info);
    }

    private void SetExhibitionProperty(CoalDumpInfo info){
        coal_dump_name.text = info.dump_name;
    }
}
