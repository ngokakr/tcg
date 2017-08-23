using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CookieTest : MonoBehaviour {

	public string stDisplay;
	public Hashtable hsHeader;

	void Start() {
		this.hsHeader = new Hashtable();
		this.stDisplay = "none";
	}

	void OnGUI() {
		if (GUI.Button(new Rect(8,8,Screen.width-16, 32), "Click")) {
			StartCoroutine(GetData("http://localhost/tcg/session_test.php"));
		}
		GUILayout.BeginArea(new Rect(8,40,Screen.width-16, Screen.height-16-32));
		GUILayout.Label(this.stDisplay);
		GUILayout.EndArea();
	}

	private IEnumerator GetData(string url) {
		WWW www;
		WWWForm form;
		foreach(DictionaryEntry entry in hsHeader){
			Debug.Log(entry.Key + " " + entry.Value);
		}
		if (this.hsHeader.Count > 0) {
			form = new WWWForm();
			form.AddField("dummy", ""); // Dummy
			www = new WWW(url, form.data,HashtableExtensions.ToDictionary( this.hsHeader));
		} else {
			www = new WWW(url);
		}
		yield return www;

		this.stDisplay = www.text;

		Dictionary<string,string> headers = www.responseHeaders;
		foreach (KeyValuePair<string,string> kvPair in headers) {
			if (kvPair.Key.ToLower().Equals("set-cookie")) {
				string stHeader = "";
				string stJoint = "";
				string[] astCookie = kvPair.Value.Split(
					new string[] {"; "}, System.StringSplitOptions.None);
				foreach (string stCookie in astCookie) {
					if (!stCookie.Substring(0,5).Equals("path=")) {
						stHeader += stJoint + stCookie;
						stJoint = "; ";
					}
				}
				if (stHeader.Length > 0) {
					this.hsHeader["Cookie"] = stHeader;
				} else {
					this.hsHeader.Clear();
				}
			}
		}
		www.Dispose();
	}
}