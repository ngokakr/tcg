using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using CardData = SystemScript.CardData;
using CardParam = SystemScript.CardParam;
using DG.Tweening;

public class NewDeckBoxScript : MonoBehaviour ,IRecieveMessage,ICardDragHandler,ILvup{
	
	int useDeck = 0;

	[System.Serializable]
	public struct BoxObjs {
//		public GameObject BoxView;
		public Transform ScrollContent;
		public Text deckName;
		public Text PageInfo;
		public Text Info;
		public Text havePiece;
		public Text SearchText;
		public CardImageScript[] BoxCardsImage;
		public ShadowMove[] RoleRefineButton;
		public GameObject[] LeftRightPageButton;
		public GameObject[] BreakGenerateButton;
		public Text LevelUpText;
		public GameObject CantGenerateButton;

//		public GameObject deckCardImages;
	}
	public Vector2 BoxCardInfoSize;
	public Vector2 DeckCardInfoSize;
	public int BoxCardInfoFontSize;
	public int DeckCardInfoFontSize;
	public BoxObjs boxObjs;
	public GameObject cardInfo;
	public CardDragScroll cardDragScroll;
	public NewCardDetailScript detailScript;
	AlertView alert;
	List<int> GroupIndexes;
	public int RefineIndex;

	int nowPage = 0;
	int MaxPage = 0;

	public int RoleRefine;
	//使用中のデッキ
	[SerializeField]
	List<CardParam> SortedDeck = new List<CardParam> ();
	[SerializeField]
	List<CardParam> SortedBox =new List<CardParam>();

	public CardDetailScript cardDatail;

	public CardParam SelectingCard;
	public int[] SelectingData;

	public bool DeckMode = true;
	public bool IDSort = false;


	public void Show() {
		cardDragScroll.Delegate = gameObject;
		Refresh ();
	}

	void Refresh () {
		//ボックス取得
		var cds = DataManager.Instance.box;
		//cd -> cp
		SortedBox = new List<CardParam> ();
		for (int i = 0; i < cds.Count; i++ ){
			CardParam param = new CardParam ();
			param.Set (cds [i]);
			SortedBox.Add (param);
		}

		//		//idソート
		//		Sort (SortedBox,true);
		int BoxCount = SortedBox.Count;//カード総数
		//
		//
		//役割、コストソート
		Sort(SortedBox);
		int kindCount = SortedBox.Count;//種類総数

		//まとめる
//		SortedBox = SystemScript.PackCardParams(SortedBox);

		//絞り込み
		SortedBox =  CardRefine(SortedBox,new List<int>(),RoleRefine);


		//デッキ取得
		useDeck = DataManager.Instance.UseDeck;
		SortedDeck = SystemScript.cdTocp( DataManager.Deck.GetDeckData(useDeck));

		//名前表示
		boxObjs.deckName.text = getDeckNames()[useDeck];

		//ソート
		Sort(SortedDeck);

		//絞り込み
//		SortedDeck = CardRefine(SortedDeck,new List<int>(),-1);



		//ボックス表示
		for (int i = 0; i < boxObjs.BoxCardsImage.Length; i++ ){
			CardImageScript img = boxObjs.BoxCardsImage [i];
			int x = nowPage * 10 + i;
			if (x < SortedBox.Count) {
				img.gameObject.SetActive (true);
				img.Set (SortedBox [x]);
				var cp = SortedBox [x];
				SetCardInfo(false,i,GetContainsCards(DataManager.Instance.UseDeck,cp.Atr,cp.ID));
			} else {
				img.gameObject.SetActive (false);
//				img.RemoveImage ();
			}
		}

		//デッキ表示
		var contents = boxObjs.ScrollContent;
		for (int i = 0; i < contents.childCount; i++ ){
			Transform t = contents.GetChild (i);
			if (i < SortedDeck.Count) {
				t.gameObject.SetActive (true);

				//lv等はボックスを参照
				//				var cp = SortedDeck [i];
				var cp = GetBoxCardParam (SortedDeck [i].ID);

				t.Find ("Card").GetComponent<CardImageScript> ().Set (cp);

				cp = new CardParam().Set( DataManager.Instance.box.Find (x => x.ID == cp.ID));

				SetCardInfo(true,i,GetContainsCards(DataManager.Instance.UseDeck,cp.Atr,cp.ID));
			} else {
				t.gameObject.SetActive (false);
//				t.Find ("Card").GetComponent<CardImageScript> ().RemoveImage ();
			}
		}


		//→表示
		Resources.UnloadUnusedAssets ();

		MaxPage = (SortedBox.Count - 1) / 10;

		boxObjs.PageInfo.text = string.Format ("CARD BOX {0}/{1}",nowPage+1,MaxPage+1);

		int cardCounts = 0;
		for (int i = 0; i < SortedDeck.Count; i++ ){
			cardCounts += SortedDeck [i].Count;
		}

		//データ表示 & カードデータ生成
		boxObjs.Info .text = string.Format("カード総数 {0}  種類 {1}\nデッキ枚数 [<color=#00ff00>{2} / 30</color>]",BoxCount,kindCount,cardCounts);

//		Invoke ("UnloadAssets", 0.02f);
	}

//	public void UnloadAssets () {
//		Resources.UnloadUnusedAssets ();
//	}

