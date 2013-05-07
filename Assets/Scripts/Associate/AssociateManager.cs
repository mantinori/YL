using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Manager of a star game
public class AssociateManager : GameManager {
	
	//Lists of each type of object in a trial
	public AssociateObject target;
	
	public List<AssociateObject> stimuli = new List<AssociateObject>();
	
	public List<Texture> images = new List<Texture>();

	public List<Texture> pracImages = new List<Texture>();
	
	public AssociateBox box;
	
	private bool goodResponse;
	
	//Return the current event
	public AssociateEvent CurrentEvent{
		get{
			//If were still practicing, return practice event
			if(practice.Count<=currentPractice) 
				return (AssociateEvent)events[currentEventNum]; 
			//otherwise return normal event
			else 
				return (AssociateEvent)practice[currentPractice];
		}
	}
	
	// Use this for initialization
	void Awake () {
		
		goodResponse =false;
		
		base.Setup(GameManager.SessionType.Associate);
		
		//Set the num of each stimuli
		target.Num = 0;
		for(int i=0; i<stimuli.Count;i++){
			stimuli[i].Num = i+1;
		}
		
		box.Reset();
		
		//Reset the objects orientation/positions
		target.Reset();
		foreach(AssociateObject aO in stimuli){
			aO.Reset();
		}
		
		//Read in the event
		events =  csv.ReadInSession();
		
		//If read in failed, generate basic list of events
		if(events == null){
			
			events = new List<EventStats>();
			
			generateEvents();
			randomizeCorrectChoice();
			
			NeuroLog.Log("Generating Random events");
		}
		
		generatePractice();
		
		randomizeEvents();
		
		//Start the game
		StartCoroutine("runSession");
	}
	
	
	//Get the corresponding image's id of a given number
	public int getMatchingNum(int num){
		switch(num){
			 case 1: return 12;
			 case 2: return 9;
			 case 3: return 11;
			 case 4: return 7;
			 case 5: return 8;
			 case 6: return 10;
			 case 7: return 4;
			 case 8: return 5;
			 case 9: return 2;
			 case 10: return 6;
			 case 11: return 3;
			 case 12: return 1;
			 case -1: return -2;
			 case -2: return -1;
			 case -3: return -4;
			 case -4: return -3;
			default: return 0;
		}
	}
	
	//rand//Method used to generate the base events
	protected override void generateEvents(){
		
		System.Random rand = new System.Random();
		bool good = false;
		List<EventStats> eS;
		do{
			eS = new List<EventStats>();
			
			int[] nums = new int[12]{3,3,3,3,3,3,3,3,3,3,3,3};
			
			for(int i=1;i<13;i++){
	
				List<int> stim = new List<int>(){getMatchingNum(i)};
				
				int min = 0;
				int max = 6;
				if(stim[0]>6){
					min = 6;
					max =12;
				}
				
				int total=0;
				
				for(int j= min;j<max;j++){
					if(!stim.Contains(j+1) && i!=j+1) total+= nums[j];
				}
				
				double threshold = rand.NextDouble();
				
				float sum = 0;
				
				for(int j=min;j<max;j++){
					if(!stim.Contains(j+1) && i!=j+1){
						sum+= ((float)nums[j])/((float)total);
					
						if(sum>threshold){
							stim.Add(j+1);
							nums[j]--;
							break;
						}
					}
				}
				
				if(stim[0]>6){
					min = 0;
					max = 6;
				}
				else{
					min = 6;
					max =12;
				}
				
				int count = 0;
				
				do{
					total=0;
					
					for(int j= min;j<max;j++){
						if(!stim.Contains(j+1) && i!=j+1)
							total+= nums[j];
					}
						
					threshold = rand.NextDouble();
					sum = 0;
					
					for(int j=min;j<max;j++){
						if(!stim.Contains(j+1) && i!=j+1){
							sum+= ((float)nums[j])/((float)total);
						
							if(sum>threshold){
								stim.Add(j+1);
								nums[j]= nums[j]-1;
								break;
							}
						}
					}
					count++;
				}while(count<2);
							
				eS.Add(new AssociateEvent(i,stim));
			}
			
			good = true;
			
			foreach(int m in nums){
				if(m!=0){ 
					Debug.Log("RETRY");
					good = false;
					break;
				}
			}
			
		}while(!good);
		
		events = eS;
	}
	
