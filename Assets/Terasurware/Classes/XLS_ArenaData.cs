using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class XLS_ArenaData : ScriptableObject
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
		public int rank;
		public int order;
		public string name;
		public int needPoint;
		public int stamina;
		public int[] HP;
		public int deck;
		public int[] ai;
		public int[] drop;
		public int money;
	}
}

