using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemScriptX : MonoBehaviour {
	
	static public void HumenLoad (int _num) {
		var textAsset = Resources.Load (""+ _num) as TextAsset;
		string text = textAsset.text;
		string[] Words = text.Split ('\n');
		for (int i = 0; i < Words.Length; i++ ){
//			Debug.Log (Words [i]);
			string word = Words [i];

			if(word .Contains ("TITLE")){
				word = word.Replace ("TITLE:", "");
				Debug.Log (word);
			}
			if(word .Contains ("BPM")){
				word = word.Replace ("BPM:", "");
				Debug.Log (word);
			}
		}

	}
}
