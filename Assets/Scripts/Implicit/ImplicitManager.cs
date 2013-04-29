using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Session manager for implicit games
public class ImplicitManager : GameManager {
	
	//The Implicit ball
	private GameObject implicitStimulus;
	
	//Positions of the game's stimuli
	private Vector2[] stimPositions;
	public Vector2[] StimPositions{
		get{return stimPositions;}
		set{stimPositions = value;}
	}
	
	//Returns the current Event for Implicit games
	public ImplicitEvent CurrentEvent{
		get{
			//If were still practicing, return the latest practice
			if(practice.Count<=currentPractice) 
				return (ImplicitEvent)events[currentEventNum]; 
			//Otherwise return the latest real event
			else 
				return (ImplicitEvent)practice[currentPractice];
		}
	}
	
	// Use this for initialization
	void Awake () {
		base.Setup(GameManager.SessionType.Implicit);
		
		implicitStimulus = GameObject.Find("Stimulus");
		
		implicitStimulus.renderer.material.color = Color.blue;
		
		stimPositions = new Vector2[4]{new Vector2(-15,10), new Vector2(5,5), new Vector2(15,-10), new Vector2(-10,-5)};
		
		//Preform the read in to get the events
		events =  csv.ReadInSession();
		
		//Generate the practice events
		generatePractice();
		
		//If the read in failed, generate the base events
		if(events ==null){
			
			events = new List<EventStats>();
			
			NeuroLog.Log("Generating Random list of events");
		
			generateEvents();
			
			randomizeEvents();
		}
		
		//Start the game
		StartCoroutine("runSession");
	}
	//Method used to generate the base events
	protected override void generateEvents(){
		
		int i=0;
		
		//Generate events
		for(i=0;i<210;i++){
			
			ImplicitEvent e=null;
			
			e = new ImplicitEvent((int)((i%4f) + 1),(int)(Mathf.FloorToInt(i/35f) + 1));
			
			events.Add(e);
		}	
	}
	
	//Method used to randomize the order of the list of events
	protected override void randomizeEvents(){
		
		System.Random rand = new System.Random();
		
		int s = 0;
		
		int i = 0;
		
		//Apply a base randomization
		while(i<events.Count){
			
			for(i=s; i<s+35;i++){
				if(i>=events.Count) break;
				
				ImplicitEvent originalStats = (ImplicitEvent)events[i];
			
				int spotToMove = rand.Next(i,s+35);
			
				//If they share the same block number
				if(((ImplicitEvent)events[i]).BlockNum == ((ImplicitEvent)events[spotToMove]).BlockNum){
				
					events[i] = events[spotToMove];
			
					events[spotToMove] = originalStats;
				}	
			}
			s+=35;
		}
		
		bool ok=true;
		bool eventGood;
			
		s = 0;
		i = 0;
		
		//Loop to make sure there are no strings of 4 similiar dot colors
		while(i<events.Count){
			
			int cycleCount = 0;
			
			do{	
				ok=true;
				
				for(i=s+3; i <s+35;i++){
	
					eventGood = true;
					
					if(((ImplicitEvent)events[i]).Dot == ((ImplicitEvent)events[i-1]).Dot
						&& ((ImplicitEvent)events[i]).Dot == ((ImplicitEvent)events[i-2]).Dot
						&& ((ImplicitEvent)events[i]).Dot  == ((ImplicitEvent)events[i-3]).Dot)
						eventGood=false;
					
					//If there are three in a row, find another element that has a different color and swap the two
					if(!eventGood){
						ok = false;
						
						int start = i+1;
						if(start>=s+35) start = s;
						
						for(int j =i+1;j<s+35;j++){
							eventGood=true;
							if(((ImplicitEvent)events[j]).Dot == ((ImplicitEvent)events[i]).Dot ||((ImplicitEvent)events[i]).BlockNum != ((ImplicitEvent)events[j]).BlockNum)
								eventGood=false;
							
							if(eventGood){
								
								ImplicitEvent originalStats = (ImplicitEvent)events[i];
					
								events[i] = events[j];
				
								events[j] = originalStats;
							
								break;
							}
							else if(j>=s+35) j=0;
						}
					}
				}
				cycleCount++;
				//Attempt 5 loops to fix any bad instances, after 5th proceed anyway
				if(cycleCount>5)ok=true;
			}while(!ok);
			
			s+=35;
		}
	}

