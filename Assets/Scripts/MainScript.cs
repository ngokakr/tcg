using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardData = SystemScript.CardData;
using LitJson;
using UnityEngine.Profiling;
using UnityEngine.UI;
//[ExecuteInEditMode]
public class MainScript : SingletonMonoBehaviour<MainScript> ,IRecieveMessage,IRegister,ILogin {
	public Transform MenuView;
	public bool CheckResourse = false;
	public bool DataReset = false;
//	public bool OfflineMode = false;
	public Text profilerText;
	public bool LogEnabled = true;
	public int MaxNum = 0;
	public int[] CardNums;
	AlertView alert;
	public void Awake() {
		#if UNITY_EDITOR

		#else
		Debug.unityLogger.logEnabled = LogEnabled;
		#endif
		Application.targetFrameRate = 30;

		TitleShow ();

	}

	public void OnRegister (string errmsg) {
		
		if (errmsg == "") {
			//登録成功
			//ボックスセット
			DataManager.Instance.Password = TestScript.Instance.Password;
		} else {
			
		}
		OnLogin (errmsg);
	}

	public void OnLogin (string errmsg) {
		alert.OpenClose (false);
		alert = null;
		//エラー表示
		if (errmsg != "") {
			alert = AlertView.Make (1, "エラー",errmsg, new string[]{ "確認" }, gameObject, 0);
			return;
		}
		DataManager.Instance.box = new List<CardData>();
		DataManager.Instance.box.AddRange(TestScript.Instance.cardsJson);
//		for (int i = 0; i < 202; i++ ){
//			DataManager.Instance.box.Add(new CardData().Set(0,i,0,3));
//		}
		var pjson = TestScript.Instance.playerJson;
		DataManager.Instance.Coin = pjson.coin;
		DataManager.Instance.Gold = pjson.dia;
		DataManager.Instance.PlayerName = pjson.name;
		DataManager.Instance.uid = pjson.uid;

		//0デッキにカード追加
		var box = DataManager.Instance.box;
		for (int i = 0; i < box.Count && i<10; i++ ){
			DataManager.Deck.SetCard (0, box [i].Atr,box[i].ID, true);
		}

		SaveData.SetInt ("UseDeck", 0);
		SaveData.SetInt("Progress",11);

		//保存
		DataManager.Instance.Save ();
//		DataManager.Instance.DataAllLoad();
		DataManager.Instance.RefreshData ();



		//画面遷移
		DataManager.Instance.SEPlay (8);
		//		StartCoroutine ("ImagesLoad");
		Resources.UnloadUnusedAssets ();
		SceneManagerx.Instance.NewScene (1);
	}

	public void OnRecieve(int _num,int _tag){
		if (_tag == 1) {
			
		}
	}


	public void TitleShow () {
//		Resources.Load ("/", typeof(Sprite));
//		Resources.LoadAsync<Sprite> ("");
		DataManager.Instance.BGMPlay (0);
		SceneManagerx.Instance.ChangeScene (0);

	}

	//プレイ開始
	public void TitleHide () {
//		OnLogin ("");

		int progress = SaveData.GetInt ("Progress", 10);

		if (DataReset)
			progress = 10;
		if (progress == 10) { //初回登録
			DataManager.Instance.box = new List<CardData> ();
			DataManager.Instance.decks = new List<List<CardData>> ();

			if (!DataManager.Instance. OfflineMode) {
				alert = AlertView.Make (0, "通信中...", "しばらくお待ちください", new string[]{ }, gameObject, 0, true);
				TestScript.Instance.Delegate = gameObject;
				TestScript.Instance.Register ();
				DataManager.Instance.Save ();
			} else {
				DataManager.Instance.Coin = 0;
				DataManager.Instance.Gold = 0;
				DataManager.Instance.PlayerName = "testuser";
				DataManager.Instance.uid = 123456789;
				var box = DataManager.Instance.box;
				//ボックスにカード追加
				for (int i = 0; i < 202; i++ ){
					DataManager.Instance.box.Add(new CardData().Set(0,i,1,3));
				}
				//0デッキにカード追加
				List<int> c = new List<int>(){14,44,49,121,129,19,16,26,6,95};
				for (int i = 0; i < c.Count; i++ ){
					DataManager.Deck.SetCard (0,0,c[i], true);
					DataManager.Deck.SetCard (0,0,c[i], true);
					DataManager.Deck.SetCard (0,0,c[i], true);
				}
				SaveData.SetInt ("UseDeck", 0);
				SaveData.SetInt("Progress",11);

				//保存
				DataManager.Instance.Save ();
				//		DataManager.Instance.DataAllLoad();
				DataManager.Instance.RefreshData ();

				//画面遷移
				DataManager.Instance.SEPlay (8);
				Resources.UnloadUnusedAssets ();
				SceneManagerx.Instance.NewScene (1);
			}
		} else if (progress == 11) {//データ読み込み
			
			DataManager.Instance.DataAllLoad ();
			if (!DataManager.Instance. OfflineMode) {
				TestScript.Instance.Delegate = gameObject;
				TestScript.Instance.Login (DataManager.Instance.uid, DataManager.Instance.Password);
			} else {
				//画面遷移
				DataManager.Instance.SEPlay (8);
				Resources.UnloadUnusedAssets ();
				SceneManagerx.Instance.NewScene (1);
			}
			//			AlertView.Make (0,"タイトル","説明",new string[]{"aa","bb"}, gameObject,0);
		}

		if (!DataManager.Instance.OfflineMode) {
			alert = AlertView.Make (0, "通信中...", "しばらくお待ちください", new string[]{ }, gameObject, 0, true);



		} else {
			
		}

	}



	void Update () {
		
	}
}