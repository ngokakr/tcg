using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BestHTTP;
using BestHTTP.SocketIO;

public class SampleSocketIO : MonoBehaviour {

	private enum ChatStates
	{
		Login,
		Chat
	}

	#region Fields
	private SocketManager Manager;
	private ChatStates State;
	public Text Detail;
	public string url = "";
	public string userName = string.Empty;
	public string message = string.Empty;
	public string chatLog = string.Empty;
	private DateTime lastTypingTime = DateTime.MinValue;
	#endregion

	void Start () {
		State = ChatStates.Login;
		SocketOptions options = new SocketOptions();
		options.AutoConnect = false;

		Manager = new SocketManager(new Uri(url), options);
		Socket pvp = Manager.GetSocket ("/pvp");
		Socket news = Manager.GetSocket ("/news");
		// Set up custom chat events
//		Manager.Socket.On("server_to_client",server_to_client);

		// The argument will be an Error object.
		pvp.On(SocketIOEventTypes.Error, (socket, packet, args) => Debug.LogError(string.Format("Error: {0}", args[0].ToString())));
		news.On(SocketIOEventTypes.Error, (socket, packet, args) => Debug.LogError(string.Format("Error: {0}", args[0].ToString())));

		// We set SocketOptions' AutoConnect to false, so we have to call it manually.

		pvp.On("OnLogin", OnLogin);
		pvp.On("OnMessage", OnMessage);
		pvp.On("OnJoin", OnJoin);
		news.On("OnMessage", OnMessage);
		Manager.Open();

//		Manager.Socket.On("user joined", OnUserJoined);
//		Manager.Socket.On("user left", OnUserLeft);
//		Manager.Socket.On("typing", OnTyping);
//		Manager.Socket.On("stop typing", OnStopTyping);
	}

	void OnDestroy()
	{
		// Leaving this sample, close the socket
		Manager.Close();
	}

	public void ButtonNotify (int _num) {
		switch(_num){
		case 0:
			{
				Debug.Log ("emit");
				Manager["/pvp"].Emit ("emit",new object[] {"xxx","text"});
			}
			break;
		case 1://ロビー入室
			{
				Manager["/pvp"].Emit ("toLobby");
			}
			break;
		}
	}

	void OnLogin(Socket socket, Packet packet, params object[] args)
	{
		Debug.Log ("ログインしました。自分のID : " + args[0]);
	}
	void OnJoin (Socket socket, Packet packet, params object[] args)
	{
		Debug.Log ("Roomに参加 : " + args[0]);
	}
	void OnMessage (Socket socket, Packet packet, params object[] args)
	{
		ShowArgs (args);
	}

	void ShowArgs (object[] args) {
		for (int i = 0; i < args.Length; i++ ){
			object temp = args [i];

			Debug.Log("["+i+"]\n"+ ShowDict (temp));
		}
	}
	string ShowDict (object temp) {
		Debug.Log (temp.ToString ());
		string text = "";
		if (temp.GetType () == typeof(Dictionary<string,object>)) {
			Debug.Log ((temp as Dictionary<string,object>).Count);
			foreach(KeyValuePair<string,object> pair in temp as Dictionary<string,object>){
				text += "["+pair.Key + "]" + ShowDict (pair.Value);
			}
		} else {
			text = temp.ToString();
		}
		text += "\n";
		return text;
	}
//
//	void server_to_client (Socket socket, Packet packet, params object[] args) {
//		Debug.Log ("s to c");
//	}

}
