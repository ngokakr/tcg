using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardParam = SystemScript.CardParam;

public class Tutorial_BattleScript : BattleScript {
	[System.Serializable]
	public struct T_DataSet {
		public int[] LPs;
		public int[] SPs;
		public List<CardParam> PlayerDeck;
		public List<CardParam> EnemyDeck;
	}
	public List<T_DataSet> t_DataSet;
	int TutorialNum = 0;//チュートリアル進度
	void StartTutorial () {
		var dat = t_DataSet [TutorialNum];
		BattleStartOffline (dat.LPs, dat.SPs, dat.PlayerDeck, dat.EnemyDeck);
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
