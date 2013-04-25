using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Session manager for stopping games
public class StoppingManager : GameManager {
	
	//The Stopping ball
	private GameObject stoppingStimulus;
	
	private float avgResponseTime = .6f;
	
	private bool secondPracticeStage;
	
	//Positions of the game's stimuli
	private Vector2[] stimPositions;
	public Vector2[] StimPositions{
		get{return stimPositions;}
		set{stimPositions = value;}
	}
	
	//Returns the current Event for Stopping games
	public StoppingEvent CurrentEvent{
		get{
			//If were still practicing, return the latest practice
			if(practice.Count<=currentPractice) 
				return (StoppingEvent)events[currentEventNum]; 
			//Otherwise return the latest real event
			else 
				return (StoppingEvent)practice[currentPractice];
		}
	}
	
	// Use this for initialization
	void Awake () {
		base.Setup(GameManager.SessionType.Stopping);
		
		secondPracticeStage = false;
		
		stoppingStimulus = GameObject.Find("Stimulus");
		
		stoppingStimulus.renderer.material.color = Color.blue;
		
		stimPositions = new Vector2[4]{new Vector2(-15,10), new Vector2(5,5), new Vector2(15,-10), new Vector2(-10,-5)};
		
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
		
		//Generate the original 8
		for(i=0;i<8;i++){
			StoppingEvent e=null;
			
			e = new StoppingEvent((int)((i%4f) + 1), true);
			
			events.Add(e);
		}
			
		//Generate the rest of the events
		for(i=0;i<140;i++){
			
			StoppingEvent e=null;
			
			e = new StoppingEvent((int)((i%4f) + 1),i<56? false:true);
			
			events.Add(e);
		}	
	}
	
