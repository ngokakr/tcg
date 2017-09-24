using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleMode = OnlineManager.BattleMode;
using CardData = SystemScript.CardData;
using CardParam = SystemScript.CardParam;
using LitJson;

public class PunBattle : Photon.MonoBehaviour ,IRecieveMessage{

	public BattleScript battleScript;
	public PhotonView photonView;
	AlertView alert;
	BattleMode battleMode;
	public bool Battling = false;
	public bool Reconnecting = false;
	public bool Reconnectend = false;
	public int Seed = 0;
	public string roomKeyword = "";
	int Initiative = -1;
	string roomName = "";
	//両方揃ったらゲーム開始
	public List<CardParam> myDeck;
	public List<CardParam> enemyDeck;
	int e_uid;
	string e_name;

	[System.Serializable]
	public struct PunData
	{
		public string roomName;
		public bool Connect;
		public bool lobby;
		public bool room;

	}

	public PunData pundata;

	/// <summary>
	/// とにかく最初に呼ぶ
	/// </summary>
	public void Matching (BattleMode mode,string keyword) {
		Battling = false;
		Reconnecting = false;
		Reconnectend = false;
		battleMode = mode;
		roomKeyword = keyword;
		myDeck = null;
		enemyDeck = null;
		Reconnectend = false;
		if (!PhotonNetwork.connected) {
			PhotonNetwork.ConnectUsingSettings(DataManager.Instance.AppVersion);
		} else 
			JoinOrCreate();
	}

	/// <summary>
	/// Joinもしくはcreateする。
	/// </summary>
	void JoinOrCreate () {
		//ルームオプション
		RoomOptions roomOptions = new RoomOptions ();
		roomOptions.MaxPlayers = 2; //部屋の最大人数
		roomOptions.IsOpen = true; //入室許可する
		roomOptions.IsVisible = false; //ロビーから見えるようにする

		if (battleMode == BattleMode.RANK) {
			PhotonNetwork.JoinRandomRoom ();
		} else if (battleMode == BattleMode.ROOM){
			if (roomKeyword == null) {//ルーム作成
				roomKeyword = "" + Random.Range (10000, 99999);
				PhotonNetwork.CreateRoom ("room:" + roomKeyword,roomOptions,null);
				alert = AlertView.Make (0,"待機中","ルームID:"+roomKeyword,new string[]{}, gameObject,1);
			} else {//ルーム入室
				PhotonNetwork.JoinRoom("room:" + roomKeyword);
			}
		}

	}

	/// <summary>
	/// ランクマッチ用
	/// </summary>
	void CreateRandomRoom (){
		RoomOptions roomOptions = new RoomOptions ();
		roomOptions.MaxPlayers = 2; //部屋の最大人数
		roomOptions.IsOpen = true; //入室許可する
		roomOptions.IsVisible = true; //ロビーから見えるようにする
		PhotonNetwork.CreateRoom ("rank:" + SystemScript.GeneratePassword (10),roomOptions,null);
		alert = AlertView.Make (1, "オンライン対戦", "対戦相手を探しています...", new string[]{ }, gameObject, 1);
	}


	#region 対戦準備

	/// <summary>
	/// マッチング成功時
	/// </summary>
	void OnMatched () {
		//データ送信
		if(alert != null)
		alert.OpenClose(false);
		Reconnectend = false;
		int uid = DataManager.Instance.uid;
		string uname = DataManager.Instance.PlayerName;
		roomName = PhotonNetwork.room.Name;
		if (PhotonNetwork.isMasterClient) {
			Initiative= Random.Range (0, 2);//0だったらマスタークライアントが先攻
			Seed = Random.Range (0, 999999999);
			PhotonNetwork.room.IsOpen = false;
		}
		myDeck = SystemScript.ShuffleCP( SystemScript.cdTocp( DataManager.Deck.GetDeckData ()));
		string deckjson = JsonMapper.ToJson(myDeck);
		photonView.RPC ("RPC_StartDatas",PhotonTargets.OthersBuffered,uid,uname,Initiative,Seed,deckjson);

		//対戦できるかチェック
		CanBattle();
	}

	[PunRPC]
	void RPC_StartDatas (int uid,string uname,int initiative,int seed,string deckjson,PhotonMessageInfo info){
		e_uid = uid;
		e_name = name;
		//デッキ
		enemyDeck = JsonMapper.ToObject<List<CardParam>> (deckjson);
		//シード & 先攻後攻
		if (info.sender.isMasterClient) {
			Seed = seed;
			Initiative = initiative == 0 ? 1 : 0;//1と0を入れ替える
		}

		//対戦できるかチェック
		CanBattle();
	}

	void CanBattle (){
//		Debug.Log (string.Format( "CanBattle{0} {1} : {2} {3}",myDeck != null,enemyDeck != null,myDeck.Count >0,enemyDeck.Count >0));
		if (myDeck != null && myDeck.Count >0 && enemyDeck != null && enemyDeck.Count >0) {//準備完了
			Debug.Log("BattleStart");
			Battling = true;
			SceneManagerx.Instance.ToBattleOnline (battleMode, DataManager.Instance.DefaultLP, DataManager.Instance.DefaultSP
				,myDeck,enemyDeck,e_name, Initiative,Seed);
		}
	}



	#endregion


	#region 対戦中

