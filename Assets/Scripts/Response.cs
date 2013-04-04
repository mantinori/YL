using UnityEngine;
using System.Collections;

//Contains info on a player's response to a trial
public class Response{
	
	//Which section did the player hit(Range changes for each game (Spatial: 1-9, Inhibition:(left =-1, right=1), Star: 1-8)
	private int dotPressed;
	public int DotPressed{
		get{return dotPressed;}
	}
	
	//How long did it take the player to respond
	private float responseTime;
	public float ResponseTime{
		get{return responseTime;}
	}
	
	//Where on the screen did the player touch
	private Vector2 touchLocation;
	public Vector2 TouchLocation{
		get{ return touchLocation;}
	}
	
	//Distance from dot's center
	private float distanceFromCenter;
	public float DistanceFromCenter{
		get{return distanceFromCenter;}
	}
	
	//What kind of touch was it? 0==good,1==bad, 2==repeat
	private int responseType;
	public int ResponseType{
		get{return responseType;}
	}
	
	//Constructor
	//sT(SessionType): What Gametype is the current game
	//rTime(float): The Response Time of the action
	//tL(Vector2): Where did the player touch on the screen
	//rType(int): What kind of response was it(Good/Bad/Repeat). (For Star)
	public Response(GameManager.SessionType sT, float rTime, Vector2 tL, int rType){
		responseTime = rTime;
		touchLocation = tL;
		responseType = rType;
		
		int pos=1;
		Vector2 center = Vector2.zero;
		
		//Calculates which section did the player press
		//Spatial 
		if(sT == GameManager.SessionType.Spatial){
			if(tL.x>Screen.width*(2f/3f)){
				pos+=2;
				
				center.x = Screen.width*(5f/6f);
			}else if(tL.x>Screen.width/3f){
				pos+=1;
				
				center.x = Screen.width*(3f/6f);
			}else
				center.x = Screen.width/6f;
			
			if(tL.y>Screen.height*(2f/3f)){
				pos+=6;
			
				center.y = Screen.height*(5f/6f);
			}
			else if(tL.y>Screen.height/3f){
				pos+=3;
				
				center.y = Screen.height*(3f/6f);
			}
			else
				center.y = Screen.height/6f;
		}
		//Inhibition
		else if(sT == GameManager.SessionType.Inhibition){
			center.y =Screen.height/2;
			
			if(tL.x< Screen.width/2){
				pos = -1;
				center.x = Screen.width*(.25f);
			}
			else{
				pos = 1;
				center.x = Screen.width*(.75f);
			}
		}
		//Star
		else if(sT == GameManager.SessionType.Star){
		
			if(tL.x>Screen.width*3f/4f)	pos+=6;
			else if(tL.x>Screen.width/2f) pos+=4;
			else if(tL.x>Screen.width/4f) pos+=2;
			
			if(tL.y >Screen.height/2f) pos++;
		}
		
		dotPressed = pos;
		
		distanceFromCenter = Vector2.Distance(tL,center);
	}
	
	//2nd Constructor for implicit
	//objPos(Vector2): the position of the gameobject
	//rTime(float): The Response Time of the action
	//tL(Vector2): Where did the player touch on the screen
	public Response(Vector2 objPos, float rTime, Vector2 tL){
		responseTime = rTime;
		touchLocation = tL;
		
		int pos = 1;
		if(objPos.x>Screen.width/2 && objPos.y<Screen.height/2) pos=2;
		else  if(objPos.x>Screen.width/2 && objPos.y>Screen.height/2) pos=3;
		else  if(objPos.x<Screen.width/2 && objPos.y>Screen.height/2) pos=4;
		
		dotPressed = pos;
		
		distanceFromCenter = Vector2.Distance(objPos,tL);
	}
}