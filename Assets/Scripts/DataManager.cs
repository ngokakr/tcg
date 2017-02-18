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
		BGMSource.clip = BGMList[_BgmNum];
		BGMSource.Play ();
		//フェードin
		while (time < _FadeTime) {
			time += Time.deltaTime;
			BGMSource.volume = time / _FadeTime;
			yield return null;
		}
	}
	public void BGMPlay (int _BgmNum) {
		BGMSource.clip = BGMList[_BgmNum];
		BGMSource.Play ();
	}
	public void SEPlay (int _SENum) {
		SESource.clip = SEList [_SENum];
		SESource.Play ();
	}

}

