using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScript : MonoBehaviour {
	public int BGM_OP;
	AudioClip Opening;
	// Use this for initialization
	public void Show () {
		DataManager.Instance.BGMPlay (BGM_OP);
	}
	public void Hide () {
		SceneManager.Instance.NewScene (1);
	}
	void Start () {
//		DataManager.Instance
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
