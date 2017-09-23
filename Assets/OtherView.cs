using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherView : MonoBehaviour,IRecieveMessage {
	public string[] Copyright;
	public string[] CopyrightURL;
	public void ButtonNotify (int num) {
		switch (num) {
		case 0://ヴァージョン
			{
				AlertView.Make (0,"バージョン確認","v"+DataManager.Instance.AppVersion,new string[]{"OK"}, gameObject,1);
			}
			break;
//		case 1://プレイヤー情報
//			{
//				
//			}
//			break;
//		case 2://公式サイト
//			{
//			}
//			break;
		case 3://著作権表記
			{
				AlertView.Make (3,"著作権表記","タップで外部サイトへ",Copyright, gameObject,1);
			}
			break;
//		case 4://利用規約
//			{
//				
//			}
//			break;
//		case 5://ヘルプ
//			{
//				AlertView.Make (0,"ヘルプ","現在作成中のコンテンツです。",new string[]{"OK"}, gameObject,1);
//			}
//			break;
		case 6://お問い合わせ
			{
				AlertView.Make (0,"お問い合わせ","ngokakr@gmail.com",new string[]{"OK"}, gameObject,1);
			}
			break;
//		case 7://データ引き継ぎ
//			{
//				AlertView.Make (0,"データ引き継ぎ","現在作成中",new string[]{"OK"}, gameObject,1);
//			}
//			break;
		default:
			{
				AlertView.Make (0,"製作中","現在作成中のコンテンツです。",new string[]{"OK"}, gameObject,1);
			}

			break;

		}
	}
	public void OnRecieve(int _num,int _tag){
		if (_num == -1)
			return;
		switch (_tag) {
		case 3:
			{
				SystemScript.OpenURL (CopyrightURL [_num]);
			}
			break;
		}
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
