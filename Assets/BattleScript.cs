using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CardParam = SystemScript.CardParam;
using DG.Tweening;
using BestHTTP;
using BestHTTP.SocketIO;

public class BattleScript : MonoBehaviour,IRecieveMessage {
	#region Toooooooooooooooooooooooooooooop
	#endregion
	#region 宣言部


	public enum Phase {
		_0_START,
		_1_WAIT,
		_2_SUMMON,
		_3_BATTLE,
		_4_DAMAGE,
		_5_END
	}
	public Phase NowPhase;

	enum GameMode {
		PRACTICE,
		FREE,
		RATE,
		CPU,

	}
	[SerializeField]
	GameMode gameMode;

	public enum Mode
	{
		NOMAL,
		SUMMON,
		SELECT,
		WINDOW_SELECT,
	}
	public Mode mode = Mode.NOMAL;

	enum NowNetwork {
		PVP_CONNECTED,
		PVP_DISCONNECTED,
		CPU,
	}
	[SerializeField]
	NowNetwork nowNetwork;

	[System.Serializable]
	public struct DebugTool
	{
		public int Atr;
		public int ID;
		public bool SetCard;
		public bool SPBoost;
		public bool Refresh;
	}
	public DebugTool debugTool;

	[System.Serializable]
	public struct UserObjs
	{
		public Text LPText;
		public Text SPText;
		public Text PlayerDataText;
		public Text DataText;
		public Transform DeckPos;
		public RectTransform HandsField;
		public List<Transform> ImagePoses;
		public Transform Creature;
	}
	public List<UserObjs> userObjs;

	public int NowTurn;
	public int NowWeather;
	public int Initiative;//先手

	//パラメータ
	public int[] DefaultLPs;
	public int[] LPs;
	public int[] SPs;
	//フィールド
	public List<CardParam> Creatures;//フィールド(-1)
	public List<CardParam> PlayerHand;//(0)
	public List<CardParam> EnemyHand;//(1)
	public List<CardParam> PlayerDeck;//(2)
	public List<CardParam> EnemyDeck;//(3)
	public List<CardParam> PlayerGrave;//(4)
	public List<CardParam> EnemyGrave;//(5)
	//オンラインデータ 先手召喚データ -> 後手召喚データ -> 選択データ等
	List<object[]> onlineDatas;
	//パスを生成する時に使う値
	int PassNum = 1; //1からスタート
	//カードを使い回す
	public List<CardImageScript> CardPool;
	//[Pass][SummonOrDiscard]
	[SerializeField]
	public int[][] SelectingPasses = new int[2][];
	//選択
	List<CardParam> SelectChoices;
	public int Select1Pass = -1;
	bool windowOpen = false;//選択ウィンドウを表示するか
//	List<CardParam> SelectWindowCards;//対象のカード
	public int nowPage = 0;

	//固定値
	const int MAX_NUM_OF_HANDS = 4;
	//現在詳細を見ているカード
	public CardParam DetailCard;
	//ターンシークエンスこるーちん
	IEnumerator seq;
	//引っ張り
	public Image WeatherImage;
	public CardImageScript DetailImage;
	public List<CardImageScript> SelectWindowCardsImage;
	public List<Text> SelectWindowArraws;
	public Text Weather;
	public Text NameText;
	public Text ParamText;
	public Text SkillText;
	public Text TurnText;
	public Text SelectWindowTitle;

	public GameObject SummonButton;
	public GameObject DiscardButton;
	public GameObject SelectButton;
	public List<Image> FlashingImages;
	public GameObject SelectImage;
	public GameObject SelectWindow;
	public GameObject SelectCardAndBG;
	public Image SelectWindowFlash;
	//スケール
	public float PoolCardScale = 0.15f;
	//ウェイト
	public float WaitTime = 0.5f;
	public enum TargetParam {
		ID,
		ATR,
		LV,
		ROLE,
		REALITY,
		GROUPS,
		NAME,
		COST,
		POWER,
		SKILL,
	}
	public enum TargetField
	{
		BOUNCE,
		DECKTOP,
		DECKBOTTOM,
		BREAK,
		CREATURE,
	}
	public enum TargetGrave
	{
		BREAK,
		USED,
		DISCARD,
	}
	public enum TargetEffect
	{
		FREEZE = 1,
		BIND = 2,
		BURN = 3,
		FLY = 4,
		THROUGH = 5,
		STEALTH = 6,
		GURD_ATTACK = 7,
		SNIPER = 8,
		MUST_SUMMON = 9,
		SEAL_SKILL = 10,
	}
	public enum LoadType 
	{
		WHEN,
		IF,
		LEFT,
		RIGHT,
	}


	public enum Timing 
	{
		NONE,
		SYSTEM_DRAW,
		TURN_START,
		TURN_END,
		BATTLE_START,
		BATTLE_END,
		SUMMON,
		DISCARD,

		DAMAGE,
		HEAL,
		CHARGE,
		NOIZ,

		REMOVE_SKILL,
		TEMP,//setTempCostなどの時に使う
		TEMP_PARAM_CHANGE,//tempによってパラメータ変更する時。


		DRAW,

		BREAK,
		MOVE,

		USED,
		EFFECT,
		REMOVE_EFFECT,

		TOHAND,
		TODECK,
		TOGRAVE,


		PARAM_CHANGE,
		WEATHER_CHANGE,

	}
	public enum CalType
	{
		ADD,
		MUL,
		DIVIDE,
		SET,
	}
	public struct SkillStruct{
		public object PileTop;//weather.isInt(0) -> true
		public Dictionary<string,object>  Dic;//

		public SkillStruct Set (object _pileTop,Dictionary<string,object> _dic) {
			PileTop = _pileTop;
			Dic = _dic;
			return this;
		}
	}
	#endregion

	#region 開始処理
	public void BattleStartOffline (int[] _LPs,int[] _SPs,List<CardParam> _PDeck,List<CardParam> _EDeck) {
		BattleStart ();//共通設定
		DefaultLPs = new int[2];
		DefaultLPs[0] =  _LPs[0];
		DefaultLPs[1] = _LPs[1];
		LPs = _LPs;
		SPs = _SPs;

		Initiative = Random.Range (0, 2);

		//デッキ展開
		MakeDeckOffline(_PDeck,_EDeck);

		//シャッフル
		DeckShuffle(0);
		DeckShuffle (1);

		//手札
		DealCard(0);
		DealCard(1);

		//更新
		Refresh();

		//SP回復
		SEPlay(0);

		//CPU対戦
		nowNetwork = NowNetwork.CPU;
		gameMode = GameMode.CPU;

		//テストƒ
		seq = Sequence();
		StartCoroutine (seq);
	}
	public void BattleStartOnline (int[] _LPs,int[] _SPs,List<CardParam> _PDeck,List<CardParam> _EDeck,int _initiative) {
		BattleStart ();//共通設定
		LPs = _LPs;
		SPs = _SPs;
		Initiative = _initiative;
		PlayerDeck = _PDeck;
		EnemyDeck = _EDeck;
		MakePasses ();

		//手札
		DealCard(0);
		DealCard(1);
		Refresh ();

		//SP回復
		SEPlay(0);
		nowNetwork = NowNetwork.PVP_CONNECTED;
		gameMode = GameMode.RATE;
		OnlineManager.Instance.SendDeck(PlayerDeck);

		seq = Sequence();
		StartCoroutine (seq);
	}
	void BattleStart () {//CPU、オンライン共通の初期化処理
		Application.targetFrameRate = 60;
		Creatures = new List<CardParam> ();
		Creatures.Add (new CardParam ().Reset());
		Creatures.Add (new CardParam ().Reset());
		PlayerDeck = new List<CardParam> ();
		EnemyDeck = new List<CardParam> ();
		PlayerHand = new List<CardParam> ();
		EnemyHand = new List<CardParam> ();
		PlayerGrave = new List<CardParam> ();
		EnemyGrave = new List<CardParam> ();
		SelectChoices = new List<CardParam> ();
		SummonButton.SetActive (false);
		DiscardButton.SetActive (false);
		SelectButton.SetActive (false);
		mode = Mode.NOMAL;
		WeatherImage.color = Color.gray;
		Weather.text = "快晴";
		NowWeather = -1;
		Select1Pass = -1;
		NowTurn = 0;
		PassNum = 1;
		RemoveAllSelectEffect ();
		for (int i = 0; i < CardPool.Count; i++ ){
			CardPool [i].gameObject.SetActive (false);
		}
		DetailCard = new CardParam ().Reset ();
		ShowDetail ();
	}
	#endregion

	#region シークエンス
	IEnumerator Sequence () {
		while (true) {
		yield return StartCoroutine (StartPhase ());
		yield return StartCoroutine (WaitPhase ());
		yield return StartCoroutine (SummonPhase ());
		yield return StartCoroutine (BattlePhase ());
		}
	}
	IEnumerator StartPhase () {
		//スタートフェーズ
		NowPhase = Phase._0_START;
		//ImageRefresh ();の為に1fまつ
		yield return null;
//		//SE
//		SEPlay(0);
		//ターン数追加
		NowTurn++;
		Refresh ();

		//ターン数表示

		//ターン開始時処理

		//常在効果発動
		yield return StartCoroutine( CheckCommonSkill(Timing.TURN_START));
		//イメージ変更
		ImageRefresh ();
	}
	IEnumerator WaitPhase () {
		//待機フェーズ
		NowPhase = Phase._1_WAIT;
		//召喚状態初期化
		mode = Mode.SUMMON;
		SelectingPasses [0] = new int[]{ -1, -1 };
		SelectingPasses [1] = new int[]{ -1, -1 };
		//手札がなければ選択なし。
		Debug.Log (string.Format ("HandCount : {0} / {1}",GetHands (0).Count,GetHands (1).Count));
		if (0 == GetHands (0).Count) {
			//プレイヤーの手札なし
			SelectingPasses [0] = new int[]{ -2, -2 };
			if(gameMode == GameMode.CPU)
				CPU ();
		}if (0 == GetHands (1).Count) {
			//対戦相手の手札なし
			SelectingPasses [1] = new int[]{ -2, -2 };
		}
		//使用しない素材はunload
		Resources.UnloadUnusedAssets();

		Judge();//勝敗判定

		//召喚or破棄するまで無限ループ & 待機中はfps15
		Application.targetFrameRate = 30;
		while((SelectingPasses[0][0] == -1 && GetHands(0).Count > 0) || SelectingPasses[1][0] == -1 && GetHands(1).Count > 0){
			yield return null;
		}
		//fpsを戻す
		Application.targetFrameRate = 60;
		mode = Mode.NOMAL;

	}
	IEnumerator SummonPhase () {
		//フェーズ
		NowPhase = Phase._2_SUMMON;
		yield return StartCoroutine( SummonCard ());

		Refresh ();
		ImageRefresh ();
		ShowDetail ();


//		SummonEffect (0);
//		SummonEffect (1);

		yield return new WaitForSeconds (WaitTime);

		yield break;
	}
	IEnumerator BattlePhase () {
		//フェーズ
		NowPhase = Phase._3_BATTLE;

		yield return null;
		//先手から
		int[] usersI = GetUsers();
		int FirstUser = GetUsers () [0];
		int SecondUser = GetUsers () [1];

		//常在効果発動(手札効果、戦場効果)
		for (int i = 0; i < GetUsers ().Length; i++) {
			if (ExistCreature (GetUsers () [i])) {
				Dictionary<string, object> objs = new Dictionary<string, object>();
				objs.Add ("targetCard",new List<CardParam>{GetCreature (GetUsers () [i])});
				yield return StartCoroutine (CheckCommonSkill ( Timing.SUMMON,objs));
			}
		}

		//スキル発動
		for (int i = 0; i < GetUsers().Length; i++ ){
			int user = GetUsers () [i];
			if (ExistCreature (user)) {
				CardParam cpx = GetCreature (user);
				yield return StartCoroutine (UseTimingSkill(cpx,"s"));
			}
		}



		CardParam a = GetCreature(FirstUser);
		CardParam b = GetCreature(SecondUser);

		List<CardParam> cards = new List<CardParam> ();
		cards.Add (a);
		cards.Add (b);

		int aRole = a.Role;
		int bRole = b.Role;
		List<int> Roles = new List<int> ();
		Roles.Add (aRole);
		Roles.Add (bRole);

		//勝利プレイヤー決定
		int differ = a.Power - b.Power;
		int winner = -1;
		int loser = -1;
		if (differ > 0) {
			winner = FirstUser;
			loser = SecondUser;
		} else if (differ < 0) {
			winner = SecondUser;
			loser = FirstUser;
		}
		CardParam winCard = GetCreature (winner);

		//ダメージを与えられるかどうか
		bool aDamage = true;
		bool bDamage = true;

		if (a.Effect.Contains (5)) {//aが貫通持ち
			if (b.Role == 2)//相手がディフェンダーならそれは行動不可
				bRole = -1;
		}
		if (b.Effect.Contains (5)) {//bが貫通持ち
			if (a.Role == 2)//相手がディフェンダーならそれは行動不可
				aRole = -1;
		}

		if (aRole == 0 && bRole == 0) {
			//アタッカー同士&勝敗あり
			if (winner != -1) {
				yield return StartCoroutine (S_ChangeLP (loser, -winCard.Power, true, winCard.Pass));
				yield return StartCoroutine (UseTimingSkill (winCard, "a"));
			}
			//先手が勝ち
//			if (differ > 0)
//				yield return StartCoroutine( S_ChangeLP (SecondUser, -a.Power,true,a.Pass));
//			//後手が勝ち
//			if (differ < 0)
//				yield return StartCoroutine( S_ChangeLP (FirstUser, -b.Power,true,b.Pass));



			
		} else if (((aRole == 0 && bRole == 2) || (aRole == 2 && bRole == 0)) && winner != -1) {
			//アタッカーvsディフェンダー&勝敗あり
			if (winner != -1) {
				yield return StartCoroutine (S_ChangeLP (loser, -Mathf.Abs (differ), true, winCard.Pass));
				yield return StartCoroutine (UseTimingSkill (winCard, "d"));
			}
//			//先手が勝ち
//			if (differ > 0)
//				yield return StartCoroutine( S_ChangeLP (SecondUser, -differ,true,a.Pass));
//			//後手が勝ち
//			if (differ < 0)
//				yield return StartCoroutine( S_ChangeLP (FirstUser, differ,true,b.Pass));
			
			
		} else {
			//残りは順番に行う

			for (int i = 0; i < 2; i++ ){
				int user = GetUsers ()[i];
				CardParam userCard = GetCreature (user);
				int RUser = ReversalUsers () [i];
				if (ExistCreature (user)) {
					int role = userCard.Role;
					if (role == 0) {
						yield return StartCoroutine( S_ChangeLP (RUser, -userCard.Power,true,userCard.Pass));
						yield return StartCoroutine (UseTimingSkill (userCard,"a"));

					}
					if (role == 1&& userCard.Power > 0) {
						yield return StartCoroutine( S_ChangeSP (user, userCard.Power,true,userCard.Pass));

					}
				}
			}
		}
		//フェーズ
		NowPhase = Phase._5_END;

		//自然消滅
		if(ExistCreature(0) || ExistCreature(1))
			SEPlay(6);
		S_Used (a.Pass);
		S_Used (b.Pass);

		//状態異常
		for (int i = 0; i < 2; i++ ){
			int user = GetUsers ()[i];
			List<int> handsPass = GetHandsPass(user);
			for (int i2 = 0; i2 < handsPass.Count; i2++ ){
				CardParam cp = GetCard(handsPass[i2]);
				for (int i3 = cp.Effect.Count-1; i3 >= 0; i3-- ){
					int eff = cp.Effect [i3];
					if (eff == 1 || eff == 2 || eff == 3) {
						//ターン送り
						cp.EffectValue [i3]--;
						//効果解消or発動
						if(cp.EffectValue [i3] <= 0){
							//火傷
							if(eff == 3)
								yield return StartCoroutine (S_RemoveCard (new List<CardParam>(){cp},TargetField.BREAK,-1));
							cp.Effect.RemoveAt (i3);
							cp.EffectValue.RemoveAt (i3);
						}
							
					}
				}
				UpdateCard (cp);
			}
		}

		//SP修正
		for (int i = 0; i < 2; i++ ){
			if (SPs [i] < 0)
				SPs [i] = 0;
		}

		Refresh ();
		ImageRefresh ();
		yield return new WaitForSeconds (WaitTime/2);
		//効果発動

		//勝敗判定
		Judge();
	}

