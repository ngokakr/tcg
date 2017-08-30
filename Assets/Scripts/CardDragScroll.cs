using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using UniRx;
using System;
using UnityEngine.Events;

//参考 http://kohki.hatenablog.jp/entry/Unity-uGUI-Scroll-Long-Press

public class CardDragScroll : ScrollRect ,IPointerDownHandler,IPointerUpHandler{
	public float DefaultY;
	float TotalDeltaY;
	float TotalDeltaX;
	float MaxY = 25f, MinY = -25f;
	float XMove = 25f;//これ以上超えたらYドラッグはしない。
	public bool Disable = false;
	int reachEnd = 0;//-1=下 0=間 1=上 
	bool YDrag = false;
	bool XDrag = false;
	Transform t;
	public GameObject Delegate;
//	public UnityEvent OnClick;

	//押下と同時
	public void OnPointerDown (PointerEventData eventData)
	{
		t = eventData.pointerPressRaycast.gameObject.transform;

		if (t.GetComponent<CardImageScript> () == null) {//カードのみを掴む
			Disable = true;
		} else {
			Disable = false;
		}
		DefaultY = t.localPosition.y;//個々のカードのY座標
		if (!Disable) {
			//デフォルト座標
//			DefaultY = transform.localPosition.y;
			//最前面に移動

//			transform.SetAsLastSibling ();
		} else {
			eventData.pointerDrag = null;
		}
	}


	public override void OnDrag(PointerEventData eventData)
	{
		if (!Disable) {
			TotalDeltaY += eventData.delta.y;
			TotalDeltaX += eventData.delta.x;
			float y = TotalDeltaY;
			//xがしきい値に到達した
			if (XMove < Math.Abs (TotalDeltaX) && !YDrag) {
				XDrag = true;
			}
			if (!XDrag) {
				//yがしきい値に到達した時
				if (MaxY <= TotalDeltaY) {
					YDrag = true;
					y = MaxY;
					if (reachEnd != 1) {
						DataManager.Instance.SEPlay (9);
						reachEnd = 1;
					}
				} else if (MinY >= TotalDeltaY) {
					YDrag = true;
					y = MinY;
					if (reachEnd != -1) {
						DataManager.Instance.SEPlay (9);
						reachEnd = -1;
					}
				} else {
					reachEnd = 0;
				}

				if (YDrag) {
					t.localPosition = new Vector3 (t.localPosition.x, DefaultY + y, 0);
					return;//スクロールビューの動作をさせない。
				}
			}
		}
		base.OnDrag (eventData);
	}

	//離す時
	public void OnPointerUp (PointerEventData eventData)
	{
		
		if (!Disable)
			RepositionMove ();
	}

	//元の位置に戻る。
	void RepositionMove () {
		if (YDrag) {//yドラッグされていた時
			t.localPosition = new Vector3 (t.localPosition.x, DefaultY, 0);
			ExecuteEvents.Execute<ICardDragHandler> (
				target: Delegate, // 呼び出す対象のオブジェクト
				eventData: null,  // イベントデータ（モジュール等の情報）
				functor: (recieveTarget, edata) => recieveTarget.OnVartical (reachEnd,t.parent.GetSiblingIndex(),1));
		}
		if (!XDrag && !YDrag) {//タップ
			ExecuteEvents.Execute<ICardDragHandler> (
				target: Delegate, // 呼び出す対象のオブジェクト
				eventData: null,  // イベントデータ（モジュール等の情報）
				functor: (recieveTarget, edata) => recieveTarget.OnCardTap (t.parent.GetSiblingIndex(),1));
		}

		if (reachEnd != 0) {//カード入れるor抜く
			DataManager.Instance.SEPlay (1);
			reachEnd = 0;
		}


		XDrag = false;
		YDrag = false;
		TotalDeltaX = 0;
		TotalDeltaY = 0;
		reachEnd = 0;
	}

}
