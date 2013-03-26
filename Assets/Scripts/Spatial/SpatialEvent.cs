using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//An Event within a Spatial game
public class SpatialEvent: EventStats{
	
	//The List of Dots that will appear during the event. Should contain values within the range of 1-9, with no repeats.
	private List<int> dots;
	public List<int> Dots{
		get{return dots;}
	}
	
	//How long should this event's delay period last. Should either be .1 or 3 seconds
	private float delay;
	public float Delay{
		get{return delay;}
		set{delay = value;}
	}
	
	//The list of responses that occurred within the probe(correct) period of the trial
	private List<Response> responses;
	public List<Response> Responses{
		get{return responses;}
	}
	
	//The list of responses that occurred within the delay(too early) period of the trial
	private List<Response> badresponses;
	public List<Response> BadResponses{
		get{return badresponses;}
	}
	
	//Adds a respones to either list of responses based on if it was a good hit
	//i(Response): The response to be added
	//goodHit(bool):Did this response occur correctly in the probe period(true) or in the delay period(false)
	public void AddResponse(Response i, bool goodHit){
		//If it was a good response
		if(goodHit){
			//Make sure there are less good responses than dots in the trial
			if(responses.Count<dots.Count){
				bool newDot=true;
			
				//Loop through the current list of responses to make sure the cyrrent response won't be a repeat.
				foreach(Response r in responses){
					if(r.DotPressed == i.DotPressed){
						newDot=false;
						break;
					}
				}
			
				//If it is a new section, add it to the list
				if(newDot) responses.Add(i);	
			}
		}
		//Otherwise, add it to the bad list
		else{
			badresponses.Add(i);
		}
	}
	//Method used to see if the player responded correctly in the trial
	public override bool respondedCorrectly(){
		float percentage=0;
		
		//Loop through the good responses and see how many match up correctly to one of the dots
		foreach(Response r in responses){
			if(dots.Contains( r.DotPressed)){
				percentage += (1f/dots.Count);
			}
		}
		
		//If the player got more than half right, they pass.
		if(percentage>.5f)
			return true;
		else 
			return false;
	}
	
	//Constructor
	//d(List<int>): list of dots that will appear in this trial
	public SpatialEvent(List<int> d){
		dots = d;
		dots.Sort();
		delay = .1f;
		responses = new List<Response>();
		badresponses = new List<Response>();
	}
}