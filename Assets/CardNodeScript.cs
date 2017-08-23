using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CardData = SystemScript.CardData;
using UnityEngine.EventSystems;

public class CardNodeScript : MonoBehaviour {
	bool ShowBtn = false;
	public Button InfoButton;

//	public int 

	[SerializeField]
	CardImageScript CardImage;
	[SerializeField]
	Text CardDataText;

	[SerializeField]
	int NameTextSize;
	[SerializeField]
	int SkillTextSize;
	[SerializeField]
	string ResoursePass;

	public int nowIndex = -1;

	public GameObject Delegate;

	public void Refresh(CardData _cd,bool _DeckMode){
		//テキスト

		//タイトル
		XLS_CardParam.Param param = DataManager.Instance.GetCardParam (_cd);


		string Title = System.String.Format 
			("{0} LV.{1} ", new string[] {param.name,_cd.LV.ToString()});
		//調整
		CardDataText.fontSize = SkillTextSize;
		CardDataText.text = Title;
		//文字色
		if (!_DeckMode) {
			if (DataManager.Deck.ContainCard (DataManager.Instance.UseDeck, _cd.uid)) {
				CardDataText.color = new Color(0f,1f,1f);
			} else {
				CardDataText.color = Color.white;
			}
		}
//		if (_inDeck)
//			CardDataText.color = new Color(0f,1f,1f);
//		else
//			CardDataText.color = Color.white;
		//カードイメージ
		CardImage.Set(new SystemScript.CardParam().Set(_cd));
		//ノードタップ時
		GetComponent<Button> ().onClick.RemoveAllListeners();
		GetComponent<Button> ().onClick.AddListener (() => AleartNotify(nowIndex,0));
		//詳細ボタンタップ時
		InfoButton.onClick.RemoveAllListeners();
		InfoButton.onClick.AddListener (() => AleartNotify(nowIndex,-1));
	}

//	public void Refresh(CardData _cd,bool _DeckMode){
//		//テキスト
//
//		//タイトル
//		XLS_CardParam.Param param = DataManager.Instance.GetCardParam (_cd);
//
//
//		string Title = System.String.Format 
//			("<size={0}>{2} LV.{1} ", new string[] {NameTextSize.ToString(),_cd.LV.ToString(),param.name});
//		if (_DeckMode) {
//			Title += "[×" + _cd.Count.ToString()+"]";
//		}
//		if (!_DeckMode) {
//			int count = DataManager.Deck.GetCard (DataManager.Instance.UseDeck, _cd.Atr, _cd.ID).Count;
//			if(count!= 0)
//				Title += "[×"+DataManager.Deck.GetCard(DataManager.Instance.UseDeck,_cd.Atr,_cd.ID).Count +"]";
//		}
//		Title += "</size>\n";
//
//		//スキル
//		var cp = new SystemScript.CardParam ().Set (_cd);
//		string SkillStr = SystemScript.GetSkillsText(cp,SystemScript.ShowType.SMART,true);
//
//		//調整
//		CardDataText.fontSize = SkillTextSize;
//		CardDataText.text = Title + SkillStr;
//		CardDataText.color = Color.white;
//		//背景色
//
////		gameObject.GetComponent<Image>().color = DataManager.Instance.CardBackColors[_cd.Atr];
////		gameObject.GetComponent<Image>().color = Color.black;
//		//カードイメージ
//		CardImage.Set(new SystemScript.CardParam().Set(_cd));
//		//タップ時の反応を設定
//		Button btn = GetComponent<Button> ();
//		btn.onClick.AddListener (() => AleartNotify(int.Parse(gameObject.name)));
//	}
	public void AleartNotify (int _num,int _type) {
		//タップ有効化
//		DataManager.Instance.TouchAble();

		// デリゲート先のOnRecieveに送る
		ExecuteEvents.Execute<IRecieveMessage>(
			target: Delegate, // 呼び出す対象のオブジェクト
			eventData: null,  // イベントデータ（モジュール等の情報）
			functor: (recieveTarget,y)=>recieveTarget.OnRecieve(_num,_type)); // 操作
	}
	public void DetailNotify (int _num) {
	}
	// Use this for initialization
	void Start () {
	}
}
