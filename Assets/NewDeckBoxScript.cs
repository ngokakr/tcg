using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using CardData = SystemScript.CardData;
using CardParam = SystemScript.CardParam;
using DG.Tweening;

public class NewDeckBoxScript : MonoBehaviour ,IRecieveMessage,ICardDragHandler{
	
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
		public GameObject CantGenerateButton;

//		public GameObject deckCardImages;
	}
	public BoxObjs boxObjs;
	public CardDragScroll cardDragScroll;
	public NewCardDetailScript detailScript;

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
		SortedBox = SystemScript.PackCardParams(SortedBox);

		//絞り込み
		SortedBox =  CardRefine(SortedBox,new List<int>(),RoleRefine);


		//デッキ取得
		SortedDeck = SystemScript.cdTocp( DataManager.Deck.GetDeckData(useDeck));

		//ソート
		Sort(SortedDeck);

		//絞り込み
		SortedDeck = CardRefine(SortedDeck,new List<int>(),-1);


		//ボックス表示
		for (int i = 0; i < boxObjs.BoxCardsImage.Length; i++ ){
			CardImageScript img = boxObjs.BoxCardsImage [i];
			int x = nowPage * 10 + i;
			if (x < SortedBox.Count) {
				img.gameObject.SetActive (true);
				img.Set (SortedBox [x]);
			} else {
				img.gameObject.SetActive (false);
				img.RemoveImage ();
			}
		}

		//デッキ表示
		var contents = boxObjs.ScrollContent;
		for (int i = 0; i < contents.childCount; i++ ){
			Transform t = contents.GetChild (i);
			if (i < SortedDeck.Count) {
				t.gameObject.SetActive (true);
				t.Find ("Card").GetComponent<CardImageScript> ().Set (SortedDeck [i]);
			} else {
				t.gameObject.SetActive (false);
				t.Find ("Card").GetComponent<CardImageScript> ().RemoveImage ();
			}
		}

		Resources.UnloadUnusedAssets ();
		//→表示


		MaxPage = (SortedBox.Count - 1) / 10;

		boxObjs.PageInfo.text = string.Format ("CARD BOX {0}/{1}",nowPage+1,MaxPage+1);

		int cardCounts = 0;
		for (int i = 0; i < SortedDeck.Count; i++ ){
			cardCounts += SortedDeck [i].Count;
		}

		//データ表示 & カードデータ生成
		boxObjs.Info .text = string.Format("カード総数 {0}  種類 {1}\nデッキ枚数 [<color=#00ff00>{2} / 30</color>]",BoxCount,kindCount,cardCounts);

	}

	public void ShowDetail () {
		detailScript.Show (SelectingCard);
	}

	/// <summary>
	/// _Role -1=ALL 0=Charger 1=Attacker 2=Defender
	/// </summary>
	List<CardParam> CardRefine (List<CardParam> _Cards,List<int> _Attribute, int _Role) {
		List<CardParam> Result = new List<CardParam> ();
		for (int i = 0; i < _Cards.Count; i++ ){
			CardParam cd = _Cards [i];

			if (_Attribute.Count != 0) {
				if (!_Attribute.Contains (cd.Atr))
					continue;
			}

			if (_Role != -1) {
				if (_Role != cd.Role)
					continue;
			}
			Result.Add (cd);
		}
		return Result;
	}
	public void OnRecieve(int _num,int _tag){

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
		Debug.Log(string.Format("num:{0} tag;{1}",_num,_tag));
		if(_tag == 0)
			BoxCardTapNotify (_num);
		if (_tag == 1)
			DeckTapNotify (_num);
		cardDragScroll.transform.GetChild(0).GetChild(0).GetComponent<ContentSizeFitter> ().SetLayoutHorizontal ();
	}

	public void OnVartical (int value,int _num, int _tag){
		if (_tag == 0) {
			BoxCardTapNotify (_num);
			SetCard (value == -1 ? false : true);
		} else if (_tag == 1) {
			DeckTapNotify (_num);
			SetCard (value == -1 ? false : true);
		}
		Debug.Log (cardDragScroll.transform.GetChild (0).GetChild (0).name);

	}
	public void OnHorizontal (int value,int _num, int _tag){
		if(_tag == 0)
			ArrowKeyNotify (value == -1 ? true : false);
	}

}
