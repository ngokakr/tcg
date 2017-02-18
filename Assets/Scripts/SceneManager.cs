using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class SceneManager : SingletonMonoBehaviour<SceneManager> {
	public GameObject[] Scenes;
	public Image FadeImg;
	public TitleScript titleScript;
	public float FadeWait;

	// Use this for initialization
	void Awake () {
		titleScript.Show ();
	}

	public void NewScene (int _SceneNum){
		StartCoroutine ("Fade", _SceneNum);
		StartCoroutine( DataManager.Instance.BGMFade(_SceneNum,FadeWait));
	}
	IEnumerator Fade (int _SceneNum) {
		//暗幕in
		float time = 0;
		FadeImg.raycastTarget = true;
		while (time < FadeWait) {
			time += Time.deltaTime;
			Color color = FadeImg.color;
			color.a = time / FadeWait;
			FadeImg.color = color;
			yield return null;
		}
		time = FadeWait;

		//アクティブ切り替え
		for (int i = 0; i < Scenes.Length; i++ ){
			Scenes [i].SetActive(false);
		}
		Scenes [_SceneNum].SetActive (true);
		FadeImg.raycastTarget = false;

		//BGM切り替え
//		DataManager.Instance.pla
		//暗幕out
		while (time > 0) {
			time -= Time.deltaTime;
			Color color = FadeImg.color;
			color.a = time / FadeWait;
			FadeImg.color = color;
			yield return null;
		}
//		FadeImg.CrossFadeAlpha (00000, FadeWait, true);
		yield return null;
	}

	// Update is called once per frame
	void Update () {
		
	}
}
