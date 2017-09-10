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

	public void OnRegister (bool success) {
		
		if (success) {
			//登録成功
			//ボックスセット
			DataManager.Instance.Password = TestScript.Instance.Password;
		} else {
			
		}
		OnLogin (success);
	}

	public void OnLogin (bool success) {
		alert.OpenClose (false);
		alert = null;
		//エラー表示
		if (!success) {
			alert = AlertView.Make (1, "通信エラー", "エラーが発生しました", new string[]{ "確認" }, gameObject, 0);
			return;
		}
		DataManager.Instance.box = new List<CardData>();
		DataManager.Instance.box.AddRange(TestScript.Instance.cardsJson);
		var pjson = TestScript.Instance.playerJson;
		DataManager.Instance.Coin = pjson.coin;
		DataManager.Instance.Gold = pjson.dia;
		DataManager.Instance.PlayerName = pjson.name;
		DataManager.Instance.uid = pjson.uid;

		DataManager.Instance.Save ();
//		DataManager.Instance.DataAllLoad();
		DataManager.Instance.RefreshData ();

		//画面遷移
		DataManager.Instance.SEPlay (8);
		//		StartCoroutine ("ImagesLoad");
		Resources.UnloadUnusedAssets ();
		SceneManager.Instance.NewScene (1);
	}

	public void OnRecieve(int _num,int _tag){
		if (_tag == 1) {
			
		}
	}


	public void TitleShow () {
//		Resources.Load ("/", typeof(Sprite));
//		Resources.LoadAsync<Sprite> ("");
		DataManager.Instance.BGMPlay (0);
		SceneManager.Instance.ChangeScene (0);

	}

	//プレイ開始
	public void TitleHide () {
		alert = AlertView.Make (0,"通信中...","しばらくお待ちください",new string[]{}, gameObject,0,true);

		int progress =  SaveData.GetInt ("Progress",10);
		if(DataReset)
			progress = 10;
		if (progress == 10) { //初回登録
			DataManager.Instance.box = new List<CardData>();
			DataManager.Instance.decks = new List<List<CardData>> ();

			//通信処理
			TestScript.Instance.Delegate = gameObject;
			TestScript.Instance.Register ();



			var box = DataManager.Instance.box;

			//ボックスにカード追加
			for(int i = 0;i<MaxNum;i++){
				DataManager.Box.AddCard (0,i, 1);
			}
			DataManager.Instance.Save ();

			//			for (int i = 0; i < CardNums.Length; i++ ){
			//				for (int i2 = 0; i2 < 3; i2++ ){
			//					DataManager.Box.AddCard (0, CardNums [i], 1);
			//				}
			//			}

			//0デッキにカード追加
			for (int i = 0; i < box.Count && i<10; i++ ){
				DataManager.Deck.SetCard (0, box [i].Atr,box[i].ID, true);
			}

			SaveData.SetInt ("UseDeck", 0);
			progress++;
			SaveData.SetInt("Progress",progress);

			DataManager.Instance.Save ();
		} else if (progress == 11) {//データ読み込み
			DataManager.Instance.DataAllLoad();
			TestScript.Instance.Delegate = gameObject;
			TestScript.Instance.Login (DataManager.Instance.uid,DataManager.Instance.Password);
			//			AlertView.Make (0,"タイトル","説明",new string[]{"aa","bb"}, gameObject,0);
		}



	}

	void Update () {
		
	}
}