	void Judge () {
		
		bool PlayerLose = false;
		bool EnemyLose = false;

		if (GetLP (0)  <= 0 || (GetHands(0).Count +GetDeck(0).Count) == 0) {
			PlayerLose = true;
		}
		if (GetLP (1)  <= 0 || (GetHands(1).Count +GetDeck(1).Count) == 0) {
			EnemyLose = true;
		}

		if (PlayerLose && EnemyLose) {
			//引き分け
			DataManager.Instance.BGMStop();
			AlertView.Make (0,"引き分け","引き分けました",new string[]{"OK"}, gameObject,2);

		}
		if (!PlayerLose && EnemyLose) {
			//勝ち
			DataManager.Instance.BGMPlay(3);
			AlertView.Make (0,"勝利！！","勝利しました",new string[]{"OK"}, gameObject,2);
		}
		if (PlayerLose && !EnemyLose) {
			//負け
			DataManager.Instance.BGMPlay(4);
			AlertView.Make (0,"敗北","敗北しました",new string[]{"OK"},gameObject,2);

		}
		if (PlayerLose || EnemyLose) {
			Application.targetFrameRate = 30;
			StopAllCoroutines ();
		}
	}
	#endregion

	#region スキル発動処理
	IEnumerator CheckCommonSkill(Timing _timing,Dictionary<string,object> _objs = null) {
		//チェックするカード
		List<int> UsePasses = new List<int> ();
		for (int i = 0; i < 2; i++ ){
			int user = GetUsers () [i];
			if(ExistCreature(user))
				UsePasses.Add (GetCreature (user).Pass);
		}
		for (int i = 0; i < 2; i++ ){
			int user = GetUsers () [i];

			List<CardParam> hand = GetHands (user);
			for (int i2 = 0; i2 < hand.Count; i2++ ){
				UsePasses.Add(hand [i2].Pass);
			}
		}

		//一時コスト、パワー変更を解除
		RemoveTemp(GetCards(UsePasses),12);
		RemoveTemp(GetCards(UsePasses),13);

		//常在効果(一時的な効果)
		for (int i = 0; i < UsePasses.Count; i++) {
			//フィールド内に存在していなければ、発動しない。
			if (!ExistCardInFieldOrHands (UsePasses [i]))
				continue;

			CardParam cp = GetCard (UsePasses[i]);
			int[] pos = GetPos (UsePasses [i]);

			for (int i2 = 0; i2 < cp.SkillTexts.Count; i2++ ){
				string skillScript = cp.SkillScript [i2];
				string SkillTiming = SystemScript.GetTiming (cp.SkillTexts [i2]);
				//手札時効果、戦場時効果、割引の常在効果のみ
				if ((SkillTiming == "f2" && pos[0] == -1) 
					|| (SkillTiming == "h2"
						&& (pos[0] == 0 || pos[0] == 1)) || SkillTiming == "w") {

					//複数発動
					string[] _Words = skillScript.Split (';');
					for (int i3 = 0; i3 < _Words.Length; i3++ ){
						string splitedScript = _Words [i3];
						if (splitedScript == "")
							continue;
						//when(),if()をチェック
						string checkedText = whenIfCheck(cp,splitedScript,_timing,_objs);
						//効果発動
						if (checkedText != null) {
							_objs = L_StaticSkill(cp,checkedText as string, _objs).Dic;
						}
					}
				}
			}

		}

		//特殊発動
		for (int i = 0; i < UsePasses.Count; i++ ){
			//フィールド内に存在していなければ、発動しない。
			if (!ExistCardInFieldOrHands (UsePasses [i]))
				continue;

			//
			CardParam cp = GetCard (UsePasses[i]);
			int[] pos = GetPos (UsePasses [i]);

			for (int i2 = 0; i2 < cp.SkillTexts.Count; i2++ ){
				string skillScript = cp.SkillScript [i2];
				string SkillTiming = SystemScript.GetTiming (cp.SkillTexts [i2]);
				//手札時効果、戦場時効果
				if ((SkillTiming == "f" && pos[0] == -1) 
					|| ((SkillTiming == "h")
						&& (pos[0] == 0 || pos[0] == 1))) {
					
					//手札効果かつコスト不足なら発動しない
					if (SkillTiming == "h" && !CanSummon (cp))
						continue;
					//手札効果かつ凍結なら発動しない
					if (SkillTiming == "h" && cp.Effect.Contains (1))
						continue;
					
					bool doneEffect = false;

					//複数発動
					string[] _Words = skillScript.Split (';');
					for (int i3 = 0; i3 < _Words.Length; i3++ ){
						string splitedScript = _Words [i3];
						if (splitedScript == "")
							continue;
						
							
						//when(),if()をチェック
						string checkedText = whenIfCheck(cp,splitedScript,_timing,_objs);
						//効果発動
						if (checkedText != null) {
							//エフェクト表示
							if (!doneEffect && !checkedText.Contains("#")) {
								doneEffect = true;
								DetailCard = cp;
								ShowDetail (i2);
								Effect ("SkillStart", cp.Pass);
								SEPlay (0);
								yield return new WaitForSeconds (WaitTime);
							}
							//エフェクト無効化フラグ消去
							checkedText = checkedText.Replace("#","");

							Debug.Log (checkedText as string);
							var rightCoro = L_DoSkill (cp,checkedText as string, _objs);
							yield return StartCoroutine(rightCoro);
							var objs = ((SkillStruct)rightCoro.Current).Dic;

							_objs = objs;
						}
					}
				}
			}
		}

		yield return _objs;
	}
	/// <summary>
	/// Whenとifを消した部分が返り値 条件を満たしていない時null
	/// </summary>
	string whenIfCheck (CardParam _cp,string _text,Timing _timing,Dictionary<string,object> _objs = null) {

		//when()のチェック
		while (_text.Contains ("when(")) {
			bool useSkill = false;
			string WhenText = L_GetRoundBrackets (_text);
			string[] timings = WhenText.Split ('|');
			for (int i = 0; i < timings.Length; i++ ){
				bool Reverse = false;
				string text = timings [i];
				//!があれば反転
				if (text.Contains ("!"))
					Reverse = true;
				//条件文を読み取る
				if (text.Contains (_timing.ToString ()) != Reverse)
					useSkill = true;
			}

			if (useSkill) {
				//条件文部分を消し去る
				_text = _text.Replace ("when(" + WhenText + ")", "");
			} else {
				return null;
			}
		}

		//if()のチェック
		while (_text.Contains ("if(")) {
			bool Reverse = false;
			string IfText = L_GetRoundBrackets (_text);
			if (IfText.Contains ("!"))
				Reverse = true;
//			//条件文を読み取る
			SkillStruct strc = L_CheckSkill(_cp,IfText,_objs);
			//条件をクリアしていなければ終わり
			if((bool)strc.PileTop == Reverse)
				return null;
			//条件文部分を消し去る
			_text = _text.Replace("if("+IfText+")","");
		}
		return _text;
	}

	//CardParam _TargetCard, Timing _timing,List<object> _obj
	IEnumerator UseTimingSkill (CardParam _cp,string _timing) {
		
		CardParam cp = _cp;
		for (int i2 = 0; i2 < cp.SkillTexts.Count; i2++) {
			string SkillTiming = SystemScript.GetTiming (cp.SkillTexts [i2]);
			if (SkillTiming == _timing) {
				//ラファエル判定
				int enemyTeam = PlayerOrEnemy(_cp.Pass,true);
				if (SkillTiming == "s" && ExistCreature(enemyTeam)) {
					var enemyCre = GetCreature(enemyTeam);
					if (HasEffect (enemyCre.Pass, TargetEffect.SEAL_SKILL)) {
						continue;
					}
				}

				string _text = cp.SkillScript [i2];
				int _skillNum = i2;
				_text = _text.Replace(" ","");
				if (_text == "") {
					Debug.LogError ("能力なし");
					yield break;
				}

				bool doneEffect = false;
				//複数発動
				string[] _Words = _text.Split (';');
				for (int i = 0; i < _Words.Length; i++ ){
					string text = _Words [i];
					if (text == "")
						continue;

					//条件文を読み取る
					string checkedText = whenIfCheck(_cp,text,Timing.SUMMON);

					//条件に合うなら発動
					if (checkedText != null) {
						text = checkedText;
					} else
						continue;

					//データ表示
					DetailCard = _cp;
					ShowDetail(_skillNum);

					//エフェクト
					if (!doneEffect) {
						doneEffect = true;
						Effect ("SkillStart", _cp.Pass);
						SEPlay (2);
						yield return new WaitForSeconds (WaitTime);
					}



					//発動
					yield return StartCoroutine(L_DoSkill(_cp,text,null));

				}
			}
		}

		yield break;


	}


	//if、whenなどを判定
	SkillStruct L_CheckSkill (CardParam _cp,string _text,Dictionary<string,object> _SquareDict) {
		Debug.Log (_cp.Name+"の能力チェック : "+_text);
		List<object> PileObj = new List<object>();
		string[] _Words = SplitWord(_text);
		int user = PlayerOrEnemy (_cp,false);
		int NotUser = PlayerOrEnemy (_cp,true);
		for (int i = 0; i < _Words.Length; i++ ){
			//現在の単語
			string word = _Words [i];
			//最後に保存したキー
			object LastKey = new object ();
			if (0 < PileObj.Count) {
				LastKey = PileObj [PileObj.Count - 1];
			}

			//不要なものを除去
			word = word.Replace (";", "");
			word = word.Replace ("!", "");

			//括弧内のワード取得
			string xyz = L_GetRoundBrackets (word);//括弧内のワード(スクリプト込み)
			object InRound = "";
			if (xyz != "")
				InRound = L_CheckSkill(_cp,xyz,_SquareDict).PileTop;//括弧内のワード(スクリプト処理済み)

			//実行系(iEnumrator)は不可
			SkillStruct strc = L_GetObj (_cp, _text, _SquareDict, LastKey, word, InRound);
			_SquareDict = strc.Dic;

			//When()やif()に実行系が含まれていたりするとエラー
			if (strc.PileTop == null) {
				Debug.LogError ("エラー" + _text + word);

			}else
				PileObj.Add (strc.PileTop);
		}
		return new SkillStruct ().Set (PileObj [PileObj.Count - 1], _SquareDict);
	}
	string[] SplitWord (string _text) {
		string[] _Words = _text.Split ('.');

		//括弧内の.はまとめる。ex) self|.|addPower(myHand.group(14)|.|count.mul(4));
		List<string> summarized = new List<string> ();
		string tempWord = "";//溜めているワード
		int openCount = 0;
		for (int i = 0; i < _Words.Length; i++ ){
			string w = _Words [i];

			for (int i2 = 0; i2 < w.Length; i2++ ){
				char w2 = w [i2];
				if (w2 == '(')
					openCount++;
				if (w2 == ')')
					openCount--;
			}
//			if (w.Contains ("("))
//				openCount++;
//			if (w.Contains (")"))
//				openCount--;

			tempWord += w;
			if (openCount == 0) {//括弧で閉じてたら突っ込む
				summarized.Add (tempWord);
				tempWord = "";
			} else {
				tempWord += ".";
			}

		}
		return summarized.ToArray ();
	}