	//Generate practice pitches
	protected override  void generatePractice(){
	
		border.renderer.enabled = true;
		
		List<EventStats> newPractice = new List<EventStats>(){new ImplicitEvent(1,0),new ImplicitEvent(2,0),new ImplicitEvent(3,0),new ImplicitEvent(4,0)};

		practice.AddRange(newPractice);
	}
	
	//Method used to perform the tutorial
	protected override  IEnumerator runTutorial(){
		
		//Show title card
		yield return StartCoroutine(showTitle("Tutorial",3));
		
		state = GameState.Tutorial;
		
		border.transform.localPosition = new Vector3(0,0,0.5f);
		List<Vector3> tutDots = new List<Vector3>(){new Vector3(stimPositions[0].x,0,stimPositions[0].y)};
		
		//Click 1st dot
		implicitStimulus.transform.position = new Vector3(stimPositions[0].x,-3.5f,stimPositions[0].y);
		
		//screen.enabled = false;
		implicitStimulus.renderer.enabled = true;
		
		yield return new WaitForSeconds(.15f);
		
		yield return StartCoroutine(tFinger.performAction(tutDots,null));
		
		yield return StartCoroutine(tFinger.exit());	
		
		//screen.enabled = true;
		implicitStimulus.renderer.enabled = false;
		
		yield return new WaitForSeconds(.1f);
		
		//Click 2nd dot
		tutDots = new List<Vector3>(){new Vector3(stimPositions[1].x,0,stimPositions[1].y)};
		
		implicitStimulus.transform.position =new Vector3(stimPositions[1].x,-3.5f,stimPositions[1].y);
		
		//screen.enabled = false;
		implicitStimulus.renderer.enabled = true;
		
		yield return new WaitForSeconds(.15f);
		
		yield return StartCoroutine(tFinger.performAction(tutDots,null));
		
		yield return StartCoroutine(tFinger.exit());	
		
		//screen.enabled = true;
		implicitStimulus.renderer.enabled = false;
		
		yield return new WaitForSeconds(.1f);
		
		//Click 3rd dot
		tutDots = new List<Vector3>(){new Vector3(stimPositions[2].x,0,stimPositions[2].y)};
		
		implicitStimulus.transform.position =new Vector3(stimPositions[2].x,-3.5f,stimPositions[2].y);
		
		//screen.enabled = false;
		implicitStimulus.renderer.enabled = true;
		
		yield return new WaitForSeconds(.15f);
		
		yield return StartCoroutine(tFinger.performAction(tutDots,null));
		
		yield return StartCoroutine(tFinger.exit());	
		
		//screen.enabled = true;
		implicitStimulus.renderer.enabled = false;
		
		yield return new WaitForSeconds(.1f);
		
		//Click 4th dot
		tutDots = new List<Vector3>(){new Vector3(stimPositions[3].x,0,stimPositions[3].y)};
		
		implicitStimulus.transform.position =new Vector3(stimPositions[3].x,-3.5f,stimPositions[3].y);
		
		//screen.enabled = false;
		implicitStimulus.renderer.enabled = true;
		
		yield return new WaitForSeconds(.15f);
		
		yield return StartCoroutine(tFinger.performAction(tutDots,null));
		
		yield return StartCoroutine(tFinger.exit());	
		
		//screen.enabled = true;
		implicitStimulus.renderer.enabled = false;
		
		yield return new WaitForSeconds(.1f);
	}
	
