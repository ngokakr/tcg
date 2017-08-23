using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ShadowMove : MonoBehaviour {
	void Start () {
		ShadowPos = GetComponent<Shadow> ().effectDistance;
	}
	Vector3 ShadowPos = new Vector3(10,-10,0);
	bool OnorOff = false;

	public void Move (bool _on,bool _Sound = false){
		if (_Sound) {
			DataManager.Instance.SEPlay (6);
		}
		if (_on) {
			if (!OnorOff) {
				GetComponent<RectTransform> ().localPosition += ShadowPos;
				GetComponent<Shadow> ().effectDistance = Vector2.zero;
				OnorOff = true;
			}
		} else {
			if (OnorOff) {
				GetComponent<RectTransform> ().localPosition -= ShadowPos;
				GetComponent<Shadow> ().effectDistance = ShadowPos;
				OnorOff = false;
			}
		}
	}
}
