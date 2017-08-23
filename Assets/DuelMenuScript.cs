using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelMenuScript : MonoBehaviour,IRecieveMessage {
	public BattleScript battleScript;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	/// <summary>
	/// 0=メイン 1=サブ 2=ロビー 3=CPU対戦
	/// </summary>
	/// <param name="_num">Number.</param>
	public void MainNotifi (int _num) {
		switch (_num) {
		case 0://メインアリーナ
		case 1://さぶアリーナ
			{
				AlertView.Make (_num,"オンライン対戦","対戦相手を探します。",new string[]{"OK","Cancel"}, gameObject,1);

			}
			break;

		case 3:
			{
//				SceneManager.Instance.ToBattle (1);
			}
			break;
		}
	}
	public void OnRecieve(int _num,int _tag){
		if (_num == 0 && _tag != -1) {
			AlertView.Make (-1,"オンライン対戦","対戦相手を探しています...",new string[]{}, gameObject,1);
			OnlineManager.Instance.Matching ();
//			SceneManager.Instance.ToBattle (_tag);
		}
	}
}
