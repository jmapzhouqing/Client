using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TipControl : MonoBehaviour{
    private Tween tween;

    public Text info;
    // Start is called before the first frame update
    void Start(){
        tween = this.GetComponent<RectTransform>().DOAnchorPosY(120, 0.5f, true).OnComplete(delegate{
            StartCoroutine(AutoDestory());
        });
        tween.Play();
    }

    IEnumerator AutoDestory(){
        yield return new WaitForSeconds(2.0f);
        GameObject.DestroyImmediate(this);
    }

    public void SetInfo(string data) {
        this.info.text = data;
    }
    // Update is called once per frame
}
