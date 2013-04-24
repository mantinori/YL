using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TutorialFinger : MonoBehaviour {
	
	//The plane object used for the finger
	public GameObject finger;
	//The main class
	private GameManager gm;
	//The z position of where the edge of the screen is
	float edgeScreen;
	//How fast the finger should move
	float speed;
	//Margin of error of how close the finger should get before tapping
	float margin;
	
	//Start method
	void Start(){
		
		//Set up base values
		gm = GameManager.main;
		
		finger.renderer.enabled =false;
		edgeScreen = gm.transform.position.z-20;
		margin = .5f;
		speed = .75f;
		//Since star game has a camera with a larger orthographic size, it will need specific values 
		if(gm.SType == GameManager.SessionType.Star){
			edgeScreen-= 90;
			speed = 4;
			margin =3;
		}
		else if(gm.SType == GameManager.SessionType.Implicit){
			speed = 1.5f;
			margin =.75f;
		}
		else if(gm.SType == GameManager.SessionType.Stopping){
			speed = 1.25f;
			margin =.75f;
		}
		
		//Move it off the edge
		transform.position = new Vector3(gm.transform.position.x,5,edgeScreen);
	}
	
	//Have the finger leave the screen
	public IEnumerator exit(){
		
		//Continue movie down until it passes the edge of the screen
		while(transform.position.z>edgeScreen){
			transform.Translate(0,0,(speed*-1f));
			
			yield return new WaitForFixedUpdate();
		}
		
		//Hide the object
		transform.position = new Vector3(gm.transform.position.x,5f,edgeScreen);
		finger.renderer.enabled = false;
	}
	
	//Have the finger move to and tap different spots on the screen
	//locationsToPress(List<Vector3>): A ordered list of positions on the screen where the finger will move to
	//objsAffected(GameObject[]): An array of gameobjects that will be affected when the finger clicks on it. (For Star)
	public IEnumerator performAction(List<Vector3> locationsToPress, GameObject[] objsAffected){
		
		finger.renderer.enabled = true;
		
		int currentTarget=0;
		
		//Continue moving until it goes to all the points
		while(currentTarget<locationsToPress.Count){
			
			//Get the Vector to the next spot
			Vector3 toSpot = locationsToPress[currentTarget] - transform.position;
			
			toSpot.y = 0;
			
			//If the finger is in the margin of error, click the screen
			if(toSpot.magnitude<=margin){
				GameObject gO = null;
				
				if(objsAffected!= null)gO = objsAffected[currentTarget];
				
				//Start and wait for tap
				yield return StartCoroutine(click(gO));
				
				//Next target
				currentTarget++;
			}
			//Otherwise move towards the point
			else{
				toSpot.Normalize();
			
				transform.Translate(toSpot* speed);
			}
		
			yield return new WaitForFixedUpdate();
		}
	}
	
	//Have the finger move to and tap different spots on the screen
	//locationsToPress(List<Vector3>): A ordered list of positions on the screen where the finger will move to
	//objsAffected(GameObject[]): An array of gameobjects that will be affected when the finger clicks on it. (For Star)
	public IEnumerator moveTo(List<Vector3> locationsToMove){
		
		finger.renderer.enabled = true;
		
		int currentTarget=0;
		
		//Continue moving until it goes to all the points
		while(currentTarget<locationsToMove.Count){
			
			//Get the Vector to the next spot
			Vector3 toSpot = locationsToMove[currentTarget] - transform.position;
			
			toSpot.y = 0;
			
			//If the finger is in the margin of error, click the screen
			if(toSpot.magnitude<=margin){
				//Next target
				currentTarget++;
			}
			//Otherwise move towards the point
			else{
				toSpot.Normalize();
			
				transform.Translate(toSpot* speed);
			}
		
			yield return new WaitForFixedUpdate();
		}
	}
	
	
	//Method used to make the finger "tap the screen"
	//gO(Gameobject) - The gameObject that will be affect by this tap. Can be null. Currently used for Star
	private IEnumerator click(GameObject gO){
		yield return new WaitForSeconds(.05f);
		
		//Rotate back
		while(finger.transform.localEulerAngles.x<45){
			finger.transform.Rotate(6.5f,0,0);
			yield return new WaitForFixedUpdate();
		}
		
		//Start fading fingerprint if were not in associate
		if(GameManager.main.SType != GameManager.SessionType.Associate)
			StartCoroutine(gm.spot.fadeFinger(new Vector2(transform.position.x,transform.position.z),-1));
		
		//If there is an object
		if(gO!=null){
			//In Star games, make the object grayed out
			if(GameManager.main.SType == GameManager.SessionType.Star){
				gO.renderer.material = GameManager.main.materials[0];
			}
		}
		
		//Rotate forward
		while(finger.transform.localEulerAngles.x>0 && finger.transform.localEulerAngles.x<50){
			finger.transform.Rotate(-6.5f,0,0);
			yield return new WaitForFixedUpdate();
		}
		
		finger.transform.localRotation = Quaternion.identity;
	}
}
