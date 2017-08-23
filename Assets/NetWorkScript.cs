using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetWorkScript : MonoBehaviour {

	// URL
	string url = "http://localhost/tcg/unity_text.php";
	// サーバへリクエストするデータ
	string user_id = "0";
	string user_name = "ufgjkohu";
	string user_pass = "default";

	// タイムアウト時間
	float timeoutsec = 5f;

	void Start () {
		StartCoroutine (Register ());
//		// サーバへPOSTするデータを設定 
//		Dictionary<string, string> dic = new Dictionary<string, string>();
//		dic.Add ("id", user_id);
//		dic.Add ("name", user_name);
//		dic.Add ("data", user_pass);
//
//		StartCoroutine(HttpPost(url, dic));  // POST
//
//		// サーバへGETするデータを設定
//		string get_param = "?id=" + user_id + "&name=" + user_name + "&data=" + user_pass;
//		StartCoroutine(HttpGet(url + get_param));  // GET

	}
	IEnumerator Register () {
		Dictionary<string, string> dic = new Dictionary<string, string>();
		dic.Add ("appVer", DataManager.Instance.AppVersion);
		dic.Add ("name", user_name);
		StartCoroutine(HttpPost("http://localhost/tcg/Register.php", dic));
		yield return null;
	}
	IEnumerator Login () {
		// サーバへPOSTするデータを設定 
		Dictionary<string, string> dic = new Dictionary<string, string>();
		dic.Add ("appVer", DataManager.Instance.AppVersion);
		dic.Add ("minorVer", DataManager.Instance.MinorVersion);
		dic.Add ("name", user_name);
		dic.Add ("userID", user_id);
		dic.Add ("pass", user_pass);
		yield return null;
	}

	// HTTP POST リクエスト
	IEnumerator HttpPost(string url, Dictionary<string, string> post)
	{
		WWWForm form = new WWWForm();
		foreach(KeyValuePair<string, string> post_arg in post) {
			form.AddField(post_arg.Key, post_arg.Value);
		}
		WWW www = new WWW(url, form);

		// CheckTimeOut()の終了を待つ。5秒を過ぎればタイムアウト
		yield return StartCoroutine(CheckTimeOut(www, timeoutsec));

		if (www.error != null) {
			Debug.Log("HttpPost NG: " + www.error);
		}
		else if (www.isDone) {
			// サーバからのレスポンスを表示
			Debug.Log("HttpPost OK: " + www.text);
		}
	}

	// HTTP GET リクエスト
	IEnumerator HttpGet(string url)
	{
		WWW www = new WWW(url);

		// CheckTimeOut()の終了を待つ。5秒を過ぎればタイムアウト
		yield return StartCoroutine(CheckTimeOut(www, timeoutsec));

		if (www.error != null) {
			Debug.Log("HttpGet NG: " + www.error);
		}
		else if (www.isDone) {
			// サーバからのレスポンスを表示
			Debug.Log("HttpGet OK: " + www.text);
		}

	}

	// HTTPリクエストのタイムアウト処理
	IEnumerator CheckTimeOut(WWW www, float timeout)
	{
		float requestTime = Time.time;

		while(!www.isDone)
		{
			if(Time.time - requestTime < timeout)
				yield return null;
			else
			{
				Debug.Log("TimeOut");  //タイムアウト
				break;
			}
		}
		yield return null;
	}
}