	SkillStruct L_StaticSkill (CardParam _cp,string _text,Dictionary<string,object> _SquareDict) {
		Debug.Log (_cp.Name+"の常在能力発動 : "+_text);
		List<object> PileObj = new List<object>();

		string[] _Words = SplitWord(_text);
		for (int i = 0; i < _Words.Length; i++ ){
			//現在の単語
			string word = _Words [i];

			//最後に保存したキー
			object LastKey = new object ();
			if (0 < PileObj.Count) {
				LastKey = PileObj [PileObj.Count - 1];
			}
			Debug.Log (word + " ("+LastKey+")");
			//不要なものを除去
			word = word.Replace (";", "");
			word = word.Replace ("!", "");

			//括弧内のワード取得
			string xyz = L_GetRoundBrackets (word);//括弧内のワード(スクリプト込み)
			object InRound = "";
			if (xyz != ""){
				InRound = L_StaticSkill (_cp, xyz, _SquareDict).PileTop;
			}
			//解析
			SkillStruct obj = L_GetObj (_cp, _text, _SquareDict, LastKey, word, InRound);


			if (obj.PileTop == null) {
				Debug.LogError ("エラー : "+ word);
			}

			PileObj.Add (obj.PileTop);
		}
		return new SkillStruct().Set(PileObj [PileObj.Count - 1],_SquareDict);
	}

	//スキル実行
	IEnumerator L_DoSkill (CardParam _cp,string _text,Dictionary<string,object> _SquareDict) {
		Debug.Log (_cp.Name+"の能力発動 : "+_text);
		List<object> PileObj = new List<object>();

		string[] _Words = SplitWord(_text);
		for (int i = 0; i < _Words.Length; i++ ){
			//現在の単語
			string word = _Words [i];

			//最後に保存したキー
			object LastKey = new object ();
			if (0 < PileObj.Count) {
				LastKey = PileObj [PileObj.Count - 1];
			}
			Debug.Log (word + " ("+LastKey+")");
			//不要なものを除去
			word = word.Replace (";", "");
			word = word.Replace ("!", "");

			//括弧内のワード取得
			string xyz = L_GetRoundBrackets (word);//括弧内のワード(スクリプト込み)
			object InRound = "";
			if (xyz != ""){
				var coro = L_DoSkill (_cp, xyz, _SquareDict);
				yield return StartCoroutine (coro);
				InRound = ((SkillStruct)coro.Current).PileTop;//括弧内のワード(スクリプト処理済み)
			}
			//まずは実行系(iEnumrator)以外かどうか試す。
			SkillStruct obj = L_GetObj (_cp, _text, _SquareDict, LastKey, word, InRound);

			//実行系が含まれていたりするときL_GetObjActionを使う。
			if (obj.PileTop == null) {
				var coro = L_GetObjAction (_cp, _text, _SquareDict, LastKey, word, InRound);
				yield return StartCoroutine (coro);
				SkillStruct strc = (SkillStruct)coro.Current;
				_SquareDict = strc.Dic;

				if (strc.PileTop == null)
					Debug.LogError ("エラー" + _text + word);
				else
					PileObj.Add (strc.PileTop);
			} else
				PileObj.Add (obj.PileTop);

		}
		yield return new SkillStruct().Set(PileObj [PileObj.Count - 1],_SquareDict);
	}

	//self -> cardparamがobject型で返る
	SkillStruct L_GetObj (CardParam _cp,string _text,Dictionary<string,object> _SquareDict,object _lastObj, string _word,object _InRound) {

		object ReturnObj = null;

		int user = PlayerOrEnemy (_cp,false);
		int NotUser = PlayerOrEnemy (_cp,true);
		//不要なものを除去
		_word = _word.Replace (";", "");
		_word = _word.Replace ("!", "");

		//角括弧内のワードを取得
		string squ = SystemScript.L_GetSquareBrackets (_word);

		//toSqu[]の場合、角括弧に代入
		if(_word.Contains("toSqu[")){
			_SquareDict[squ] = _lastObj;
			ReturnObj =  (_lastObj);
		}


		//[point]などの場合　括弧内の文字を_SquareDict内から探す。
		if (squ != "") {

			if(_SquareDict.ContainsKey (squ)){
				ReturnObj =  (_SquareDict [squ]);//角括弧があった瞬間に
			}
			else{
				string str = "キーなし:" + squ + "\n";
				Debug.LogError (str);
			}
		}

		//カッコ内のワードを削除
		_word = L_RemoveRoundBrakets(_word);

		//キーワードで場合分け
		switch (_word) {
		//List<cardParam>基本形
		case "self"://そのカード自身
			{
				List<CardParam> lcp = new List<CardParam> (){ _cp };
				ReturnObj= (lcp);
			}
			break;
		case "my"://自分
			{
				ReturnObj= (user);
			}
			break;
		case "enemy":
			{
				ReturnObj= (NotUser);
			}
			break;
		case "defLP":
			{
				ReturnObj = DefaultLPs[(int)_lastObj];
			}
			break;
		case "LP":
			{
				ReturnObj = GetLP((int)_lastObj);
			}
			break;
		case "SP":
			{
				ReturnObj = GetSP((int)_lastObj);
			}
			break;

			//基本系
		case "weather":
			{ 
				ReturnObj= (NowWeather);
			}
			break;
		case "hand":
			{
				List<CardParam> lcp = new List<CardParam> ();
				lcp.AddRange (GetHands (user));
				lcp.AddRange (GetHands (NotUser));
				ReturnObj = lcp;
			}
			break;
		case "myHand":
			{
				ReturnObj= (GetHands (user));
			}
			break;
		case "enemyHand":
			{
				ReturnObj= (GetHands (NotUser));
			}
			break;
		case "myDeck":
			{
				ReturnObj= (GetDeck(user));
			}
			break;
		case "enemyDeck":
			{
				ReturnObj= (GetDeck(NotUser));
			}
			break;
		case "myGrave":
			{
				ReturnObj = GetGrave (user);
			}
			break;
		case "enemyGrave":
			{
				ReturnObj = GetGrave (NotUser);
			}
			break;
		case "cre":
			{
				var cre = new List<CardParam> ();

				if(ExistCreature(user))
					cre.Add(GetCreature (user));
				if(ExistCreature(NotUser))
					cre.Add(GetCreature (NotUser));
				ReturnObj = cre;
			}
			break;
		case "myCre":
			{
				if (ExistCreature (user)) {
					ReturnObj = (new List<CardParam> (){ (GetCreature (user)) });
				} else {
					ReturnObj = new List<CardParam> ();
				}

			}
			break;
		case "enemyCre":
			{
				if (ExistCreature (NotUser)) {
					ReturnObj = (new List<CardParam> (){ (GetCreature (NotUser)) });
				} else {
					ReturnObj = new List<CardParam> ();
				}
			}
			break;

			//List<CardParam>絞り込み系
		case "a":
		case "c":
		case "d":
			{
				List<CardParam> lcp = _lastObj as List<CardParam>;
				ReturnObj=( lcp.FindAll(f => f.Role == SystemScript.GetRoleInt(_word)));
			}
			break;
		case "-a":
		case "-c":
		case "-d":
			{
				List<CardParam> lcp = _lastObj as List<CardParam>;
				ReturnObj=( lcp.FindAll(f => f.Role != SystemScript.GetRoleInt(_word.Replace("-",""))));
			}
			break;

			//int系
		case "power":
			{
				ReturnObj= (((List<CardParam>)_lastObj) [0].GetCalPower());
			}
			break;
		case "cost":
			{
				ReturnObj= (((List<CardParam>)_lastObj) [0].GetCalCost());
			}
			break;
		case "role":
			{
				ReturnObj= (((List<CardParam>)_lastObj) [0].Role);
			}
			break;
		case "count":
			{
				ReturnObj= ((_lastObj as List<CardParam>).Count);
			}
			break;
		case "group":
			{
				ReturnObj= (((List<CardParam>)_lastObj) [0].Groups);
			}
			break;
		case "turn":
			{
				ReturnObj= NowTurn;
			}
			break;
		case "team":
			{
				ReturnObj= (PlayerOrEnemy (((List<CardParam>)_lastObj) [0]));
			}
			break;
		case "true":
			{
				ReturnObj = true;
			}
			break;
		case "false":
			{
				ReturnObj = false;
			}
			break;
		}

		//bool系
		int RetInt = -1;
		if (int.TryParse (_word,out RetInt)) {//整数値かどうか
			ReturnObj=(RetInt);
		}

		if (_word.Contains ("containInt(")) {
			Debug.Log ("containInt:" + ((List<int>)_lastObj).Contains ((int)_InRound));
			ReturnObj = ((List<int>)_lastObj).Contains ((int)_InRound);
		}
		if (_word.Contains ("isInt(")) {
			Debug.Log ("isInt:" +( (int)_lastObj == (int)_InRound));
			ReturnObj= ((int)_lastObj == (int)_InRound);
		}
		if (_word.Contains ("isNotInt(")) {
			Debug.Log ("isNotInt:" +( (int)_lastObj != (int)_InRound));
			ReturnObj= ((int)_lastObj != (int)_InRound);
		}
		if(_word.Contains ("contains(")){
			
			if (_lastObj != null && _InRound != null && ((List<CardParam>)_InRound).Count > 0) {
				bool iscard = false;
				var baseCard = (List<CardParam>)_lastObj;
				var target = ((List<CardParam>)_InRound)[0];

				for (int i = 0; i < baseCard.Count; i++ ){
					if (baseCard[i].Pass == target.Pass) {
						iscard = true;
						break;
					}
				}
				ReturnObj = iscard;
			}else
				ReturnObj=(false);
		}
		if (_word.Contains ("exist")) {//カード以外は不可
			ReturnObj= (0 < ((List<CardParam>)_lastObj).Count);
		}
		if (_word.Contains ("over(")) {
			ReturnObj= ((int)_lastObj > (int)_InRound);
		}
		if (_word.Contains ("under(")) {
			ReturnObj= ((int)_lastObj < (int)_InRound);
		}
		if (_word.Contains ("orOver(")) {
			ReturnObj= ((int)_lastObj >= (int)_InRound);
		}
		if (_word.Contains ("orUnder(")) {
			ReturnObj= ((int)_lastObj <= (int)_InRound);
		}


		//int系
		if (_word.Contains ("add(")) {
			ReturnObj= ((int)_lastObj + (int)_InRound);
		}
		if (_word.Contains ("mul(")) {
			Debug.Log (_lastObj + ":" + _InRound);
			ReturnObj= ((int)_lastObj * (int)_InRound);
		}
		if(_word.Contains ("divi(")){
			ReturnObj= ((int)((int)_lastObj / (int)_InRound));
		}
		//List<CardData>系(続き)
		if (_word.Contains ("group(")) {//
			List<CardParam> lcp = _lastObj as List<CardParam>;
			ReturnObj=( lcp.FindAll(f => f.Groups.Contains((int)_InRound)));
		}
		if (_word.Contains ("id(")) {//
			List<CardParam> lcp = _lastObj as List<CardParam>;
			ReturnObj=( lcp.FindAll(f => f.ID == (int)_InRound));
		}
		if (_word.Contains ("top(")) {//上からx枚
			List<CardParam> lcp = _lastObj as List<CardParam>;
			if (lcp.Count <= (int)_InRound) {
				ReturnObj = lcp;
			} else {
				List<CardParam> topX = new List<CardParam> ();
				for (int i = 0; i < (int)_InRound; i++ ){
					topX.Add (lcp [i]);
				}
				ReturnObj = topX;
			}
		}
		if (_word.Contains ("top(")) {//上からN枚
			List<CardParam> lcp = new List<CardParam>();
			lcp.AddRange (_lastObj as List<CardParam>);
			int selectCount = (int)_InRound;//取り出す枚数
			if (selectCount < lcp.Count) {
				List<CardParam> temp = new List<CardParam>();
				for (int i2 = 0; i2 < selectCount; i2++ ){
					temp.Add(lcp [i2]);
				}
				ReturnObj= (temp);
			} else {//全選択
				ReturnObj= (lcp);
			}
		}
		if(_word.Contains("random(")){//.random(3) これまでの範囲内からX枚ランダムで選択
			//カードをシャッフルしてから枚数ぶん取り出す
			List<CardParam> lcp = new List<CardParam>();
			lcp.AddRange (_lastObj as List<CardParam>);
			CardShuffle (lcp);
			int selectCount = (int)_InRound;//取り出す回数
			if (selectCount < lcp.Count) {//ランダムで選ぶ
				List<CardParam> temp = new List<CardParam>();
				for (int i2 = 0; i2 < selectCount; i2++ ){
					temp.Add(lcp [i2]);
				}
				ReturnObj= (temp);
			} else {//全選択
				ReturnObj= (lcp);
			}
		}

		if(_word.Contains("costAbove(")){//.costBelow(10) これまでの範囲内からコストが高いものを選択
			int costPoint = (int)_InRound;
			List<CardParam> lcp = _lastObj as List<CardParam>;
			ReturnObj=( lcp.FindAll(f => f.GetCalCost() >= costPoint));
		}
		if(_word.Contains("costBelow(")){//.costBelow(10) これまでの範囲内からコストが低いものを選択
			int costPoint = (int)_InRound;
			List<CardParam> lcp = _lastObj as List<CardParam>;
			ReturnObj=( lcp.FindAll(f => f.GetCalCost() <= costPoint));
		}
		if(_word.Contains("powerAbove(")){//.costBelow(10) これまでの範囲内からコストが高いものを選択
			int costPoint = (int)_InRound;
			List<CardParam> lcp = _lastObj as List<CardParam>;
			ReturnObj=( lcp.FindAll(f => f.GetCalPower() >= costPoint));
		}
		if(_word.Contains("powerBelow(")){//.costBelow(10) これまでの範囲内からコストが低いものを選択
			int costPoint = (int)_InRound;
			List<CardParam> lcp = _lastObj as List<CardParam>;
			ReturnObj=( lcp.FindAll(f => f.GetCalPower() <= costPoint));
		}

		//実行系
		if (_word.Contains ("setTempCost(")) {
			List<CardParam> lcp = _lastObj as List<CardParam>;
			ChangeTemp (lcp,TargetParam.COST, (int)_InRound, _cp.Pass,CalType.SET);
			lcp = ReloadCard (lcp);
			ReturnObj = (lcp);
		}
		if (_word.Contains ("addTempCost(")) {
			List<CardParam> lcp = _lastObj as List<CardParam>;
			ChangeTemp (lcp,TargetParam.COST, (int)_InRound, _cp.Pass,CalType.ADD);
			lcp = ReloadCard (lcp);
			ReturnObj = (lcp);
		}

		return new SkillStruct ().Set (ReturnObj, _SquareDict);
	}

