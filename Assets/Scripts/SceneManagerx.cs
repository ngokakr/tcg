using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using CardParam = SystemScript.CardParam;
public class SceneManagerx : SingletonMonoBehaviour<SceneManagerx> {
	public GameObject[] Scenes;
	public Image FadeImg;
	public float FadeWait;
	public MenuScript menuScript;
	public BattleScript battleScript;
	void Awake () {
	}

	public void NewScene (int _SceneNum){
		StartCoroutine (Fade(_SceneNum));
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
		ChangeScene(_SceneNum);
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
		yield return null;
	}

	public void ChangeScene (int _SceneNum) {
		//アクティブ切り替え
		for (int i = 0; i < Scenes.Length; i++ ){
			Scenes [i].SetActive(false);
		}
		Scenes [_SceneNum].SetActive (true);
		switch (_SceneNum) {
		case 1:
			{
				Scenes [_SceneNum].GetComponent<MenuScript> ().StartShow ();
			}
			break;
		}
	}
	public void ToCPUArenaBattle (XLS_ArenaData.Param param,int difficulty) {
		ChangeScene (2);
//		param.
//		battleScript.gameObject.SetActive (true);
		StartCoroutine( DataManager.Instance.BGMFade(2,FadeWait));
		int useDeck = DataManager.Instance.UseDeck;
		battleScript.BattleStartOffline (new int[]{ 40, param.HP[difficulty] }, new int[]{ 10, 10 }
			,SystemScript.cdTocp( DataManager.Deck.GetDeckData(useDeck)), SystemScript.GetEnemyDeck (param.deck));
//		if (_num == 0) {
//		}
//		if (_num == 1) {
////			battleScript.BattleStartOffline (new int[]{ 40, 40 }, new int[]{ 10, 10 }
////				,SystemScript.cdTocp( DataManager.Deck.GetDeckData (useDeck)), SystemScript.GetEnemyDeck (0));
//			battleScript.BattleStartOffline (new int[]{ 40, 40 }, new int[]{ 10, 10 }
//				,SystemScript.cdTocp( DataManager.Deck.GetDeckData (useDeck)),SystemScript.cdTocp( DataManager.Deck.GetDeckData (useDeck)));
//		}
		
	}

	public void ToTestMatch (int num) {
		ChangeScene (2);
		StartCoroutine( DataManager.Instance.BGMFade(2,FadeWait));
		int useDeck = DataManager.Instance.UseDeck;
		if (num == 5) {
			battleScript.BattleStartOffline (new int[]{ 50, 50 }, new int[]{ 10, 200 }
				, SystemScript.cdTocp (DataManager.Deck.GetDeckData (useDeck)), SystemScript.GetEnemyDeck (num));
		} else {
			battleScript.BattleStartOffline (new int[]{ 50, 50 }, new int[]{ 10, 10 }
			, SystemScript.cdTocp (DataManager.Deck.GetDeckData (useDeck)), SystemScript.GetEnemyDeck (num));
		}
	}
	public void ToBattleOnline (OnlineManager.BattleMode _battleMode,int[] _LPs,int[] _SPs,List<CardParam> _player,List<CardParam> _enemy,string ename,int _initiative,int _seed) {
		ChangeScene (2);
		battleScript.gameObject.SetActive (true);
		StartCoroutine( DataManager.Instance.BGMFade(2,FadeWait));
		battleScript.BattleStartOnline (_LPs, _SPs, _player, _enemy,ename, _initiative,_seed);

	}
	// Update is called once per frame
	void Update () {
		
	}
}
