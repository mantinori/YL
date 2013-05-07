using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//The main manager of a spatial game
public class SpatialManager : GameManager {
	
	//The spatial dots
	public GameObject[] dots= new GameObject[9];
	
	//Returns the current event
	public SpatialEvent CurrentEvent{
		get{
			//If there are still practices, return the latest
			if(practice.Count<=currentPractice) 
				return (SpatialEvent)events[currentEventNum];
			//Otherwise return the latest main event 
			else 
				return (SpatialEvent)practice[currentPractice];
		}
	}
	
	// Use this for initialization
	void Awake () {
		
		base.Setup(GameManager.SessionType.Spatial);
		
		//Hide all the balls
		foreach(GameObject g in dots){
			g.renderer.enabled = false;
			g.renderer.material.color = Color.blue;
		}
		
		//Read in the events
		events = csv.ReadInSession();
		
		//Generate the practice events
		generatePractice();
		
		//If there was an error reading in the events, generate the basic set
		if(events ==null){
			events = new List<EventStats>();
			
			NeuroLog.Log("Generating Random list of events");
		
			generateEvents();
		}
		
		randomizeEvents();
		
		//Start the game
		StartCoroutine("runSession");
	}
	
	//Generate the basic group of events within a Spatial game
	protected override void generateEvents(){
		
		int i=0;
		
		System.Random rand = new System.Random();
		
		//Generate the 1 dots
		for(i=0;i<20;i++){
			
			List<int> d =new List<int>();
		
			d.Add(rand.Next(1,10));
			
			SpatialEvent e = new SpatialEvent(d);
		
			//First 10 has a delay of .1, the others have 3 seconds
			if(i<10)
				e.Delay = .1f;
			else
				e.Delay = 3f;
			
			events.Add(e);
		}
		
		//Generate 2 dots
		for(i=0;i<20;i++){
			
			List<int> d =new List<int>();
			
			while(d.Count<2){
				int num = rand.Next(1,10);
				if(!d.Contains(num)){
					d.Add(num);
				}
			}
			
			SpatialEvent e = new SpatialEvent(d);
		
			//First 10 has a delay of .1, the others have 3 seconds
			if(i<10)
				e.Delay = .1f;
			else
				e.Delay = 3f;
			
			events.Add(e);
		}
		
		//Generate 3 dots
		for(i=0;i<20;i++){
			
			List<int> d =new List<int>();
			
			while(d.Count<3){
				int num = rand.Next(1,10);
				if(!d.Contains(num)){
					d.Add(num);
				}
			}
			
			SpatialEvent e = new SpatialEvent(d);
		
			//First 10 has a delay of .1, the others have 3 seconds
			if(i<10)
				e.Delay = .1f;
			else
				e.Delay = 3f;
			
			events.Add(e);
		}
	}
	
