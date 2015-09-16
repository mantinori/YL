using UnityEngine;
using System.Collections;

//Class used to control the black spot used to indicate where the player pressed
public class FadingFingerprint : MonoBehaviour {
	
	// Hide the finger print at initializtion
	void Start () {
		GetComponent<Renderer>().enabled = false;
	}
	
	//Method used to fade the spot
	//pos(vector2): Where the player touched the screen
	//dot(int): Which section did the player touch(Program will calculate it if value is -1). For Spatial
	public IEnumerator fadeFinger(Vector2 pos, int dot){
		//If the program is playing a Star game, just play the sound.
		if (GameManager.main.SType == GameManager.SessionType.Star ){
			GameManager.main.playSound(1);
		}
		//Otherwise
		else{
			//Make the spot reappear where the player touched the screen
			if(GameManager.main.SType == GameManager.SessionType.Spatial)
				GetComponent<Renderer>().material.color = Color.white;
			else
				GetComponent<Renderer>().material.color = Color.black;
			
			transform.position = new Vector3(pos.x, 2f, pos.y);
			GetComponent<Renderer>().enabled = true;
			float a = 1;
			
			Color c = GetComponent<Renderer>().material.color;
			
			float p =1;
			
			//Spatial pitch calculator
			if(GameManager.main.SType == GameManager.SessionType.Spatial){
				if(dot==-1){
					p = .8f;
			
					if(pos.x>5) p+=.1f;
					else if(pos.x>-5f) p+=.05f;
			
					if(pos.y<-5) p+=.3f;
					else if(pos.y<5) p+=.15f;
				}
				else{
					p = 1 + ((dot-5)* .05f);
				}
			}
			//Inhibition pitch calculator
			else if(GameManager.main.SType == GameManager.SessionType.Inhibition){
				if(pos.x>0)
					p = 1.2f;
				else 
					p =.8f;
			}
			else if(GameManager.main.SType == GameManager.SessionType.Implicit){

				if(pos.x<0 && pos.y>0) p =.85f;
				else if(pos.x>0 && pos.y>0) p=1f;
				else if(pos.x>0 && pos.y<0) p=1.15f;
				else p = 1.3f;
			}
			else if(GameManager.main.SType == GameManager.SessionType.Associate){
				if(dot ==0) p =.75f;
				else p=1.25f;
			}else if(GameManager.main.SType ==  GameManager.SessionType.Stopping){
				if(dot ==0) p =.75f;
				else p=1f;
			}
			//Have the game manager play the pitch altered sound
			GameManager.main.playSound(p);
			
			//Loop until the spot is fully transparent
			while(a>0){
				
				a-=.05f;
				if(a<0)	a=0;
				c.a = a;
				
				GetComponent<Renderer>().material.color = c;
				
				yield return new WaitForFixedUpdate();
			}
		
			GetComponent<Renderer>().enabled = false;
		}
	}
}