using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Reciever : MonoBehaviour , RecieveInterface {

	//privateはダメ
	public void OnRecieve (int _x){
		Debug.Log("受け取った！"+ _x);
	}
}
public interface RecieveInterface : IEventSystemHandler {
	void OnRecieve (int _x);
}




