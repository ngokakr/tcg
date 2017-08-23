using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardData = SystemScript.CardData;
using LitJson;
using UnityEngine.Profiling;
using UnityEngine.UI;
//[ExecuteInEditMode]
public class MainScript : SingletonMonoBehaviour<MainScript> ,IRecieveMessage {
	public Transform MenuView;
	public bool CheckResourse = false;
	public bool DataReset = false;
	public Text profilerText;
	public bool LogEnabled = true;
	public int MaxNum = 0;
	public int[] CardNums;
	public void Awake() {
		#if UNITY_EDITOR

		#else
		Debug.logger.logEnabled = LogEnabled;
		#endif
		Application.targetFrameRate = 30;
		int progress =  SaveData.GetInt ("Progress",10);
		if(DataReset)
		progress = 10;
		if (progress == 10) { //空っぽのボックス
			DataManager.Instance.box = new List<CardData>();
			DataManager.Instance.decks = new List<List<int>> ();

			var box = DataManager.Instance.box;
//			int[] Counts = {1,3,3,3,2
//				,2,3,3,2,3,3,1,1};
//			//ボックスにカード追加
//			for (int i = 0; i < Counts.Length; i++ ){
//				for (int i2 = 0; i2 < Counts[i]; i2++ ){
//					DataManager.Box.AddCard(0,i,1);
//				}
//			}

			//ボックスにカード追加
			for(int i = 0;i<MaxNum;i++){
				DataManager.Box.AddCard (0,i, 1);
			}
//			for (int i = 0; i < CardNums.Length; i++ ){
//				for (int i2 = 0; i2 < 3; i2++ ){
//					DataManager.Box.AddCard (0, CardNums [i], 1);
//				}
//			}

			//0デッキにカード追加
			for (int i = 0; i < box.Count && i<30; i++ ){
				DataManager.Deck.SetCard (0, box [i].uid, true);
			}
			SaveData.SetInt ("UseDeck", 0);
			progress++;
			SaveData.SetInt("Progress",progress);

			DataManager.Instance.Save ();
		}

		if (progress == 11) {//データ読み込み
//			AlertView.Make (0,"タイトル","説明",new string[]{"aa","bb"}, gameObject,0);
		}

		DataManager.Instance.DataAllLoad();
		TitleShow ();
	}




	public void TitleShow () {
//		Resources.Load ("/", typeof(Sprite));
//		Resources.LoadAsync<Sprite> ("");
		DataManager.Instance.BGMPlay (0);
		SceneManager.Instance.ChangeScene (0);

	}
//	IEnumerator ImagesLoad () {
//		
//		for (int i = 0; i < 200; i++ ){
//			yield return StartCoroutine (LoadAsyncSpriteCoroutine (i, "CardImage/0/cd_" + i));
//		}
//		for (int i = 0; i < 200; i++ ){
//			yield return StartCoroutine (LoadAsyncSpriteCoroutine (i, "CardImage/0/cd_" + i));
//		}
//	}
//
//	IEnumerator LoadAsyncSpriteCoroutine (int num,string filePath) {
//		ResourceRequest resReq = Resources.LoadAsync (filePath);
//		Debug.Log ("Go");
//		while (resReq.isDone == false) {
//			Debug.Log ("No."+num+ " Loading progress:" + resReq.progress.ToString ());
//			yield return 0;
//		}
//
//	}

	public void TitleHide () {
		DataManager.Instance.SEPlay (8);
//		StartCoroutine ("ImagesLoad");
		Resources.UnloadUnusedAssets ();
		SceneManager.Instance.NewScene (1);
	}
	public void OnRecieve(int _num,int _tag){
		

	}

	void Update () {
//		uint monoUsed = Profiler.GetMonoUsedSize ();
//		uint monoSize = Profiler.GetMonoHeapSize ();
//		uint totalUsed = Profiler.GetTotalAllocatedMemory (); // == Profiler.usedHeapSize
//		uint totalSize = Profiler.GetTotalReservedMemory ();
//		string text = string.Format(
//			"mono:{0}/{1} kb({2:f1}%)\n" +
//			"total:{3}/{4} kb({5:f1}%)\n", 
//			monoUsed/1024, monoSize/1024, 100.0*monoUsed/monoSize, 
//			totalUsed/1024, totalSize/1024, 100.0*totalUsed/totalSize);
//		profilerText.text = text;
//		if (CheckResourse) {
//			
//			CheckResourse = false;
//			Debug.Log (Resources.FindObjectsOfTypeAll<Texture>().Length);
//		}
	}
}