	//Main method of the game
	protected override IEnumerator runSession(){
		
		//Show the tutorial
		yield return StartCoroutine(runTutorial());
	
		//Show Practice screen
		yield return StartCoroutine(showTitle("Practice",3));
		
		screen.enabled = false;
		
		//Main Session
		while(currentEventNum< events.Count){
			
			if(practicing){
				if(CurrentEvent.Dot==1)
					implicitStimulus.transform.position = new Vector3(-20,-3.5f,10);
				else if(CurrentEvent.Dot==2)
					implicitStimulus.transform.position = new Vector3(-20,-3.5f,-10);
				else if(CurrentEvent.Dot==3)
					implicitStimulus.transform.position = new Vector3(20,-3.5f,10);
				else
					implicitStimulus.transform.position = new Vector3(20,-3.5f,-10);
			}
			else{
				implicitStimulus.transform.position = new Vector3(stimPositions[CurrentEvent.Dot-1].x, -3.5f,stimPositions[CurrentEvent.Dot-1].y);
			}
			
			startTime = Time.time;
			
			implicitStimulus.renderer.enabled = true;
			
			float currentTime =0;
			
			state = GameState.Probe;
			//Wait for either the player's response or the time limit
			while(CurrentEvent.Response == null && currentTime < 1f){
				currentTime+= Time.deltaTime;
				yield return new WaitForFixedUpdate();
			}
			
			if(currentTime>=1f) CurrentEvent.TimedOut = true;
			
			if(CurrentEvent.Response != null) yield return new WaitForSeconds(.1f);
			
			//ITI, black screen
			state = GameState.ITI;
			//screen.enabled = true;
			
			implicitStimulus.renderer.enabled = false;
			yield return new WaitForSeconds(.1f);
			
			//Get the next event
			nextEvent();
			
			//If we reached the end of the practice list, check to see if the player passed
			if(practicing && currentPractice>=practice.Count){
				
				//Count the nummber of correct responses
				practiceSessionCount++;
				int numCorrect=0;
				for(int i = currentPractice-4; i< currentPractice;i++){
					if(practice[i].respondedCorrectly()) numCorrect+=1;
				}
				
				NeuroLog.Log("Total Correct Responded: " + numCorrect);
				
				//If the numCorrect is greater than 5, or played the practice 4 times, continue on 
				if(numCorrect>=3 || practiceSessionCount>=4){
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
		csv.WriteOut(true);
		
		//SessionTitle screen
		yield return StartCoroutine(showTitle("Session Over",3));
		
		Debug.Log("GAME OVER, Returning to menu");
		
		//Return to menu
		Application.LoadLevel("menu");
	}
	
	// Constantly check for player input
	void Update () {	
			
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
			
			touching = true;
			
			if(State == GameManager.GameState.Probe){
				Ray ray = camera.ScreenPointToRay(touchPos);
				RaycastHit hit = new RaycastHit();
				//If the raycast of the touch hit something
				if(Physics.Raycast(ray, out hit)) {
					//Hit the Stimulus
					if(hit.collider.name == "Stimulus"){
				
						//Reverts the y orientation
						touchPos.y = Screen.height - touchPos.y;
						
						//Calculate the response time
						float time = Time.time - startTime;
						
						if(time<=1f){
							
							float x = ((implicitStimulus.transform.position.x + 26.5f)/53f) * Screen.width;
							
							float y = ((implicitStimulus.transform.position.z - 15f)/-30f) * Screen.height;
							
							Vector2 targPos = new Vector2(x,y);
							
							//Create the respones
							Response r = new Response(targPos, time,new Vector2(touchPos.x,touchPos.y));
									
							Vector2 screenPos = Vector2.zero;
							
							screenPos.x = ((touchPos.x/Screen.width) * 53) - 26.5f;
							screenPos.y = ((touchPos.y/Screen.height) * -30) +15;
							
							//Start the fade spot
							StartCoroutine(spot.fadeFinger(screenPos, -1));
							
							//Add the response
							CurrentEvent.Response =r;
						}
					}
				}
			}
		}
	}
}