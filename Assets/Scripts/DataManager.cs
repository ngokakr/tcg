using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardData = SystemScript.CardData;
using CardParam =SystemScript.CardParam;
using LitJson;
/// <summary>
/// データベースの取得
/// BGM,SE,画像のロード
/// BGM,SEの再生
/// </summary>

[ExecuteInEditMode]
public class DataManager : SingletonMonoBehaviour<DataManager> {

	//アプリバージョン
	public string AppVersion;
	public string MinorVersion;

	//セーブデータ
	public string PlayerName;
	public int Coin;
	public int Gold;
	public int Piece;
//	public Dictionary<string,CardData> box = new Dictionary<string, CardData>();
//	public List<Dictionary<string,CardData>> decks = new List<Dictionary<string, CardData>>();
	public List<CardData> box = new List<CardData>();
	public List< List<int>> decks = new List<List<int>> ();
	public List<string> deckName;
	public int UseDeck = 0;
	public int CardDataSystem;//CardDataやCardParamに変更を加える時に使用する。

	//基本データ
	public Color[] AttributeColors;
	public Color[] CardBackColors;
	public Color[] AtcChaDef; 
	public int[] MaxLV;

	//プレハブ
	public AlertView AlertPrefab;

	//アラートようの親
	public Transform[] AlertParents;

	//タッチ無効化用
	public GameObject[] TouchDisableObj;

	//サウンド関連
	public AudioSource BGMSource;
	public AudioSource SESource;
	public AudioSource BattleSESourse;
	public List<AudioClip> BGMList;
	public List<AudioClip> SEList;
	public List<AudioClip> BattleSEList;

	//カードデータ
	public XLS_CardParam xls_CardParam;
	public XLS_Groups xls_Groups;
	public XLS_ParamEffect xls_ParamEffect;
	public XLS_EnemyDeck xls_EnemyDeck;
	public XLS_CardPack xls_CardPack;
	public XLS_ArenaData xls_ArenaData;

	[System.Serializable]
	public struct CardImageParts 
	{
		public Sprite[] BG;
		public Sprite[] Frame;
		public Sprite[] Reality;
		public Sprite[] Role;
		public Sprite[] Power;
		public Sprite[] Attribute;
	}
	public CardImageParts cardParts;