	public void ShowDetail () {
//		int id = SelectingCard.ID;
//		CardData cd = DataManager.Instance.box.Find (x => x.ID == id);
		CardParam cp = GetBoxCardParam(SelectingCard.ID);
		detailScript.Show (cp);
		if (cp.Count >= SystemScript.needPoint (cp)) {
			//強化可能
			boxObjs.LevelUpText.text = "<color=#00ff00>レベルアップ</color>";
		} else {
			//強化不可
			boxObjs.LevelUpText.text = "<color=#cccccc>レベルアップ</color>";
		}
		boxObjs.LevelUpText.text += string.Format( "\n<size=18>{0}/{1}</size>",cp.Count,SystemScript.needPoint(cp));

		//無効化
		if (!DataManager.Instance.LVSystem) {
			boxObjs.LevelUpText.text = "";
		}
	}

	public CardParam GetBoxCardParam (int id) {
		return new CardParam ().Set (DataManager.Instance.box.Find (x => x.ID == id));
	}



	/// <summary>
	/// _Role -1=ALL 0=Charger 1=Attacker 2=Defender
	/// </summary>
	List<CardParam> CardRefine (List<CardParam> _Cards,List<int> _Attribute, int _Role) {
		
		var xlsParams = DataManager.Instance.xls_CardParam.sheets[0].list;
		string refineText = "";
		List<CardParam> Result = new List<CardParam> ();
		if (RefineIndex == 0) {
			refineText = "絞り込み";
		} else if (1 <= RefineIndex && RefineIndex <= 5) {
			string[] x = { "灼熱", "大雨", "竜巻", "光明", "暗黒" };
			refineText = x [RefineIndex - 1];
		} else {
			refineText = DataManager.Instance.xls_Groups.sheets [0].list [GroupIndexes [RefineIndex]].name;
		}
		for (int i = 0; i < _Cards.Count; i++ ){
			CardParam cd = _Cards [i];
			var param = xlsParams [cd.ID];
			if (_Attribute.Count != 0) {
				if (!_Attribute.Contains (cd.Atr))
					continue;
			}

			if (_Role != -1) {
				if (_Role != cd.Role)
					continue;
			}
			if (RefineIndex == 0) {
				//絞り込みなし
			} else if (1 <= RefineIndex&& RefineIndex <= 5) {
				//天候絞り込み
				bool exist = false;
				string[] x = {"灼熱","大雨","竜巻","光明","暗黒"};
				string g = x [RefineIndex - 1];
				for (int i2 = 0; i2 < param.skill.Length; i2++ ){
					if (param.skill [i2].Contains (g))
						exist = true;
				}
				if (!exist)
					continue;
			} else {
				//グループ絞り込み
				bool exist = false;
				if(! param.group.Contains (GroupIndexes [RefineIndex]))
					continue;
			} 
	
			Result.Add (cd);
		}
		boxObjs.SearchText.text = refineText;
		return Result;
	}


	public void Sort (List<CardParam> _cps) {
		_cps.Sort ((a, b) => {
			//ID順
			if(IDSort){
				return a.ID - b.ID;
			}

			//役割順 (c,a,dの順にする)
			int result = 0;
			int A_role = 0;
			int B_role = 0;
			if(a.Role == 0)
				A_role = 1;
			if(a.Role == 2)
				A_role = 2;
			if(b.Role == 0)
				B_role = 1;
			if(b.Role == 2)
				B_role = 2;

			result = A_role - B_role;
			if(result !=0)
				return result;

			result = a.Cost - b.Cost;
			if(result !=0)
				return result;

			result = a.Atr - b.Atr;
			if(result !=0)
				return result;

			result = a.ID - b.ID;
			return result;
		});
	}

