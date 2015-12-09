using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StimTest1Event: MemTest1Event {

	// which quadrant is correct
	protected int cuedLoc;
	public int CuedLoc{
		get{return cuedLoc;}
	}

	public override bool respondedCorrectly(){
		foreach(Response r in responses ) {
			//Debug.Log(quadrant +" == " + r.QuadrantTouched);
			if(targetLoc == r.QuadrantTouched) {
				return true;
			}
		}

		return false;
	}
	
	public StimTest1Event(int c, int t, string[] s) : base(c,t,s) {
		cuedLoc = c;
	}
}