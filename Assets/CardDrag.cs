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
	float XMove = 25f;
	public bool Disable = false;
	int reachEnd = 0;//-1=下 0=間 1=上 
	bool YDrag = false;
	bool XDrag = false;
//	public UnityEvent OnDown;
	public GameObject Delegate;

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


	public void OnDrag(PointerEventData eventData)
	{
		if (!Disable) {
			TotalDeltaY += eventData.delta.y;
			TotalDeltaX += eventData.delta.x;
			float y = TotalDeltaY;
			//しきい値に到達した時

			if (MaxY <= TotalDeltaY) {
				y = MaxY;
				if (reachEnd != 1) {
					DataManager.Instance.SEPlay (9);
					reachEnd = 1;
				}
			} else if (MinY >= TotalDeltaY) {
				y = MinY;
				if (reachEnd != -1) {
					DataManager.Instance.SEPlay (9);
					reachEnd = -1;
				}
			} else {
				reachEnd = 0;
			}

			transform.localPosition = new Vector3 (transform.localPosition.x, DefaultY + y, 0);
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
//
		if (reachEnd != 0) {//カード入れるor抜く
			DataManager.Instance.SEPlay (1);
			reachEnd = 0;
		}
		TotalDeltaY = 0;
		transform.localPosition = new Vector3 (transform.localPosition.x, DefaultY, 0);

	}

}
