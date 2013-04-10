using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//An Event within a Spatial game
public class AssociateEvent: EventStats{
	
	//The number value of the image that the player will have to match
	private int targetImage;
	public int TargetImage{
		get{return targetImage;}
	}
	
	//List of number representing the images that will appear in the trial
	private List<int> stimuli;
	public List<int> Stimuli{
		get{return stimuli;}
	}
	
	//The list of responses that occurred during the event
	private List<Response> responses;
	public List<Response> Responses{
		get{return responses;}
		set{responses = value;}
	}

	//Method used to see if the player responded correctly in the trial
	public override bool respondedCorrectly(){
		if(responses.Count==1) return true;
		else return false;
	}
	
	//Constructor
	//d(int): the target image of the event
	//images(List<int>: list of numbers representing the images that will appear on the bottom
	public AssociateEvent(int targ, List<int> images){
		targetImage = targ;
		stimuli = images;
		responses = new List<Response>();
	}
}