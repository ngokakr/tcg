using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using UnityEngine;
using UnityEngine.UI;
using BestHTTP;


public class TestScript : MonoBehaviour {
	public bool Reset = false;
	public bool TestTrigger = false;
	public bool RegistTrigger = false;
	public bool LoginTrigger = false;
	public string UserName = "";
	public string Password = "";
	public string URL;
	public string Path;

	[Serializable]
	public struct PlayerData
	{
		public int uID;
		public int Coin;
		public int Dia;
		public int rate;
		public int loginDays;
	}

	public PlayerData playerData;

	public void Awake (){
//		HTTPRequest request = new HTTPRequest (new Uri (URL+Path), HTTPMethods.Post, OnRequestFinished);
//		request.AddField ("name", "akr");
////		request.AddField ("password",StringUtils.GeneratePassword(30));
//		request.AddField ("password",Password);
//		request.Send ();
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
			Regist();
		}

		if (LoginTrigger) {
			LoginTrigger = false;
			Login();
		}
	}

	void Test () {
		HTTPRequest request = new HTTPRequest (new Uri (URL), HTTPMethods.Get, OnRequestFinished);
		request.Send ();
	}

	void Regist () {
		HTTPRequest request = new HTTPRequest (new Uri (URL+"users/"), HTTPMethods.Post, OnRegist);
		request.AddField ("name", UserName);
		Password = StringUtils.GeneratePassword (30);
		request.AddField ("password",Password);
		request.Send ();
	}
	void Login () {
		HTTPRequest request = new HTTPRequest (new Uri (URL+"login/"), HTTPMethods.Post, OnLogin);
		request.AddField ("uid", playerData.uID+"");
		request.AddField ("password",Password);
		request.Send ();
	}

	void OnRegist (HTTPRequest request, HTTPResponse response) {
		Debug.Log("Request Finished! Text received: " + response.DataAsText);

		var main = GetDic (response.DataAsText);
		int status = GetStatus (main);
		if (status == 200 && DataType(main) == "login") {
			//登録成功
			SetData(main);
		} else {
			//登録失敗
		}


	}

	void SetData (Dictionary<string,object> main) {
		var x = main["data"];
		var y = GetDic ((string)x);

		UserName = (string)y ["name"];
		playerData.Coin = Convert.ToInt32( y ["coin"]);
		playerData.uID= Convert.ToInt32( y ["uid"]);
		playerData.Dia = Convert.ToInt32 (y ["dia"]);
		playerData.loginDays = Convert.ToInt32 (y ["loginDays"]);
		playerData.rate = Convert.ToInt32 (y ["rate"]);
	}


	void OnLogin (HTTPRequest request, HTTPResponse response) {
		Debug.Log("Request Finished! Text received: " + response.DataAsText);

		var main = GetDic (response.DataAsText);
		int status = GetStatus (main);
		if (status == 200 && DataType (main) == "login") {
			SetData (main);
		}
	}

	string DataType (Dictionary<string,object> main) {
		return  (string)(GetDic ((string)main ["data"]) ["type"]);
//		return (string)data ["type"];
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
