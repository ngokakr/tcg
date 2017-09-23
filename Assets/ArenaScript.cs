using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArenaScript : MonoBehaviour,IRecieveMessage{
	public List<GameObject> RankObjects;
	public List<List<int>> Datas;//[Eランク=0][Boss=0 + ランカーナンバー]
	public string[] RankName = {"E","D","C","B","A","S","SS"};
	string[] Difficulty = { "Nomal", "Hard", "Extreme" };
	public int[] RankersCount;
	public int MaxRank = 0;
	public int MaxOrder = 0;
	public int stars = 0;
	public float FixedHeight = 0f;
	public float CellHeight = 0f;
	public float Space = 0f;
	int selectRank = 0;
	int selectOrder = 0;
	XLS_ArenaData.Param selectRanker;

	void Awake () {
		//E~Aランカー BOSS~9ランカーまで。
		Datas = new List<List<int>> ();
		for (int i = 0; i < 5; i++ ){
			List<int> rankData = new List<int> ();
			for (int i2 = 0; i2 < 10; i2++ ){
				rankData.Add (0);
			}
			Datas.Add (rankData);
		}
	}

	public void Refresh () {
		Awake(); //とりあえず

		var xls_arenaParams = DataManager.Instance.xls_ArenaData.sheets[0].list;
		//現在のスター数とランクを取得する。
		stars = 0;
		MaxRank = 0;
		MaxOrder = 9;
		//セーブデータ検索ループ
		for (int i = 0; i < Datas.Count; i++ ){
			//Eランクから
			List<int> rankData = Datas[i];
			//スターの合計を計算
			for (int i2 = 0; i2<rankData.Count; i2++ ){
				stars += rankData [i2];
				if (i2 == 0 && 1 <= rankData [i2]) {//ボスを倒していたら最大ランクを+1
					MaxRank++;
				}
			}
		}

		//ランクごとにアクティブ切り替え
		for (int i = 0; i < RankObjects.Count; i++ ){
			if (i <= MaxRank) {
				RankObjects [i].SetActive (true);
			} else {
				RankObjects [i].SetActive (false);
			}
		}

		//マスターデータループ
		int x;
		MaxOrder = 10;
		for (x = 0; x < xls_arenaParams.Count; x++ ){
			var param = xls_arenaParams [x];
			Transform node = RankObjects[param.rank].transform.Find("Ranker").Find(param.order.ToString());
			if (param.rank <= MaxRank && param.needPoint <= stars && param.rank <= MaxRank) {//ランクに到達&必要なポイントを超えている
				//存在させる
				node.gameObject.SetActive (true);
				//ボタン化させる
				if (node.GetComponent<Button>() == null) {//通知設定
					var btn2 = node.gameObject.AddComponent<Button>();
					btn2.onClick.AddListener(()=>ButtonNotify(param.rank,param.order));
				}

				if(param.order ==0)
					node.Find ("Name").GetComponent<Text> ().text = string.Format ("BOSS:{0}",param.name);
				else
					node.Find ("Name").GetComponent<Text> ().text = string.Format ("{0}-{1}:{2}",RankName[param.rank], param.order, param.name);
				node.Find ("Data").GetComponent<Text> ().text = string.Format ("♥×{0}:",param.stamina);
				for (int i2 = 0; i2 < 3; i2++ ){
					if (i2 < Datas [param.rank] [param.order]) {
						node.Find ("Data").GetComponent<Text> ().text += "★";

					} else {
						node.Find ("Data").GetComponent<Text> ().text += "☆";
					}
				}
				if (param.order == 0)
					MaxOrder = 10;
				else
					MaxOrder--;
			} else {
				node.gameObject.SetActive (false);
			}
		}

		//攻略済のランクは非表示
		for (int i = 0; i < MaxRank; i++ ){
			var xy = RankObjects [i].GetComponent<RectTransform> ().sizeDelta;
			RankObjects [i].GetComponent<RectTransform> ().sizeDelta = new Vector2 (xy.x,CalcHeight(RankersCount[i]));
		}

		//現在のランク
		var xy2 = RankObjects [MaxRank].GetComponent<RectTransform> ().sizeDelta;
		RankObjects [MaxRank].GetComponent<RectTransform> ().sizeDelta = new Vector2 (xy2.x,CalcHeight(RankersCount[MaxRank]-MaxOrder));


	}

	//アリーナ表示枠のサイズ計算
	float CalcHeight (int CellCount) {
		return  FixedHeight + CellHeight * CellCount + Space * (CellCount-1);
	}

	//セルをタップしたときの処理
	public void ButtonNotify (int rank,int order) {
		selectRank = rank;
		selectOrder = order;
		var paramList = DataManager.Instance.xls_ArenaData.sheets[0].list;
		for (int i = 0; i <paramList .Count ; i++ ){
			var param = paramList [i];
			if (param.rank == rank && param.order == order) {
				selectRanker = param;
				break;
			}
		}
		List<string> selection = new List<string> ();
		int nowProgress = Datas [rank] [order];
		for (int i = 0; i < 3; i++ ){
			if(i <= nowProgress)
				selection.Add (Difficulty [i]);
		}
		AlertView.Make (0,"難易度選択",string.Format("vs {0}",selectRanker.name),selection.ToArray(), gameObject,1);
	}

	//アラートタップ処理
	public void OnRecieve(int _num,int _tag){
//		Debug.Log (_num);
		if (_num == 0) {
			SceneManagerx.Instance.ToCPUArenaBattle (selectRanker,_num);
			Debug.Log ("対戦開始");
		}
	}

}
