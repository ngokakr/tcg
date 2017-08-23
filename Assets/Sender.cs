using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class Sender : MonoBehaviour {

	// Use this for initialization
	void Start () {
		ExecuteEvents.Execute<RecieveInterface>(
			target: gameObject, 
			eventData: null, 
			functor: (reciever, eventData) => reciever.OnRecieve(2)
		); 
	}

}
