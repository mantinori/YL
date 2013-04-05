using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Session manager for inhibition games
public class InhibitionManager : GameManager {
	
	//The Inhibit ball
	private GameObject inhibStimulus;
	
	//Returns the current Event for Inhibit games
	public InhibitionEvent CurrentEvent{
		get{
			//If were still practicing, return the latest practice
			if(practice.Count<=currentPractice) 
				return (InhibitionEvent)events[currentEventNum]; 
			//Otherwise return the latest real event
			else 
				return (InhibitionEvent)practice[currentPractice];
		}
	}
	
	// Use this for initialization
	void Awake () {
		base.Setup(GameManager.SessionType.Inhibition);
		
		inhibStimulus = GameObject.Find("Stimulus");
		
		//Preform the read in to get the events
		events =  xml.ReadInSession();
		
		//Generate the practice events
		generatePractice();
		
		//If the read in failed, generate the base events
		if(events ==null){
			
			events = new List<EventStats>();
			
			NeuroLog.Log("Generating Random list of events");
		
			generateEvents();
		}
		
		randomizeEvents();
		
		//Start the game
		StartCoroutine("runSession");
	}
	//Method used to generate the base events
	protected override void generateEvents(){
		
		int i=0;
		
		//Generate yellow dots
		for(i=0;i<30;i++){
			
			InhibitionEvent e=null;
			
			if(i<15) e = new InhibitionEvent('l', "yellow");
			else e = new InhibitionEvent('r', "yellow");
			
			events.Add(e);
		}
		
		//Generate purple dots
		for(i=0;i<30;i++){
			
			InhibitionEvent e=null;
			
			if(i<15) e = new InhibitionEvent('l', "purple");
			else e = new InhibitionEvent('r', "purple");
			
			events.Add(e);
		}	
	}
	
	//Method used to randomize the order of the list of events
	protected override void randomizeEvents(){
		
		System.Random rand = new System.Random();
		
		//Apply a base randomization
		for(int i=0; i<59;i++){
			InhibitionEvent originalStats = (InhibitionEvent)events[i];
			
			int spotToMove = rand.Next(i,60);
			
			events[i] = events[spotToMove];
			
			events[spotToMove] = originalStats;
		}
		
		bool ok=true;
		bool eventGood;
		
		int cycleCount = 0;
		
		//Loop to make sure there are no strings of 4 similiar dot colors
		do{	
			ok=true;
			
			for(int i=3; i <60;i++){

				eventGood = true;
				
				if(((InhibitionEvent)events[i]).DotColor == ((InhibitionEvent)events[i-1]).DotColor
					&& ((InhibitionEvent)events[i]).DotColor == ((InhibitionEvent)events[i-2]).DotColor
					&& ((InhibitionEvent)events[i]).DotColor  == ((InhibitionEvent)events[i-3]).DotColor)
					eventGood=false;
				
				//If there are three in a row, find another element that has a different color and swap the two
				if(!eventGood){
					ok = false;
					
					int start = i+1;
					if(start>=60) start = 0;
					
					for(int j =i+1;j<60;j++){
						eventGood=true;
						if(((InhibitionEvent)events[j]).DotColor == ((InhibitionEvent)events[i]).DotColor)
							eventGood=false;
						
						if(eventGood){
							
							InhibitionEvent originalStats = (InhibitionEvent)events[i];
				
							events[i] = events[j];
			
							events[j] = originalStats;
						
							break;
						}
						else if(j==59) j=0;
					}
				}
			}
			cycleCount++;
			//Attempt 5 loops to fix any bad instances, after 5th proceed anyway
			if(cycleCount>5)ok=true;
		}while(!ok);
	}

	//Generate practice pitches
	protected override  void generatePractice(){
	
		border.renderer.enabled = true;
		
		List<EventStats> newPractice = new List<EventStats>();
		
		InhibitionEvent eS;
		
		//Only 7 events for practice, 2 yellow 5 purple
		for(int j = 0;j<7;j++){
			
			string c= "purple";
			char s='l';
			
			if(j<2)	c = "yellow";
			
			if(j%2==0) s='r';
			
			eS = new InhibitionEvent(s,c);
			
			newPractice.Add(eS);
		}
		
		System.Random rand = new System.Random();
		
		//Just simply randomize the order
		for(int i=0; i<newPractice.Count-1;i++){
			InhibitionEvent originalStats = (InhibitionEvent)newPractice[i];
			
			int spotToMove = rand.Next(i,newPractice.Count);
			
			newPractice[i] = newPractice[spotToMove];
			
			newPractice[spotToMove] = originalStats;
		}
		
		practice.AddRange(newPractice);
	}
	