	//Randomize the order of stimuli in the individual events
	private void randomizeCorrectChoice(){
		System.Random rand = new System.Random();
		
		int[] count = new int[4]{0,0,0,0};
		
		for(int i=0;i<events.Count;i++){
			
			List<int> stillGood = new List<int>();
			
			int toAdd;
			
			for(int j=0;j<count.Length;j++){
				if(count[j]<4) stillGood.Add(j);
			}
			
			toAdd = rand.Next(0,stillGood.Count);
			
			count[toAdd]++;
				
			int num = stillGood[toAdd];
			
			int originalValue = ((AssociateEvent)events[i]).Stimuli[num];
			
			((AssociateEvent)events[i]).Stimuli[num] = ((AssociateEvent)events[i]).Stimuli[0];
			
			((AssociateEvent)events[i]).Stimuli[0] = originalValue;
		}
	}
	
	//Method used to randomize the order of the list of events
	protected override void randomizeEvents(){
		
		System.Random rand = new System.Random();
		
		//Apply a base randomization
		for(int i=0; i<events.Count-1;i++){
			AssociateEvent originalStats = (AssociateEvent)events[i];
			
			int spotToMove = rand.Next(i,events.Count);
			
			events[i] = events[spotToMove];
			
			events[spotToMove] = originalStats;
		}
	}

