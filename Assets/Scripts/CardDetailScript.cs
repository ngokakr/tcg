using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CardParam = SystemScript.CardParam;
public class CardDetailScript : MonoBehaviour {
	public CanvasGroup DetailCanvas;
	public Text Name;
	public Text Param;
	public Text Skill;
	public CardImageScript CardImage;
	public CardImageScript CardBigImage;
	public GameObject Big;
	public Image LvUpImage;
	public Text LvUpText;
	public int DeckInCount;
	public int BoxCount;
	public GameObject[] Buttons;
	public DeckBoxScript deckBoxScript;
	public float FadeTime = 0.1f;
	public void Start () {
	}
	public void ShowHide (bool _Show) {
		if (_Show == false)
			DataManager.Instance.SEPlay (2);
		StartCoroutine (Fade(_Show));
	}
	public void HideNoSE () {
		StartCoroutine (Fade(false));
	}
	IEnumerator Fade (bool _in) {
		if (_in != DetailCanvas.blocksRaycasts) {
			DetailCanvas.blocksRaycasts = _in;
			float time = 0f;
			while (time < FadeTime) {
				time += Time.deltaTime;
				if (_in)
					DetailCanvas.alpha = time / FadeTime;
				else
					DetailCanvas.alpha = 1 - (time / FadeTime);
				yield return null;
			}
		}
	}
	public void Refresh (CardParam _param,bool _DeckMode) {
//		DeckInCount = DataManager.Deck.GetCard(DataManager.Instance.UseDeck,_param.Atr,_param.ID).Count;
//		BoxCount = DataManager.Box.GetCard(_param.Atr,_param.ID).Count;

		//名前+種族
		string Title = System.String.Format 
			("LV.{0} {1} ", new string[] {_param.LV.ToString(),_param.Name.ToString()});
		if (_param.Groups.Count > 0) {
			Title += "\n<size=25>" + SystemScript.GetGroup (_param.Groups) + "</size>";
		}
		Name.text = Title;

		//パラメータ

		Param.text = System.String.Format 
			("{0}\n役割:{1}\nコスト:{2}\nパワー:{3}"
				, new string[] {SystemScript.GetReality(_param.Rea,true),SystemScript.GetRole(_param.Role),_param.Cost.ToString(),_param.Power.ToString()});
		//
		Skill.text = SystemScript.GetSkillsText(_param,SystemScript.ShowType.DETAIL,true);

		//LVUP関連
//		LvUpText.text = System.String.Format ("LV.{0}→LV.{1}\n{2} / {3}枚",new string[]{_param.LV.ToString(),(_param.LV+1).ToString(),BoxCount.ToString(),999.ToString()});
		LvUpText.text  = "強化";
		//イメージ変更
		CardImage.Set (_param);
		CardBigImage.Set (_param);

		//ボタン変更
		for (int i = 0; i < Buttons.Length; i++ ){
			GameObject btn = Buttons [i];
			if(i== DeckInCount)
				btn.GetComponent<ShadowMove> ().Move (true,false);
			else
				btn.GetComponent<ShadowMove> ().Move (false,false);
		}
//		Param.text = _param.

	}

//	public void SetCard (int _num) {
//		for (int i = 0; i < Buttons.Length; i++ ){
//			GameObject btn = Buttons [i];
//			if(i== _num)
//				btn.GetComponent<ShadowMove> ().Move (true,true);
//			else
//				btn.GetComponent<ShadowMove> ().Move (false,false);
//		}
//		deckBoxScript.SetCard (_num);
//		ShowHide (false);
//	}
	public void BigCard (bool _Show) {
		DataManager.Instance.SEPlay (5);
		Big.SetActive (_Show);
	}
	public void LVUp () {
		
	}
}
