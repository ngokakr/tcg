using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MenuScript : MonoBehaviour {
	public CanvasGroup[] Views;
	int NowViewNum;
	public float FadeSpeed = 0.3f;
	// Use this for initialization
	void Start () {
		
	}
	public void ShowView (int _ViewNum) {
		//SE
		DataManager.Instance.SEPlay(3);
		//古いのをフェードアウト新しいのをフェードイン
		SystemScript.Effect.Fade (this, Views[NowViewNum],Views[_ViewNum],FadeSpeed);
		NowViewNum = _ViewNum;
		//フェードアウト後に切り替え
		Invoke (((System.Action)ViewChange).Method.Name,FadeSpeed);//ViewChangeの名前を動的に取得
	}
	void ViewChange () {
		//非アクティブに
		for (int i = 0; i < Views.Length; i++ ){
			Views [i].gameObject.SetActive(false);
		}
		//アクティブに
		Views [NowViewNum].gameObject.SetActive (true);
	}
	// Update is called once per frame
	void Update () {
		
	}
}