	//Method used to randomize the order of the list of events
	protected override void randomizeEvents(){
		
		System.Random rand = new System.Random();
		
		//Apply a base randomization
		for(int i=0; i<59;i++){
			SpatialEvent originalStats = (SpatialEvent)events[i];
			
			int spotToMove = rand.Next(i,60);
			
			events[i] = events[spotToMove];
			
			events[spotToMove] = originalStats;
		}
		
		bool ok=true;
		bool eventGood;
		
		int cycleCount = 0;
		
		//Loop to make sure there are no strings of 3 same number of dots or delays
		do{	
			ok=true;
			
			for(int i=2; i <60;i++){

				eventGood = true;
				
				if(((SpatialEvent)events[i]).Dots.Count == ((SpatialEvent)events[i-1]).Dots.Count
					&& ((SpatialEvent)events[i]).Dots.Count  == ((SpatialEvent)events[i-2]).Dots.Count)
					eventGood=false;
				
				if(((SpatialEvent)events[i]).Delay == ((SpatialEvent)events[i-1]).Delay 
					&& ((SpatialEvent)events[i]).Delay  == ((SpatialEvent)events[i-2]).Delay)
					eventGood=false;
				
				if(!eventGood){
					ok = false;
					
					int start = i+1;
					if(start>=60) start = 0;
					
					for(int j =i+1;j<60;j++){
						eventGood=true;
						if(((SpatialEvent)events[j]).Dots.Count == ((SpatialEvent)events[i-1]).Dots.Count
							&& ((SpatialEvent)events[j]).Dots.Count  == ((SpatialEvent)events[i-2]).Dots.Count)
							eventGood=false;
				
						if(((SpatialEvent)events[j]).Delay == ((SpatialEvent)events[i-1]).Delay
							&& ((SpatialEvent)events[j]).Delay  == ((SpatialEvent)events[i-2]).Delay)
							eventGood = false;
						
						if(eventGood){
							
							SpatialEvent originalStats = (SpatialEvent)events[i];
				
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
		
		System.Random rand = new System.Random();
		
		border.renderer.enabled = true;
		
		List<EventStats> newPractice = new List<EventStats>();
		
		SpatialEvent eS;
		
		//Create a event for every combination of events
		for(int j = 0;j<6;j++){
			
			List<int> a1 =new List<int>();
			
			if(j<2){
				a1.Add(rand.Next(1,10));
			}
			else if(j<4){
				while(a1.Count<2){
					int num = rand.Next(1,10);
					if(!a1.Contains(num)) a1.Add(num);
				}
			}
			else{
				while(a1.Count<3){
					int num = rand.Next(1,10);
					if(!a1.Contains(num)) a1.Add(num);
				}
			}
			eS = new SpatialEvent(a1);
			
			if(j%2==0) eS.Delay = 3;
			else eS.Delay = .1f;
			
			newPractice.Add(eS);
		}
		
		//Just do a base randomization
		for(int i=0; i<newPractice.Count-1;i++){
			SpatialEvent originalStats = (SpatialEvent)newPractice[i];
			
			int spotToMove = rand.Next(i,newPractice.Count);
			
			newPractice[i] = newPractice[spotToMove];
			
			newPractice[spotToMove] = originalStats;
		}
		
		practice.AddRange(newPractice);
	}
	
	//Run the tutorial
	protected override IEnumerator runTutorial(){
		
		yield return StartCoroutine(showTitle("Tutorial",3));
		
		state = GameState.Tutorial;
		
		border.renderer.material.mainTexture = eyeBorderImage;
		
		text.renderer.enabled = false;
		
		//Encoding
		//Display the dots
		
		List<GameObject> tutDots = new List<GameObject>(){dots[3]};
		
		foreach(GameObject i in tutDots){
			i.renderer.enabled = true;
		}
		screen.enabled = false;
			
		yield return new WaitForSeconds(2f);
			
		//Delay
		
		border.renderer.material.mainTexture = simpleBorderImage;
		screen.material.color = Color.white;
		
		screen.enabled = true;
		foreach(GameObject g in dots){
			g.renderer.enabled = false;
		}
			
		yield return new WaitForSeconds(.1f);
			
		//Probe
		screen.material.color = Color.gray;
		
		List<Vector3> posToPress = new List<Vector3>(){dots[3].transform.position};
			
		yield return StartCoroutine(tFinger.performAction(posToPress,null));
		
		StartCoroutine(tFinger.exit());	
		
		//ITI
		screen.material.color = Color.black;
		yield return new WaitForSeconds(.5f);
		
		//Encoding
		//Display the dots
		
		border.renderer.material.mainTexture = eyeBorderImage;
		
		tutDots = new List<GameObject>();
		
		tutDots.Add(dots[1]);
		tutDots.Add(dots[5]);
		tutDots.Add(dots[6]);
				
		foreach(GameObject i in tutDots){
			i.renderer.enabled = true;
		}
		screen.enabled = false;
			
		yield return new WaitForSeconds(2f);
			
		//Delay
		screen.material.color = Color.white;
		
		border.renderer.material.mainTexture = simpleBorderImage;
		
		screen.enabled = true;
		foreach(GameObject g in dots){
			g.renderer.enabled = false;
		}
			
		yield return new WaitForSeconds(3f);
			
		//Probe
		screen.material.color = Color.gray;
			
		posToPress = new List<Vector3>(){dots[1].transform.position,dots[5].transform.position, dots[6].transform.position};
		
		yield return StartCoroutine(tFinger.performAction(posToPress,null));
		
		StartCoroutine(tFinger.exit());	
		
		//ITI
		screen.material.color = Color.black;
		yield return new WaitForSeconds(.5f);
	}
	
	//Main method
	protected override IEnumerator runSession(){
		
		//Run tutorial
		yield return StartCoroutine(runTutorial());
		
		//Show Practice Screen
		yield return StartCoroutine(showTitle("Practice",3));
		
		//Main Session
		while(currentEventNum<events.Count){

			//Encoding
			//Display the dots
			state = GameState.Encoding;
			foreach(int i in CurrentEvent.Dots){
				dots[i-1].renderer.enabled = true;
			}
			screen.enabled = false;
				
			yield return new WaitForSeconds(2f);
			
			//Delay
			state = GameState.Delay;
			screen.material.color = Color.white;
			screen.enabled = true;
			foreach(GameObject g in dots){
				g.renderer.enabled = false;
			}
			
			startTime = CurrentEvent.Delay + Time.time;
			
			yield return new WaitForSeconds(CurrentEvent.Delay);
				
			float timeLimit = 3;
			if(practicing) timeLimit = 4;
			
			//Probe
			screen.material.color = Color.gray;
			
			state = GameState.Probe;
			
			float currentTime=0;
			
			while(CurrentEvent.Responses.Count<CurrentEvent.Dots.Count && currentTime < timeLimit){
				currentTime+= Time.deltaTime;
				yield return new WaitForFixedUpdate();
			}
			
			if(currentTime>=timeLimit) CurrentEvent.TimedOut = true; 
			
			if(CurrentEvent.Responses.Count==CurrentEvent.Dots.Count) yield return new WaitForSeconds(.1f);
			
			//ITI
			state = GameState.ITI;
			screen.material.color = Color.black;
			screen.enabled = true;
			yield return new WaitForSeconds(.5f);
			
			nextEvent();
			
			//If the player is still practicing, and reached the end of the practice list check to see if the player did good enough to move on
			if(practicing && currentPractice>=practice.Count){
				
				practiceSessionCount++;
				int numCorrect=0;
				for(int i = currentPractice-6; i< currentPractice;i++){
					if(practice[i].respondedCorrectly()) numCorrect+=1;
				}
				
				NeuroLog.Log("Total Correct Responded: " + numCorrect);
				
				//If at least 3 correct, or 4 tries already, move on to the test phase
				if(numCorrect>=3 || practiceSessionCount>=4){
					NeuroLog.Log("Continuing to MainSession");
					
					yield return StartCoroutine(showTitle("Test",3));
					
					border.SetActive(false);
					
					practicing = false;
				}
				//Otherwise retry practice
				else{
					NeuroLog.Log("Redo Practice");
					
					generatePractice();
					
					yield return StartCoroutine(runTutorial());
						
					yield return StartCoroutine(showTitle("Practice",3));
				}
			}
		}
		
		gameOver = true;
		
		//Writeout the log file
		csv.WriteOut(true);
		
		yield return StartCoroutine(showTitle("Session Over",3));
		
		Debug.Log("GAME OVER, Returning to menu");
		
		//Return to the menu
		Application.LoadLevel("menu");
	}
	
	// Update is called once per frame
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
		
		//Not touching
		if(!currentTouch)
			touching = false;
		//Touching the screen, not holding
		else if(!touching && currentTouch){	
			
			touching = true;
			
			//If the game state is either Probe or delay
			if(state == GameState.Probe || state == GameState.Delay){
				
				Vector2 fixedPos = touchPos;
				
				//Inverse the y
				fixedPos.y = Screen.height - fixedPos.y;
						
				float time = Time.time - startTime;
				
				//Good Response
				if(state == GameState.Probe){
					
					Response r = new Response(sType,time,new Vector2(fixedPos.x,fixedPos.y),0);	
					
					Vector2 screenPos = Vector2.zero;
					
					screenPos.x = ((fixedPos.x/Screen.width) * 53) - 26.5f;
					screenPos.y = ((fixedPos.y/Screen.height) * -30) +15;
					
					//Start fading circle
					StartCoroutine(spot.fadeFinger(screenPos, r.DotPressed));
				
					CurrentEvent.AddResponse(r, true);
				}
				//Bad Response, too early
				else{
					Response r = new Response(sType,time,new Vector2(fixedPos.x,fixedPos.y),1);
					
					CurrentEvent.AddResponse(r, false);
				}
			}
		}
	}
}