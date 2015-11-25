using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Session manager for implicit games
public class MemAttention2Manager : MemAttention1Manager {

	protected override void Awake () {
		base.Setup(GameManager.SessionType.MemAttentEnc2);
		
		stimulus = GameObject.Find("Stimulus");
		
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

			Texture2D tex = Resources.Load<Texture2D>("stimuli/" + CurrentEvent.Stimulus);

			AudioClip audio = Resources.Load<AudioClip>("audio/" + CurrentEvent.Stimulus);

			startTime = Time.time;

			// set text label
			stimulusText.text = CurrentEvent.Stimulus;
			stimulusText.gameObject.SetActive(true);

			stimulus.GetComponent<Renderer>().enabled = false;

			audioSource.PlayOneShot(audio);

			yield return new WaitForSeconds(1f);

			screenIndex = 1;

			stimulusText.gameObject.SetActive(false);
			
			//ITI, blank screen
			state = GameState.ITI;

			yield return new WaitForSeconds(1f);

			screenIndex = 2;

			float currentTime = 0;

			state = GameState.Probe;

			stimulus.GetComponent<Renderer>().material.mainTexture = tex;
			Vector3 worldPos = Camera.main.ScreenToWorldPoint(stimPositions[CurrentEvent.Quadrant-1]);
			stimulus.transform.position = new Vector3(worldPos.x, -3.5f, worldPos.z);

			stimulus.GetComponent<Renderer>().enabled = true;

			yield return new WaitForSeconds(1f);

			stimulus.GetComponent<Renderer>().enabled = false;

			stimulusText.gameObject.SetActive(false);

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
		Application.LoadLevel("menu");
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
			Vector2 targPos = stimPositions[CurrentEvent.Quadrant - 1];

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