using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class XLS_CardParam : ScriptableObject
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
		public int attribute;
		public string role;
		public int reality;
		public int[] group;
		public string name;
		public int cost;
		public int power;
		public string[] skill;
		public string[] script;
		public int[] effect;
		public int[] value;
	}
}

