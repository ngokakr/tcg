using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CardData = SystemScript.CardData;
using CardParam = SystemScript.CardParam;
using DG.Tweening;
public class DeckBoxScript : MonoBehaviour ,IRecieveMessage{

	[System.Serializable]
	public struct DeckObjs {
		public GameObject DeckView;
		public Transform ScrollContent;
		public Image[] Images;

		public Text Title;
		public Text Info;
		public int RoleRefine;
	}

	public DeckObjs deckObjs;
	int useDeck = 0;

	[System.Serializable]
	public struct BoxObjs {
		public GameObject BoxView;
		public Transform ScrollContent;
		public Text Info;
		public ShadowMove[] RoleRefineButton;
		public int RoleRefine;
	}
	public BoxObjs boxObjs;


	public CardNodeScript cardNode;

	public InfiniteScroll[] infinitys;
	public ItemControllerLimited[] itemCtrls;

	//使用中のデッキ
	[SerializeField]
	List<CardParam> SortedDeck = new List<CardParam> ();
	[SerializeField]
	List<CardParam> SortedBox =new List<CardParam>();
	public CardDetailScript cardDatail;

	public CardParam SelectingCard;

	public bool DeckMode = true;
	public bool IDSort = false;

	public void Refresh () {
		Resources.UnloadUnusedAssets ();
		if (DeckMode)
			DeckRefresh ();
		else
			BoxRefresh ();
	}

	public void DeckShow () {
		//アクティブ
		useDeck = DataManager.Instance.UseDeck;
		deckObjs.DeckView.SetActive (true);
		boxObjs.BoxView.SetActive (false);
		DeckMode = true;
		DeckRefresh ();
	}