	/// <summary>
	/// 返り値はSkillStruct型
	/// </summary>
	IEnumerator L_GetObjAction (CardParam _cp,string _text,Dictionary<string,object> _SquareDict,object _lastObj, string _word,object _InRound) {
		//不要なものを除去
		_word = _word.Replace (";", "");
		_word = _word.Replace ("!", "");

		//カッコ内削除
		_word = L_RemoveRoundBrakets (_word);

		int user = PlayerOrEnemy (_cp,false);
		int NotUser = PlayerOrEnemy (_cp,true);

//		List<object> PileObj = new List<object>();
		object ReturnObj = null;

		//場合分け
		switch (_word) {
		case "select1":
			{
				
				var lcp = (List<CardParam>)_lastObj;
				int targetCount = ((List<CardParam>)_lastObj).Count;

				//対象なし
				if (targetCount == 0) {
					ReturnObj = (new List<CardParam> ());
				} else {

					int field = GetPos ((lcp) [0].Pass) [0];
					List<int> gamen = new List<int> (){ -1, 0, 1 };

					if (targetCount == 1) {//1枚
						ReturnObj = _lastObj;
					} else if (gamen.Contains (field)) {//盤面対象
						//エフェクト表示
						SetSelectEffect ((List<CardParam>)_lastObj);
						//モード
						mode = Mode.SELECT;
						//リフレッシュ
						Refresh();
						ImageRefresh ();
						//選択肢を設定
						SelectChoices = (List<CardParam>)_lastObj;
						//選択するまで待つ & fpsセット
						Application.targetFrameRate = 30;
						Select1Pass = -1;
						while (Select1Pass == -1)
							yield return null;
						Application.targetFrameRate = 60;
						//選択後、
						mode = Mode.NOMAL;
						RemoveAllSelectEffect ();
						List<CardParam> temp = new List<CardParam> ();
						temp.Add (GetCard (Select1Pass));

						ReturnObj = (temp);
					} else {//盤面外(デッキ等)
						//選択対象をセット
//					SelectWindowCards = lcp;
						SelectChoices = lcp;
						SelectWindow.SetActive (true);
						windowOpen = true;
						SelectCardAndBG.SetActive (true);

						SelectWindowArraws [0].color = Color.gray;


						//その他表示
						nowPage = 0;//1ページ目
						RefreshSelectWindow ();

						//モード
						mode = Mode.SELECT;

						//選択するまで待つ
						Select1Pass = -1;
						while (Select1Pass == -1)
							yield return null;
						//選択後、
						SelectWindow.SetActive (false);
						windowOpen = false;
						SelectCardAndBG.SetActive (false);

						mode = Mode.NOMAL;
						List<CardParam> temp = new List<CardParam> ();
						temp.Add (GetCard (Select1Pass));
						ReturnObj = (temp);
					}
				}
			}
			break;

		case "removeSkill":
			{
				yield return StartCoroutine (S_RemoveSkill (((List<CardParam>)_lastObj), _cp.Pass));
				ReturnObj= (ReloadCard( (List<CardParam>)_lastObj));
			}
			break;
		}

		//その他場合分け

		//実行系

		if (_word.Contains ("draw(")) {//プレイヤーの指定が必要
			int target = (int)_lastObj;
			SEPlay (13);
			for (int i = 0; i < (int)(_InRound); i++ ){
				yield return  StartCoroutine(S_MoveCard(GetCard(target+2,0).Pass,new int[]{target,-1},false));

			}
			yield return new WaitForSeconds (WaitTime);

			ReturnObj = _lastObj;
		}


//		if(_word.Contains("moveTo(")){//万能移動
//			int target = (int)_lastObj;
//
//		}
		if (_word.Contains ("setWeather(")) {
			//実行
			yield return  StartCoroutine(S_SetWeather(_cp.Pass,(int)(_InRound)));
			ReturnObj = (NowWeather);
		}
		if (_word.Contains ("setRole(")) {
			yield return StartCoroutine (S_ChangeRole ((List<CardParam>)_lastObj, (int)_InRound));
			ReturnObj = ((List<CardParam>)_lastObj);
		}
		if(_word.Contains("reset")){
			yield return StartCoroutine (S_ResetCard ((List<CardParam>)_lastObj,_cp.Pass));
			ReturnObj = ((List<CardParam>)_lastObj);
		}
		if(_word.Contains("addEffect(")){
			yield return StartCoroutine (S_AddEffect ((List<CardParam>)_lastObj, (int)_InRound,0,_cp.Pass));
			ReturnObj = ((List<CardParam>)_lastObj);
		}
		if(_word.Contains("changeCard(")){
			yield return StartCoroutine (S_ChangeCard ((List<CardParam>)_lastObj, (int)_InRound,_cp.Pass));
			ReturnObj = ((List<CardParam>)_lastObj);
		}

		if (_word.Contains ("add") || _word.Contains ("set")|| _word.Contains ("mul")||_word.Contains ("divi")) {

			CalType ct = CalType.ADD;
			if (_word.Contains ("set"))
				ct = CalType.SET;
			if (_word.Contains ("mul"))
				ct = CalType.MUL;
			if (_word.Contains ("divi"))
				ct = CalType.DIVIDE;

			if (_word.Contains ("LP") || _word.Contains ("SP")) {
				if (_word.Contains ("LP")) {
					yield return StartCoroutine( S_ChangeLP ((int)_lastObj, (int)_InRound, true,_cp.Pass,ct));
					ReturnObj = (GetLP((int)_lastObj));
				} else {
					yield return StartCoroutine( S_ChangeSP ((int)_lastObj, (int)_InRound, true,_cp.Pass,ct));
					ReturnObj = (GetSP((int)_lastObj));
				}
			} else if(_word.Contains ("Cost") || _word.Contains ("Power")){
				//実行
				List<CardParam> lcp = new List<CardParam> ();
				lcp.AddRange (_lastObj as List<CardParam>);

				//
				TargetParam tp = TargetParam.COST;
				if (_word.Contains ("Cost"))
					tp = TargetParam.COST;
				if (_word.Contains ("Power"))
					tp = TargetParam.POWER;

				if (_word.Contains ("Temp")) {//一時的変化
					ChangeTemp(lcp,tp,(int)_InRound,_cp.Pass,ct);
					lcp = ReloadCard (lcp);
					ReturnObj = (lcp);

				} else {//能力変化
					yield return StartCoroutine (S_ChangeParams (lcp, tp, (int)_InRound, _cp.Pass, ct));

					lcp = ReloadCard (lcp);
					ReturnObj = (lcp);
				}
			}
		}
		//


		//移動

		if (_word.Contains ("exchange(")) {
			List<CardParam> lcp = new List<CardParam> ();
			lcp.AddRange (_lastObj as List<CardParam>);
			var inr = (List<CardParam>)_InRound;
			if (lcp!= null && lcp.Count == 1 && inr != null && inr.Count == 1) {
				//交換成功
				Debug.Log("ex"+inr[0].Name);
				yield return StartCoroutine (S_ExchangeCard (lcp[0].Pass,inr[0].Pass));
			}

			ReturnObj = ((List<CardParam>)_lastObj);
		}
		if (_word.Contains ("break")) {
			List<CardParam> lcp = new List<CardParam> ();
			lcp.AddRange (_lastObj as List<CardParam>);

			yield return StartCoroutine (S_RemoveCard (lcp,TargetField.BREAK,_cp.Pass));
			ReturnObj = ((List<CardParam>)_lastObj);
		}
		if (_word.Contains ("toCre")) {
			List<CardParam> lcp = new List<CardParam> ();
			lcp.AddRange (_lastObj as List<CardParam>);
			yield return StartCoroutine (S_RemoveCard (lcp, TargetField.CREATURE,_cp.Pass));

			ReturnObj = ((List<CardParam>)_lastObj);
		}

		if (_word.Contains ("toHand")) {
			List<CardParam> lcp = new List<CardParam> ();
			lcp.AddRange (_lastObj as List<CardParam>);


			yield return StartCoroutine (S_RemoveCard (lcp,TargetField.BOUNCE,_cp.Pass));

			ReturnObj = ((List<CardParam>)_lastObj);
		}

		if (_word.Contains ("toDeckTop")) {
			List<CardParam> lcp = new List<CardParam> ();
			lcp.AddRange (_lastObj as List<CardParam>);

			yield return StartCoroutine (S_RemoveCard (lcp,TargetField.DECKTOP,_cp.Pass));
			ReturnObj = ((List<CardParam>)_lastObj);
		}
		if (_word.Contains ("toDeckBottom")) {
			List<CardParam> lcp = new List<CardParam> ();
			lcp.AddRange (_lastObj as List<CardParam>);
			yield return StartCoroutine (S_RemoveCard (lcp,TargetField.DECKBOTTOM,_cp.Pass));
			ReturnObj = ((List<CardParam>)_lastObj);
		}


		if (ReturnObj == null) {
			Debug.LogError ("エラー:"+_text +"["+ _word+"]");
		} else
			yield return new SkillStruct ().Set (ReturnObj, _SquareDict);
	}
	#endregion

//	string L_GetSquareBrackets(string _text){
//		if (!_text.Contains ("["))
//			return "";
//		int startIndex = -1;
//		int endIndex = -1;
//		//		//1つ目の括弧を消す。
//		//		int index = _text.IndexOf ("(");
//		//		_text.Remove (0, index+1);
//		//		count++;
//		//現在入れ子になっている数
//		int count = 0;
//		for (int i = 0; i < _text.Length; i++ ){
//			char x = _text [i];
//			if (x == '[') {
//				count++;
//				if(startIndex == -1)
//					startIndex = i;
//			}
//			if (x == ']') {
//				count--;
//				if(endIndex == -1 && count == 0)
//					endIndex = i;
//			}
//		}
//
//		// "xx(xx(x)x)xx" startIndex = 2 endIndex = 9
//		return _text.Substring (startIndex + 1, endIndex - startIndex - 1);
//	}

	string L_RemoveRoundBrakets (string _text) {
		string rem = L_GetRoundBrackets (_text);//括弧内のワード(スクリプト込み)
		if (rem != "") {
			return _text.Replace (rem, "");
		} else {
			return _text;
		}
	}
	string L_GetRoundBrackets (string _text) {
		return SystemScript.GetRoundBrackets (_text);
	}


	IEnumerator Skill_MoveCard (List<int> _Passes,TargetField _TargetField) {
		for (int i = 0; i < _Passes.Count; i++ ){
			int pass = _Passes [i];
			switch (_TargetField) {
			case TargetField.BOUNCE:
				{
					
				}
				break;
			case TargetField.BREAK:
				{
					ToGrave (pass);
				}
				break;
			}
		}
		yield return null;
	}

	public void OnRecieve(int _num,int _tag){
		SceneManagerx.Instance.NewScene (1);
	}
	int[] GetUsers(){
		if (Initiative == 0)
			return new int[]{ 0, 1 };
		else
			return new int[]{ 1, 0 };
	}
	int[] ReversalUsers() {
		if (Initiative == 0)
			return new int[]{ 1,0 };
		else
			return new int[]{ 0,1 };
	}
	IEnumerator SummonCard () {
		bool Summoned = false;
		for (int i = 0; i < 2; i++ ){
			int pass = SelectingPasses [i][0];
			if (pass == -2)//手札が無いから何もしない
				continue;
			
			int SummonOrDiscard = SelectingPasses [i] [1];
			if (SummonOrDiscard == 0) {//召喚
				Debug.Log ("召喚:"+pass);
				ToCreature (pass);
//				S_Move(pass,new int[]{-1,i});
				Summoned = true;
				yield return StartCoroutine (S_ChangeSP (i, -GetCard (pass).GetCalCost (), false));
			} else if (SummonOrDiscard == -1) {//破棄
				S_Discard (pass);
				Debug.Log ("破棄:"+pass);
			}
		}
		//効果音
		if (Summoned)
			SEPlay (1); //召喚時の音
		else
			SEPlay(7); //鈴の音

		//手札引き直し
		DealCard(0);
		DealCard (1);
	}



	#region 便利機能


