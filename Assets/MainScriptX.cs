using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScriptX : MonoBehaviour {
	public int x = 0;
	public AudioClip[] bgms;
	public string[] fumen;
	public GameObject[] Scenes;



	public void ButtonNotify (int _num) {
		
		gameObject.GetComponent<AudioSource> ().clip = bgms [_num];
		Scenes [1].SetActive (false);
		Scenes [3].SetActive (true);
		Invoke ("PlayBGM", 3f);
		SystemScriptX.HumenLoad (_num);
	}

	public void PlayBGM () {
		gameObject.GetComponent<AudioSource> ().Play();
	}

	//_numは次に出すシーン番号
	public void ChangeScene (int _num) {
		for (int i = 0; i < Scenes.Length; i++ ){
			Scenes [i].SetActive (false);
		}
		Scenes [_num].SetActive (true);
	}
	// Use this for initialization
	void Start () {

	}
	void Awake () {
		Debug.Log ("不要なスクリプト");
	
	}
	
	// Update is called once per frame
	void Update () {
	}
}
