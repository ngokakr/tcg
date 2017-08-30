using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using UniRx;
using System;
using UnityEngine.Events;

public class CardDrag : MonoBehaviour, IDragHandler,IPointerDownHandler, IPointerUpHandler {
	float DefaultY;
	float TotalDeltaY;
	float TotalDeltaX;
	public float MaxY = 25f, MinY = -25f;
	public float XMove = 25f;
	public float XPagingTime =  0f;
	public float XPagingInterval = 0.25f;
	public bool Disable = false;
	public int reachEnd = 0;//-1=下 0=間 1=上 
	public bool YDrag = false;
	public bool XDrag = false;
//	public UnityEvent OnDown;
	public GameObject Delegate;
	public int Tag;

	public void Awake () {
		DefaultY = transform.localPosition.y;
	}

	//押下と同時
	public void OnPointerDown (PointerEventData eventData)
	{
		if (!Disable) {
			//デフォルト座標

			//最前面に移動
			transform.SetAsLastSibling ();
		} else {
			eventData.pointerDrag = null;
		}
	}

	public void Update () {
		
		if (XDrag && reachEnd != 0 && XPagingTime <= 0) {
			ExecuteEvents.Execute<ICardDragHandler> (
				target: Delegate, // 呼び出す対象のオブジェクト
				eventData: null,  // イベントデータ（モジュール等の情報）
				functor: (recieveTarget, edata) => recieveTarget.OnHorizontal (reachEnd,int.Parse( name), Tag)); // 操作
			XPagingTime = XPagingInterval;
//			DataManager.Instance.SEPlay (1);
		}
		if(0 < XPagingTime)
			XPagingTime -= Time.deltaTime;
	}
	public void OnDrag(PointerEventData eventData)
	{
		if (!Disable) {
			TotalDeltaY += eventData.delta.y;
			TotalDeltaX += eventData.delta.x;
			float y = TotalDeltaY;
			float x = TotalDeltaX;

			if (!YDrag) {
				//xがしきい値に到達した
				if (XMove <= TotalDeltaX) {
					XDrag = true;
					reachEnd = 1;
//					if (XPagingTime <= 0) {
//						reachEnd = 1;
//						ExecuteEvents.Execute<ICardDragHandler> (
//							target: Delegate, // 呼び出す対象のオブジェクト
//							eventData: null,  // イベントデータ（モジュール等の情報）
//							functor: (recieveTarget, edata) => recieveTarget.OnHorizontal (reachEnd, Tag)); // 操作
//						XPagingTime = XPagingInterval;
//
//					}
//					XPagingTime -= Time.deltaTime;
				} else if (-XMove >= TotalDeltaX) {
					XDrag = true;
					reachEnd = -1;
//					if (XPagingTime <= 0) {
//						reachEnd = -1;
//						ExecuteEvents.Execute<ICardDragHandler> (
//							target: Delegate, // 呼び出す対象のオブジェクト
//							eventData: null,  // イベントデータ（モジュール等の情報）
//							functor: (recieveTarget, edata) => recieveTarget.OnHorizontal (reachEnd, Tag)); // 操作
//						XPagingTime = XPagingInterval;
//
//					}
//					XPagingTime -= Time.deltaTime;
				} else {
					reachEnd = 0;
					XPagingTime = 0f;
				}
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
					transform.localPosition = new Vector3 (transform.localPosition.x, DefaultY + y, 0);

				}
			}
		}
	}

	//離す時
	public void OnPointerUp (PointerEventData eventData)
	{
		if (!Disable)
			RepositionMove ();
	}

	//元の位置に戻る。
	void RepositionMove () {
//		// デリゲート先のOnRecieveに送る
//		ExecuteEvents.Execute<IRecieveMessage>(
//			target: Delegate, // 呼び出す対象のオブジェクト
//			eventData: null,  // イベントデータ（モジュール等の情報）
//			functor: (recieveTarget,y)=>recieveTarget.OnRecieve(_num,Tag)); // 操作
//
		if (XDrag) {//xドラッグされていた時
//			ExecuteEvents.Execute<ICardDragHandler> (
//				target: Delegate,
//				eventData: null,
//				functor: (recieveTarget,edata)=>recieveTarget.OnHorizontal(reachEnd,Tag));
//
//			);
			Debug.Log(reachEnd);
		} else if (YDrag) {//yドラッグされていた時
			transform.localPosition = new Vector3 (transform.localPosition.x, DefaultY, 0);
			ExecuteEvents.Execute<ICardDragHandler> (
				target: Delegate, // 呼び出す対象のオブジェクト
				eventData: null,  // イベントデータ（モジュール等の情報）
				functor: (recieveTarget, edata) => recieveTarget.OnVartical (reachEnd,int.Parse(name), Tag));
			
		} else {//クリック
//			DataManager.Instance.SEPlay (0);
			ExecuteEvents.Execute<ICardDragHandler> (
				target: Delegate, // 呼び出す対象のオブジェクト
				eventData: null,  // イベントデータ（モジュール等の情報）
				functor: (recieveTarget, edata) => recieveTarget.OnCardTap (int.Parse(name), Tag));
		}

		if (reachEnd != 0) {//カード入れるor抜く
//			DataManager.Instance.SEPlay (1);
			reachEnd = 0;
		}
		OnDisable ();
		transform.localPosition = new Vector3 (transform.localPosition.x, DefaultY, 0);

	}
	void OnDisable() {
		XDrag = false;
		YDrag = false;
		TotalDeltaX = 0;
		TotalDeltaY = 0;
	}
}
