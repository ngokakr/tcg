using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class XLS_EnemyCard : ScriptableObject
{	
	public List<Sheet> sheets = new List<Sheet> ();

	[System.SerializableAttribute]
	public class Sheet
	{
		public string name = string.Empty;
		public List<Param> list = new List<Param>();
	}

	[System.SerializableAttribute]
	public class Param
	{
		
		public int id;
		public int CardAtr;
		public int CardId;
		public int Lv;
		public int Time;
		public int[] adds;
	}
}

