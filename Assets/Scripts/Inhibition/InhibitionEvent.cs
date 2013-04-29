using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//A Event within a Inhibition Game
public class InhibitionEvent: EventStats{
	
	//The side this event appears on. It should either be 'l' or 'r'
	private char side;
	public char Side{
		get{return side;}
	}
	
	//What color should the dot be. It should either be 'yellow'(press same side) or 'purple'(press opposite)
	private string dotColor;
	public string DotColor{
		get{return dotColor;}
		set{dotColor = value;}
	}
	
	//The player's response to this event
	private Response response;
	public Response Response{
		get{return response;}
		set{response = value;}
	}
	
	//Method used to see if the player responded correctly to this event
	public override bool respondedCorrectly(){
		bool shouldaWentLeft =false;
		
		if(side=='l') shouldaWentLeft=true;
		
		if(dotColor =="purple") shouldaWentLeft = !shouldaWentLeft;
		
		//If the player didn't respond, auto fail
		if(response == null) return false;
		
		//If the player press on the left side(-1) and he should have went left OR the player pressed right(1) and he should have went right, then the player passed
		if((response.DotPressed == -1 && shouldaWentLeft) || (response.DotPressed ==1 && !shouldaWentLeft))
			return true;
		//Otherwise, the player failed
		else
			return false;
	}
	
	//Constructor
	//s(char): The side the event should appear on
	//c(string): The color of the event
	public InhibitionEvent(char s, string c){
		side= s;
		dotColor = c;
		response =null;
	}
}