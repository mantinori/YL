using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Session manager for implicit games
public class MemTest1Manager : GameManager {

	[SerializeField]
	protected TextMesh stimulusText;
	
	//The stimulus images
	[SerializeField]
	protected GameObject[] stimuli;

	[SerializeField]
	protected bool kidsMode = false;

	protected int screenIndex = 0;
	
	//Positions of the game's stimuli
	protected Vector2[] stimPositions;
	public Vector2[] StimPositions{
		get{return stimPositions;}
		set{stimPositions = value;}
	}
	
	public MemTest1Event CurrentEvent{
		get{
			//If were still practicing, return the latest practice
			if(practice.Count<=currentPractice) 
				return (MemTest1Event)events[currentEventNum]; 
			//Otherwise return the latest real event
			else 
				return (MemTest1Event)practice[currentPractice];
		}
	}

	void Awake () {
		base.Setup(GameManager.SessionType.MemTest1);

		stimPositions = new Vector2[4]{new Vector2(Screen.width * .75f, Screen.height * .75f), 
			new Vector2(Screen.width * .75f, Screen.height / 4f), 
			new Vector2(Screen.width / 4f, Screen.height / 4f), 
			new Vector2(Screen.width / 4f, Screen.height * .75f)};

		//Preform the read in to get the events
		events = csv.ReadInSession();
		
		//Generate the practice events
		generatePractice();
		
		//If the read in failed, generate the base events
		if(events == null){
			NeuroLog.Log("Failed to load list of events");
		} else {
			// randomize
			ShuffleEventsNonRepeat(2);

			//Start the game
			StartCoroutine("runSession");
		}
		
	}

	protected void ShuffleEventsNonRepeat(int maxRepeats) {
		events.Shuffle();

		if(events.Count < maxRepeats) return;

		// go through and make sure no more than 2 in a row

		int index = 0;
		while(index < events.Count - 1) {
			MemTest1Event evt = events[index] as MemTest1Event;
			MemTest1Event nextEvt = events[index + 1] as MemTest1Event;

			int numRepeats = 0;
			int totalAttempts = 0;
			while(totalAttempts < 100 && evt.Stimuli[evt.TargetLoc - 1] == nextEvt.Stimuli[nextEvt.TargetLoc - 1]) {
				// it's a repeated event, increment total so far
				numRepeats++;
				if(numRepeats > maxRepeats) {
					// if more than X repeats, move item to the end
					events.Remove(nextEvt);
					events.Add(nextEvt);

					numRepeats--;

					totalAttempts++;
				}

				nextEvt = events[index + 1] as MemTest1Event;
			} 

			index++;

		}

		// DEBUG
//		string listString = "";
//		foreach(EventStats e in events) {
//			MemTest1Event evt = e as MemTest1Event;
//			listString += evt.Stimuli[evt.TargetLoc - 1] + "\n";
//		}
//		Debug.Log(listString);
	}

	//Generate practice pitches
	protected override void generatePractice(){
		
		border.GetComponent<Renderer>().enabled = true;
		
		// pull practice from first 8 in events list
		List<EventStats> newPractice = events.GetRange(0, 8);
		
		// then delete them from events
		events.RemoveRange(0, 8);
		
		practice.AddRange(newPractice);
	}


	//Main method of the game
	protected override IEnumerator runSession(){
		
		//Show the menu
		yield return StartCoroutine(showMenu(false));

		//Show Practice screen
		yield return StartCoroutine(showTitle("Practice",3));
		
		screen.enabled = false;
		
		//Main Session
		while(currentEventNum < events.Count){

			screenIndex = 0;

			string targetName = CurrentEvent.Stimuli[CurrentEvent.TargetLoc - 1];

			AudioClip audio = Resources.Load<AudioClip>("audio/" + targetName);

			startTime = Time.time;

			// set text label
			stimulusText.text = targetName;
			stimulusText.gameObject.SetActive(true);

			audioSource.PlayOneShot(audio);

			yield return new WaitForSeconds(2f);

			screenIndex = 1;

			stimulusText.gameObject.SetActive(false);
			
			//ITI, blank screen
			state = GameState.ITI;

			yield return new WaitForSeconds(1f);

			screenIndex = 2;

			//float currentTime = 0;

			state = GameState.Probe;

			float onsetTime = Time.time - startTime;

			CurrentEvent.OnsetTime = onsetTime;

			// put all four stimuli on screen
			for(int i = 0; i < CurrentEvent.Stimuli.Length; i++) {
				Texture2D tex = Resources.Load<Texture2D>("stimuli/" + CurrentEvent.Stimuli[i]);
				GameObject stimulus = stimuli[i];
				stimulus.GetComponent<Renderer>().material.mainTexture = tex;
				Vector3 worldPos = Camera.main.ScreenToWorldPoint(stimPositions[i]);
				stimulus.transform.position = new Vector3(worldPos.x, -3.5f, worldPos.z);
				stimulus.GetComponent<Renderer>().enabled = true;

			}
		
			if(kidsMode) {
				yield return new WaitForSeconds(1f);
			} else {
				yield return new WaitForSeconds(.5f);
			}

			for(int i = 0; i < CurrentEvent.Stimuli.Length; i++) {
				GameObject stimulus = stimuli[i];
				stimulus.GetComponent<Renderer>().enabled = false;
			}

			if(kidsMode) {
				yield return new WaitForSeconds(.5f);
			} else {
				yield return new WaitForSeconds(1f);
			}

			while(isPaused) {
				yield return new WaitForEndOfFrame();
			}

			//Get the next event
			nextEvent();

			//If we reached the end of the practice list, check to see if the player passed
			if(practicing && currentPractice>=practice.Count){
				
				practiceSessionCount++;
				
				practicing = false;
				
				screen.enabled = true;
				
				//Show the menu
				yield return StartCoroutine(showMenu(true));
				
				if(!practicing) {
					NeuroLog.Log("Continuing to MainSession");
					
					border.SetActive(false);
					
					yield return StartCoroutine(showTitle("Test",3));
					
				} else {
					
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
		Application.LoadLevel(1);
	}
	
	// Constantly check for player input
	void Update () {	
			
		if(screen.enabled) return;

		bool currentTouch = false;
		
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
		if(!currentTouch) {
			touching = false;
		} else if(!touching && currentTouch){	
			//If a player has touched the screen and released

			//Debug.Log("touchPos="+touchPos);

			touching = true;

			//Calculate the response time
			float time = Time.time - startTime;

			// stimulus position
			Vector2 targPos = stimPositions[CurrentEvent.TargetLoc - 1];
			Vector2 cuePos = stimPositions[CurrentEvent.CuedLoc - 1];

			//Create the respones
			Response r = new Response(targPos, cuePos, time, touchPos, screenIndex);

			// finger/dot indicator position
			Vector3 worldPos = Camera.main.ScreenToWorldPoint(touchPos);
			Vector2 fingerPos = new Vector2(worldPos.x, worldPos.z);

			//Start the fade spot
			StartCoroutine(spot.fadeFinger(fingerPos, -1));

			// don't store any responses if pause
			if(isPaused) return;

			//Add the response
			CurrentEvent.Responses.Add(r);
				

		}
	}
}