	public void SendCommands (List<BattleScript.Command> cmds) {
		string json = JsonMapper.ToJson (cmds);
		photonView.RPC ("RPC_CommandDatas",PhotonTargets.OthersBuffered,json);
	}

	public void SendCommand (BattleScript.Command cmd) {
		string json = JsonMapper.ToJson (cmd);
		photonView.RPC ("RPC_CommandData",PhotonTargets.OthersBuffered,json);
	}

	[PunRPC]
	public void RPC_CommandDatas(string cmds,PhotonMessageInfo info){
		var data = JsonMapper.ToObject<List<BattleScript.Command>> (cmds);
		battleScript.CommandsNotify (data);
	}

	[PunRPC]
	public void RPC_CommandData(string cmd,PhotonMessageInfo info){
		var data = JsonMapper.ToObject<BattleScript.Command> (cmd);
		battleScript.CommandNotify (data);
	}

	IEnumerator Reconnect () {
		alert = AlertView.Make (-1, "再接続中", "しばらくお待ち下さい", new string[]{}, gameObject, 2,true);

		Reconnecting = true;
		Reconnectend = false;
		for (int i = 0; i < 15; i++ ){
			PhotonNetwork.ReconnectAndRejoin ();
			yield return new WaitForSeconds (1f);
			if (PhotonNetwork.connected) {
				yield break;
			}
		}
		Reconnecting = false;
		Reconnectend = true;
		alert.OpenClose (false);
		PhotonNetwork.Disconnect ();
		battleScript.DisconnectJudge (1);
	}

	public void GameEnd () {
		Reconnecting = false;
		Reconnectend = true;
		PhotonNetwork.Disconnect ();
	}
	#endregion

	public void OnRecieve(int _num,int _tag){
		if (_tag == 0 || _tag == 1) {//ルーム解散
			battleMode = BattleMode.NONE;
			PhotonNetwork.Disconnect();
		}

	}


	void OnConnectedToPhoton(){
		Debug.Log ("OnConnectedToPhoton");
	}
	void OnLeftRoom(){
		Debug.Log ("OnLeftRoom");
	}
	void OnMasterClientSwitched(PhotonPlayer newMasterClient){
		Debug.Log ("OnMasterClientSwitched:" + newMasterClient.NickName); 
	}
	void OnPhotonCreateRoomFailed(object[] codeAndMsg) {
		Debug.Log ("OnPhotonCreateRoomFailed:" + codeAndMsg [0] + codeAndMsg [1]);
	}
	void OnPhotonJoinRoomFailed(object[] codeAndMsg){
		Debug.Log ("OnPhotonJoinRoomFailed:" + codeAndMsg [0] + codeAndMsg [1]);
		if (Battling) {
			alert.OpenClose (false);
			Reconnectend = true;
			battleScript.DisconnectJudge (1);
			PhotonNetwork.Disconnect ();
		} else {
			AlertView.Make (-1, "エラー", "入室できませんでした", new string[]{ "OK" }, gameObject, 1);
			battleMode = BattleMode.NONE;
			PhotonNetwork.Disconnect ();
		}
	}
	void OnCreatedRoom(){
		Debug.Log ("OnCreatedRoom");
	}
	void OnJoinedLobby(){
		Debug.Log ("OnJoinedLobby");
		//
		JoinOrCreate();
	}
	void OnLeftLobby () {
		Debug.Log ("OnLeftLobby");
	}
	void OnJoinedRoom(){
		Debug.Log ("OnJoinedRoom");
		if (PhotonNetwork.room.PlayerCount == 2) {
			OnMatched ();
		}
	}
	void OnDisconnectedFromPhoton(){
		Debug.Log ("OnDisconnectedFromPhoton");
		if (!Battling && battleMode != BattleMode.NONE) {
			if(alert != null)
			alert.OpenClose (false);
			AlertView.Make (-1, "エラー", "サーバーから切断されました。", new string[]{ "OK" }, gameObject, 1);
		} 
		if(Battling){//対戦中
			if (Reconnecting == false &&Reconnectend == false) {
				StartCoroutine (Reconnect ());
			}
		}
	}
	void OnConnectionFail(DisconnectCause cause){
		Debug.Log ("OnConnectionFail");
	}
	void OnFailedToConnectToPhoton(DisconnectCause cause){
		Debug.Log ("OnFailedToConnectToPhoton");

	}
	void OnPhotonPlayerConnected(PhotonPlayer newPlayer){
		Debug.Log ("OnPhotonPlayerConnected");
		OnMatched ();
	}
	void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer){
		Debug.Log ("OnPhotonPlayerDisconnected");
	}
	void OnPhotonRandomJoinFailed(object[] codeAndMsg){
		Debug.Log ("OnPhotonRandomJoinFailed:" + codeAndMsg [0] + codeAndMsg [1]);
		CreateRandomRoom ();
	}
	void OnConnectedToMaster(){
		Debug.Log ("OnConnectedToMaster");
	}
	void OnPhotonMaxCccuReached(){
		Debug.Log ("OnPhotonMaxCccuReached");
	}
	void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged){
		Debug.Log ("OnPhotonCustomRoomPropertiesChanged");
	}
	public void OnPhotonCustomRoomPropertiesChanged( ExitGames.Client.Photon.Hashtable i_propertiesThatChanged )
	{
		Debug.Log ("OnPhotonCustomRoomPropertiesChanged");
	}
}
