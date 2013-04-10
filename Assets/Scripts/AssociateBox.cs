using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AssociateBox : MonoBehaviour {
	
	List<Transform> lines = new List<Transform>();
	
	// Use this for initialization
	void Start () {
		for(int i =0; i<transform.childCount;i++){
			lines.Add(transform.GetChild(i));
		}
		
		Reset();
	}
	
	public void Reset(){
		foreach(Transform gO in lines){
			gO.localScale = new Vector3(0,1,0);
		}
		
		transform.position = new Vector3(0,-4.5f,7.5f);
	}
	
	public void updateLines(float percentage){
		foreach(Transform gO in lines){
			if(gO.localRotation.y > 0)
				gO.localScale = new Vector3(percentage *.8f,1,.025f);
			else
				gO.localScale = new Vector3(percentage *2f,1,.025f);
		}
	}
}