	[System.Serializable]
	public struct FileNames
	{
		public string CardResourseFolder;
		public string CardName;
	}
	public FileNames fileNames;

//	/// <summary>
//	/// 0 = 普通 1 = 元版
//	/// </summary>
//	public CardImageParts GetParts (int _type){
//
//		string ResoursePass = "CardParts";
//		if (_type == 1) {
//			
//		}
//	}
	void Awake () {
	}
	void Update () {
		#if UNITY_EDITOR
		AppVersion = Application.version;
		#endif
	}
	public class Box {
		public static List<CardParam> GetCardParam () {
			return SystemScript.cdTocp (DataManager.Instance.box);
		}
		public static void AddCard (int _atr,int _id,int _lv) {
			var boxDat = DataManager.Instance.box;
			//ユニークなidをつける
			int uid = 0;
			bool unique = true;
			do {
				uid = Random.Range (0, 999999);
				for (int i = 0; i < boxDat.Count; i++) {
					if (boxDat [i].uid == uid) {
						unique = false;
						break;
					}
				}
			} while(unique == false);
			//カードを追加
			var cd = new CardData ().Set (_atr, _id, _lv, 1, uid);
			DataManager.Instance.box.Add (cd);
			DataManager.Instance.Save ();
		}
		public static void LevelUpByCard(CardData _Base,CardData _Material) { //カードによる強化
			var boxDat = DataManager.Instance.box;
			//先に削除
			for (int i = 0; i < boxDat.Count; i++ ){
				if (boxDat [i].uid == _Material.uid) {
					boxDat.RemoveAt (i);
					break;
				}
				if (i == boxDat.Count - 1)
					Debug.LogError ("Not Found Card : "+_Material.uid);
			}
			LevelUp (_Base);
		}
		public static void LevelUp(CardData _Base) { //ポイントによる強化
			var boxDat = DataManager.Instance.box;
			//レベルアップ
			for (int i = 0; i < boxDat.Count; i++ ){
				if (boxDat [i].uid == _Base.uid) {
					CardData cd = boxDat [i];
					cd.LV++;
					boxDat [i] = cd;
					break;
				}
				if (i == boxDat.Count - 1)
					Debug.LogError ("Not Found Card : "+_Base.uid);
			}
			DataManager.Instance.Save ();
		}
		public static CardData GetCardData (int _uid) {
			for (int i = 0; i < DataManager.Instance.box.Count; i++ ){
				CardData cd = DataManager.Instance.box [i];
				if (cd.uid == _uid) {
					return cd;
				}
			}
			Debug.LogError ("Not Found Card : "+_uid);
			return new CardData();
		}
	}
//	public class Box {
//		public static CardData GetCard (int _atr,int _ID) {
//			CardData cd = new CardData ();
//			string key = SystemScript.GetKey (_atr, _ID);
//			cd = DataManager.Instance.box [key];
//			return cd;
//		}
//		public static void AddCard (CardData _cd) {
//			Dictionary<string,CardData> box = DataManager.Instance.box;
//			string key =SystemScript.GetKey (_cd.Atr,_cd.ID);
//			CardData cd = new CardData ();
//			//存在するなら増やし、存在しないなら追加
//			if (box.ContainsKey (key)) {
//				cd = box [key];
//				cd.Count += _cd.Count;
//			} else {
//				cd = _cd;
//			}
//			box [key] = cd;
//			DataManager.Instance.box = box;
//			DataManager.Instance.Save ();
//		}
//		public static void LevelUp (CardData _cd) { //Countのぶん減らす。LV+1
//			Dictionary<string,CardData> box = DataManager.Instance.box;
//			string key =SystemScript.GetKey (_cd.Atr,_cd.ID);
//			CardData cd = box [key];
//			cd.Count -= _cd.Count;
//			cd.LV++;
//			box [key] = cd;
//			DataManager.Instance.box = box;
//			DataManager.Instance.Save ();
//		}
//
//	}
	public class Deck
	{
//		public static CardData GetCard (int _deckNum,int _atr,int _ID) {
//			//デッキに入っていなければ、Boxから取ってきて個数だけ変えて返す。
//
//			string key = SystemScript.GetKey (_atr, _ID);
//			if (DataManager.Instance.decks [_deckNum].ContainsKey (key)) {
//				return DataManager.Instance.decks [_deckNum] [key];
//			} else {
//				CardData cd = DataManager.Box.GetCard (_atr, _ID);
//				cd.Count = 0;
//				return cd;
//			}
//		}
		/// <summary>
		/// デッキに入れる枚数をCountに設定した数にする
		/// </summary>
		public static void SetCard(int _deckNum,int _unique_id,bool _Set) {
			var decks = DataManager.Instance.decks;
			while (decks.Count <= _deckNum){//デッキ枠が足りてなければ作る。
				decks.Add (new List<int>());
			}
			var deck = DataManager.Instance.decks[_deckNum];
			for (int i = 0; i < deck.Count; i++ ){
				if (deck [i] == _unique_id) {
					if (!_Set) {
						deck.RemoveAt (i);
//						DataManager.Instance.Save ();
						return;
					} else {
						Debug.LogError ("すでに入っているカード"+_unique_id);
						return;
					}
				}
			}
			deck.Add (_unique_id);
//			DataManager.Instance.Save ();
		}
		public static List<CardData> GetDeckData (int _deckNum = -1) {
			if (_deckNum == -1)
				_deckNum = DataManager.Instance.UseDeck;
			List<int> uids = DataManager.Instance.decks[_deckNum];
			List<CardData> lcp = new List<CardData> ();
			for (int i = 0; i < uids.Count; i++ ){
				lcp.Add(Box.GetCardData( uids [i]));
			}
			return lcp;
		}
		public static bool ContainCard (int _deckNum,int _uid) {
			//デッキに含まれているかどうか
			return DataManager.Instance.decks[_deckNum].Contains (_uid);
		}
	}
//	public class Deck
//	{
//
//		public static CardData GetCard (int _deckNum,int _atr,int _ID) {
//			//デッキに入っていなければ、Boxから取ってきて個数だけ変えて返す。
//
//			string key = SystemScript.GetKey (_atr, _ID);
//			if (DataManager.Instance.decks [_deckNum].ContainsKey (key)) {
//				return DataManager.Instance.decks [_deckNum] [key];
//			} else {
//				CardData cd = DataManager.Box.GetCard (_atr, _ID);
//				cd.Count = 0;
//				return cd;
//			}
//		}
//		/// <summary>
//		/// デッキに入れる枚数をCountに設定した数にする
//		/// </summary>
//		public static void SetCard(int _deckNum,CardData _cd) {
//			List<Dictionary<string,CardData>> decks = DataManager.Instance.decks;
//			while (decks.Count <= _deckNum){//デッキ枠が足りてなければ作る。
//				decks.Add (new Dictionary<string, CardData> ());
//			}
//			Dictionary<string,CardData> deck = decks [_deckNum];
//			string key =SystemScript.GetKey (_cd.Atr,_cd.ID);;
//			if (_cd.Count ==0) {//0なら消す
//				deck.Remove (key);
//			} else {
//				deck [key] = _cd;
//			}
//			decks [_deckNum] = deck;
//			DataManager.Instance.decks = decks;
//			DataManager.Instance.Save ();
//		}
//		public static List<CardParam> GetDeck () {
//			Dictionary<string,CardData> d = new Dictionary<string, CardData>();
//			d = DataManager.Instance.decks [DataManager.Instance.UseDeck];
//			List<CardParam> lcp = new List<CardParam> ();
//			foreach (string key in d.Keys) {
//				CardParam cp = new CardParam ().Set (d [key]);
//				lcp.Add (cp);
//			}
//			return lcp;
//		}
//	}


//	public CardData CardFromDic (Dictionary<string,CardData> box,int _atr,int _ID){
//		string key = SystemScript.GetKey (_atr, _ID);;
//		if (box.ContainsKey (key)) {
//			return box [key];
//		} else {
//			return new CardData ().Set (_atr, _ID,1, 0);
//		}
//	}