	public void BoxRoleRefine (int _num){
		//アニメーション
		for (int i = 0; i < boxObjs.RoleRefineButton.Length; i++ ){
			boxObjs.RoleRefineButton [i].Move (false,false);
		}
		boxObjs.RoleRefineButton [_num].Move (true,true);

		//
		switch (_num) {
		case 0:
			RoleRefine = -1;
			break;
		case 1:
			RoleRefine = 1;
			break;
		case 2:
			RoleRefine = 0;
			break;
		case 3:
			RoleRefine = 2;
			break;

		}
		nowPage = 0;
		Refresh ();
	}

	public void BoxCardTapNotify (int num) {
//		DataManager.Instance.SEPlay (0);
		SelectingCard = SortedBox [nowPage * 10 + num];
		ShowDetail ();
	}
	public void DeckTapNotify (int num){
		SelectingCard = SortedDeck [num];
		ShowDetail ();
	}
	public void SkillTextTapNotify () {
		DataManager.Instance.SEPlay (7);

	}

	public void ArrowKeyNotify (bool right) {
		DataManager.Instance.SEPlay (7);
		if (right) {
			nowPage++;
		} else {
			nowPage--;
		}

		//調整
		if (nowPage < 0)
			nowPage = 0;
		if (MaxPage < nowPage) {
			nowPage = MaxPage;
		}
		Refresh ();
	}
	public void SetCard (bool _Set){
		DataManager.Deck.SetCard (useDeck,SelectingCard.Atr,SelectingCard.ID,_Set);
		DataManager.Instance.Save ();
		Refresh ();
	}

	public void RoleNotify (int num) {
		RoleRefine = num;
		nowPage = 0;
		Refresh ();
	}
	public void OnCardTap (int _num, int _tag){
//		Debug.Log(string.Format("num:{0} tag;{1}",_num,_tag));
		if(_tag == 0)
			BoxCardTapNotify (_num);
		if (_tag == 1)
			DeckTapNotify (_num);
		DataManager.Instance.SEPlay (0);

		SelectingData = new int[]{ _num, _tag };
	}

	public void OnVartical (int value,int _num, int _tag){
		if (_tag == 0) {
			BoxCardTapNotify (_num);
			SetCard (value == -1 ? false : true);
		} else if (_tag == 1) {
			DeckTapNotify (_num);
			SetCard (value == -1 ? false : true);
		}
		cardDragScroll.transform.GetChild(0).GetChild(0).GetComponent<ContentSizeFitter> ().horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
		cardDragScroll.transform.GetChild(0).GetChild(0).GetComponent<ContentSizeFitter> ().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
		DataManager.Instance.SEPlay (1);
	}
	public void OnHorizontal (int value,int _num, int _tag){
		if(_tag == 0)
			ArrowKeyNotify (value == -1 ? true : false);
		
	}

	public void SetCardInfo (bool Deck,int num,int value) {
//		Transform parent;
		Transform InfoTargetCard;
		if (Deck) {
			InfoTargetCard = boxObjs.ScrollContent.GetChild(num);
		} else {
//			parent = 
			InfoTargetCard = boxObjs.BoxCardsImage [num].transform;
		}


		Transform Info = InfoTargetCard.Find ("CardInfo");

		//表示を消す
		if (value <= 0 || (value == 1 && Deck)) {
			if (Info != null)
				Destroy (Info.gameObject);
			
			return;
		}

		//新規生成
		if (Info == null) {
			Info = Instantiate (cardInfo.transform,InfoTargetCard);
			Info.localPosition = Vector3.zero;
			Info.localScale = Vector3.one;
			Info.name = "CardInfo";
		}

		if(Deck){
//			x.SetParent(
			Info.Find ("Image").GetComponent<RectTransform> ().sizeDelta = DeckCardInfoSize;
			Info.Find ("Text").GetComponent<Text> ().fontSize = DeckCardInfoFontSize;
			Info.Find ("Text").GetComponent<Text>().text = "×"+value;

		} else{
			Info.Find ("Image").GetComponent<RectTransform> ().sizeDelta = BoxCardInfoSize;

			string str = "";
			if (value == 3) {
				str = "セット上限\n";
			} else {
				str = "使用中\n";
			}
			Info.Find ("Text").GetComponent<Text> ().fontSize = BoxCardInfoFontSize;
			Info.Find ("Text").GetComponent<Text>().text = str+"×"+value;
		}

	}