	void MakeDeckOffline (List<CardParam> _playerDeck,List<CardParam> _enemyDeck) {
		PlayerDeck = new List<CardParam> ();
		//		for (int i = 0; i < _playerDeck; i++ ){
		//			CardParam cp = _playerDeck [i];
		//			CardParam x = new CardParam ();
		//			x.Set (cp.Atr, cp.ID, cp.LV, 1);
		//			x.Pass = MakePass ();
		//			PlayerDeck.Add (x);
		//		}
		EnemyDeck = new List<CardParam> ();
		//		for (int i = 0; i < _enemyDeck; i++ ){
		//			CardParam cp = _enemyDeck [i];
		//			CardParam x = new CardParam ();
		//			x.Set (cp.Atr, cp.ID, cp.LV, 1);
		//			x.Pass = MakePass ();
		//			EnemyDeck.Add (x);
		//		}

		for (int i = 0; i < _playerDeck.Count; i++ ){
			CardParam cp = _playerDeck [i];
			for (int i2 = 0; i2 < cp.Count; i2++ ){
				CardParam x = new CardParam ();
				x.Set (cp.Atr, cp.ID, cp.LV, 1);
				x.Pass = MakePass ();
				PlayerDeck.Add (x);
			}
		}
		for (int i = 0; i < _enemyDeck.Count; i++ ){
			CardParam cp = _enemyDeck [i];
			for (int i2 = 0; i2 < cp.Count; i2++ ){
				CardParam x = new CardParam ();
				x.Set (cp.Atr, cp.ID, cp.LV, 1);
				x.Pass = MakePass ();
				EnemyDeck.Add (x);
			}
		}
	}
	void MakePasses () {
		//		List<CardParam> d = new List<CardParam> ();
		int[] users = GetUsers ();
		for (int j = 0; j < users.Length; j++ ){
			int u = users [j];
			List<CardParam> d = GetDeck (u);
			List<CardParam> lcp = new List<CardParam> ();
			for (int i = 0; i < d.Count; i++ ){
				CardParam cp = d [i];
				for (int i2 = 0; i2 < cp.Count; i2++ ){
					CardParam x = new CardParam ();
					x.Set (cp.Atr, cp.ID, cp.LV, 1);
					x.Pass = MakePass ();
					lcp.Add (x);
				}
			}
			if (u == 0)
				PlayerDeck = lcp;
			else
				EnemyDeck = lcp;
		}
	}

	int MakePass (){
		PassNum++;
		return PassNum;
	}
	void DeckShuffle (int _Target) {
		List<CardParam> lcp  = new List<CardParam> ();
		if(_Target == 0)
			lcp = GetField (2);
		if(_Target == 1)
			lcp = GetField (3);

		for (int i = 0; i < lcp.Count; i++ ){
			CardParam temp = lcp [i];
			int randomIndex = Random.Range (0, lcp.Count);
			lcp [i] = lcp [randomIndex];
			lcp [randomIndex] = temp;
		}
	}
	List<CardParam> CardShuffle (List<CardParam> _lcp) {
		for (int i = 0; i < _lcp.Count; i++ ){
			CardParam temp = _lcp [i];
			int randomIndex = Random.Range (0, _lcp.Count);
			_lcp [i] = _lcp [randomIndex];
			_lcp [randomIndex] = temp;
		}
		return _lcp;
	}
	void Refresh () {
		for (int i = 0; i < 2; i++ ){
			userObjs [i].LPText.text = LPs [i].ToString ();
			userObjs [i].SPText.text = SPs [i].ToString ();
			userObjs[i].PlayerDataText.text = System.String.Format( "山札:{0}枚 墓地:{1}",GetDeck(i).Count.ToString(),GetGrave(i).Count.ToString());
		}
		for (int i = 0; i < 5; i++ ){

		}
		TurnText.text = "TURN "+ NowTurn;

	}




	/// <summary>
	/// 0=自分 1=対戦相手
	/// </summary>
	void DealCard (int _Target) {
		if (_Target == 0) {//自分を対象
			if (PlayerHand.Count < MAX_NUM_OF_HANDS) {
				//追加
				int DrawCount = MAX_NUM_OF_HANDS-PlayerHand.Count;
				for (int i = 0; i < DrawCount; i++ ){
					S_Draw (0);
				}
			}
//
//			while (PlayerHand.Count > MAX_NUM_OF_HANDS) {
//				//山札に戻す
//				MoveCard(new int[]{0,PlayerHand.Count-1},new int[]{2,0});
//
//			}
		}
		if(_Target == 1){//対戦相手を対象
			if (EnemyHand.Count < MAX_NUM_OF_HANDS) {
				//追加
				int DrawCount = MAX_NUM_OF_HANDS-EnemyHand.Count;
				for (int i = 0; i < DrawCount; i++ ){
					S_Draw (1);
				}
			}
//
//			while (EnemyHand.Count > MAX_NUM_OF_HANDS) {
//				//山札に戻す
//				MoveCard(new int[]{1,PlayerHand.Count-1},new int[]{3,0});
//
//			}
		}
	}

	#endregion

	#region Get系



	/// <summary>
	/// -1=Creatures
	/// 0=PlayerHand 1=EnemyHand
	/// 2=PlayerDeck 3=EnemyDeck
	/// 4=PlayerGrave 5=EnemyGrave
	/// </summary>
	CardParam GetCard(int _Field,int _num) {
		List<CardParam> cp = GetField (_Field);
		if (_num < cp.Count) {
			return cp [_num];
		} else {
			Debug.LogError (_num + " is Null");
			return new CardParam ().Reset();
		}
	}

	int GetLP (int _Target) {
		return LPs [_Target];
	}
	int GetSP (int _Target){
		return SPs [_Target];
	}

	List<CardParam> GetCards (List<int> _pass) {
		List<CardParam> lcp = new List<CardParam> ();
		for (int i = 0; i < _pass.Count; i++ ){
			lcp.Add (GetCard( _pass[i]));
		}
		return lcp;
	}

	/// <summary>
	/// 存在しなければCardParam ().Reset();
	/// </summary>
	CardParam GetCard (int _Pass) {
		if (_Pass > 0) {
			for (int i = -1; i < 6; i++) {
				List<CardParam> lcp = GetField (i);
				for (int i2 = 0; i2 < lcp.Count; i2++) {
					if (lcp [i2].Pass == _Pass)
						return lcp [i2];
				}
			}
		}

		Debug.LogError (_Pass + "(pass) is Null");
		return new CardParam ().Reset();
	}

	int[] GetPos (int _Pass) {
		for (int i = -1; i < 6; i++ ){
			List<CardParam> lcp = GetField (i);
			for (int i2 = 0; i2 < lcp.Count; i2++ ){
				if (lcp [i2].Pass == _Pass)
					return new int[]{ i, i2 };
			}
		}
		Debug.LogError (_Pass + "(pass) is Null");
		return new int[]{ };
	}

	/// <summary>
	/// -1=Creatures
	/// 0=PlayerHand 1=EnemyHand
	/// 2=PlayerDeck 3=EnemyDeck
	/// 4=PlayerGrave 5=EnemyGrave
	/// </summary>
	List<CardParam> GetField (int _Field) {
		switch (_Field) {
		case -1:
			return Creatures;
			break;
		case 0:
			return PlayerHand;
			break;
		case 1:
			return EnemyHand;
			break;
		case 2:
			return PlayerDeck;
			break;
		case 3:
			return EnemyDeck;
			break;
		case 4:
			return PlayerGrave;
			break;
		case 5:
			return EnemyGrave;
			break;
		}
		Debug.LogError (_Field + " is Unknown Field");
		return new List<CardParam> ();
	}
	CardParam GetCreature (int _Target) {
		if (_Target == 0)
			return Creatures [0];
		if (_Target == 1)
			return Creatures[1];
		Debug.LogError ("Error");
		return new CardParam();
	}

	List<CardParam> GetHands (int _Target) {
		if (_Target == 0)
			return PlayerHand;
		if (_Target == 1)
			return EnemyHand;
		Debug.LogError ("Error");
		return null;
	}
	List<int> GetHandsPass(int _Target){
		if (!(_Target == 0 || _Target == 1)) {
			Debug.LogError ("Error:"+_Target);
			return new List<int> ();
		}
		List<int> Ret = new List<int>();
		var hand = GetHands (_Target);
		for (int i = 0; i < hand.Count; i++ ){
			Ret.Add( hand [i].Pass);
		}
		return Ret;
	}

	List<CardParam> GetDeck (int _Target) {
		if (_Target == 0)
			return PlayerDeck;
		if (_Target == 1)
			return EnemyDeck;
		Debug.LogError ("Error");
		return null;
	}
	List<CardParam> GetGrave (int _Target) {
		if (_Target == 0)
			return PlayerGrave;
		if (_Target == 1)
			return EnemyGrave;
		Debug.LogError ("Error");
		return null;
	}
	List<Transform> GetHandPoses (int _Target) {
		if(_Target == 0|| _Target == 1)
			return userObjs [_Target].ImagePoses;
		Debug.LogError ("Error");
		return null;
	}
	Transform GetCreaturePos (int _Target) {
		if (_Target == 0 || _Target == 1)
			return userObjs [_Target].Creature;
		Debug.LogError ("Error");
		return null;
	}

	#endregion

	//移動系(上位)

	#region [S_系]
	bool S_Draw (int _Target) {
		if (_Target == 0) {//自分を対象
			if (PlayerHand.Count < MAX_NUM_OF_HANDS) {
				MoveCard (new int[]{ 2, 0 }, new int[]{ 0, -1 });
				return true;
			}
		}
		if(_Target == 1){//対戦相手を対象
			if (EnemyHand.Count < MAX_NUM_OF_HANDS) {
				MoveCard (new int[]{ 3, 0 }, new int[]{ 1, -1 });
				return true;
			}
		}
		return false;
	}

	//自然消滅
	void S_Used (int _Pass) {
		ToGrave (_Pass);
	}
	//破壊
	void S_Destroy (int _Pass) {
		ToGrave (_Pass);
	}
	//破棄
	void S_Discard (int _Pass) {
		ToGrave (_Pass);
	}

	#endregion

