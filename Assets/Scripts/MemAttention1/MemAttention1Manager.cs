using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Session manager for implicit games
public class MemAttention1Manager : GameManager {

	[SerializeField]
	protected TextMesh stimulusText;

	//The stimulus image
	protected GameObject stimulus;

	protected int screenIndex = 0;

	//Positions of the game's stimuli
	protected Vector2[] stimPositions;
	public Vector2[] StimPositions{
		get{return stimPositions;}
		set{stimPositions = value;}
	}

	public MemAttentionEvent CurrentEvent{
		get{
			//If were still practicing, return the latest practice
			if(practice.Count<=currentPractice) 
				return (MemAttentionEvent)events[currentEventNum]; 
			//Otherwise return the latest real event
			else 
				return (MemAttentionEvent)practice[currentPractice];
		}
	}
	
	// Use this for initialization
	protected virtual void Awake () {
		base.Setup(GameManager.SessionType.MemAttentEnc1);
		
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

			Texture2D tex = Resources.Load<Texture2D>("stimuli/" + CurrentEvent.Stimulus);

			AudioClip audio = Resources.Load<AudioClip>("audio/" + CurrentEvent.Stimulus);

			// set text label
			stimulusText.text = CurrentEvent.Stimulus;
			stimulusText.gameObject.SetActive(true);

			stimulus.GetComponent<Renderer>().material.mainTexture = tex;

			audioSource.PlayOneShot(audio);

			Vector3 worldPos = Camera.main.ScreenToWorldPoint(stimPositions[CurrentEvent.Quadrant-1]);
			stimulus.transform.position = new Vector3(worldPos.x, -3.5f, worldPos.z);
			
			startTime = Time.time;
			
			stimulus.GetComponent<Renderer>().enabled = true;
			
			float currentTime = 0;
			
			state = GameState.Probe;

			yield return new WaitForSeconds(2f);

			//ITI, black screen
			state = GameState.ITI;

			stimulus.GetComponent<Renderer>().enabled = false;

			stimulusText.gameObject.SetActive(false);

			yield return new WaitForSeconds(.1f);
			
			//Get the next event
			nextEvent();

			//If we reached the end of the practice list, check to see if the player passed
			if(practicing && currentPractice >= practice.Count){
				
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

}