	public void DeckRefresh () {
		//デッキ取得
		SortedDeck = SystemScript.cdTocp( DataManager.Deck.GetDeckData(useDeck));
		//そーと
		Sort(SortedDeck);
		//絞り込み
//		SortedDeck = CardRefine(SortedDeck,new List<int>(),-1);

		//データ表示 & カードデータ生成
		deckObjs.Info .text = "デッキ枚数 ["+
			SortedDeck.Count+ " / 30]\n■タップで詳細を見る";
		
		//Node破壊
//		foreach ( Transform n in deckObjs.ScrollContent )
//		{
//			GameObject.Destroy(n.gameObject);
//		}
//		List<CardParam> lcp = new List<CardParam>();
//		for (int i = 0; i < 30; i++ ){
//			lcp.AddRange (SortedDeck);
//		}
		itemCtrls [0].cards = SortedDeck;
		itemCtrls [0].DeckMode = DeckMode;
		infinitys [0].show ();

//		//Node生成
//		for (int i = 0; i < SortedDeck.Count; i++ ){
//			CardNodeScript node = Instantiate (cardNode);
//			node.name = i + "";
//			node.Delegate = gameObject;
//			node.transform.SetParent (deckObjs.ScrollContent);
//			node.transform.localScale = Vector3.one;
//			node.transform.localPosition = Vector3.zero;
//			CardData cd = new CardData ().Set (SortedDeck [i]);
//			node.Refresh (cd,false);
//				
//		}
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

	public int NumberOfCard (List<CardParam> _Cards) {
		return _Cards.Count;
	}

	public void BoxShow () {
		deckObjs.DeckView.SetActive (false);
		boxObjs.BoxView.SetActive (true);
		DeckMode = false;
		BoxRefresh ();
	}
	public void BoxRefresh () {
		//ボックス取得
		var cds = DataManager.Instance.box;
		//そーと
		SortedBox = new List<CardParam> ();
		for (int i = 0; i < cds.Count; i++ ){
			CardParam param = new CardParam ();
			param.Set (cds [i]);
			SortedBox.Add (param);
		}
		Sort (SortedBox);

		//絞り込み
		SortedBox = CardRefine(SortedBox,new List<int>(),boxObjs.RoleRefine);

//		//Node破壊
//		foreach ( Transform n in boxObjs.ScrollContent )
//		{
//			GameObject.Destroy(n.gameObject);
//		}
//		//Node生成
//		for (int i = 0; i < SortedBox.Count; i++ ){
//			CardNodeScript node = Instantiate (cardNode);
//			node.name = i + "";
//			node.Delegate = gameObject;
//			node.transform.SetParent (boxObjs.ScrollContent);
//			node.transform.localScale = Vector3.one;
//			node.transform.localPosition = Vector3.zero;
//			CardData cd = new CardData ().Set (SortedBox [i]);
//			//色変更
//			if(DataManager.Deck.ContainCard(useDeck,cd.uid)){
//				node.Refresh (cd, true);
//			} else {
//				node.Refresh (cd,false);
//			}
//
//		}
		//デッキ取得
		SortedDeck = SystemScript.cdTocp( DataManager.Deck.GetDeckData(useDeck));
		//絞り込み
		SortedDeck = CardRefine(SortedDeck,new List<int>(),-1);
		//データ表示 & カードデータ生成
		boxObjs.Info .text = "デッキ枚数 ["+
			SortedDeck.Count+ " / 30]\n■タップで詳細を見る";

		itemCtrls [1].cards = SortedBox;
		itemCtrls [1].DeckMode = DeckMode;
		infinitys [1].show ();
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
	public void ViewChange (bool _ToDeck) {
		StartCoroutine (ViewChangeI(_ToDeck));
	}

	IEnumerator ViewChangeI (bool _ToDeck) {
		DataManager.Instance.SEPlay (0);
		float DurTime = 0.1f;
		transform.DOScaleX (0f,DurTime).SetEase(Ease.Linear);
		yield return new WaitForSeconds (DurTime);

		if (_ToDeck)
			DeckShow ();
		else
			BoxShow ();

		transform.DOScaleX (1f,DurTime).SetEase(Ease.Linear);
	}

	/// <summary>
	/// _type 0=ノードタップ -1=詳細表示
	/// </summary>
	/// <param name="_num">Number.</param>
	/// <param name="_tag">Tag.</param>
	public void OnRecieve(int _num,int _type){
		if (_type == 0)
			DataManager.Instance.SEPlay (0);
		else if (_type == -1) {

			DataManager.Instance.SEPlay (7);
		}
		Debug.Log (_num);
		if (DeckMode) {
			//デッキモード
			SelectingCard = SortedDeck [_num];

			if (_type == 0) {
				//取り除く
				DataManager.Deck.SetCard (useDeck, SelectingCard.Atr,SelectingCard.ID, false);
			} else
				//詳細表示
				DetailShow (SortedDeck [_num]);
		} else {
			//ボックスモード
			SelectingCard = SortedBox[_num];
			if (_type == 0) {
				//デッキにカードが含まれているか
				if (DataManager.Deck.ContainCard (useDeck,SelectingCard.Atr,SelectingCard.ID)) {
					//取り除く
					DataManager.Deck.SetCard(useDeck,SelectingCard.Atr,SelectingCard.ID,false);
				} else {
					//デッキに追加
					DataManager.Deck.SetCard(useDeck,SelectingCard.Atr,SelectingCard.ID,true);
				}
			} else
				//詳細表示
				DetailShow (SortedBox [_num]);
		}
		Refresh ();
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
			boxObjs.RoleRefine = -1;
			break;
		case 1:
			boxObjs.RoleRefine = 1;
			break;
		case 2:
			boxObjs.RoleRefine = 0;
			break;
		case 3:
			boxObjs.RoleRefine = 2;
			break;
			
		}
		Refresh ();
	}

	public void PlayerHandNotify (int _num) {
		
	}
	public void EnemyHandNotify (int _num){
		
	}
	public void DetailShow (CardParam _cp) {
		
		cardDatail.Refresh (_cp,DeckMode);
		cardDatail.ShowHide (true);
	}
	public void SetCard (bool _Set){
		DataManager.Deck.SetCard (useDeck,SelectingCard.Atr,SelectingCard.ID,_Set);
		Refresh ();
	}

}