	//Generate practice pitches
	protected override  void generatePractice(){
		List<EventStats> newPractice = new List<EventStats>();
		
		newPractice.Add(new AssociateEvent(-1,new List<int>(){-4,-3,-2,-5}));
		newPractice.Add(new AssociateEvent(-4,new List<int>(){-5,-2,-1,-3}));
		newPractice.Add(new AssociateEvent(-2,new List<int>(){-3,-1,-5,-4}));
		newPractice.Add(new AssociateEvent(-3,new List<int>(){-4,-5,-1,-2}));
		
		System.Random rand = new System.Random();
		
		//Just simply randomize the order
		for(int i=0; i<newPractice.Count-1;i++){
			AssociateEvent originalStats = (AssociateEvent)newPractice[i];
			
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
		
		screen.enabled = false;
		
		border.transform.localPosition = new Vector3(0,0,0.5f);
		List<Vector3> tutDots = new List<Vector3>(){new Vector3(0f,-5f,0)};
		
		//Troubled first pair
		target.renderer.material.mainTexture = pracImages[0];
		
		stimuli[0].renderer.material.mainTexture = pracImages[3];
		stimuli[1].renderer.material.mainTexture = pracImages[1];
		stimuli[2].renderer.material.mainTexture = pracImages[2];
		stimuli[3].renderer.material.mainTexture = pracImages[4];
		
		yield return StartCoroutine(startEvent());
		
		yield return new WaitForSeconds(.5f);
		
		yield return StartCoroutine(tFinger.moveTo(tutDots));
		
		yield return new WaitForSeconds(1f);
		
		tutDots = new List<Vector3>(){new Vector3(-18f,-5f,-7.5f)};
		
		yield return StartCoroutine(tFinger.performAction(tutDots,null));
		
		StartCoroutine(spot.fadeFinger(new Vector2(tFinger.transform.position.x,tFinger.transform.position.z),0));
		
		yield return new WaitForSeconds(.75f);
		
		tutDots = new List<Vector3>(){new Vector3(6f,-5f,-7.5f)};
		
		yield return StartCoroutine(tFinger.performAction(tutDots,null));
		StartCoroutine(spot.fadeFinger(new Vector2(tFinger.transform.position.x,tFinger.transform.position.z),0));
		
		yield return new WaitForSeconds(.5f);
		
		tutDots = new List<Vector3>(){new Vector3(-6f,-5f,-7.5f)};
		
		yield return StartCoroutine(tFinger.performAction(tutDots,null));
		StartCoroutine(spot.fadeFinger(new Vector2(tFinger.transform.position.x,tFinger.transform.position.z),1));
		
		StartCoroutine(tFinger.exit());	
		
		yield return StartCoroutine(endEvent(1));
		
		yield return new WaitForSeconds(.25f);
		
		//Confidant first pair
		tutDots = new List<Vector3>(){new Vector3(0f,-5f,0f)};
		target.renderer.material.mainTexture = pracImages[1];
		stimuli[0].renderer.material.mainTexture = pracImages[0];
		stimuli[1].renderer.material.mainTexture = pracImages[3];
		stimuli[2].renderer.material.mainTexture = pracImages[4];
		stimuli[3].renderer.material.mainTexture = pracImages[2];
		
		yield return StartCoroutine(startEvent());
		
		yield return new WaitForSeconds(.5f);
		
		yield return StartCoroutine(tFinger.moveTo(tutDots));
		
		yield return new WaitForSeconds(.5f);
		
		tutDots = new List<Vector3>(){new Vector3(-18f,-5f,-7.5f)};
		
		yield return StartCoroutine(tFinger.performAction(tutDots,null));
		StartCoroutine(spot.fadeFinger(new Vector2(tFinger.transform.position.x,tFinger.transform.position.z),1));
		
		StartCoroutine(tFinger.exit());	
		
		yield return StartCoroutine(endEvent(0));
		
		yield return new WaitForSeconds(.25f);
		
		//Troubled second pair
		tutDots = new List<Vector3>(){new Vector3(0f,-5f,0)};
		
		target.renderer.material.mainTexture = pracImages[2];
		
		stimuli[0].renderer.material.mainTexture = pracImages[4];
		stimuli[1].renderer.material.mainTexture = pracImages[1];
		stimuli[2].renderer.material.mainTexture = pracImages[3];
		stimuli[3].renderer.material.mainTexture = pracImages[0];
		
		yield return StartCoroutine(startEvent());
		
		yield return new WaitForSeconds(.5f);
		
		yield return StartCoroutine(tFinger.moveTo(tutDots));
		
		yield return new WaitForSeconds(1f);
		
		tutDots = new List<Vector3>(){new Vector3(-18f,-5f,-7.5f)};
		
		yield return StartCoroutine(tFinger.performAction(tutDots,null));
		StartCoroutine(spot.fadeFinger(new Vector2(tFinger.transform.position.x,tFinger.transform.position.z),0));
		
		yield return new WaitForSeconds(.5f);
		
		tutDots = new List<Vector3>(){new Vector3(6f,-5f,-7.5f)};
		
		yield return StartCoroutine(tFinger.performAction(tutDots,null));
		StartCoroutine(spot.fadeFinger(new Vector2(tFinger.transform.position.x,tFinger.transform.position.z),1));
		
		StartCoroutine(tFinger.exit());	
		
		yield return StartCoroutine(endEvent(2));
		
		yield return new WaitForSeconds(.25f);
		
		//Confidant second pair
		tutDots = new List<Vector3>(){new Vector3(0f,-5f,0f)};
		target.renderer.material.mainTexture = pracImages[3];
		stimuli[0].renderer.material.mainTexture = pracImages[1];
		stimuli[1].renderer.material.mainTexture = pracImages[4];
		stimuli[2].renderer.material.mainTexture = pracImages[0];
		stimuli[3].renderer.material.mainTexture = pracImages[2];
		
		yield return StartCoroutine(startEvent());
		
		yield return new WaitForSeconds(.5f);
		
		yield return StartCoroutine(tFinger.moveTo(tutDots));
		
		yield return new WaitForSeconds(.5f);
		
		tutDots = new List<Vector3>(){new Vector3(18f,-5f,-7.5f)};
		
		yield return StartCoroutine(tFinger.performAction(tutDots,null));
		StartCoroutine(spot.fadeFinger(new Vector2(tFinger.transform.position.x,tFinger.transform.position.z),1));
		
		StartCoroutine(tFinger.exit());	
		
		yield return StartCoroutine(endEvent(3));
		
		yield return new WaitForSeconds(.25f);
	}
	
	//Have all the objects move onto the scene
	private IEnumerator startEvent(){
		
		//Move the box into the scene
		box.Reset();
		
		//Move all objects on the screen
		do{
			target.transform.Translate(new Vector3(0,0,-.5f));
			
			for(int i = 0;i<4;i++){
				stimuli[i].transform.Translate(new Vector3(0,0,.5f));
			}
			
			yield return new WaitForFixedUpdate();
		}while(target.transform.position.z>7.5f);
	}
	
	//Have the matching two meat and dance, then have everything exit the scene
	private IEnumerator endEvent(int correct){
		
		//Match up the objects
		yield return StartCoroutine(stimuli[correct].matchUp());
		
		//Dance off
		StartCoroutine(stimuli[correct].dance());
		StartCoroutine(target.dance());
		
		//Wait for them to finish bouncing
		while(target.Dancing && stimuli[correct].Dancing){
			yield return new WaitForFixedUpdate();
		}
		
		//Move all objects off the screen
		do{
			target.transform.Translate(new Vector3(0,0,.5f));
			box.transform.Translate(new Vector3(0,0,.5f));
			
			for(int i = 0; i<4;i++){
				if(stimuli[i].Num == correct+1)
					stimuli[i].transform.Translate(new Vector3(0,0,.5f));
				else
					stimuli[i].transform.Translate(new Vector3(0,0,-.5f));
			}
			
			yield return new WaitForFixedUpdate();
		}while(target.transform.position.z<20);
		
		target.Reset();
		foreach(AssociateObject aO in stimuli){
			aO.Reset();
		}
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
				
			//Probe
			//Set up the stimuli
			if(CurrentEvent.TargetImage>0)
				target.renderer.material.mainTexture = images[CurrentEvent.TargetImage-1];
			else{
				int num = (CurrentEvent.TargetImage*-1)-1;
				target.renderer.material.mainTexture = pracImages[num];
			}
			for(int i = 0; i<4;i++){
				if(CurrentEvent.Stimuli[i]>0)
					stimuli[i].renderer.material.mainTexture = images[CurrentEvent.Stimuli[i]-1];
				else{
					int num = (CurrentEvent.Stimuli[i]*-1)-1;
					stimuli[i].renderer.material.mainTexture = pracImages[num];
				}
			}
			
			yield return StartCoroutine(startEvent());
			
			startTime = Time.time;
			
			state = GameState.Probe;
			//Wait for either the player's response or the time limit
			while(!goodResponse){
				yield return new WaitForFixedUpdate();
			}
			
			//ITI, dancing objects
			state = GameState.ITI;
			goodResponse = false;
			
			int correct = CurrentEvent.Stimuli.IndexOf(getMatchingNum(CurrentEvent.TargetImage));
			
			yield return StartCoroutine(endEvent(correct));
			
			//Get the next event
			nextEvent();
			
			yield return new WaitForSeconds(.25f);
			
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
		
		gameOver = true;
		
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
			
		//Get the touch location based on the platform type
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
		//Not holding, touch
		else if(!touching && currentTouch){		
			touching = true;
			//If were in the probe state
			if(state == GameState.Probe){
				Ray ray = camera.ScreenPointToRay(touchPos);
				RaycastHit hit = new RaycastHit();
				//If the raycast of the touch hit something
				if(Physics.Raycast(ray, out hit)) {
					
					//Hit one of the stimuli
					if(hit.collider.name.Contains("stimulus")){
						
						int stimNum = CurrentEvent.Stimuli[int.Parse(hit.collider.name.Replace("stimulus",""))-1];
						
						if(getMatchingNum(CurrentEvent.TargetImage) == stimNum)
							goodResponse = true;
						
						Vector2 fixedPos = touchPos;
						
						fixedPos.y = Screen.height - fixedPos.y;
					
						float responseTime = Time.time - startTime;
						
						Vector2 screenPos = Vector2.zero;
				
						screenPos.x = ((fixedPos.x/Screen.width) * 53.4f) - 26.7f;
						screenPos.y = ((fixedPos.y/Screen.height) * -30) +15;
						
						//Convert the goodResponse to an int
						int i = goodResponse ? 1 : 0;
						
						//Start the fade spot
						StartCoroutine(spot.fadeFinger(screenPos, i));
						
						Response r;
						r = new Response(stimNum, responseTime,new Vector2(fixedPos.x,fixedPos.y));
						
						//Add the response
						CurrentEvent.Responses.Add(r);
					}
				}
			}
		}
	}
}