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
			events.Shuffle();

			//Start the game
			StartCoroutine("runSession");
		}
		
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
		
		//Show the tutorial
		yield return StartCoroutine(runTutorial());
	
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

			// put all four stimuli on screen
			for(int i = 0; i < CurrentEvent.Stimuli.Length; i++) {
				Texture2D tex = Resources.Load<Texture2D>("stimuli/" + CurrentEvent.Stimuli[i]);
				GameObject stimulus = stimuli[i];
				stimulus.GetComponent<Renderer>().material.mainTexture = tex;
				Vector3 worldPos = Camera.main.ScreenToWorldPoint(stimPositions[i]);
				stimulus.transform.position = new Vector3(worldPos.x, -3.5f, worldPos.z);
				stimulus.GetComponent<Renderer>().enabled = true;

			}
		
			yield return new WaitForSeconds(.5f);

			for(int i = 0; i < CurrentEvent.Stimuli.Length; i++) {
				GameObject stimulus = stimuli[i];
				stimulus.GetComponent<Renderer>().enabled = false;
			}

			yield return new WaitForSeconds(1f);

			//Get the next event
			nextEvent();

			//If we reached the end of the practice list, check to see if the player passed
			if(practicing && currentPractice>=practice.Count){
				
				//Count the nummber of correct responses
				practiceSessionCount++;

				NeuroLog.Log("Continuing to MainSession");
				
				border.SetActive(false);
				
				yield return StartCoroutine(showTitle("Study",3));
				
				practicing = false;

			}
		}
		
		gameOver = true;
		
		//Writeout 
		csv.WriteOut(true);
		
		//SessionTitle screen
		yield return StartCoroutine(showTitle("Session Over",3));
		
		Debug.Log("GAME OVER, Returning to menu");
		
		//Return to menu
		Application.LoadLevel(2);
	}
	
	// Constantly check for player input
	void Update () {	
			
		if(screen.enabled) return;

		bool currentTouch = false;
		
		//Get the touch location based on the platform
		if(Application.platform == RuntimePlatform.IPhonePlayer){
			if(Input.touchCount>0){
				touchPos = Input.touches[0].position;
				currentTouch = true;
			}
		} else {
			if(Input.GetMouseButtonDown(0)){
				touchPos = Input.mousePosition;			
				currentTouch = true;
			}
		}
		
		//Not Touching
		if(!currentTouch) {
			touching = false;
		} else if(!touching) {	
			//If a player has touched the screen and released

			//Debug.Log("touchPos="+touchPos);

			touching = true;

			//Calculate the response time
			float time = Time.time - startTime;

			// stimulus position
			Vector2 targPos = stimPositions[CurrentEvent.ValidLoc - 1];

			//Create the respones
			Response r = new Response(targPos, time, touchPos, screenIndex);

			// finger/dot indicator position
			Vector3 worldPos = Camera.main.ScreenToWorldPoint(touchPos);
			Vector2 fingerPos = new Vector2(worldPos.x, worldPos.z);

			//Start the fade spot
			StartCoroutine(spot.fadeFinger(fingerPos, -1));
			
			//Add the response
			CurrentEvent.Responses.Add(r);
				

		}
	}
}