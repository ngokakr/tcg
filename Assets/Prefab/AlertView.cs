using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
public class AlertView : MonoBehaviour {
	public float DefaultHeight = 125f;
	public float NodeHeight = 125f;
	public float Maxheight = 400f;
	public int Tag = 0;
	public Color DefaultColor;
	[SerializeField]
	private GameObject Content;
	[SerializeField]
	private Image TitleImg;
	[SerializeField]
	private Text Title;
	[SerializeField]
	private Text Description;
	public GameObject Node;
	public GameObject Delegate;
	public GameObject CancelButton;

	private bool Selected = false;

	/// <summary>
	/// _Scene Title/0 Menu/1 Battle/2
	/// </summary>
	static public AlertView Make (int _tag,string _Title,string _Description, string[] _choices, GameObject _delegate,int _Scene,bool _CantCancel = false) {
		//タップ無効化
		DataManager.Instance.TouchDisable (_Scene);
		//アラート生成
		AlertView _alt = Instantiate (DataManager.Instance.AlertPrefab);
		_alt.transform.SetParent(DataManager.Instance.AlertParents[_Scene]);
		_alt.Show (_delegate, _Title, _Description, _choices,_tag,_CantCancel);
		return _alt;
	}
	public void Show (GameObject _delegate,string _Title,string _Description, string[] _choices,int _tag,bool _CantCancel) {


		//フラグ
		Selected = false;
		//タグ
		Tag = _tag;
		//デリゲート
		Delegate =_delegate;
		//SE
		DataManager.Instance.SEPlay(4);
		//テキスト変更
		Title.text = _Title;
		Description.text = _Description;

//		//タイトル色
//		if (_warn) {
//			TitleImg.color = Color.red;
//		}
//
		//自動サイズ調整
		GetComponent<RectTransform> ().localPosition = Vector3.zero;
		Vector2 size = GetComponent<RectTransform> ().sizeDelta;
		float height = DefaultHeight + NodeHeight * _choices.Length;
		if (Maxheight < height)
			height = Maxheight;
		size = new Vector2 (size.x,height);
//		Debug.Log (size);
		GetComponent<RectTransform> ().sizeDelta = size;



		//ノード作成
		int childCount = Content.transform.childCount;
		for (int i = 0; i < childCount; i++ ){
			DestroyImmediate (Content.transform.GetChild (0).gameObject);
		}
		for (int i = 0; i < _choices.Length; i++ ){
			GameObject node = Instantiate(Node);
			Button btn = node.GetComponent<Button> ();
			int n = i;
			btn.onClick.AddListener (() => AleartNotify(n));
			btn.transform.GetComponentInChildren<Text> ().text = _choices [i];
			node.transform.SetParent(Content.transform);
			node.transform.localScale = Vector3.one;
		}

		//キャンセルボタンを押せなくする
		if (_CantCancel) {
			Destroy (CancelButton);
		}

		//alertアニメーション
		OpenClose(true);
	}
	// Use this for initialization
	void Start () {
		
	}
	public void AleartNotify (int _num) {
		//二連タップ不可
		if (Selected)
			return;
		Selected = true;

		//タップ有効か
		DataManager.Instance.TouchAble();

		//効果音
		if (_num == -1) {
			DataManager.Instance.SEPlay (2);
		} else {
			DataManager.Instance.SEPlay (5);
		}
		//alertアニメーション
		OpenClose (false);

		// デリゲート先のOnRecieveに送る
		ExecuteEvents.Execute<IRecieveMessage>(
			target: Delegate, // 呼び出す対象のオブジェクト
			eventData: null,  // イベントデータ（モジュール等の情報）
			functor: (recieveTarget,y)=>recieveTarget.OnRecieve(_num,Tag)); // 操作


	}
	public void OpenClose (bool _Open) {
		float Duration = 0.3f;
		if (_Open) {
			transform.GetComponent<RectTransform>().localScale = new Vector3(1f,0,1f);
			transform.GetComponent<RectTransform> ().DOScaleY (1f,Duration).SetEase (Ease.OutBounce);
		} else {
			transform.GetComponent<RectTransform> ().DOScaleY (0f,0.1f).SetEase (Ease.Linear);
			Destroy (gameObject,0.15f);
		}
	}
	// Update is called once per frame
	void Update () {
	}
}
