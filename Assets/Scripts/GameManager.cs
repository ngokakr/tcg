using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GameManager : SingletonMonoBehaviour<GameManager> {
	public PlayerStruct playerData;
	public static string AppVer;

	[System. Serializable]
	public struct PlayerStruct {
		//名前
		public string Name;
		//通貨
		public int Silver;
		public int Gold;
		//
	}
	public class Scene : MonoBehaviour {
		public void SceneChange (int _SceneNum){
			SceneManagerx.Instance.NewScene (_SceneNum);
		}
	}
	public class Audio : MonoBehaviour
	{

		public void BGMFade (int _BgmNum,float _FadeTime = 0.3f) {
			StartCoroutine(DataManager.Instance.BGMFade (_BgmNum, _FadeTime));
		}
		public void BGMPlay (int _BgmNum) {
			DataManager.Instance.BGMPlay (_BgmNum);
		}
		public void SEPlay (int _SENum) {
			DataManager.Instance.SEPlay (_SENum);
		}
	}
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
