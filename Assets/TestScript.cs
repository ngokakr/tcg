using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using LitJson;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using BestHTTP;

using Carddata = SystemScript.CardData;

public interface IRegister :  IEventSystemHandler {
	void OnRegister(string errmsg);//TestScriptからデータをとる。
}
public interface ILogin :  IEventSystemHandler {
	void OnLogin(string errmsg);//TestScriptからデータをとる。
}
public interface IBuy :  IEventSystemHandler {
	void OnBuy(List<object> cid,string errmsg);//TestScriptからデータをとる。
}
public interface ILvup :  IEventSystemHandler {
	void OnLvup(int cid,int lv,string errmsg);//TestScriptからデータをとる。
}

public class TestScript : SingletonMonoBehaviour<TestScript> {
	public bool Reset = false;
	public bool TestTrigger = false;
	public bool RegistTrigger = false;
	public bool LoginTrigger = false;
	public bool BuyTrigger = false;
	public bool DownloadTrigger = false;
	public string UserName = "";
	public string Password = "";
	public GameObject Delegate;

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
			Register();
		}

		if (LoginTrigger) {
			LoginTrigger = false;
			Login(playerJson.uid,Password);
		}

		if (BuyTrigger) {
			BuyTrigger = false;
			Buy("coin",1);
		}

		if (DownloadTrigger) {
			DownloadTrigger = false;
			Download ();
		}
	}

	void Test () {
		HTTPRequest request = new HTTPRequest (new Uri (URL), HTTPMethods.Get, OnRequestFinished);
		request.Send ();
	}

	public void Register () {
		if (DataManager.Instance.OfflineMode)
			return;
		Debug.Log ("Register");
		HTTPRequest request = new HTTPRequest (new Uri (URL+"users/"), HTTPMethods.Post, OnRegister);
		request.AddField ("name", UserName);
		Password = StringUtils.GeneratePassword (30);
		request.AddField ("password",Password);
		request.Send ();
	}

	public void Login (int uid,string password) {
		if (DataManager.Instance.OfflineMode)
			return;
		Debug.Log ("Login");
		HTTPRequest request = new HTTPRequest (new Uri (URL+"login/"), HTTPMethods.Post, OnLogin);
		request.AddField ("uid", uid+"");
		request.AddField ("password",password);
		request.Send ();
	}

	public void Buy (string kind,int count) {
		if (DataManager.Instance.OfflineMode)
			return;
		HTTPRequest request = new HTTPRequest (new Uri (URL+"shop/"), HTTPMethods.Post, OnBuy);
		request.AddField ("kind", kind);
		request.AddField ("count",count.ToString());
		request.Send ();
	}

	public void Lvup (int cid,int to_lv) {
		if (DataManager.Instance.OfflineMode)
			return;
		HTTPRequest request = new HTTPRequest (new Uri (URL+"lvup/"), HTTPMethods.Post, OnLvup);
		request.AddField ("cid", cid.ToString());
		request.AddField ("to_lv",to_lv.ToString());
		request.Send ();
	}

	public void Download () {
		var request = new HTTPRequest (new Uri ("https://drive.google.com/open?id=0B463Jpbf6-RIa2w5ejNiY0Zxbzg"), (req, resp) => {
			List<byte[]> fragments = resp.GetStreamedFragments ();

			// Write out the downloaded data to a file:
			using (FileStream fs = new FileStream (Application.persistentDataPath + "/data", FileMode.Append))
				foreach (byte[] data in fragments)
					fs.Write (data, 0, data.Length);
			if (resp.IsStreamingFinished)
				Debug.Log ("Download finished!");
		});
		request.UseStreaming = true;
		request.StreamFragmentSize = 1 * 1024 * 1024; // 1MB
		request.DisableCache = true;
		request.Send ();

	}


	string GetErrorMessage (HTTPRequest request) {
		var state = request.State;
		Debug.Log (request.State);
		switch (state) {
		case HTTPRequestStates.Aborted:
			return "リクエストが中断されました";
			break;
		case HTTPRequestStates.ConnectionTimedOut:
		case HTTPRequestStates.TimedOut:
			return "要求がタイムアウトしました";
			break;
		case HTTPRequestStates.Error:
			return "通信エラーが発生しました";
			break;
		}
		return "";
	}

	void OnRegister (HTTPRequest request, HTTPResponse response) {
		//エラー
		string errorMessage = GetErrorMessage (request);
		if (errorMessage == "") {
			try{
			Debug.Log ("Request Finished! Text received: " + response.DataAsText);
			var main = GetDic (response.DataAsText);
			int status = GetStatus (main);

			if (status == 200) {
				//登録成功
				SetLoginData (main);
			} else {
				//エラー
				errorMessage = "サーバーエラーが発生しました";
			}
			} catch (Exception e){
				errorMessage = "サーバーエラーが発生しました";
			}
		}
		ExecuteEvents.Execute<IRegister>(
			target: Delegate, // 呼び出す対象のオブジェクト
			eventData: null,  // イベントデータ（モジュール等の情報）
			functor: (recieveTarget,y)=>recieveTarget.OnRegister(errorMessage)); // 操作
	}

	void OnLogin (HTTPRequest request, HTTPResponse response) {
		string errorMessage = GetErrorMessage (request);
		if (errorMessage == "") {
			try{
				Debug.Log ("Request Finished! Text received: " + response.DataAsText);
				var main = GetDic (response.DataAsText);
				int status = GetStatus (main);

				if (status == 200) {
					//ログイン成功
					SetLoginData (main);
				} else {
					//エラー
					errorMessage = "サーバーエラーが発生しました";
				}
			} catch (Exception e ){
				errorMessage = "サーバーエラーが発生しました";
			}
		}
		ExecuteEvents.Execute<ILogin> (
			target: Delegate, // 呼び出す対象のオブジェクト
			eventData: null,  // イベントデータ（モジュール等の情報）
			functor: (recieveTarget, y) => recieveTarget.OnLogin (errorMessage)); // 操作
	}

	void OnBuy (HTTPRequest request, HTTPResponse response) {
		string errorMessage = GetErrorMessage (request);
		List<object> buyCards = null;
		if (errorMessage == "") {
			try{
				Debug.Log ("Request Finished! Text received: " + response.DataAsText);
				var main = GetDic (response.DataAsText);
				int status = GetStatus (main);

				if (status == 200) {
					//購入成功
					var data = GetDic ((string)main ["data"]);
					buyCards = (List<object>)data ["ids"];//購入したカード
				} else {
					//エラー
					errorMessage = "サーバーエラーが発生しました";
				}
			} catch (Exception e ){
				errorMessage = "サーバーエラーが発生しました";
			}
		}
		ExecuteEvents.Execute<IBuy> (
			target: Delegate, // 呼び出す対象のオブジェクト
			eventData: null,  // イベントデータ（モジュール等の情報）
			functor: (recieveTarget, y) => recieveTarget.OnBuy (buyCards,errorMessage)); // 操作
	}

	void OnLvup (HTTPRequest request, HTTPResponse response) {
		string errorMessage = GetErrorMessage (request);
		int cid = -1;
		int lv = -1;
		if (errorMessage == "") {
			try{
				Debug.Log ("Request Finished! Text received: " + response.DataAsText);
				var main = GetDic (response.DataAsText);
				int status = GetStatus (main);

				if (status == 200) {
					//購入成功
					var data = GetDic ((string)main ["data"]);
					cid = toInt( data ["cid"]);//購入したカード
					lv = toInt(data["lv"]);
				} else {
					//エラー
					errorMessage = "サーバーエラーが発生しました";
				}
			} catch (Exception e ){
				errorMessage = "サーバーエラーが発生しました";
			}

		}
		ExecuteEvents.Execute<ILvup> (
			target: Delegate, // 呼び出す対象のオブジェクト
			eventData: null,  // イベントデータ（モジュール等の情報）
			functor: (recieveTarget, y) => recieveTarget.OnLvup (cid,lv,errorMessage)); // 操作
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
