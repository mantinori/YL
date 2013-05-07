using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//An Event within a Spatial game
public class StoppingEvent: EventStats{
	
	//The number of the dot that will appear during the event. Should contain values within the range of 1-4
	private int dot;
	public int Dot{
		get{return dot;}
	}
	
	//Whether the ball will turn orange(false) or remain blue(true)
	private bool go;
	public bool Go{
		get{return go;}
	}
	
	//When did the object turn colors, 0 if go is true
	/* XXX- NOT NEEDED SINCE GAME UPDATE 5/7/13
	private float turningTime;
	public float TurningTime{
		get{return turningTime;}
		set{turningTime = value;}
	}
	*/
	
	//The list of responses that occurred within the probe(correct) period of the trial
	private Response response;
	public Response Response{
		get{return response;}
		set{response = value;}
	}
	
	//Method used to see if the player responded correctly in the trial
	public override bool respondedCorrectly(){
		//If the player got more than half right, they pass.
		if(response != null){
			if(go)
				return true;
			else
				return false;
		}
		else{ 
			if(go)
				return false;
			else
				return true;
		}
	}
	
	//Constructor
	//d(int): dot that will appear in this trial
	public StoppingEvent(int d, bool g){
		dot = d;
		go = g;
		//turningTime = 0;
		response = null;
	}
}