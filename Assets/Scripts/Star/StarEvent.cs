using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//An Event within a Star Game
public class StarEvent: EventStats{
	
	//How many little stars will there be in this trial
	private int numLittleStars;
	public int NumLittleStars{
		get{return numLittleStars;}
		set{numLittleStars = value;}
	}
	
	//How many big stars will there be in this trial
	private int numBigStars;
	public int NumBigStars{
		get{return numBigStars;}
		set{numBigStars = value;}
	}
	
	//How many dots will there be in this trial
	private int numDots;
	public int NumDots{
		get{return numDots;}
		set{numDots = value;}
	}
	
	//How many triangles will there be in this trial
	private int numTriangles;
	public int NumTriangles{
		get{return numTriangles;}
		set{numTriangles = value;}
	}
	
	//How many unclicked little stars has the player already pressed
	private int numGoodTouches;
	public int NumGoodTouches{
		get{return numGoodTouches;}
		set{numGoodTouches = value;}
	}
	
	//How many nonlittle stars has the player pressed
	private int numBadTouches;
	public int NumBadTouches{
		get{return numBadTouches;}
		set{numBadTouches = value;}
	}
	
	//How many already pressed little stars has the player pressed
	private int repeatTouches;
	public int RepeatTouches{
		get{return repeatTouches;}
		set{repeatTouches = value;}
	}
	
	//A list of the player's responses in this trial
	private List<Response> responses;
	public List<Response> Responses{
		get{return responses;}
	}
	
	//A preset list of what/where certain objects should be set up within the trial
	private List<StarObject> objects;
	public List<StarObject> Objects{
		get{return objects;}
		set{objects = value;}
	}
	
	//How long did this trial last
	private float duration;
	public float Duration{
		get{return duration;}
		set{duration = value;}
	}
	
	//Why did this trial end. Possible choices include "Completed" if the player gets all the stars,
	//"TimedOut", if the player ran out of time, and "Skipped" if the player presses the next arrow.
	private string endCondition;
	public string EndCondition{
		get{return endCondition;}
		set{endCondition = value;}
	}
	
	//Takes a response and adds it to the list of it.
	// 'i'(Response) = A response to be added to the list
	public void AddResponse(Response i){
		//Check to see what type of response it was, and increment the corresponding list
		if(i.ResponseType ==0)numGoodTouches++;
		else if(i.ResponseType ==1)numBadTouches++;
		else repeatTouches++;

		responses.Add(i);
	}
	
	//Calculate the average time it took per correct target
	public float AvgTimePerTarget(){
		//Total average
		float average=0;
		
		//the last time of a correct touch
		float prevTime =0;
		
		//Loop through the list of responses
		for(int i = 0;i<responses.Count;i++){
			//If the response type is '0'(correct)
			if(responses[i].ResponseType ==0){
				
				//Add The difference of the latest time and the previous time to the average total 
				average += (responses[i].ResponseTime - prevTime);
				
				//Update previous time to the current time
				prevTime = responses[i].ResponseTime;
			}
		}
		
		//If there were good touches, then divide the total average by the number of touches
		if(numGoodTouches>0)
			average = average/ numGoodTouches;
		
		//return the average
		return average;
	}
	
	//Calculate the average time it took per any action
	public float AvgTimePerAction(){
		float average=0;
		
		//Loop through the list of responses
		for(int i = 0;i<responses.Count;i++){
			//Add the difference between the response's time and the last response's time to the list
			if(i == 0) average += responses[i].ResponseTime;
			else average += (responses[i].ResponseTime - responses[i-1].ResponseTime);
		}
		
		//If there were actually responses in the trial, divide the total by the number of responses.
		if(responses.Count>0)
			average = average/ responses.Count;
		
		//Return the average
		return average;
	}
	
	//Calculate the Standard Deviation
	public float StD(){
		float average=0;
		int i =0;
		//List of the times of the correct taps
		List<float> times = new List<float>();
		
		//Loop through the list of responses getting the total and individual response time
		for(i = 0;i<responses.Count;i++){
			//If type 0(correct)
			if(responses[i].ResponseType ==0){
				//Add the response time difference to the average total
				if(times.Count == 0) average += responses[i].ResponseTime;
				else average += (responses[i].ResponseTime - times[times.Count-1]);
				
				times.Add(responses[i].ResponseTime);
			}
		}
		
		//Calculate the average
		if(numGoodTouches>0)
			average = average/numGoodTouches;
		
		float newAvg = 0;
		
		//For each time
		for(i =0;i<times.Count;i++){
			//Minus the original average from the time, and raise it to the second power
			times[i] = Mathf.Pow((times[i] - average),2);	
			newAvg +=times[i]; 
		}
		
		//Calculate the new average, and return the square root
		newAvg = newAvg/times.Count;
		
		return Mathf.Sqrt(newAvg);
	}
	
