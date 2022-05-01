using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

public class ReLink : MonoBehaviour
{
    private Tween tween;

    public Text info;


    public delegate void LinkServer();

    public LinkServer server;

    // Start is called before the first frame update
    void Start()
    {
        tween = this.GetComponent<RectTransform>().DOAnchorPosY(-120, 0.5f, true);
        tween.Play();
    }

    public void SetInfo(string data)
    {
        this.info.text = data;
    }

    public void Link() {
        if (server != null) {
            server.Invoke();
        }
        GameObject.DestroyImmediate(this.gameObject);
    }
}
