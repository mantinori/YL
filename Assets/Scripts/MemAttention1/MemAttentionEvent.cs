using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//An Event within a Spatial game
public class MemAttentionEvent: EventStats{
	
	//The number of the dot that will appear during the event. Should contain values within the range of 1-4
	private int quadrant;
	public int Quadrant{
		get{return quadrant;}
	}
	
	//Which block is this dot apart of
	private string stimulus;
	public string Stimulus{
		get{return stimulus;}
		set{stimulus = value;}
	}
	
	//The list of responses that occurred within the probe(correct) period of the trial
	private Response response;
	public Response Response{
		get{return response;}
		set{response = value;}
	}
	
	//Method used to see if the player responded correctly in the trial
	public override bool respondedCorrectly(){
		//If the player got more than half right, they pass.
		if(response != null)
			return true;
		else 
			return false;
	}
	
	//Constructor
	//d(int): dot that will appear in this trial
	public MemAttentionEvent(int d, string s){
		quadrant = d;
		stimulus = s;
		response = null;
	}
}