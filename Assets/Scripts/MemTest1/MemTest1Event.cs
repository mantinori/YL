using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//An Event within a Spatial game
public class MemTest1Event: EventStats {

	// which quadrant is target stimuli in
	protected int targetLoc;
	public int TargetLoc{
		get{return targetLoc;}
	}

	// which quadrant is correct
	protected int validLoc;
	public int ValidLoc{
		get{return validLoc;}
	}
	
	// stimulus in location1
	protected string[] stimuli;
	public string[] Stimuli{
		get{return stimuli;}
		set{stimuli = value;}
	}
	
	//The list of responses that occurred during the event
	protected List<Response> responses;
	public List<Response> Responses{
		get{return responses;}
		set{responses = value;}
	}

	
	//Method used to see if the player responded correctly in the trial
	public override bool respondedCorrectly(){
		foreach(Response r in responses ) {
			//Debug.Log(quadrant +" == " + r.QuadrantTouched);
			if(validLoc == r.QuadrantTouched) {
				return true;
			}
		}

		return false;
	}
	
	//Constructor
	//d(int): dot that will appear in this trial
	public MemTest1Event(int v, int t, string[] s){
		validLoc = v;
		targetLoc = t;
		stimuli = s;
		responses = new List<Response>();
	}
}