	//初期ロード
	public void DataAllLoad () {
		//ボックス
		string PrefsBox = SaveData.GetString ("Box", "");
		if (PrefsBox != "") {
			List<List<int>> boxList = JsonMapper.ToObject<List<List<int>>> (PrefsBox);
			box = new List<CardData> ();
			for (int i = 0; i < boxList.Count; i++ ){
				List<int> l = boxList [i];
				CardData cd = new CardData ().Set (l[0],l[1],l[2],l[3],l[4]);
				box.Add (cd);
			}

		} else {
			Debug.LogError ("Box is Null");
		}
		//デッキ
		string PrefsDeck = SaveData.GetString ("Deck", "");
		if (PrefsBox != "") {
			decks = JsonMapper.ToObject<List<List<int>>>(PrefsDeck);
		} else {
			Debug.LogError ("Deck is Null");
		}
		//ユーザー名
		PlayerName = SaveData.GetString("Name","");
		Coin = SaveData.GetInt ("Coin", 0);
		Gold = SaveData.GetInt ("Gold", 0);
		UseDeck = SaveData.GetInt ("UseDeck", 0);
	}
	public void Save () {
		//ボックス
		List<List<int>> boxList = new List<List<int>>();
		for (int i = 0; i < box.Count; i++ ){
			CardData cd = box [i];
			List<int> item = new List<int> (){cd.Atr,cd.ID,cd.LV,cd.Count,cd.uid};
			boxList.Add (item);
		}
		SaveData.SetString ("Box" ,JsonMapper.ToJson (boxList));
		//デッキ
		SaveData.SetString("Deck",JsonMapper.ToJson(decks));
		//そのた
		SaveData.SetString("Name",PlayerName);
		SaveData.SetInt ("Coin",Coin);
		SaveData.SetInt ("Gold",Gold);
		SaveData.SetInt ("UseDeck",UseDeck);

		SaveData.Save ();
	}

	public XLS_CardParam.Param GetCardParam (CardData _cd) {
		return xls_CardParam.sheets [_cd.Atr].list [_cd.ID];
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

		if (_BgmNum != 2) {
			BGMPlay (_BgmNum);
			//フェードin
			while (time < _FadeTime) {
				time += Time.deltaTime;
				BGMSource.volume = time / _FadeTime;
				yield return null;
			}
		} else {
			BGMPlay (_BgmNum);
			//いきなり大音量で
			BGMSource.volume = 1f;
		}

	}
	public void BGMPlay (int _BgmNum) {
//		if (_BgmNum == 2) {//メニュー画面
//			BGMSource.time = 28.4f;//resort
//		}
		BGMSource.clip = BGMList[_BgmNum];
		BGMSource.Play ();
	}
	public void BGMStop () {
		BGMSource.Stop ();
	}
	public void SEPlay (int _SENum) {
		SESource.clip = SEList [_SENum];
		SESource.Play ();
	}
	public void BattleSEPlay (int _SENum) {
//		BattleSESourse.clip = BattleSEList [_SENum];
		BattleSESourse.PlayOneShot (BattleSEList [_SENum]);
		
	}
	public void TouchDisable (int _num) {
		TouchDisableObj [_num].SetActive (true);
	}
	public void TouchAble (){
		for (int i = 0; i < TouchDisableObj.Length; i++ ){
			TouchDisableObj [i].SetActive (false);
		}
	}
	public  class Scene  {
		public void SceneChange (int _SceneNum){
			SceneManager.Instance.NewScene (_SceneNum);
		}
	}


}