	//Method used to randomize the order of the list of events
	protected override void randomizeEvents(){
		
		System.Random rand = new System.Random();
		
		int i = 0;
		
		//Apply a base randomization to initial ones
		for(i=0; i<8;i++){
			StoppingEvent originalStats = (StoppingEvent)events[i];
		
			int spotToMove = rand.Next(i,8);
		
			events[i] = events[spotToMove];
		
			events[spotToMove] = originalStats;	
		}
		
		//Apply base randomization to the rest
		for(i=8; i<events.Count;i++){
			StoppingEvent originalStats = (StoppingEvent)events[i];
			
			int spotToMove = rand.Next(i,events.Count);
					
			events[i] = events[spotToMove];
			
			events[spotToMove] = originalStats;	
		}
		
		bool ok=true;
		bool eventGood;

		i = 0;
		
		int cycleCount =0;
		
		//Loop to make sure there are no strings of 4 similiar dot colors
		do{	
			ok=true;
			
			for(i=8; i <events.Count;i++){

				eventGood = true;
				
				if(((StoppingEvent)events[i]).Dot == ((StoppingEvent)events[i-1]).Dot
					&& ((StoppingEvent)events[i]).Dot == ((StoppingEvent)events[i-2]).Dot
					&& ((StoppingEvent)events[i]).Dot  == ((StoppingEvent)events[i-3]).Dot)
					eventGood=false;
				
				if(((StoppingEvent)events[i]).Go == ((StoppingEvent)events[i-1]).Go
					&& ((StoppingEvent)events[i]).Go == ((StoppingEvent)events[i-2]).Go
					&& ((StoppingEvent)events[i]).Go  == ((StoppingEvent)events[i-3]).Go)
					eventGood=false;
				
				//If there are three in a row, find another element that has a different color and swap the two
				if(!eventGood){
					ok = false;
					
					int start = i+1;
					if(start>=events.Count) start = 8;
					
					for(int j =i+1;j<events.Count;j++){
						eventGood=true;
						if(((StoppingEvent)events[j]).Dot == ((StoppingEvent)events[i]).Dot || ((StoppingEvent)events[j]).Go == ((StoppingEvent)events[i]).Go)
							eventGood=false;
						
						if(eventGood){
							StoppingEvent originalStats = (StoppingEvent)events[i];
				
							events[i] = events[j];
			
							events[j] = originalStats;
						
							break;
						}
						else if(j>=events.Count) j=8;
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
		
		List<EventStats> newPractice = new List<EventStats>(){new StoppingEvent(1,true),new StoppingEvent(4,secondPracticeStage? false:true),new StoppingEvent(3,secondPracticeStage? false:true),new StoppingEvent(2,true)};

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
		stoppingStimulus.transform.position = new Vector3(stimPositions[0].x,-3.5f,stimPositions[0].y);
		
		screen.enabled = false;
		
		yield return new WaitForSeconds(.15f);
		
		yield return StartCoroutine(tFinger.performAction(tutDots,null));
		
		screen.enabled = true;
		
		yield return new WaitForSeconds(.05f);
		
		//Click 2nd dot
		tutDots = new List<Vector3>(){new Vector3(stimPositions[1].x,0,stimPositions[1].y)};
		
		stoppingStimulus.transform.position =new Vector3(stimPositions[1].x,-3.5f,stimPositions[1].y);
		
		screen.enabled = false;
		
		yield return new WaitForSeconds(.15f);
		
		Vector3 halfWay = new Vector3((stimPositions[1].x + stimPositions[0].x)/2,0,(stimPositions[1].y +stimPositions[0].y)/2 );
		
		yield return StartCoroutine(tFinger.moveTo(new List<Vector3>(){halfWay}));
		
		stoppingStimulus.renderer.material.color = new Color(1,.75f,0);
		
		yield return StartCoroutine(tFinger.moveTo(tutDots));
		
		yield return new WaitForSeconds(1f);
		
		screen.enabled = true;
		
		stoppingStimulus.renderer.material.color = Color.blue;
		
		yield return new WaitForSeconds(.05f);
		
		//Click 3rd dot
		tutDots = new List<Vector3>(){new Vector3(stimPositions[2].x,0,stimPositions[2].y)};
		
		stoppingStimulus.transform.position =new Vector3(stimPositions[2].x,-3.5f,stimPositions[2].y);
		
		screen.enabled = false;
		
		yield return new WaitForSeconds(.15f);
		
		yield return StartCoroutine(tFinger.performAction(tutDots,null));
		
		screen.enabled = true;
		
		yield return new WaitForSeconds(.05f);
		
		//Click 4th dot
		tutDots = new List<Vector3>(){new Vector3(stimPositions[3].x,0,stimPositions[3].y)};
		
		stoppingStimulus.transform.position =new Vector3(stimPositions[3].x,-3.5f,stimPositions[3].y);
		
		stoppingStimulus.renderer.material.color = Color.blue;
		
		screen.enabled = false;
		
		yield return new WaitForSeconds(.15f);
		
		halfWay = new Vector3((stimPositions[3].x + stimPositions[2].x)/2,0,(stimPositions[3].y +stimPositions[2].y)/2 );
		
		yield return StartCoroutine(tFinger.moveTo(new List<Vector3>(){halfWay}));
		
		stoppingStimulus.renderer.material.color = new Color(1,.75f,0);
		
		yield return StartCoroutine(tFinger.moveTo(tutDots));
		
		yield return new WaitForSeconds(.6f);
		
		yield return StartCoroutine(tFinger.exit());	
		
		screen.enabled = true;

		yield return new WaitForSeconds(.05f);
	}
	
	//Main method of the game
	protected override IEnumerator runSession(){
		
		//Show the tutorial
		yield return StartCoroutine(runTutorial());
	
		//Show Practice screen
		yield return StartCoroutine(showTitle("Practice",3));
		
		//Main Session
		while(currentEventNum< events.Count){
			
			stoppingStimulus.renderer.material.color = Color.blue;
			
			if(practicing){
				if(CurrentEvent.Dot==1)
					stoppingStimulus.transform.position = new Vector3(-20,-3.5f,10);
				else if(CurrentEvent.Dot==2)
					stoppingStimulus.transform.position = new Vector3(20,-3.5f,10);
				else if(CurrentEvent.Dot==3)
					stoppingStimulus.transform.position = new Vector3(-20,-3.5f,-10);
				else
					stoppingStimulus.transform.position = new Vector3(20,-3.5f,-10);
			}
			else{
				stoppingStimulus.transform.position = new Vector3(stimPositions[CurrentEvent.Dot-1].x, -3.5f,stimPositions[CurrentEvent.Dot-1].y);
			}
			
			screen.enabled = false;
			
			startTime = Time.time;
			
			float currentTime =0;
			
			bool changed = false;
			
			state = GameState.Probe;
			
			if(!CurrentEvent.Go){
				CurrentEvent.TurningTime = (avgResponseTime/2f);
			}
			
			//Wait for either the player's response or the time limit
			while(CurrentEvent.Response == null && currentTime < .7f){
				currentTime+= Time.deltaTime;
				
				if(!CurrentEvent.Go && currentTime> CurrentEvent.TurningTime && !changed)
					stoppingStimulus.renderer.material.color = new Color(1,.75f,0);
					
				yield return new WaitForFixedUpdate();
			}
			
			//ITI, black screen
			state = GameState.ITI;
			screen.enabled = true;
			
			if(currentTime>=.7f) CurrentEvent.TimedOut = true;
			
			if(CurrentEvent.Response == null && !practicing){
				if(!CurrentEvent.Go){
					avgResponseTime+=.025f;
					Mathf.Clamp(avgResponseTime,0,1.4f);
				}
			}
			
			//Calculate the avgResponseTime of the first 8
			if(currentEventNum==7){
				avgResponseTime = 0;
				for(int i = 0;i<8;i++){
					if(((StoppingEvent)events[i]).Response != null)
						avgResponseTime += ((StoppingEvent)events[i]).Response.ResponseTime;
					else avgResponseTime +=.7f;
				}
				
				avgResponseTime = avgResponseTime/8;
			}
			
			yield return new WaitForSeconds(.05f);
			
			//Get the next event
			nextEvent();
			
			//If we reached the end of the practice list, check to see if the player passed
			if(practicing && currentPractice>=practice.Count){
				
				//Count the nummber of correct responses
				int numCorrect=0;
				for(int i = currentPractice-4; i< currentPractice;i++){
					if(practice[i].respondedCorrectly()) numCorrect+=1;
				}
				
				NeuroLog.Log("Total Correct Responded: " + numCorrect);
				
				//If the numCorrect is greater than 5, or played the practice 4 times, continue on 
				if((numCorrect>=4 && secondPracticeStage) || practiceSessionCount>=3){
					NeuroLog.Log("Continuing to MainSession");
					
					border.SetActive(false);
					
					yield return StartCoroutine(showTitle("Test",3));
					
					practicing = false;
				}
				else if(!secondPracticeStage && numCorrect>=3){
					NeuroLog.Log("Moving to second practice stage");
					
					secondPracticeStage = true;
					
					generatePractice();
				}
				//Otherwise retry the practice
				else{
					practiceSessionCount++;
					
					NeuroLog.Log("Redo Practice");
					
					secondPracticeStage = false;
					
					generatePractice();
					
					yield return StartCoroutine(runTutorial());
					
					yield return StartCoroutine(showTitle("Practice",3));
				}
			}
		}
		
		//Writeout 
		xml.WriteOut(true);
		
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
						
						//Make sure it was before the cutoff
						if(time<=.7f){
							float x = ((stoppingStimulus.transform.position.x + 26.5f)/53f) * Screen.width;
							
							float y = ((stoppingStimulus.transform.position.z - 15f)/-30f) * Screen.height;
							
							Vector2 targPos = new Vector2(x,y);
							
							//Create the respones
							Response r = new Response(targPos, time,new Vector2(touchPos.x,touchPos.y));
									
							Vector2 screenPos = Vector2.zero;
							
							screenPos.x = ((touchPos.x/Screen.width) * 53) - 26.5f;
							screenPos.y = ((touchPos.y/Screen.height) * -30) +15;
							
							//Start the fade spot
							
							int good = 1;
							if(!CurrentEvent.Go)
								good = 0;
							
							StartCoroutine(spot.fadeFinger(screenPos, good));
							
							//Add the response
							CurrentEvent.Response =r;
							
							//If were in the real game
							if(currentEventNum>7 && !CurrentEvent.Go){
								avgResponseTime-=.025f;
									
								Mathf.Clamp(avgResponseTime,0,1.4f);
							}	
						}
					}
				}
			}
		}
	
	}
}