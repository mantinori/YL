using UnityEngine;
using System.Collections;

public class EventStats{
	
	//Did the player respond in time
	private bool timedOut =false;
	public bool TimedOut{
		get{return timedOut;}	
		set{timedOut = value;}
	}
	
	//Did the player respond in time
	private bool completed =false;
	public bool Completed{
		get{return completed;}	
		set{completed = value;}
	}
	
	//Base method for determining if te player correctly responded to a practice trial
	public virtual bool respondedCorrectly(){return true;}
}