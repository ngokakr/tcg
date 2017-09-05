using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using LitJson;
using UnityEngine;
using UnityEngine.UI;
using BestHTTP;
using Carddata = SystemScript.CardData;

public class TestScript : SingletonMonoBehaviour<TestScript> {
	public bool Reset = false;
	public bool TestTrigger = false;
	public bool RegistTrigger = false;
	public bool LoginTrigger = false;
	public bool BuyTrigger = false;
	public string UserName = "";
	public string Password = "";

	public string URL;
	public string Path;

	[Serializable]
	public struct ReceiveData
	{
		public int code;
		public string type;
	}

	[Serializable]
	public struct PlayerJson
	{
		public string name;
		public int uid;
		public int coin;
		public int dia;
		public int rate;
		public int loginDays;
	}
	public PlayerJson playerJson;

//	[Serializable]
//	public struct CardsJson
//	{
//		public List<int> carddata;
//	}
	public List<Carddata> cardsJson;
//	List<List<int>> CardsJson;


//	[MessagePack]

	[System.Serializable]
	public class LoginData
	{
//		public int state;//200なら
		public string name;
		public int uid;
		public int coin;
		public int dia;
		public int loginDays;
		public int rate;

		public string noticeURL;

		public List<List<int>> boxdata;//[カード][id,lv,count]
		public List<List<List<int>>> deckdata;//[デッキno][カード][カードid,count]
		public List<PresentData> presents;//プレゼントデータ
		public List<FriendData> frends;//フレンドデータ
	}
	public LoginData loginData;

	[System.Serializable]
	public class PresentData
	{
		public int id;
		public int compen;//0=プレゼントデータ 1=補填データ
		public string title;
		public string detail;
		public string kind;//
		public int point;
//		public string msg;
		public bool received;
		public string startt;
		public string endt;
	}

	[System.Serializable]
	public class FriendData//
	{
		public int uid;
		public int icon;
		public int status;//0=繋がりなし 1=こちらがリクエスト申請中 2=リクエストを受け取り中 3=フレンド
		public string name;
		public int rate;
		public string lastLogin;
	}

//
//	public void Awake (){
//		loginData.name = "akira";
//		loginData.uid = 235240503;
//		loginData.coin = 3500;
//		loginData.dia = 60;
//		loginData.loginDays = 4;
//		loginData.rate = 1580;
//
//		var box = DataManager.Instance.box;
//		loginData.boxdata = new List<List<int>> ();
//		for (int i = 0; i < box.Count; i++ ){
//			var card = box [i];
//			int id = card.ID;
//			int lv = card.LV;
//			int count = card.Count;
//			loginData.boxdata.Add (new List<int> (){ id, lv ,count});
//		}
//
//		var decks = DataManager.Instance.decks;
//		loginData.deckdata = new List<List<List<int>>> ();
//		for (int i = 0; i < decks.Count; i++ ){
//			List<List<int>> result = new List<List<int>> ();
//			List<SystemScript.CardData> deck = decks [i];
//			for (int i2 = 0; i2 < deck.Count; i2++ ){
//				var card = deck [i2];
//				List<int> cardInts = new List<int> ();
//				cardInts.Add (card.ID);
//				cardInts.Add (card.Count);
//				result.Add (cardInts);
//			}
//			loginData.deckdata.Add (result);
//		}
//
//		loginData.presents = new List<PresentData> ();
//		var present = new PresentData ();
//		present.title = "第二回公式戦開催祝！";
//		present.compen = 0;
//		present.detail = "お祝いとして1000コインプレゼント!";
//		present.id = 2;
//		present.kind = "coin";
//		present.point = 1000;
//		present.received = false;
//		DateTime dt = new DateTime (2017,9,1,15,45,0,DateTimeKind.Local);
//		present.startt = dt.ToString ();
//
////		present.start = 
//		loginData.presents.Add (present);
//
//
////		var x = JsonMapper.ToJson (loginData);
////		Debug.Log (x);
//
//
////
////
////		loginData.boxdata = new List<List<int>> ();
////		loginData.boxdata.Add (new List<int> (){ 3, 5 });
////		List<List<int>> d = new List<List<int>> ();
////		d.Add (new List<int> (){ 6, 2, 7, 9 });
////		d.Add (new List<int> (){ 6, 2, 7, 9 });
////		loginData.deckdata = new List<List<List<int>>> ();
////		loginData.deckdata.Add (new List<List<int>> (){ new List<int> (){ 3, 5 }, new List<int> (){ 3, 5 } });
//		var x = JsonMapper.ToJson (loginData);
//		Debug.Log (x);
////		HTTPRequest request = new HTTPRequest (new Uri (URL+Path), HTTPMethods.Post, OnRequestFinished);
////		request.AddField ("name", "akr");
//////		request.AddField ("password",StringUtils.GeneratePassword(30));
////		request.AddField ("password",Password);
////		request.Send ();
//	}
	void Update () {
		if(Reset){
			Reset = false;
			Awake ();
		}
		if (TestTrigger) {
			TestTrigger = false;
			Test ();
		}
		if (RegistTrigger) {
			RegistTrigger = false;
			Regist();
		}

		if (LoginTrigger) {
			LoginTrigger = false;
			Login();
		}

		if (BuyTrigger) {
			BuyTrigger = false;
			Buy();
		}
	}

	void Test () {
		HTTPRequest request = new HTTPRequest (new Uri (URL), HTTPMethods.Get, OnRequestFinished);
		request.Send ();
	}

