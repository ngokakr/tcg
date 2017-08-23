using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class XLS_Skills : ScriptableObject
{	
	public List<Param> param = new List<Param> ();

	[System.SerializableAttribute]
	public class Param
	{
		
		public int id;
		public int Target;
		public int Command;
		public float Result0;
		public float Result1;
		public string SkillText;
	}
}