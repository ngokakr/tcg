using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// データベースの取得
/// BGM,SE,画像のロード
/// BGM,SEの再生
/// </summary>

[ExecuteInEditMode]
public class DataManager : SingletonMonoBehaviour<DataManager> {
	public AudioSource BGMSource;
	public AudioSource SESource;
	public List<AudioClip> BGMList;
	public List<AudioClip> SEList;
	void Awake () {
	}
	public IEnumerator BGMFade (int _BgmNum,float _FadeTime = 0.3f) {
		float time = _FadeTime;
		//フェードout
		while (time > 0) {
			time -= Time.deltaTime;
			BGMSource.volume = time / _FadeTime;
			yield return null;
		}

		time = 0;
		BGMPlay (_BgmNum);
		//フェードin
		while (time < _FadeTime) {
			time += Time.deltaTime;
			BGMSource.volume = time / _FadeTime;
			yield return null;
		}
	}
	public void BGMPlay (int _BgmNum) {
		if (_BgmNum == 2) {//メニュー画面
			BGMSource.time = 28.4f;//resort
		}
		BGMSource.clip = BGMList[_BgmNum];
		BGMSource.Play ();
	}
	public void SEPlay (int _SENum) {
		SESource.clip = SEList [_SENum];
		SESource.Play ();
	}
	public  class Scene  {
		public void SceneChange (int _SceneNum){
			SceneManager.Instance.NewScene (_SceneNum);
		}
	}
}

