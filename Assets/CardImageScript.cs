using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CardParam = SystemScript.CardParam;
using UiEffect;
[ExecuteInEditMode]
public class CardImageScript : MonoBehaviour {
	public int Pass;
	public Image CardImage;
	public Text CostText;
	public Text PowerText;

	public Image BG;
	public Image Frame;
	public Image Reality;
	public Image Role;
	public Image Power;
	public Image Attribute;

	public bool Change = false;
	public int TestAtr = 0;
	public int TestID = 0;
	public float MaxHeight = 100f;
	public float MaxWidth = 100f;

	int Atr=-1;
	int ID=-1;
	int role = -1;
	public void Set (CardParam _cp,bool _CanSummon = true) {
		if (!(_cp.Atr == Atr && _cp.ID == ID && _cp.Role == role)) {
			Atr = _cp.Atr;
			ID = _cp.ID;
			role = _cp.Role;
			CardImage.sprite = SystemScript.GetCardSprite (_cp);
			CardImage.SetNativeSize ();
			float width = CardImage.sprite.rect.width;
			float height = CardImage.sprite.rect.height;
			float widthRate = width / MaxWidth;
			float HeightRate = height / MaxHeight;
//			Debug.Log (width + ":"+height + ":"+MaxWidth + ":"+MaxHeight + ":"+ widthRate + " : " + HeightRate);
			if (widthRate > HeightRate)
				CardImage. transform.localScale = new Vector3( 1 / widthRate,1 / widthRate);
			else
				CardImage. transform.localScale = new Vector3( 1 / HeightRate,1 / HeightRate);

//			GetComponent<RectTransform> ().sizeDelta = size;

			if (Attribute != null) {
				DataManager.CardImageParts parts = DataManager.Instance.cardParts;

				BG.sprite = parts.BG [_cp.Role * 4 + _cp.Rea];
				Frame.sprite = parts.Frame [_cp.Role];
				Reality.sprite = parts.Reality [_cp.Rea];
				Power.sprite = parts.Power [_cp.Role];
				Role.sprite = parts.Role [_cp.Role];
				Attribute.sprite = parts.Attribute [_cp.Atr];
			} else {
//				Color[] top = new Color[3];
//				Color[] btm = new Color[3];
//				top [0] = new Color (118f / 255f, 0f, 240f / 255f);
//				btm [0] = new Color (33f / 255f, 0f, 51f / 255f);
//				top [1] = new Color (0f / 255f,  190f/255f, 240f / 255f);
//				btm [1] = new Color (11f / 255f, 17f/255f, 90f / 255);
//				top [2] = Color.white;
//				btm [2] = new Color (23f / 255f, 23f/255f, 23f / 255);
//
//				BG.GetComponent<GradientColor> ().colorTop = top [_cp.Role];
//				BG.GetComponent<GradientColor> ().colorBottom = btm [_cp.Role];
//				BG.GetComponent<GradientColor> ().Refresh ();
				BG.sprite = SystemScript.GetSprite ("CardParts/BG/" + _cp.Role + "/" + _cp.Rea);
				Frame.sprite = SystemScript.GetSprite ("CardParts/frame/" + _cp.Role);
				Reality.sprite = SystemScript.GetSprite("CardParts/cost/"+_cp.Rea);
				Power.sprite = SystemScript.GetSprite("CardParts/power/"+_cp.Role);

			}
		}
		Color a = new Color (1f, 1f, 1f, 1f);
		if(_CanSummon == false)
			a = new Color (1f, 1f, 1f, 0.5f);
		
		CardImage.color = a;
		BG.color = a;
		Frame.color = a;
		Reality.color = a;
		Power.color = a;
		if(Role != null)
			Role.color = a;
		if(Attribute != null)
		Attribute.color = a;
		Power.color = a;

//		int tempCost = 0;
//		int tempPower = 0;
//
//		for (int i = 0; i < _cp.Effect.Count; i++ ){
//			int effNum = _cp.Effect [i];
//			if (effNum == 12) {//コスト補正
//				tempCost = _cp.EffectValue[i];
//			}
//			if (effNum == 13) {//パワー補正
//				tempPower = _cp.EffectValue[i];
//			}
//		}

		CostText.text = (_cp.Cost).ToString();
		PowerText.text = (_cp.Power).ToString();
		Pass = _cp.Pass;
	}

	public void RemoveImage () {
		CardImage.sprite = null;
	}
//	public void 
	// Use this for initialization
	public void DestroyCard () {
	}

	public void RevivalCard () {
		
	}
//	public void 
	void Start () {
		
	}
	
//	// Update is called once per frame
	void Update () {
		if (Change) {
			Set (new CardParam ().Set (new SystemScript.CardData ().Set (TestAtr, TestID, 0, 1)));
			Change = false;
		}
	}
}