	//Method used to perform the tutorial
	protected override  IEnumerator runTutorial(){
		
		//Show title card
		yield return StartCoroutine(showTitle("Tutorial",3));
		
		state = GameState.Tutorial;
		
		border.transform.localPosition = new Vector3(0,0,0.5f);
		List<Vector3> tutDots = new List<Vector3>(){new Vector3(-13.25f,-5f,0)};
		
		//Click Yellow Left
		inhibStimulus.renderer.material.color = Color.yellow;
		
		inhibStimulus.transform.position = new Vector3(-13.25f,-6f,0);
		
		screen.enabled = false;
		
		yield return new WaitForSeconds(1f);
		
		yield return StartCoroutine(tFinger.performAction(tutDots,null));
		
		StartCoroutine(tFinger.exit());	
		
		screen.enabled = true;
		
		yield return new WaitForSeconds(.5f);
		
		//Click Yellow Right
		tutDots = new List<Vector3>(){new Vector3(13.25f,-5f,0)};
		
		inhibStimulus.transform.position = new Vector3(13.25f,-6f,0);
		
		screen.enabled = false;
		
		yield return new WaitForSeconds(1f);
		
		yield return StartCoroutine(tFinger.performAction(tutDots,null));
		
		StartCoroutine(tFinger.exit());	
		
		screen.enabled = true;
		
		yield return new WaitForSeconds(.5f);
		
		//Click Purple Left
	 	tutDots = new List<Vector3>(){new Vector3(13.25f,-5f,0)};
		inhibStimulus.transform.position = new Vector3(-13.25f,-6f,0);
		
		inhibStimulus.renderer.material.color = new Color(1f,.431f,.804f,1);
		
		screen.enabled = false;
		
		yield return new WaitForSeconds(1f);
		
		yield return StartCoroutine(tFinger.performAction(tutDots,null));
		
		StartCoroutine(tFinger.exit());	
		
		screen.enabled = true;
		
		yield return new WaitForSeconds(.5f);
		
		//Click Purple Right
		 tutDots = new List<Vector3>(){new Vector3(-13.25f,-5f,0)};
		inhibStimulus.transform.position = new Vector3(13.25f,-6f,0);
		
		screen.enabled = false;
		
		yield return new WaitForSeconds(1f);
		
		yield return StartCoroutine(tFinger.performAction(tutDots,null));
		
		StartCoroutine(tFinger.exit());	
		
		screen.enabled = true;
		
		yield return new WaitForSeconds(.5f);
	}
	
	//Main method of the game
	protected override IEnumerator runSession(){
		
		//Show the tutorial
		yield return StartCoroutine(runTutorial());
	
		//Show Practice screen
		yield return StartCoroutine(showTitle("Practice",3));
		
		//Main Session
		while(currentEventNum< events.Count){
				
			//Probe
			//Set up the ball's color
			if(CurrentEvent.DotColor =="yellow") inhibStimulus.renderer.material.color = Color.yellow;
			else inhibStimulus.renderer.material.color = new Color(1f,.431f,.804f,1);
			
			//Set up the ball's position
			if(CurrentEvent.Side =='l') inhibStimulus.transform.position = new Vector3(-13.25f,-6f,0);
			else inhibStimulus.transform.position = new Vector3(13.25f,-6f,0);
			
			screen.enabled = false;
			
			startTime = Time.time;
			
			float currentTime =0;
			
			state = GameState.Probe;
			//Wait for either the player's response or the time limit
			while(CurrentEvent.PlayerResponse == null && currentTime < 2.5f){
				currentTime+= Time.deltaTime;
				yield return new WaitForFixedUpdate();
			}
			
			if(currentTime>=2.5f) CurrentEvent.TimedOut = true;
			
			if(CurrentEvent.PlayerResponse != null) yield return new WaitForSeconds(.1f);
			
			//ITI, black screen
			state = GameState.ITI;
			screen.enabled = true;
			yield return new WaitForSeconds(.5f);
			
			//Get the next event
			nextEvent();
			
			//If we reached the end of the practice list, check to see if the player passed
			if(practicing && currentPractice>=practice.Count){
				
				//Count the nummber of correct responses
				practiceSessionCount++;
				int numCorrect=0;
				for(int i = currentPractice-7; i< currentPractice;i++){
					if(practice[i].respondedCorrectly()) numCorrect+=1;
				}
				
				NeuroLog.Log("Total Correct Responded: " + numCorrect);
				
				//If the numCorrect is greater than 5, or played the practice 4 times, continue on 
				if(numCorrect>=5 || practiceSessionCount>=4){
					NeuroLog.Log("Continuing to MainSession");
					
					border.SetActive(false);
					
					yield return StartCoroutine(showTitle("Test",3));
					
					practicing = false;
				}
				//Otherwise retry the practice
				else{
					NeuroLog.Log("Redo Practice");
					
					generatePractice();
					
					yield return StartCoroutine(runTutorial());
					
					yield return StartCoroutine(showTitle("Practice",3));
				}
			}
		}
		
		//Writeout 
		xml.WriteOut();
		
		//SessionTitle screen
		yield return StartCoroutine(showTitle("Session Over",3));
		
		Debug.Log("GAME OVER, Returning to menu");
		
		//Return to menu
		Application.LoadLevel("menu");
	}
	
	// Constantly check for player input
	void Update () {	
		
		//If were in the probe mode
		if(state == GameState.Probe){
			
			bool currentTouch;
			
			//Get the touch location based on the platform
			if(Application.platform == RuntimePlatform.IPhonePlayer){
				if(Input.touchCount>0){
					touchPos = Input.touches[0].position;
				
					currentTouch = true;
				}
				else currentTouch =false;		
			}
			else{
				if(Input.GetMouseButton(0)){
					touchPos = Input.mousePosition;
				
					currentTouch = true;
				}
				else currentTouch =false;
			}
			
			//Not Touching
			if(!currentTouch)
				touching = false;
			//If a player has touched the screen, not holding
			else if(!touching && currentTouch){	
				
				//Reverts the y orientation
				touchPos.y = Screen.height - touchPos.y;
				
				touching = true;
				
				//Calculate the response time
				float time = Time.time - startTime;
				
				//Create the respones
				Response r = new Response(sType, time,new Vector2(touchPos.x,touchPos.y), 0);
				
				int d=8;
				if(touchPos.x< Screen.width/2)
					d=2;
						
				Vector2 screenPos = Vector2.zero;
				
				screenPos.x = ((touchPos.x/Screen.width) * 53) - 26.5f;
				screenPos.y = ((touchPos.y/Screen.height) * -30) +15;
				
				//Start the fade spot
				StartCoroutine(spot.fadeFinger(screenPos, d));
				
				//Add the response
				CurrentEvent.PlayerResponse =r;
			}
		}
	}
}