using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleMode = OnlineManager.BattleMode;
using CardData = SystemScript.CardData;
using LitJson;

public class PunBattle : Photon.MonoBehaviour ,IRecieveMessage{

	public PhotonView photonView;
	AlertView alert;
	BattleMode battleMode;
	bool Battling = false;
	public int Seed = 0;
	public string roomKeyword = "";
	int Initiative = -1;

	//両方揃ったらゲーム開始
	List<CardData> myDeck;
	List<CardData> enemyDeck;
	int e_uid;
	string e_name;

	/// <summary>
	/// とにかく最初に呼ぶ
	/// </summary>
	public void Matching (BattleMode mode,string keyword) {
		battleMode = mode;
		roomKeyword = keyword;
		myDeck = null;
		enemyDeck = null;
		if (!PhotonNetwork.connected) {
			PhotonNetwork.ConnectUsingSettings(DataManager.Instance.AppVersion);
		}
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


	/// <summary>
	/// マッチング成功時
	/// </summary>
	void OnMatched (PhotonPlayer newPlayer) {
		//データ送信
		int uid = DataManager.Instance.uid;
		string uname = DataManager.Instance.PlayerName;
		if (PhotonNetwork.isMasterClient) {
			Initiative= Random.Range (0, 2);//0だったらマスタークライアントが先攻
			Seed = Random.Range (0, 999999999);

		}
		myDeck = DataManager.Deck.GetDeckData ();
		string deckjson = JsonMapper.ToJson(myDeck);
		photonView.RPC ("RPC_StartDatas",PhotonTargets.OthersBuffered,uid,uname,Initiative,Seed,deckjson);

		//対戦できるかチェック
		CanBattle();
	}

	[PunRPC]
	void RPC_StartDatas (PhotonMessageInfo info,int uid,string uname,int initiative,int seed,string deckjson){
		e_uid = uid;
		e_name = name;
		//デッキ
		enemyDeck = JsonMapper.ToObject<List<CardData>> (deckjson);
		//シード & 先攻後攻
		if (info.sender.isMasterClient) {
			Seed = seed;
			Initiative = initiative == 0 ? 1 : 0;//1と0を入れ替える
		}

		//対戦できるかチェック
		CanBattle();
	}

	void CanBattle (){
		if (myDeck != null && enemyDeck != null) {//準備完了
			SceneManagerx.Instance.ToBattleOnline (battleMode, DataManager.Instance.DefaultLP, DataManager.Instance.DefaultSP
				, SystemScript.cdTocp(myDeck),SystemScript.cdTocp(enemyDeck) , Initiative);
		}
	}

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
		AlertView.Make (-1,"エラー","入室できませんでした",new string[]{"OK"}, gameObject,1);
		battleMode = BattleMode.NONE;
		PhotonNetwork.Disconnect ();
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
	void OnDisconnectedFromPhoton(){
		Debug.Log ("OnDisconnectedFromPhoton");
		if (!Battling && battleMode != BattleMode.NONE) {
			alert.OpenClose (false);
			AlertView.Make (-1,"エラー","サーバーから切断されました。",new string[]{"OK"}, gameObject,1);
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
		OnMatched (newPlayer);
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
