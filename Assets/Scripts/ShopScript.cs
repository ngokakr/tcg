using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardData = SystemScript.CardData;
using CardParam =SystemScript.CardParam;

[ExecuteInEditMode]
public class ShopScript : MonoBehaviour,IRecieveMessage ,IBuy{
	public bool ShowButton = false;


	[System.Serializable]
	public struct Reality {
		public float Legend;
		public float Gold;
		public float Silver;
	}

	public List<Reality> realities;

	[System.Serializable]
	public struct CardPackStruct {
		public string PackName;
		public List<CardData> LegendCards;
		public List<CardData> GoldCards;
		public List<CardData> SilverCards;
		public List<CardData> BronzeCards;
	}

	public AlertView alert;
	public List< CardPackStruct>  cardPacks;
	int NowSelectPack = 0;
	int useType = 0;
	int useCount = 0;
	int usePoint = 0;
	public int cardsInPack = 5;//1パックあたりの枚数
	public int[] Matomegai = { 10, 3, 1 };//
	public List<CardData> lcd;
	// Use this for initialization
	void Start () {
		
	}

	 void Show () {
		cardPacks = new List<CardPackStruct> ();
		List<XLS_CardPack.Sheet> sheets = DataManager.Instance.xls_CardPack.sheets;
		for (int i = 0; i < sheets.Count; i++) {
			XLS_CardPack.Sheet sheet = sheets [i];
			CardPackStruct cardStr = new CardPackStruct ();
			cardStr.BronzeCards = new List<CardData> ();
			cardStr.SilverCards = new List<CardData> ();
			cardStr.GoldCards = new List<CardData> ();
			cardStr.LegendCards = new List<CardData> ();
			cardStr.PackName = sheet.name;

			for (int i2 = 0; i2 < sheet.list.Count; i2++) {
				XLS_CardPack.Param param = sheet.list [i2];
				CardData cd = new CardData ().Set (param.cardAtr, param.cardID, param.cardLV,1);
				CardParam cp = new CardParam ().Set (cd);
				switch (cp.Rea) {
				case 0:
					cardStr.BronzeCards.Add (cd);
					break;
				case 1:
					cardStr.SilverCards.Add (cd);
					break;
				case 2:
					cardStr.GoldCards.Add (cd);
					break;
				case 3:
					cardStr.LegendCards.Add (cd);
					break;
				}
			}

			cardPacks.Add (cardStr);

		}



	}

	public void BuyPack (int _num) {
		string UsePointName = (_num == 0) ? "Coin" : "Gold";
		var reas = realities [_num];
		List<string> ls = new List<string> ();

		for (int i = 0; i < Matomegai.Length; i++ ){
			ls.Add(string.Format("{0}パック購入 : {1}{2}",Matomegai [i],Matomegai[i]*300,UsePointName)) ;
		}
		AlertView.Make (_num,UsePointName+"で購入","購入数を選択してください・",ls.ToArray(), gameObject,1);
	}
	public void OnRecieve(int _num,int _tag){
		//tag: 0=Coin 1= Gold _num: 選択肢

		if (_num == -1 || _tag == -1)
			return;
		Debug.Log (_tag + ":" + _num);
		useType = _tag;
		useCount = Matomegai [_num];
		usePoint = useCount * 300;
		GetPacks ();
	}
	public void GetPacks () {
		string UsePointName = (useType == 0) ? "coin" : "dia";
		//通信処理
		TestScript.Instance.Delegate = gameObject;
		TestScript.Instance.Buy (UsePointName,useCount);

		//アラート
		alert = AlertView.Make (-1,"通信中...","しばらくお待ちください",new string[]{}, gameObject,1,true);

	}

	int toInt (object obj){
		return System.Convert.ToInt32 (obj);
	}

	//購入後処理
	public void OnBuy(List<object> cid,string errmsg) {
		//アラートを消す
		alert.OpenClose (false);

		if (cid == null || cid.Count == 0) {
			//エラー

			alert = AlertView.Make (-1, "エラー",errmsg, new string[]{ "確認" }, gameObject, 1);
		} else {
			//成功

			//カード追加
			lcd = new List<CardData>();
			for (int i = 0; i < cid.Count; i++ ){
				int id = toInt (cid [i]);
				DataManager.Box.AddCard(0,id,1);
				lcd.Add (new CardData ().Set (0, id, 1, 1));
			}
			//ポイント消費
			DataManager.ChangePoint(useType,-usePoint);
			DataManager.Instance.RefreshData ();
			DataManager.Instance.Save ();


			var paramDatas = DataManager.Instance.xls_CardParam;
			List<string> datas = new List<string>();
			for (int i = 0; i < lcd.Count; i++ ){
				CardData cd = lcd [i];
				var param = DataManager.Instance.xls_CardParam.sheets [cd.Atr].list [cd.ID];
				datas.Add (SystemScript.GetReality(param.reality,true)+ " " + param.name);
			}
			AlertView.Make (-1,"入手カード","パックを購入しました",datas.ToArray(), gameObject,1);
		}
	}

	//オフライン用
//	public void GetPacks (int _useType,int _Count) {
//		//ポイント消費
//
//		//パック入手枚数
//		int cardsCount = _Count * cardsInPack;
//
//		//生成するカード
//		lcd = new List<CardData> ();
//
//
//		//listを一旦まとめる
//		List<List<CardData>> packsDatas
//		= new List<List<CardData>>{
//			cardPacks[NowSelectPack].LegendCards
//			,cardPacks[NowSelectPack].GoldCards
//			,cardPacks[NowSelectPack].SilverCards
//			,cardPacks[NowSelectPack].BronzeCards
//		};
//
//		//Coin,Gold買いによって変わる。
//		Reality reality = realities [_useType];
//		List<float> realityDatas = new List<float>() { reality.Legend, reality.Gold, reality.Silver };
//		//生成処理
//		for (int i = 0; i < cardsCount; i++ ){
//			//レアリティ選択
//			float temp = Random.Range (0, 100);
//			int SelectedReality = -1;
//
//			for (int i2 = 0; i2 < realityDatas.Count; i2++ ){
//				SelectedReality = i2+1;
//				temp -= realityDatas [i2];
//				if (temp <= 0 || (_useType == 1 &&  i % cardsInPack == cardsInPack-1 && i2 == realityDatas.Count-1)) {//ゴールドパック&5で割って4余る枚目&最低+1レアの時
//					SelectedReality = i2;
//					break;
//				}
//			}
//
//			//カード最終決定
//			List<CardData> SelectedRealityCards = packsDatas[SelectedReality];
//			CardData cd = SelectedRealityCards [Random.Range (0, SelectedRealityCards.Count)];
//			DataManager.Box.AddCard (cd.Atr, cd.ID, cd.LV);
//			lcd.Add (cd);
//
//		}
//		DataManager.Instance.Save ();
//		//入手カード表示
//		var paramDatas = DataManager.Instance.xls_CardParam;
//		List<string> datas = new List<string>();
//		for (int i = 0; i < lcd.Count; i++ ){
//			CardData cd = lcd [i];
//			var param = DataManager.Instance.xls_CardParam.sheets [cd.Atr].list [cd.ID];
//			datas.Add (SystemScript.GetReality(param.reality,true)+ " " + param.name);
//		}
//		AlertView.Make (-1,"入手カード","パックを購入しました",datas.ToArray(), gameObject,1);
//
//	}
	void Gacha (int _packNum,int _cardCount,int _bulkBuying) {
		
	}
	// Update is called once per frame
	public void Update () {
		if (ShowButton = true) {
			ShowButton = false;
			Show ();
		}
	}
}
