using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Session manager for implicit games
public class MemAttention1Manager : GameManager {

	[SerializeField]
	private TextMesh stimulusText;

	//The stimulus image
	private GameObject stimulus;
	
	//Positions of the game's stimuli
	private Vector2[] stimPositions;
	public Vector2[] StimPositions{
		get{return stimPositions;}
		set{stimPositions = value;}
	}
	
	//The ordered list of stimuli that will appear in the task
	private List<MemAttentionEvent> ordered;
	
	private List<MemAttentionEvent> random;
	
	
	//Returns the current Event for Implicit games
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
	void Awake () {
		base.Setup(GameManager.SessionType.MemAttentEnc1);
		
		stimulus = GameObject.Find("Stimulus");

		stimPositions = new Vector2[4]{new Vector2(12,8), new Vector2(12,-8), new Vector2(-12,-8), new Vector2(-12,8)};
		
		//Preform the read in to get the events
		events = csv.ReadInSession();
		
		//Generate the practice events
		generatePractice();
		
		//If the read in failed, generate the base events
		if(events ==null){

			NeuroLog.Log("Failed to load list of events");
		} else {
			//Start the game
			StartCoroutine("runSession");

		}
		
	}

	//Generate practice pitches
	protected override  void generatePractice(){
	
		border.GetComponent<Renderer>().enabled = true;

		// pull practice from first 8 in events list
		List<EventStats> newPractice = events.GetRange(0, 8);

		// then delete them from events
		events.RemoveRange(0, 8);

		//new List<EventStats>(){new MemAttentionEvent(1,"truck"),new MemAttentionEvent(2,"kite"),new MemAttentionEvent(3,"flower"),new MemAttentionEvent(4,"paint")};

		practice.AddRange(newPractice);
	}
	
	//Method used to perform the tutorial
	protected override  IEnumerator runTutorial(){

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
		while(currentEventNum < events.Count){

			Texture2D tex = Resources.Load<Texture2D>("stimuli/" + CurrentEvent.Stimulus);

			AudioClip audio = Resources.Load<AudioClip>("audio/" + CurrentEvent.Stimulus);

			// set text label
			stimulusText.text = CurrentEvent.Stimulus;
			stimulusText.gameObject.SetActive(true);

			stimulus.GetComponent<Renderer>().material.mainTexture = tex;

			audioSource.PlayOneShot(audio);

			stimulus.transform.position = new Vector3(stimPositions[CurrentEvent.Quadrant-1].x, -3.5f,stimPositions[CurrentEvent.Quadrant-1].y);
			
			startTime = Time.time;
			
			stimulus.GetComponent<Renderer>().enabled = true;
			
			float currentTime =0;
			
			state = GameState.Probe;
			//Wait for either the player's response or the time limit
			while(CurrentEvent.Response == null && currentTime < 2f){
				currentTime += Time.deltaTime;
				yield return new WaitForFixedUpdate();
			}

			if(currentTime >= 2f) CurrentEvent.TimedOut = true;

			if(CurrentEvent.Response != null) yield return new WaitForSeconds(.1f);
			
			//ITI, black screen
			state = GameState.ITI;
			//screen.enabled = true;
			
			stimulus.GetComponent<Renderer>().enabled = false;

			stimulusText.gameObject.SetActive(false);

			yield return new WaitForSeconds(.1f);
			
			//Get the next event
			nextEvent();

			//If we reached the end of the practice list, check to see if the player passed
			if(practicing && currentPractice>=practice.Count){
				
				//Count the nummber of correct responses
				practiceSessionCount++;

					NeuroLog.Log("Continuing to MainSession");
					
					border.SetActive(false);
					
					yield return StartCoroutine(showTitle("Test",3));
					
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
				Ray ray = GetComponent<Camera>().ScreenPointToRay(touchPos);
				RaycastHit hit = new RaycastHit();
				//If the raycast of the touch hit something
				if(Physics.Raycast(ray, out hit)) {
					//Hit the Stimulus
					if(hit.collider.name == "Stimulus"){
				
						Vector2 fixedPos = touchPos;
						
						//Reverts the y orientation
						fixedPos.y = Screen.height - fixedPos.y;
						
						//Calculate the response time
						float time = Time.time - startTime;
						
						if(time<=1f){
							
							float x = ((stimulus.transform.position.x + 26.7f)/53.4f) * Screen.width;
							
							float y = ((stimulus.transform.position.z - 15f)/-30f) * Screen.height;
							
							Vector2 targPos = new Vector2(x,y);
							
							//Create the respones
							Response r = new Response(targPos, time,new Vector2(fixedPos.x,fixedPos.y));
									
							Vector2 screenPos = Vector2.zero;
							
							screenPos.x = ((fixedPos.x/Screen.width) * 53.4f) - 26.7f;
							screenPos.y = ((fixedPos.y/Screen.height) * -30) +15;
							
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