	#region スキル系
//	IEnumerator S_SelectWindow (List<CardParam> lcp) {
//		if(lcp.Count == 0)
//			yield break;
//		else if (lcp.Count == 1)
//			yield return lcp;
//		else {
//			//選択対象をセット
//			SelectWindowCards = lcp;
//			SelectWindow.SetActive (true);
//			SelectWindowArraws [0].color = Color.gray;
//
//			//アクティブ化&画像変更
//			for (int i = 0; i < 10; i++ ){
//				if (i < lcp.Count) {
//					SelectWindowCardsImage [i].gameObject.SetActive (true);
//					SelectWindowCardsImage [i].Set (lcp [i]);
//				} else {
//					SelectWindowCardsImage [i].gameObject.SetActive (false);
//				}
//			}
//
//			//その他表示
//			nowPage = 0;//1ページ目
//			RefreshSelectWindow();
//
//			//モード
//			mode = Mode.SELECT;
//		}
//
//	}
	void RefreshSelectWindow () {
		//アクティブ化&画像変更

		for (int i = 0; i < 10; i++ ){
			int x = i + nowPage * 10;
			if (x < SelectChoices.Count) {
				SelectWindowCardsImage [i].gameObject.SetActive (true);
				SelectWindowCardsImage [i].Set (SelectChoices [x]);
			} else {
				SelectWindowCardsImage [i].gameObject.SetActive (false);
			}
		}

		var lcp = SelectChoices;
		int y = ((lcp.Count - 1) / 10);
		Debug.Log (y);
		int MaxPage = y;//実際はこれに+1した値
		if (nowPage == 0) {//<の設定
			SelectWindowArraws [0].color = Color.gray;
		} else
			SelectWindowArraws [0].color = Color.white;
		
		if (nowPage == MaxPage) {//>の設定
			SelectWindowArraws [1].color = Color.gray;
		} else {
			SelectWindowArraws [1].color = Color.white;
		}

		int field = GetPos (lcp [0].Pass)[0];//カードの1枚目を見て判断する。
		string title = "";
		switch (field) {
		case 2:
			title += "自分のデッキ";
			break;
		case 3:
			title += "相手のデッキ";
			break;
		case 4:
			title += "自分の墓地";
			break;
		case 5:
			title += "相手の墓地";
			break;

		}
		title += " " + (nowPage + 1) +"/"+ (MaxPage + 1);
		SelectWindowTitle.text = title;
	}
	IEnumerator S_SetWeather (int _Pass,int _Weather) {
		NowWeather = _Weather;
		Weather.text = "快晴";
		if (_Weather == -1)
			WeatherImage.color = Color.gray;
		else {
			WeatherImage.color = DataManager.Instance.AttributeColors [NowWeather];
			switch (NowWeather) {
			case 0:
				Weather.text = "灼熱天候";
				break;
			case 1:
				Weather.text = "大雨天候";
				break;
			case 2:
				Weather.text = "竜巻天候";
				break;
			case 3:
				Weather.text = "光明天候";
				break;
			case 4:
				Weather.text = "暗黒天候";
				break;
			}
		}
		SEPlay (10);
		yield return new WaitForSeconds (WaitTime);
	}
	IEnumerator S_ChangeRole (List<CardParam> _Cards,int _Role) {
		_Cards = ReloadCard (_Cards);
		for (int i = 0; i < _Cards.Count; i++ ){
			CardParam cp = _Cards [i];
			cp.Role = _Role;
			UpdateCard (cp);
			Effect ("PowerUp", cp.Pass);
			SEPlay (10);
		}
		Refresh ();
		ImageRefresh ();
		yield return new WaitForSeconds (WaitTime);

		yield break;
	}
	IEnumerator S_RemoveSkill (List<CardParam> _Cards,int _Invoker) {
		_Cards = ReloadCard (_Cards);
		for (int i = 0; i < _Cards.Count; i++) {
			CardParam cp = _Cards [i];

			//効果

			Dictionary<string, object> objs = new Dictionary<string, object>();
			objs.Add ("targetCard",new List<CardParam>{cp});
			objs.Add ("invoker",new List<CardParam>{GetCard( _Invoker)});
			objs.Add ("active", true);
			var coro = CheckCommonSkill (Timing.REMOVE_SKILL, objs);
			yield return  StartCoroutine (coro);
			objs = (Dictionary<string, object>)coro.Current;

			cp = ((List<CardParam>)objs ["targetCard"])[0];
			if ((bool)objs ["active"] == false)//無効化フラグ
				continue;

			cp.SkillTexts = new List<string> ();
			cp.SkillScript = new List<string> ();

			UpdateCard (cp);//カード更新

			Effect ("Attack2", cp.Pass);
			SEPlay (11);
		}

		Refresh ();
		ImageRefresh ();
		yield return new WaitForSeconds (WaitTime);
		yield break;
	}
	IEnumerator S_MoveCard (int _pass,int[] _to,bool _Wait = true) {
		//int[] f = GetPos(_pass);
		MoveCard (_pass, _to);
		Refresh ();
		ImageRefresh ();
		if(_Wait)
		yield return new WaitForSeconds (WaitTime);
	}
	IEnumerator S_ResetCard (List<CardParam> _Cards,int invoer) {
		
		_Cards = ReloadCard (_Cards);

		for (int i = 0; i < _Cards.Count; i++ ){
			CardParam cp = _Cards[i];

			//特殊能力チェック
			Dictionary<string, object> objs = new Dictionary<string, object>();
			objs.Add ("targetCard",new List<CardParam>{cp});
			objs.Add ("invoker",new List<CardParam>{GetCard( invoer)});
			objs.Add ("active", true);

			var coro = CheckCommonSkill (Timing.PARAM_CHANGE, objs);
			yield return  StartCoroutine (coro);
			objs = (Dictionary<string, object>)coro.Current;

			cp = ((List<CardParam>)objs ["targetCard"])[0];

			if ((bool)objs ["active"] == false)//無効化フラグ
				continue;

			//カード変更
			cp.Set (cp.Atr, cp.ID, cp.LV, cp.Count,cp.uid);
			UpdateCard (cp);

			//エフェクト表示
			Effect ("Attack2", cp.Pass);
			SEPlay (11);
		}
		Refresh ();
		ImageRefresh ();
		yield return new WaitForSeconds (WaitTime);
		yield break;
	}
	IEnumerator S_ChangeCard (List<CardParam> _Cards,int CardID,int invoer) {
		_Cards = ReloadCard (_Cards);

		for (int i = 0; i < _Cards.Count; i++ ){
			CardParam cp = _Cards[i];

			//特殊能力チェック
			Dictionary<string, object> objs = new Dictionary<string, object>();
			objs.Add ("targetCard",new List<CardParam>{cp});
			objs.Add ("invoker",new List<CardParam>{GetCard( invoer)});
			objs.Add ("active", true);

			var coro = CheckCommonSkill (Timing.PARAM_CHANGE, objs);
			yield return  StartCoroutine (coro);
			objs = (Dictionary<string, object>)coro.Current;

			cp = ((List<CardParam>)objs ["targetCard"])[0];

			if ((bool)objs ["active"] == false)//無効化フラグ
				continue;

			//カード変更
			cp.Set (cp.Atr, CardID, cp.LV, cp.Count,cp.uid);
			UpdateCard (cp);

			//エフェクト表示
			Effect ("Attack2", cp.Pass);
			SEPlay (11);
		}
		Refresh ();
		ImageRefresh ();
		yield return new WaitForSeconds (WaitTime);
		yield break;
		
	}
	IEnumerator S_ChangeParams (List<CardParam> _Cards,TargetParam _TargetParam,int _Point,int _Invoker,CalType _calType) {
//		Dictionary<string,object> objs = new Dictionary<string, object> ();
//		objs[""
//		yield return StartCoroutine(CheckCommonSkill (GetCreature (GetUsers () [i]), Timing.PARAM_CHANGE))


		_Cards = ReloadCard (_Cards);

		for (int i = 0; i < _Cards.Count; i++ ){
			CardParam cp = _Cards[i];

			int DefaultPoint = 0;

			//初期値を設定
			if (_TargetParam == TargetParam.COST)
				DefaultPoint = cp.Cost;
			if (_TargetParam == TargetParam.POWER)
				DefaultPoint = cp.Power;

			//計算方法を設定

			if (_calType == CalType.MUL)
				_Point = DefaultPoint * (_Point -1);
			if (_calType == CalType.DIVIDE)
				_Point =  ( DefaultPoint / _Point)- DefaultPoint;
			if (_calType == CalType.SET)
				_Point = _Point - DefaultPoint;
			
			//特殊能力チェック
			Dictionary<string, object> objs = new Dictionary<string, object>();
			objs.Add ("targetCard",new List<CardParam>{cp});
			objs.Add ("targetParam", _TargetParam.ToString ());
			objs.Add ("point", _Point);
			objs.Add ("invoker",new List<CardParam>{GetCard( _Invoker)});
			objs.Add ("active", true);

			var coro = CheckCommonSkill (Timing.PARAM_CHANGE, objs);
			yield return  StartCoroutine (coro);
			objs = (Dictionary<string, object>)coro.Current;

			cp = ((List<CardParam>)objs ["targetCard"])[0];
			_Point = (int)objs ["point"];

			if ((bool)objs ["active"] == false)//無効化フラグ
				continue;

			//コストとパワーで場合分け
			switch (_TargetParam) {
			case TargetParam.COST:
				{
					if (cp.Cost + _Point < 0)
						cp.Cost = 0;
					else
						cp.Cost += _Point;

					if (_Point < 0) {
						Effect ("PowerUp", cp.Pass);
						SEPlay (10);
					} else if (_Point > 0) {
						Effect ("Attack2", cp.Pass);
						SEPlay (11);
					}
				}
				break;
			case TargetParam.POWER:
				{
					if (cp.Power + _Point < 0)
						cp.Power = 0;
					else
						cp.Power += _Point;

					if (_Point > 0) {
						Effect ("PowerUp", cp.Pass);
						SEPlay (10);
					} else if (_Point < 0) {
						Effect ("Attack2", cp.Pass);
						SEPlay (11);
					}
					
				}
				break;

			}
			UpdateCard (cp);
		}
		Refresh ();
		ImageRefresh ();
		yield return new WaitForSeconds (WaitTime);
		yield break;
	}
	IEnumerator S_RemoveCard (List<CardParam> _Cards,TargetField _TargetField,int _invoker,bool _wait = true) {
		for (int i = 0; i < _Cards.Count; i++) {
			//特殊能力チェック
			CardParam cp = _Cards[i];
			Dictionary<string, object> objs = new Dictionary<string, object>();
			objs.Add ("targetCard",new List<CardParam>{cp});
			objs.Add ("targetField", _TargetField.ToString ());
			objs.Add ("invoker",new List<CardParam>{GetCard( _invoker)});
			objs.Add ("active", true);

			Timing timing = Timing.BREAK;
			switch (_TargetField) {
			case TargetField.BREAK:
				timing = Timing.BREAK;
				break;
			case TargetField.BOUNCE:
			case TargetField.CREATURE:
			case TargetField.DECKBOTTOM:
			case TargetField.DECKTOP:
				timing = Timing.MOVE;
				break;
			}
			var coro = CheckCommonSkill (timing, objs);
			yield return  StartCoroutine (coro);
			objs = (Dictionary<string, object>)coro.Current;

			cp = ((List<CardParam>)objs ["targetCard"])[0];

			if ((bool)objs ["active"] == false)//無効化フラグ
				continue;

			//効果発動
			int pass = _Cards [i].Pass;
			switch (_TargetField) {
			case TargetField.BREAK:
				{
					Effect ("Break", pass);//先にエフェクト表示
					SEPlay(11);
					ToGrave (pass);

				}
				break;
			case TargetField.BOUNCE:
				{
					SEPlay (13);
					ToHand (pass);
				}
				break;
			case TargetField.DECKBOTTOM:
				{
					ToDeck (pass);
				}
				break;
			case TargetField.DECKTOP:
				{
					ToDeck (pass, true);
				}
				break;
			case TargetField.CREATURE:
				{
					SEPlay (1);
					ToCreature (pass,PlayerOrEnemy(pass));
				}
				break;
			}
		}
		Refresh ();
		ImageRefresh ();
		if (_wait) {
			yield return new WaitForSeconds (WaitTime);
		}

	}
//	void S_ExchangeCard (int _a,int _b) {
//		ExchangeCard (_a, _b);
//	}
	IEnumerator S_ExchangeCard (int _a,int _b) {
		ExchangeCard (_a, _b);
		Refresh ();
		ImageRefresh ();
		yield return new WaitForSeconds (WaitTime);
	}

	IEnumerator S_AddEffect (List<CardParam> _cards,int _effect,int _effValue,int _Invoker,bool _Wait = true) {
		_cards = ReloadCard (_cards);
		for (int i = 0; i < _cards.Count; i++ ){
			CardParam cp = _cards [i];
			//特殊能力チェック
			Dictionary<string, object> objs = new Dictionary<string, object>();
			objs.Add ("targetCard",new List<CardParam>{cp});
			objs.Add ("targetEffect",_effect);
			objs.Add ("targetEffectValue",_effValue);
			objs.Add ("invoker",new List<CardParam>{GetCard( _Invoker)});
			objs.Add ("active", true);//無効化フラグ

			var coro = CheckCommonSkill (Timing.EFFECT, objs);
			yield return  StartCoroutine (coro);
			objs = (Dictionary<string, object>)coro.Current;

			cp = ((List<CardParam>)objs ["targetCard"])[0];
			_effect = (int)objs ["targetEffect"];
			_effValue = (int)objs ["targetEffectValue"];

			if ((bool)objs ["active"] == false)//無効化フラグ
				continue;

			//凍結、拘束、火傷の継続ターン数
			switch (_effect) {
			case 1:
				_effValue = 2;
				break;
			case 2:
				_effValue = 3;
				break;
			case 3:
				_effValue = 2;
				break;
			}
			//効果付与実行
			List<int> eff= new List<int>();
			eff.AddRange (cp.Effect);
			eff.Add (_effect);

			List<int> effV= new List<int>();
			effV.AddRange (cp.EffectValue);
			effV.Add (_effValue);

			cp.Effect = eff;
			cp.EffectValue = effV;
			//		_cp.Effect.Add (_effect);
			//		_cp.EffectValue.Add (_effValue);
			UpdateCard (cp);

			Effect ("Attack2",cp.Pass);
			SEPlay (11);
		}
		if (_Wait) {
			Refresh ();
			ImageRefresh ();
			yield return new WaitForSeconds (WaitTime);
		}
	}
	/// <summary>
	/// _effect=-1で全て解除
	/// </summary>
	IEnumerator S_RemoveEffect (List<CardParam> _cards,int _effect,int _Invoker,bool _wait = true) {
		_cards = ReloadCard (_cards);
		//状態異常解除

		for (int i = 0; i < _cards.Count; i++ ){
			CardParam cp = _cards [i];

			if (_wait) {
				//特殊能力チェック
				Dictionary<string, object> objs = new Dictionary<string, object> ();
				objs.Add ("targetCard", new List<CardParam>{ cp });
				objs.Add ("targetEffect", _effect);
				objs.Add ("invoker", new List<CardParam>{ GetCard (_Invoker) });
				objs.Add ("active", true);//無効化フラグ

				var coro = CheckCommonSkill (Timing.REMOVE_EFFECT, objs);
				yield return  StartCoroutine (coro);
				objs = (Dictionary<string, object>)coro.Current;

				cp = ((List<CardParam>)objs ["targetCard"]) [0];
				_effect = (int)objs ["targetEffect"];

				if ((bool)objs ["active"] == false)//無効化フラグ
				yield break;
			}

			for (int i3 = cp.Effect.Count-1; i3 >= 0; i3-- ){
				int eff = cp.Effect [i3];
				if (eff == _effect || _effect == -1) {
					cp.Effect.RemoveAt (i3);
					cp.EffectValue.RemoveAt (i3);
				}
			}
		}

		if (_wait) {
			Refresh ();
			ImageRefresh ();
			yield return new WaitForSeconds (WaitTime);
		}

	}


	void S_AddSkill(int _Pass,string _Skill) {
		CardParam cp = GetCard (_Pass);
		cp.SkillTexts.Add (_Skill);
		UpdateCard (cp);
	}

	#endregion

	#region 移動系
	//移動系(下位) (スキルなどから直接読んではいけない)