	//Generate a list of ints that indicate how many correct taps were in each section
	public List<int> correctPerArea(){
		//List signifying the different sections
		List<int> numPerRegion = new List<int>(){0,0,0,0,0,0,0,0};
		
		//Loop through the list of responses
		foreach(Response r in responses){
			//If the response is of type 0(correct)
			if(r.ResponseType ==0){
				//Increment the slot of the list that matches the response's section
				numPerRegion[r.DotPressed-1] = numPerRegion[r.DotPressed-1]+1;
			}
		}
		
		//Return the List
		return numPerRegion;
	}
	
	//Calculate the average x distance of the correct responses(from the side of the screen)
	public float AvgDistanceofTargets(){
		float average=0;
		
		//Loop through the list of responses
		for(int i = 0;i<responses.Count;i++){
			//If type is 0(correct)
			if(responses[i].ResponseType ==0){
				//Add touch's x location to the average total
				average += responses[i].TouchLocation.x;
			}
		}
		
		//If there were touches, average it out
		if(numGoodTouches>0)
			average = average/ numGoodTouches;
		
		//Return average
		return average;
	}
	
	//Gets the average response time for the first ten correct responses
	public float AvgTimeStart(){
		float average =0;
		float prevTime =-1;
		
		int count=0;
		
		//Loop through the responses
		foreach(Response r in responses){
			//If response was of type 0(correct)
			if(r.ResponseType ==0){
				//Add the differences of the current time and previous to the average
				if(prevTime == -1) average += r.ResponseTime;
				else average += (r.ResponseTime - prevTime);
				
				prevTime = r.ResponseTime;
				count++;
			}
			//Once we reached the first ten, break out
			if(count>=10) break;
		}
		//Make sure the player at least responded to one, then calculate the average
		if(count>0) average = average/ count;
		else average = -1;
		
		//Return average
		return average;
	}
	
	//Gets the average response time for the last ten correct responses
	public float AvgTimeLast(){
		float average=0;
		int count=0;
		
		List<Response> r = new List<Response>();
		//Got through the list of responses and collect all the correct responses in a new list
		for(int i = 0;i<responses.Count;i++){
			if(responses[i].ResponseType ==0){
				r.Add(responses[i]);
			}
		}
		
		//Loop backwards through the new list, adding up the response times
		for(int i = responses.Count-1;i>-1;i--){
			if(i == 0) average += responses[i].ResponseTime;
			else average += (responses[i].ResponseTime - responses[i-1].ResponseTime);
			
			count++;
			if(count>=10) break;
		}
		
		//Return average
		average = average/ count;
		
		return average;
	}
	
	//Calculate the average distance in between correct taps
	public float AvgDistance(){
		float average=0;
		
		Vector2 prevTouch = new Vector2(-1,-1);
		
		//Loops through the list of responses
		for(int i = 0;i<responses.Count;i++){
			//If the type is 0(correct)
			if(responses[i].ResponseType ==0){
				//Add the difference between the two touch locations to the average
				if(prevTouch!=new Vector2(-1,-1)) average += Vector2.Distance(prevTouch, responses[i].TouchLocation);
				
				prevTouch = responses[i].TouchLocation;
			}
		}
		
		//If there were good touches, average them out
		if(numGoodTouches>0)
			average = average/numGoodTouches;
		
		//Return the average
		return average;	
	}
	
	//Used for practices. Checks to see if the player correctly tapped at least 4 little stars and the percent of correct taps is over 75%
	public override bool respondedCorrectly(){
		//Calculate the percentage of good taps within the list of responses
		float percentage= (float)numGoodTouches/(float)(numGoodTouches+ numBadTouches +repeatTouches);
		//If its greater than .75 and there were at least 4 good ones, the player passes
		if(numGoodTouches>4 &&  percentage>.75f)
			return true;
		else 
			return false;
	}
	
	//Constructor, sets all variables to base values
	public StarEvent(){
		responses = new List<Response>();
		objects = new List<StarObject>();
		numBigStars= 31;
		numLittleStars = 34;
		numTriangles = 7;
		numDots = 6;
		numGoodTouches=0;
		numBadTouches = 0;
		repeatTouches = 0;
		duration = 0;
		endCondition = "";
	}
}