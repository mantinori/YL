using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//An Event within a Spatial game
public class StimEncodingEvent: EventStats {
	
	private string stimulus;
	public string Stimulus{
		get{return stimulus;}
		set{stimulus = value;}
	}
	
	//The list of responses that occurred during the event
	private List<Response> responses;
	public List<Response> Responses{
		get{return responses;}
		set{responses = value;}
	}
	
	//Constructor
	//d(int): dot that will appear in this trial
	public StimEncodingEvent(string s){
		stimulus = s;
		responses = new List<Response>();
	}
}