	bool ToGrave (int _Pass,TargetGrave _TargetGrave = TargetGrave.BREAK) {
		if (!ExistCard (_Pass))
			return false;
		Debug.Log ("ToGrave");
		CardImageScript cardImage = GetCardImage (GetCard (_Pass));
		//カード画像消去
		if (cardImage != null) {
			var slid = cardImage.gameObject.GetComponent<SlidableButton> ();
			slid.isButtonDown = false;
			slid.isPointerDown = false;
			slid.isPointerIn = false;
			cardImage.gameObject.SetActive (false);
		}
		int[] From = GetPos (_Pass);
		int ToField = 4;
		if (PlayerOrEnemy (From) == 1)
			ToField = 5;
		int[] To = new int[]{ ToField, -1 };
		return MoveCard (From,To);
	}
	/// <summary>
	/// カードを持ち主の手札に戻す
	/// </summary>
	/// <param name="_Left">1番左にカードを持ってくるかどうか</param>
	/// <param name="_Target">-1以外の場合、指定したユーザーに送る</param>
	bool ToHand (int _Pass,bool _Left = false,int _Target = -1) {
		if (!ExistCard (_Pass))
			return false;
		int[] From = GetPos (_Pass);
		int ToField  = (_Target != -1) ? _Target : PlayerOrEnemy(_Pass);

		int[] To = new int[]{ ToField, -1 };
		if (_Left)
			To [1] = 0;
		return MoveCard (From, To);
	}
	/// <summary>
	/// カードを山札の1番下に置く
	/// </summary>
	/// <param name="_Top">1番上に置くかどうか</param>
	/// <param name="_Target">-1以外の場合、指定したユーザーに送る</param>
	bool ToDeck(int _Pass,bool _Top = false,int _Target = -1) {
		if (!ExistCard (_Pass))
			return false;
		int[] From = GetPos (_Pass);
		int ToField = 2;
		if (PlayerOrEnemy (From) == 1)
			ToField = 3;
		if (_Target != -1)
			ToField = _Target + 2;

		int[] To = new int[]{ ToField, -1 };
		if (_Top)
			To [1] = 0;
		return MoveCard (From, To);
	}
	bool ToCreature (int _Pass,int _Target = -1) {
		if (!ExistCard (_Pass))
			return false;
		if(_Target == -1)
			_Target = PlayerOrEnemy (_Pass);
		//すでに存在するならそれを自然消滅させる
		if(ExistCreature(_Target))
			ToGrave (GetCreature(_Target).Pass,TargetGrave.USED);
		return MoveCard(_Pass,new int[]{-1,_Target});
	}
	void ExchangeCard (int _a,int _b) {
		//同じなら無視
		if (_a == _b)
			return;

		SEPlay (1);
		//
		int[] aPos = GetPos(_a);
		int[] bPos = GetPos (_b);

		//クリーチャーが関係しない場合
		if (aPos [0] != -1 && bPos [0] != -1) {
			List<CardParam> FromCards = GetField (aPos [0]);
			List<CardParam> ToCards = GetField (bPos[0]);

			CardParam aCard = FromCards [aPos [1]];
			CardParam bCard = ToCards [bPos [1]];
			FromCards [aPos [1]] = bCard;
			FromCards [bPos [1]] = aCard;
			return;
		}

		//クリーチャー同士
		if(aPos[0] == -1 && bPos[0] == -1){
			CardParam aCard = Creatures [0];
			CardParam bCard = Creatures [1];
			Creatures [1] = aCard;
			Creatures [0] = bCard;
			return;
		}

		//クリーチャーとその他で入れ替え
		int CreaturePass = (aPos[0] == -1) ? _a : _b;
		int OtherPass = (aPos[0] == -1) ? _b : _a;
		CardParam CreatureCard = GetCard(CreaturePass);
		CardParam OtherCard = GetCard (OtherPass);
		if (ExistCard(CreatureCard.Pass)) {
			//クリーチャーが存在する場合は「クリーチャーが関係しない場合」と同じ
			List<CardParam> FromCards = GetField (aPos [0]);
			List<CardParam> ToCards = GetField (bPos [0]);
			CardParam aCard = FromCards [aPos [1]];
			CardParam bCard = ToCards [bPos [1]];
			FromCards [aPos [1]] = bCard;
			ToCards [bPos [1]] = aCard;
		} else {
			//クリーチャーが存在しない場合は「場に移動する」と同じ
			ToCreature(OtherCard.Pass,PlayerOrEnemy(CreaturePass));
		}
	}

	bool MoveCard (int _Pass,int[] _To){
		return MoveCard (GetPos (_Pass), _To);
	}

	bool MoveCard (int[] _From,int[] _To){
		CardParam FromCard = new CardParam ().Reset();
		List<CardParam> FromCards = GetField (_From [0]);
		List<CardParam> ToCards = GetField (_To [0]);



		//カードを取ってくる、ただしクリーチャーだけ別処理
		if (_From [0] == -1) {
			if (ExistCreature (_From [1])) {
				//カードが存在した
				FromCard = GetCreature (_From [1]);
			} else
				return false;
		} else {
			//クリーチャー以外の処理
			if (_From [1] < FromCards.Count) {
				//カードが存在した
				FromCard = FromCards [_From [1]];
			} else {
				//カードが存在しない
				return false;
			}
		}
		//挿入、ただしクリーチャーだけ別処理
		if (_To [0] == -1) {
			Creatures [_To [1]] = FromCard;
		} else {
			//カードを配置する
			if (_To [1] == -1)//追加
				ToCards.Add (FromCard);
			else//挿入
				ToCards.Insert (_To [1], FromCard);
		}

		//削除、ただしクリーチャーだけ別処理
		if (_From [0] == -1) {
			Creatures [_From [1]] = new CardParam ().Reset();
		} else {
			FromCards.RemoveAt (_From [1]);
		}
		return true;
	}
	#endregion

	#region 更新、判断系

	List<CardParam> ReloadCard (List<CardParam>_card) {
		List<CardParam> temp = new List<CardParam> ();
		for (int i = 0; i < _card.Count; i++ ){
			temp.Add (GetCard (_card [i].Pass));
		}
		return temp;
	}
	void UpdateCard (CardParam _cp) {
		int[] Pos = GetPos (_cp.Pass);

		CardParam cp = new CardParam ();
		cp = _cp;
		cp.Effect = new List<int> ();
		cp.Effect.AddRange (_cp.Effect);
		cp.EffectValue = new List<int> ();
		cp.EffectValue.AddRange (_cp.EffectValue);
		cp.Groups = new List<int> ();
		cp.Groups.AddRange (_cp.Groups);
		cp.SkillScript = new List<string> ();
		cp.SkillScript.AddRange (_cp.SkillScript);
		cp.SkillTexts = new List<string> ();
		cp.SkillTexts.AddRange (_cp.SkillTexts);

//		cp.Atr = _cp.Atr;
//		cp.Cost = _cp.Cost;

		//クリーチャー
		if (Pos [0] == -1) {
			Creatures [Pos [1]] = cp;
			return;
		}
		//そのた
		List<CardParam> lcp = GetField (Pos[0]);

		GetField (Pos [0]) [Pos [1]] = cp;
//		lcp [Pos [1]] = _cp;
	}

	//判断系
	int PlayerOrEnemy (int _Pass,bool _Reverse = false) {
		return PlayerOrEnemy( GetPos (_Pass),_Reverse);
	}
	int PlayerOrEnemy (CardParam _cp,bool _Reverse = false){
		return PlayerOrEnemy (GetPos(_cp.Pass),_Reverse);
	}
	int PlayerOrEnemy (int[] pos,bool _Reverse = false) {
		
		//クリーチャー
		if (pos[0] == -1) {
			if (_Reverse) {
				return pos [1] == 0 ? 1 : 0;
			}
				
			return pos [1];
		}

		List<int> PlayerFields = new List<int>{ 0, 2, 4 };

		if (_Reverse) {//反転
			if (PlayerFields.Contains (pos[0]))
				return 1;
			else
				return 0;
		}
		//自分のゾーン
		if (PlayerFields.Contains (pos[0]))
			return 0;
		else
			return 1;
		
	}
	bool ExistCardInFieldOrHands (int _Pass) {
		int[] pos = GetPos (_Pass);
		List<int> FieldOrHands = new List<int>{ -1, 0, 1 };
		if (FieldOrHands.Contains (pos [0]))
			return true;
		return false;
	}

	bool ExistCard (int _Pass) {
		string CardName = GetCard (_Pass).Name;
		return (CardName != null || CardName == "");
	}

	bool ExistCreatureByPass (int _Pass) {
		return ExistCreature (PlayerOrEnemy (_Pass));
	}

	bool ExistCreature (int _Target) {
		string creatureName = GetCard (-1, _Target).Name;
		return (creatureName != null && creatureName != "");
	}
	bool HasEffect (int _Pass,TargetEffect _effect) {
		return GetCard (_Pass).Effect.Contains ((int)_effect);
	}

	public bool CanSummon (CardParam _cp) {

		//一時的変化を考慮する。
		//		_cp = CalculateParam (_cp);

		int[] Pos = GetPos (_cp.Pass);

		if(_cp.GetCalCost() <= GetSP(PlayerOrEnemy(_cp)) && (Pos[0] == 0 || Pos[0] == 1)) {//コストが足りている
			//その他の召喚条件など(凍結等)
			if (_cp.Effect.Contains (1) || _cp.Effect.Contains (2)) {//凍結、拘束を受けている
				return false;
			}

			return true;
		}

		return false;
	}
	public bool CanDiscard (CardParam _cp) {
		int[] Pos = GetPos (_cp.Pass);
		if (Pos [0] == 0 || Pos [0] == 1){
			return true;//手札にあれば、基本は捨てることが可能
		}

		return false;
	}

	int CompareInt (int _a,int _b) {
		int temp = (_a > _b) ? 0
			: (_a < _b) ? 1
			: -1;
		return temp;
	}

	#endregion

	//カードイメージ系
	#region カードイメージ系
	void ImageRefresh () {
		Debug.Log ("ImageRefresh()");
		for (int i = 0; i < 2; i++ ){
			//<<手札>>

			//手札の枚数だけポジション用オブジェクトのアクティブ化
			for (int i2 = 0; i2 < userObjs[i].ImagePoses.Count; i2++ ){
				if (i2 < GetHands (i).Count) {
					userObjs [i].ImagePoses [i2].gameObject.SetActive (true);
				} else {
					userObjs [i].ImagePoses [i2].gameObject.SetActive (false);
				}
				LayoutRebuilder.ForceRebuildLayoutImmediate (userObjs[i].HandsField);
			}


			//画像変更
			List<CardParam> hands = GetHands (i);
			//<<手札>>
			string d = "手札:";
			for (int i2 = 0; i2 < hands.Count; i2++ ){
				d += hands [i2].Name;
			}
			Debug.Log (d);

			for (int i2 = 0; i2 < hands.Count; i2++) {
				
				CardParam cp = hands [i2];
				CardImageScript cardImage = GetCardImage (cp);
				if (cardImage == null) {
					//新規カード
					cardImage = GetNewImage (cp);
					cardImage.transform.SetAsLastSibling ();
					cardImage.transform.position = userObjs [i].DeckPos.position;
					cardImage.transform.localScale = Vector3.one;
				}
//				//アクティブ判定
//				bool ForceActive = false;
//
//				for (int i3 = 0; i3 < SelectChoices.Count; i3++ ){
//					if (SelectChoices [i3].ID == cp.ID)
//						ForceActive = true;
//				}
//				if (mode == Mode.SELECT && ForceActive) {
//					cardImage.Set(CalculateParam(cp),true);
//				} else
					cardImage.Set(CalculateParam(cp),CanSummon(cp));
				//カードの移動
				cardImage.transform.DOMove (GetHandPoses (i) [i2].position,0.5f);
				cardImage.transform.DOScale (1f * PoolCardScale, 0.3f);
			}

			//<<クリーチャー>>
			CardParam creature = GetCreature(i);

			if (ExistCreature(i)) {//クリーチャーがいる時
				Debug.Log(creature.Name);
				CardImageScript creatureImg = GetCardImage (creature);
				if (creatureImg == null) {
					//万が一存在しないなら、新規カード
					creatureImg = GetNewImage (creature);
					creatureImg.transform.SetAsLastSibling ();
					creatureImg.transform.position = userObjs [i].DeckPos.position;
					creatureImg.transform.localScale = new Vector3 (1f * PoolCardScale,1f * PoolCardScale,1);
				}
				//カードの移動
				creatureImg.Set (CalculateParam(creature));//不透明化
				creatureImg.transform.DOKill();
				creatureImg.transform.DOMove (GetCreaturePos (i).position, 0.2f);
				creatureImg.transform.DOScale (1.3f*PoolCardScale, 0.2f);
			}



		}
		//盤面外は削除
		List<int> HandOrCre = new List<int> (){ -1, 0, 1 };
		for (int i = 0; i < CardPool.Count; i++ ){
			GameObject go = CardPool [i].gameObject;
			if (go.activeSelf) {
				int pass = int.Parse (go.name);
				int field = GetPos (pass)[0];
				if (!HandOrCre.Contains (field)) {
					go.SetActive (false);
				}
			}


		}
	}

	CardImageScript GetCardImage (CardParam _cp) {//プール上のイメージを返す
		
		List<CardImageScript> pool = CardPool;
		for (int i = 0; i < pool.Count; i++ ){
			CardImageScript image = pool [i];
			//存在するなら
			if (image.gameObject.activeSelf) {
				if (image.Pass == _cp.Pass)
					return image;
			}
		}
//		Debug.LogError ("手札または場に存在しないカード");
		return null;
	}
	CardImageScript GetNewImage (CardParam _cp) {

		List<CardImageScript> pool = CardPool;
		for (int i = 0; i < pool.Count; i++) {
			CardImageScript obj = pool [i];
			if (!obj.gameObject.activeSelf) {
				obj.Set (_cp);
				obj.gameObject.SetActive (true);
				obj.name = _cp.Pass.ToString();
				//ボタンタップ用
				obj.GetComponent<SlidableButton>().onButtonDown.RemoveAllListeners();
				obj.GetComponent<SlidableButton> ().onButtonDown.AddListener (() => {
					CardTapNotify (obj.Pass);
				});
				return obj;
			}
		}
		return null;
	}

	#endregion

	#region エフェクト系


	void SetSelectEffect (List<CardParam> _selects) {

		for (int i = 0; i < _selects.Count; i++ ){
			CardImageScript cardImage = GetCardImage (_selects [i]);

			GameObject obj = Instantiate (SelectImage);
			obj.transform.SetParent (cardImage.transform);
			Image img = obj.GetComponent<Image> ();
			img.transform.localScale = Vector3.one;
			RectTransform rect = img.transform.GetComponent<RectTransform> ();
			rect.offsetMax = new Vector3 (2f,2f, 1f);
			rect.offsetMin = new Vector3 (-2f,-2f, 1f);
			img.name = "selectEffect";
			img.color = new Color (0f, 1f, 0f, 0.75f);
			DOTween.ToAlpha (
				() => img.color, 
				color => img.color = color,
				0.25f,                             // 最終的なalpha値
				1f
			).SetLoops (-1, LoopType.Restart);

		}
	}
	void RemoveAllSelectEffect () {
		for (int i = 0; i < CardPool.Count; i++ ){
			Transform t = CardPool [i].transform.Find ("selectEffect");
			if(t != null)
				Destroy (t.gameObject);
		}
	}

