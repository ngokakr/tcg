using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class AjustParticle : MonoBehaviour {
	ParticleSystem p;
	public float Duration;
	public bool Loop = false;
	public bool Change;
	// Use this for initialization
	void Start () {
		p = GetComponent<ParticleSystem> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Change & Duration > 0) {
			Change = false;

			p.Stop ();

			//自身
			ChangeParam (p);

			//子
			var ps = transform.GetComponentsInChildren<ParticleSystem> ();
			for (int i = 0; i < ps.Length; i++ ){
				ChangeParam (ps [i]);
			}

			p.Play ();
		}
			
	}

	void ChangeParam (ParticleSystem _ps) {
		
//		float dia = Duration / _ps.main.duration;
		var module = _ps.main;
		module.simulationSpeed = Duration;
		module.loop = Loop;
//		module.duration*= dia;
//
//		var curve = module.startLifetime;
//		if(curve.mode == ParticleSystemCurveMode.Constant)
//			curve.constant *= dia;
//		if (curve.mode == ParticleSystemCurveMode.TwoConstants) {
//			curve.constantMin *= dia;
//			curve.constantMax *= dia;
//		}
//
//		var curveS = module.startSpeed;
//		if(curveS.mode == ParticleSystemCurveMode.Constant)
//			curveS.constant /= dia;
//		if (curveS.mode == ParticleSystemCurveMode.TwoConstants) {
//			curveS.constantMin /= dia;
//			curveS.constantMax /= dia;
//		}
//
//		module.startLifetime = curve;
//		module.startSpeed = curveS;
		module.scalingMode = ParticleSystemScalingMode.Hierarchy;
	}
}