	int GetContainsCards (int decknum,int atr,int ID) {
		int index = DataManager.Instance.decks [decknum].FindIndex (x => x.Atr == atr && x.ID == ID);
		if (index == -1)
			return 0;
		return DataManager.Instance.decks [decknum] [index].Count;
	}
	public void LvUpNotify (){
		//無効化
		if (!DataManager.Instance.LVSystem) {
			return;
		}

		CardParam cp = GetBoxCardParam (SelectingCard.ID);
		if (cp.Count >= SystemScript.needPoint (cp)) {
			

			AlertView.Make (0,"強化",string.Format(  cp.Name +"\nLV.{0} → LV.{1}",cp.LV,cp.LV+1),new string[]{"OK","Camcel"}, gameObject,1);

		}
//		AlertView.Make (0,"強化","カードを強化しますか",new string[]{"OK"}, gameObject,2);
	}
	public void OnLvup(int cid,int lv,string errmsg) {
		alert.OpenClose (false);
		alert = null;
		//エラー表示
		if (errmsg != "") {
			alert = AlertView.Make (-1, "エラー",errmsg, new string[]{ "確認" }, gameObject, 1);
			return;
		}

		//レベルアップ処理
		DataManager.Box.LevelUp(cid);

		//ボックスデータ更新
		Refresh();

		//再タップ処理
		OnCardTap(SelectingData[0],SelectingData[1]);

		//再取得
		CardParam cp = GetBoxCardParam (SelectingCard.ID);

		//アラート表示
		AlertView.Make (1, "強化成功", string.Format (cp.Name +":LV.{0}\nパワー+1", cp.LV), new string[]{ "OK" }, gameObject, 1);
		DataManager.Instance.SEPlay (10);

	}
	public void OnRecieve(int _num,int _tag){
		if (_num == -1)
			return;
		if (_tag == 0) {
			if (_num == 0) {
				//レベルアップ処理
				CardParam cp = GetBoxCardParam (SelectingCard.ID);
				TestScript.Instance.Delegate = gameObject;
				TestScript.Instance.Lvup (cp.ID,cp.LV+1);

				alert = AlertView.Make (0,"通信中...","しばらくお待ちください",new string[]{}, gameObject,1,true);

//				//レベルアップ処理
//				DataManager.Box.LevelUp(cp.ID);
//
//				Refresh();
//				//再タップ処理
//				OnCardTap(SelectingData[0],SelectingData[1]);
//
//				AlertView.Make (1, "強化成功", string.Format (cp.Name +":LV.{0}\nパワー+1", cp.LV+1), new string[]{ "OK" }, gameObject, 1);
//				DataManager.Instance.SEPlay (10);
			}
		} else if(_tag == 1){
			//強化完了
		} else if (_tag == 2) {
			//デッキ変更
			DataManager.Instance.UseDeck = _num;
			Refresh ();
		} else if(_tag == 3){
			//絞り込み
			RefineIndex = _num;
			BoxRoleRefine (0);
		}
	}

	public void ChangeDeck () {
		

		AlertView.Make (2,"デッキ選択","編集するデッキを選択してください",getDeckNames().ToArray(), gameObject,1);
	}

	public List<string> getDeckNames () {
		List<string> lstr = new List<string> ();
		for (int i = 0; i < 30; i++ ){
			var deck = DataManager.Instance.decks [i];
			int count = 0;
			for (int i2 = 0; i2 < deck.Count; i2++ ){
				count += deck [i2].Count;
			}
			string str = "";
			if (i == useDeck) {
				if (count == 30)
					str += "<color=#00ffff>";
				else
					str += "<color=#ffff00>";
			} else {
				if (count == 30)
					str += "";
				else
					str += "<color=#aaaaaa>";

			}
			str += "デッキ " + (i + 1) + string.Format (" [{0}/30]", count);
			if(i == useDeck || count != 30)
				str += "</color>";
			lstr.Add (str);


		}
		return lstr;
	}

	public void RefineNotify () {
		List<int> indexes = new List<int> ();
		List<string> result = new List<string> ();
		//天候
		result = new List<string>() {"なし","灼熱天候","大雨天候","竜巻天候","光明天候","暗黒天候"};
		indexes = new List<int> (){-1,0,1,2,3,4 };

		//グループ
		var groupParams = DataManager.Instance.xls_Groups.sheets [0].list;
		for (int i = 0; i < groupParams.Count; i++ ){
			string groupName = groupParams [i].name;
			if (groupName != "") {
				result.Add (groupName);
				indexes.Add (i);
			}
		}
		GroupIndexes = indexes;
		AlertView.Make (3,"絞り込み","天候/種族で絞り込み",result.ToArray(), gameObject,1);

	}

}