	/// <summary>
	/// カードが場に出た時の処理
	/// </summary>
	void SummonEffect (int _Target) {
		//エフェクト
		CardParam cp = GetCard(-1,_Target);
		if (ExistCreature (_Target)) {
			int RoleInt = cp.Role;
			switch (RoleInt) {
			case 0:
				Effect ("SummonAttacker",userObjs[_Target].Creature);
				break;
			case 1:
				Effect ("SummonCharger",userObjs[_Target].Creature);
				break;
			case 2:
				Effect ("SummonDefender",userObjs[_Target].Creature);
				break;
			}
		}
	}

	void Effect (string _EffectName,int _pass) {
		Transform Card = GetCardImage (GetCard (_pass)).transform;
		Effect (_EffectName, Card);

	}
	void Effect (string _EffectName,Transform _transform) {
		Vector3 pos = _transform.localPosition;
		GameObject obj = Instantiate ((GameObject)Resources.Load ("Effect/"+_EffectName));
		obj.transform.SetParent (_transform.parent);
		obj.transform.localPosition = new Vector3 (pos.x, pos.y,pos.z -100);
		obj.transform.localScale = Vector3.one;
		Destroy (obj, obj.transform.GetComponentInChildren<ParticleSystem>().main.duration);
	}

	#endregion

	void ShowDetail (int _SkillNum = -1) {
		//設定
		CardParam cp = CalculateParam(DetailCard);
		//召喚画像

		if (cp.Name == null || cp.Name == "") {
			DetailImage.gameObject.SetActive (false);
			NameText.text = "";
			ParamText.text = "";
			SkillText.text = "";

			return;
		}
		DetailImage.gameObject.SetActive (true);
		//名前+種族
		string Title = System.String.Format 
			("LV.{0} {1} ", new string[] {cp.LV.ToString(),cp.Name.ToString()});
		Title += string.Format( "\n<size=16>{0}</size>",SystemScript.GetGroup(cp.Groups));
		NameText.text = Title;

		//パラメータ
		ParamText.text = System.String.Format 
			("役割:{0}\nコスト:{1}\nパワー:{2}"
				, new string[] {SystemScript.GetRole(cp.Role),cp.Cost.ToString(),cp.Power.ToString()});

		//スキル
		SkillText.text = SystemScript.GetSkillsText(cp,SystemScript.ShowType.BATTLE,true,_SkillNum);
		//イメージ
		DetailImage.Set (cp);

		//ボタン表示、非表示
		SummonButton.SetActive (false);
		DiscardButton.SetActive (false);
		SelectButton.SetActive (false);
		int[] Pos = GetPos (cp.Pass);
		if (Pos [0] == 0 && mode == Mode.SUMMON) {
			SummonButton.SetActive (CanSummon (cp));
			DiscardButton.SetActive (CanDiscard (cp));
		}
		if (mode == Mode.SELECT) {
			for (int i = 0; i < SelectChoices.Count; i++ ){
				if (SelectChoices [i].Pass == cp.Pass) {
					//選択ボタンを表示
					SelectButton.SetActive (true);
				}
			}
		}

	}
	#region Notify系

	public void SelectNotify (int _num) {
//		SEPlay (14);
		if (_num == -1) {
			//ページ戻る
			if (nowPage != 0) {
				nowPage--;
				RefreshSelectWindow ();
			}
		} else if (_num == -2) {
			if (nowPage != ((SelectChoices.Count - 1) / 10)) {
				nowPage++;
				RefreshSelectWindow ();
			}
		} else {
			
			DetailCard = SelectChoices [_num + nowPage * 10];
			ShowDetail ();
		}
	}
	public void WindowOpen () {
		if (windowOpen) {
			SelectCardAndBG.SetActive (false);
			windowOpen = false;
		} else {
			SelectCardAndBG.SetActive (true);
			windowOpen = true;
		}
	}

	public void SummonNotify(int _num) {
		if (_num == 0) {//召喚ボタン
			SelectingPasses[0] =  new int[]{ DetailCard.Pass,0};
		}
		if (_num == -1) {//破棄
			SelectingPasses[0] = new int[]{DetailCard.Pass,-1};
		}
		if (_num == 1) {//選択
			Select1Pass = DetailCard.Pass;
			SelectButton.SetActive (false);
		}
		if (gameMode == GameMode.CPU && GetHands(1).Count > 0) {
			
			CPU ();
		}
		 

	}
	public void CardTapNotify (int _Pass) {
		DetailCard = GetCard (_Pass);
		ShowDetail ();
	}
	#endregion

	#region CPU系

	public void CPU () {
		//相手の
		int CPUSelect = 0;
		List<CardParam> lcp = GetField(1);
		List<int> Points = new List<int>();
		int CPU_SP = GetSP (1);
		int CPU_LP = GetLP (1);
		for (int i = 0; i < lcp.Count; i++ ){
			int Point = 1;
			CardParam cp = lcp [i];
			//召喚可能
			if (CanSummon (cp))
				Point += 50;
			Points.Add (Point);
		}
		//最終判定
		int PointTotal = 0;

		for (int i = 0; i < lcp.Count; i++ ){
			PointTotal += Points [i];
		}
		int SelectPoint = Random.Range (0, PointTotal);
		for (int i = 0; i < lcp.Count; i++ ){
			SelectPoint -= Points [i];
			if (SelectPoint <= 0) {
				CPUSelect = i;
				break;
			}
		}
		CardParam CPUCard = GetCard (1, CPUSelect);
		SelectingPasses [1] = new int[]{CPUCard.Pass,(CanSummon(CPUCard)) ? 0 : -1};
	}

	#endregion

	#region データ変更系

	/// <summary>
	/// 特殊能力チェックしない
	/// </summary>
	void ChangeTemp (List<CardParam> _Cards,TargetParam _TargetParam,int _Point,int _Invoker,CalType _calType) {
		_Cards = ReloadCard (_Cards);

		for (int i = 0; i < _Cards.Count; i++ ){
			CardParam cp = _Cards[i];

			int DefaultPoint = 0;

			//初期値を設定
			if (_TargetParam == TargetParam.COST)
				DefaultPoint = cp.Cost;
			if (_TargetParam == TargetParam.POWER)
				DefaultPoint = cp.Power;

			//コストとパワーで場合分け
			int _effect = 12;
			if (_TargetParam == TargetParam.POWER)
				_effect = 13;
			
			//修正値を計算
			int arrayNum = -1;
			int modify = 0;
			for (int i2 = 0; i2 < cp.Effect.Count; i2++ ){
				if (cp.Effect [i2] == _effect) {
					modify = cp.EffectValue [i2];;
					DefaultPoint += modify;
					arrayNum = i2;
				}
			}

			//計算方法を設定

			if (_calType == CalType.MUL)
				_Point = DefaultPoint * (_Point -1);
			if (_calType == CalType.DIVIDE)
				_Point =  ( DefaultPoint / _Point)- DefaultPoint;
			if (_calType == CalType.SET)
				_Point = _Point - DefaultPoint;
			

			if (arrayNum == -1) {
				//効果付与実行
				List<int> eff = new List<int> ();
				eff.AddRange (cp.Effect);
				eff.Add (_effect);

				List<int> effV = new List<int> ();
				effV.AddRange (cp.EffectValue);
				effV.Add (_Point + modify);

				cp.Effect = eff;
				cp.EffectValue = effV;
			} else {
				cp.EffectValue [arrayNum] = _Point + modify;
			}

			UpdateCard (cp);
		}
		Refresh ();
		ImageRefresh ();
	}
	void RemoveTemp (List<CardParam> _cards,int _effect) {
		_cards = ReloadCard (_cards);
		for (int i = 0; i < _cards.Count; i++) {
			CardParam cp = _cards [i];

			for (int i3 = cp.Effect.Count - 1; i3 >= 0; i3--) {
				int eff = cp.Effect [i3];
				if (eff == _effect || _effect == -1) {
					cp.Effect.RemoveAt (i3);
					cp.EffectValue.RemoveAt (i3);
				}
			}
			UpdateCard (cp);
		}
	}

	IEnumerator S_ChangeLP(int _TargetUser,int _Point,bool _Sound = true,int _Invoker = -1,CalType _calType = CalType.ADD){
		if (_Point == 0)
			yield break;

		int DefaultPoint = GetLP (_TargetUser);


		if (_calType == CalType.MUL)
			_Point = DefaultPoint * (_Point -1);
		if (_calType == CalType.DIVIDE)
			_Point =  ( DefaultPoint / _Point)- DefaultPoint;
		if (_calType == CalType.SET)
			_Point = _Point - DefaultPoint;
		
		//効果
		Dictionary<string, object> objs = new Dictionary<string, object>();

		objs.Add ("targetUser", _TargetUser);
		objs.Add ("point", _Point);
		objs.Add ("invoker",new List<CardParam>{GetCard( _Invoker)});

		Debug.Log (GetCard (_Invoker).Name + "が攻撃");

		if (0 < _Point) {
			var coro = CheckCommonSkill (Timing.HEAL, objs);
			yield return  StartCoroutine (coro);
			objs = (Dictionary<string, object>)coro.Current;
		}if (0 > _Point) {
			var coro = CheckCommonSkill (Timing.DAMAGE, objs);
			yield return  StartCoroutine (coro);
			objs = (Dictionary<string, object>)coro.Current;
		}
		_TargetUser = (int)objs["targetUser"];
		_Point = (int)objs ["point"];

		if (_TargetUser == 0 || _TargetUser == 1) {

//			if (_Point > 0) {
//				LPs [_TargetUser]++;
//			} else {
//				LPs [_TargetUser]--;
//			}
			LPs [_TargetUser] += _Point;

		} else {
			Debug.LogError ("Error");
		}

		if (_Sound) {
			if (0 < _Point) {
				
				SEPlay (9);
				Effect ("Heal", userObjs [_TargetUser].LPText.transform);
			} else if (0 > _Point) {
				SEPlay (3);
				Effect ("Attack", userObjs [_TargetUser].LPText.transform);
			}
		}
		Refresh ();
		ImageRefresh ();
		yield return new WaitForSeconds (WaitTime);

	}

	IEnumerator S_ChangeSP (int _TargetUser,int _Point,bool _Sound = true,int _Invoker = -1,CalType _calType = CalType.ADD){

		if (_Point == 0)
			yield break;
		
		int DefaultPoint = GetSP (_TargetUser);

		if (_calType == CalType.MUL)
			_Point = DefaultPoint * (_Point -1);
		if (_calType == CalType.DIVIDE)
			_Point =  ( DefaultPoint / _Point)- DefaultPoint;
		if (_calType == CalType.SET)
			_Point = _Point - DefaultPoint;
		
		//効果
		Dictionary<string, object> objs = new Dictionary<string, object>();
		objs.Add ("targetUser", _TargetUser);
		objs.Add ("point", _Point);
		objs.Add ("invoker",new List<CardParam>{GetCard( _Invoker)});


		if (0 < _Point) {
			var coro = CheckCommonSkill (Timing.CHARGE, objs);
			yield return  StartCoroutine (coro);
			objs = (Dictionary<string, object>)coro.Current;
		}if (0 > _Point) {
			var coro = CheckCommonSkill (Timing.NOIZ, objs);
			yield return  StartCoroutine (coro);
			objs = (Dictionary<string, object>)coro.Current;
		}
		_TargetUser = (int)objs["targetUser"];
		_Point = (int)objs ["point"];


		if (_TargetUser == 0 || _TargetUser == 1) {
			//SP0未満防止
			int nowSP = SPs [_TargetUser];
			if (nowSP + _Point < 0)
				SPs [_TargetUser] = 0;
			else
				SPs [_TargetUser] += _Point;
		} else {
			Debug.LogError ("Error");
		}


		if (_Sound) {
			if (0 < _Point) {
				SEPlay (4);
				Effect ("Charge", userObjs [_TargetUser].SPText.transform);
			} else if (0 > _Point) {
				SEPlay (8);
				Effect ("Attack", userObjs [_TargetUser].SPText.transform);
			}
			Refresh ();
			ImageRefresh ();
			yield return new WaitForSeconds (WaitTime);
		}

	}

	#endregion

	#region その他

	CardParam CalculateParam (CardParam _cp) {
		_cp.Power = _cp.GetCalPower ();
		_cp.Cost = _cp.GetCalCost ();
		return _cp;
	}

	// 効果音

	void SEPlay (int _num) {
		DataManager.Instance.BattleSEPlay (_num);
	}

	#endregion

	// Use this for initialization
	void Start () {
		
		for (int i = 0; i < FlashingImages.Count; i++ ){
			var image = FlashingImages[i].GetComponent<Image>();
			DOTween.ToAlpha (
				() => image.color, 
				color => image.color = color,
				0f,                             // 最終的なalpha値
				1f
			).SetLoops (-1, LoopType.Restart);
		}
	}

	#region 通信系
	public void OnlineData (object[] args) {
		int user = (int)args [0];
		int index = (int)args [1];
		int[] data = (int[])args [2];

	}
	#endregion

	#region デバッグ系
	void Update () {
		if (debugTool.SetCard) {
			CardParam cp = DetailCard;
			int pass = cp.Pass;
			SystemScript.CardData cd = new SystemScript.CardData ();

			cd.Atr = debugTool.Atr;
			cd.ID = debugTool.ID;
			cp.Set (cd);
			cp.Pass = pass;
			UpdateCard (cp);

			Refresh ();
			ImageRefresh ();
			debugTool.SetCard = false;
		}
		if (debugTool.SPBoost) {
			StartCoroutine( S_ChangeLP (0, 200, true));
			StartCoroutine( S_ChangeLP (1, 200, true));
			StartCoroutine( S_ChangeSP (0, 100, true));
			StartCoroutine( S_ChangeSP (1, -100, true));

			debugTool.SPBoost = false;
		}
		if (debugTool.Refresh) {
			Refresh ();
			ImageRefresh ();

			debugTool.Refresh = false;
		}

			
	}
	#endregion
}
