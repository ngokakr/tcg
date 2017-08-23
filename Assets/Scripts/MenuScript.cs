using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
public class MenuScript : MonoBehaviour ,IRecieveMessage {
	public CanvasGroup[] Views;
	public GameObject[] Tabs;
	public Vector3 TabPosition;
	int NowViewNum;
	public float FadeSpeed = 0.3f;
	public ArenaScript arenaScript;
	public NewDeckBoxScript newdeckBoxScript;

	public bool SEOn = true;

	// Use this for initialization
	void Start () {
	}
	/// <summary>
	/// 他のシーンから移る時に使用
	/// </summary>
	public void StartShow () {
		TabImgChange(0);
		NowViewNum = 0;
		ViewActive ();
	}
	public void ShowView (int _ViewNum) {
		//SE
		if(SEOn)
			DataManager.Instance.SEPlay(3);

		//変更前
		switch (NowViewNum) {
		case 0:
			{

			}
			break;
		case 2://Deck
			{
				//詳細を隠す
//				deckBoxScript.cardDatail.HideNoSE ();
				//デッキ保存
				DataManager.Instance.Save();
			}
			break;
		}

		//タブ
		TabImgChange(_ViewNum);
		StartCoroutine (IFade (Views [NowViewNum], Views [_ViewNum], FadeSpeed));
		NowViewNum = _ViewNum;
		//フェードアウト後に切り替え
		Invoke (((System.Action)ViewActive).Method.Name,FadeSpeed);//ViewChangeの名前を動的に取得

		switch (NowViewNum) {
		case 0:
			{

			}
			break;
		case 1:
			{
				arenaScript.Refresh ();
			}
			break;

		case 2://Deck
			{
				newdeckBoxScript.Show ();
			}
			break;
		}
	}
	private static IEnumerator IFade(CanvasGroup _old,CanvasGroup _new,float _FadeTime)
	{

		float time = _FadeTime;

		//フェードout
		while (time > 0) {
			time -= Time.deltaTime;
			_old.alpha = time / _FadeTime;
			yield return null;
		}

		time = 0;

		//フェードin
		while (time < _FadeTime) {
			time += Time.deltaTime;
			_new.alpha = time / _FadeTime;
			yield return null;
		}


		yield return null;
	}

	void ViewActive () {
		//非アクティブに
		for (int i = 0; i < Views.Length; i++ ){
			Views [i].gameObject.SetActive(false);
		}
		//アクティブに
		Views [NowViewNum].gameObject.SetActive (true);

	}

	void TabImgChange (int _num) {
		float duration = 0.05f;
		for (int i = 0; i < Tabs.Length; i++ ){
			Tabs[i].transform.GetChild (0).GetComponent<RectTransform> ().DOLocalMove (Vector3.zero,duration,true);
			Tabs [i].transform.GetChild (0).GetComponent<Shadow> ().effectDistance = TabPosition;
		}
		Tabs [_num].transform.GetChild (0).GetComponent<RectTransform> ().DOLocalMove(TabPosition,duration,true);
		Tabs [_num].transform.GetChild (0).GetComponent<Shadow> ().effectDistance = Vector2.zero;

	}
	public void OnRecieve(int _num,int _tag){
		
	}
	// Update is called once per frame
	void Update () {
		
	}
}
