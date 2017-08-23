using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CardParam = SystemScript.CardParam;

public class NewCardDetailScript : MonoBehaviour {
	public Text NameAndGroup;
	public Text Param;
	public Text SkillText;
	public CardImageScript CardImg;

	public void Show (CardParam cp) {
		NameAndGroup.text = cp.Name;
		if (cp.Groups.Count > 0) {
			NameAndGroup.text += "\n<size=25>" + SystemScript.GetGroup (cp.Groups) + "</size>";
		}
		Param.text = System.String.Format 
			("{0}\n役割:{1}\nコスト:{2}\nパワー:{3}"
				, new string[] {SystemScript.GetReality(cp.Rea,true),SystemScript.GetRole(cp.Role),cp.Cost.ToString(),cp.Power.ToString()});
		
		SkillText.text = SystemScript.GetSkillsText(cp,SystemScript.ShowType.DETAIL,true);
		CardImg.Set (cp);
	}
}
