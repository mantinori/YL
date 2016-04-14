using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Session manager for implicit games
public class StimTest1Manager : MemTest1Manager {

	[SerializeField]
	protected GameObject cueArrow;

	float[] arrowDirections = new float[]{-35f,35f,145f,215f};

	void Awake () {
		base.Setup(GameManager.SessionType.StimTest1);
		
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

			// set arrow direction
			cueArrow.SetActive(true);
			cueArrow.transform.eulerAngles = new Vector3(0f,arrowDirections[CurrentEvent.CuedLoc - 1], 0f);

			audioSource.PlayOneShot(audio);

			yield return new WaitForSeconds(2f);

			screenIndex = 1;

			stimulusText.gameObject.SetActive(false);

			cueArrow.SetActive(false);

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


}