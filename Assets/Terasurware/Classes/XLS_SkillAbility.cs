using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class XLS_SkillAbility : ScriptableObject
{	
	public List<Param> param = new List<Param> ();

	[System.SerializableAttribute]
	public class Param
	{
		
		public int id;
		public string Name;
		public string SkillText;
		public int type;
		public int[] Dtype;
		public int attribute;
		public int cost;
		public float point;
		public int speed;
		public int range;
		public int hitRatio;
		public int times;
		public int role;
		public int addState;
		public string effeckFile;
	}
}