	public void Regist () {
		HTTPRequest request = new HTTPRequest (new Uri (URL+"users/"), HTTPMethods.Post, OnRegist);
		request.AddField ("name", UserName);
		Password = StringUtils.GeneratePassword (30);
		request.AddField ("password",Password);
		request.Send ();
	}

	public void Login () {
		HTTPRequest request = new HTTPRequest (new Uri (URL+"login/"), HTTPMethods.Post, OnLogin);
		request.AddField ("uid", playerJson.uid+"");
		request.AddField ("password",Password);
		request.Send ();
	}

	public void Buy () {
		HTTPRequest request = new HTTPRequest (new Uri (URL+"shop/"), HTTPMethods.Post, OnRegist);
		request.AddField ("kind", "coin");
		request.AddField ("count","1");
		request.Send ();
	}



	void OnRegist (HTTPRequest request, HTTPResponse response) {
		Debug.Log("Request Finished! Text received: " + response.DataAsText);

		var main = GetDic (response.DataAsText);
		int status = GetStatus (main);
		if (status == 200 && DataType(main) == "login") {
			//登録成功

			SetLoginData(main);
		} else {
			//登録失敗
		}


	}

	void OnLogin (HTTPRequest request, HTTPResponse response) {
		Debug.Log("Request Finished! Text received: " + response.DataAsText);

		var main = GetDic (response.DataAsText);
		int status = GetStatus (main);
		if (status == 200 && DataType (main) == "login") {
			SetLoginData (main);
		}
	}

	void OnBuy (HTTPRequest request, HTTPResponse response) {
		Debug.Log("Request Finished! Text received: " + response.DataAsText);
	}

	void SetLoginData (Dictionary<string,object> main) {
		
		var data =  GetDic((string)main["data"]);

		//それぞれのjsonを取得
		var pjson = ((Dictionary<string,object>)data ["pjson"])["data"];
		var cjson = ((Dictionary<string,object>)data ["cjson"])["data"];

		//プレイヤーjson¥
		playerJson = JsonMapper.ToObject<PlayerJson> ((string)pjson);

		//dictionary変換 -> 配列変換
		List<object> x =  (List<object>)GetDic((string)cjson)["cd"];
		cardsJson = new List<Carddata> ();
		for (int i = 0; i < x.Count; i++ ){
			List<object> y = (List<object>)x [i];
			Carddata cd = new Carddata ().Set (0, toInt(y[0]),toInt(y[1]),toInt(y[2]));
			cardsJson.Add (cd);
		}


//		Debug.Log( GetDic((string)cjson)["cd"]);
		//それぞれのdataをDictionaryで取得
//		var pdata = GetDic ((string)pjson ["data"]);
//		var cdata = GetDic ((string)cjson ["data"]);

		//jsonからclassをつくる
//		cardsJson = JsonMapper.ToObject<List<CardsJson>> ((string) GetDic((string)cjson)["cd"]);
		//pjsonから
//
//
//		var logind = GetDic ((string)data);
//
//		UserName = (string)logind ["name"];
//		playerJson.coin = Convert.ToInt32( logind ["coin"]);
//		playerJson.uid= Convert.ToInt32( logind ["uid"]);
//		playerJson.dia = Convert.ToInt32 (logind ["dia"]);
//		playerJson.loginDays = Convert.ToInt32 (logind ["loginDays"]);
//		playerJson.rate = Convert.ToInt32 (logind ["rate"]);
	}

	int toInt (object obj){
		return System.Convert.ToInt32 (obj);
	}


	string DataType (Dictionary<string,object> main) {
		return (string)((Dictionary<string,object>)main ["meta"])["type"];
	}

	Dictionary<string,object> GetDic (string str) {
		return Json.Deserialize (str) as Dictionary<string,object>;
	}

	Dictionary<string,object> GetDic (Dictionary<string,object> Data,string key){
		return Data[key] as Dictionary<string,object>;
	}

	int GetStatus (Dictionary<string,object> main){
		return int.Parse( (string)GetDic (main, "meta")["code"]);
	}

	List<object> GetKinds (Dictionary<string,object> main) {
		var data = GetDic (main, "data");
		return data ["kind"] as List<object>;
	}

	List<object> GetValues (Dictionary<string,object> main) {
		var data = GetDic (main, "data");
		return data ["value"] as List<object>;
	}

	void OnRequestFinished(HTTPRequest request, HTTPResponse response) {
		
		Debug.Log("Request Finished! Text received: " + response.DataAsText);
		var main = Json.Deserialize (response.DataAsText) as Dictionary<string,object>;

		var meta = main ["meta"] as Dictionary<string,object>;
		int code = int.Parse( (string)meta ["code"]);

		var data = main["data"] as Dictionary<string,object>;
		var kind = data ["kind"] as List<object>;
		var value = data["value"] as List<object>;



		Debug.Log (kind [0]);
//		Debug.Log (code + kind.Count + value.Count + "");

	}
	public static class StringUtils
	{
		private const string PASSWORD_CHARS = 
			"0123456789abcdefghijklmnopqrstuvwxyz";

		public static string GeneratePassword( int length )
		{
			var sb  = new System.Text.StringBuilder( length );
			var r   = new System.Random();

			for ( int i = 0; i < length; i++ )
			{
				int     pos = r.Next( PASSWORD_CHARS.Length );
				char    c   = PASSWORD_CHARS[ pos ];
				sb.Append( c );
			}

			return sb.ToString();
		}
	}





}
