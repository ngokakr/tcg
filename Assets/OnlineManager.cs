using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardParam = SystemScript.CardParam;
using BestHTTP;
using BestHTTP.SocketIO;
using LitJson;

public class OnlineManager : SingletonMonoBehaviour<OnlineManager> {
	
	public string url = "";

	SocketManager Manager;
	BattleScript battleScript;

	enum Status {
		DISCONNECTED,
		CONNECTING,
		CONNECTED,
	}
	[SerializeField]
	Status state;
	// Use this for initialization
	void Start () {
		//
		battleScript = GameObject.FindObjectOfType<BattleScript>();

		//接続していない
		state = Status.DISCONNECTED;


		SocketOptions options = new SocketOptions();
		options.AutoConnect = false;

		//ネームスペースわけ
		Manager = new SocketManager(new System.Uri(url), options);
		Socket pvp = Manager.GetSocket ("/pvp");
		Socket news = Manager.GetSocket ("/news");

		//エラー処理
		pvp.On(SocketIOEventTypes.Error, (socket, packet, args) => Debug.LogError(string.Format("Error: {0}", args[0].ToString())));
		news.On(SocketIOEventTypes.Error, (socket, packet, args) => Debug.LogError(string.Format("Error: {0}", args[0].ToString())));

		//
		pvp.On("OnMatched",OnMatched);
		pvp.On ("OnJoin", OnJoin);

		Manager.Open();
	}
	//マッチング開始
	public void Matching () {
		List<CardParam> deck = SystemScript.cdTocp (DataManager.Deck.GetDeckData (DataManager.Instance.UseDeck));
		deck = SystemScript.ShuffleCP(deck);
		Manager["/pvp"].Emit ("toLobby",new object[] {JsonMapper.ToJson(deck)});
	}



	public void SendDeck (List<CardParam> _deck) {
		Debug.Log( JsonMapper.ToJson (_deck));
	}

	void OnMatched (Socket socket, Packet packet, params object[] args) {
		//deck0が先行
		List<CardParam> deck0 = JsonMapper.ToObject<List<CardParam>>(""+ args [0]);
		List<CardParam> deck1 = JsonMapper.ToObject<List<CardParam>>(""+ args [1]);
//		Debug.Log (args [2]);
		int myNum = System.Convert.ToInt32( args[2]); //自分はどっちか
		List<CardParam> pDeck = (myNum == 0) ? deck0 : deck1;
		List<CardParam> eDeck = (myNum == 0) ? deck1 : deck0;
		SceneManager.Instance.ToBattleOnline (0, new int[]{ 60, 60 }, new int[]{ 10, 10 }, pDeck, eDeck, myNum);

	}
	void OnJoin (Socket socket, Packet packet, params object[] args)
	{
		Debug.Log ("Roomに参加 : " + args[0]);
	}

	void OnDeck (Socket socket, Packet packet, params object[] args) 
	{
		
	}

	void OnMessage (Socket socket, Packet packet, params object[] args)
	{
		battleScript.OnlineData (args);
	}

	// Update is called once per frame
	void Update () {
		
	}
}
