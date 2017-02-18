using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	public static class Effect {
		public static void Fade (MonoBehaviour i_behaviour,CanvasGroup _old,CanvasGroup _new,float _time) {
//			_canvas.alpha
			i_behaviour.StartCoroutine(IFade(_old,_new,_time));
		}
		private static IEnumerator IFade(CanvasGroup _old,CanvasGroup _new,float _FadeTime)
		{

			float time = _FadeTime;

			//フェードout
			while (time > 0) {
				time -= Time.deltaTime;
				_old.alpha = time / _FadeTime;
				yield return null;
			}

			time = 0;

			//フェードin
			while (time < _FadeTime) {
				time += Time.deltaTime;
				_new.alpha = time / _FadeTime;
				yield return null;
			}


			yield return null;
		}
	}
	// Update is called once per frame
	void Update () {
		
	}
}
