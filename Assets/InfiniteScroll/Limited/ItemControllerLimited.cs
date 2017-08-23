using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CardData = SystemScript.CardData;
using CardParam = SystemScript.CardParam;

[RequireComponent(typeof(InfiniteScroll))]
public class ItemControllerLimited : UIBehaviour, IInfiniteScrollSetup {

	public List<CardParam> cards = new List<CardParam>();
	public bool DeckMode;
	[SerializeField, Range(1, 999)]
	private int max = 30;

	public void OnPostSetupItems()
	{
		max = cards.Count;
		var infiniteScroll = GetComponent<InfiniteScroll>();
		infiniteScroll.onUpdateItem.RemoveAllListeners();
		infiniteScroll.onUpdateItem.AddListener(OnUpdateItem);
//		GetComponentInParent<ScrollRect>().movementType = ScrollRect.MovementType.Elastic;

		var rectTransform = GetComponent<RectTransform>();
		var delta = rectTransform.sizeDelta;
		delta.y = infiniteScroll.itemScale * max;
		rectTransform.sizeDelta = delta;
	}

	public void OnUpdateItem(int itemCount, GameObject obj)
	{
		if(itemCount < 0 || itemCount >= max || (itemCount >= cards.Count)) {
			obj.SetActive (false);
		}
		else {
			obj.SetActive (true);
			var item = obj.GetComponentInChildren<CardNodeScript>();
			item.nowIndex = itemCount;
			item.Refresh (new CardData ().Set (cards [itemCount]),DeckMode);
		}
	}
	public void OnResetItem(GameObject obj)
	{
		int itemCount = obj.GetComponent<CardNodeScript>().nowIndex;
		if (itemCount < 0 || itemCount >= max || (itemCount >= cards.Count)) {
			obj.SetActive (false);
		} else {
			obj.SetActive (true);
			var item = obj.GetComponentInChildren<CardNodeScript> ();
			item.Refresh (new CardData ().Set (cards [itemCount]), DeckMode);
		}
	}
}
