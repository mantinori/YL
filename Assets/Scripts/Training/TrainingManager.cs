using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Session manager for implicit games
public class TrainingManager : GameManager {

	[SerializeField]
	protected TextMesh stimulusText;

	//The stimulus image
	protected GameObject stimulus;

	protected int screenIndex = 0;

	// Use this for initialization
	protected virtual void Awake () {
		base.Setup(GameManager.SessionType.None);

		stimulus = GameObject.Find("Stimulus");

		//Start the game
		StartCoroutine("runSession");
	}

	protected override IEnumerator runSession(){
		
		//Show Practice screen
		yield return StartCoroutine(showTitle("Training",2));

		screen.enabled = false;

		while(true){

			screenIndex = 0;

			yield return new WaitForSeconds(2f);

		}
		
		gameOver = true;		

		//SessionTitle screen
		yield return StartCoroutine(showTitle("Session Over",3));
		
		Debug.Log("GAME OVER, Returning to menu");
		
		//Return to menu
		Application.LoadLevel(1);
	}

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

			// finger/dot indicator position
			Vector3 worldPos = Camera.main.ScreenToWorldPoint(touchPos);
			Vector2 fingerPos = new Vector2(worldPos.x, worldPos.z);

			//Start the fade spot
			StartCoroutine(spot.fadeFinger(fingerPos, -1));


		}
	}

}