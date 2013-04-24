using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//The Base Class for a Base Manager
public class GameManager : MonoBehaviour {
	
	//Static getter for this class
	public static GameManager main = null;
	
	//The Screen in the scene
	protected Renderer screen;
	
	//Textmesh used to writeout title cards
	public TextMesh text;
	
	//Plane used to present the border of the scene
	public GameObject border;
	//The simple black bar border texture
	public Texture simpleBorderImage;
	//The border texture that has the eyeBalls in the center. (Used in Spatial)
	public Texture eyeBorderImage;
	
	//The List of events the player must complete in this session
	protected List<EventStats> events;
	public List<EventStats> Events{
		get{return events;}
	}
	
	//List of practice events the player will have to complete before starting the main task
	protected List<EventStats> practice;
	public List<EventStats> Practice{
		get{return practice;}
	}
	
	//The finger image in the scene, used for tutorials
	public TutorialFinger tFinger;
	
	//The fading black circle, showing where the player pressed
	public FadingFingerprint spot;
	
	//The XmlManager class of the scene
	protected XmlManager xml;
	
	//The current event number
	protected int currentEventNum;
	
	//Is the player currently practicing
	protected bool practicing;
	
	//Total number of practice events
	protected int practiceSessionCount;
	
	//The current practice number
	protected int currentPractice;
	
	//Increments either the event or practice number
	public void nextEvent(){
		if(!practicing)
			currentEventNum++; 
		else 
			currentPractice++;
	}
	
	//List of materials to be used in the scene
	public Material[] materials;
	
	//AudioSource to play sounds
	protected AudioSource audioSource;
	
	//Which gametype is running
	public enum SessionType{Spatial, Inhibition, Star, Implicit, Associate, Stopping};
	protected SessionType sType;
	public SessionType SType{
		get{return sType;}
		set{sType = value;}
	}
	
	//What is the current game state
	//Title:A title card is up
	//Tutorial:The tutorial is playing
	//Encoding: The player is presented an object(s) to remember
	//Delay: Time period in between when the objects are presented and the player should respond
	//Probe: The period where the player can repond
	//ITI: Short time(.5 seconds) in between a player's response and the next trial
	public enum GameState{Title,Tutorial,Encoding,Delay,Probe,ITI};
	protected GameState state;
	public GameState State{
		get{return state;}
	}
	
	//When the game turned into probing state
	protected float startTime;
	
	//If the player is currently touching the screen
	protected bool touching=false;
	
	//Where did the player touch the screen
	protected Vector3 touchPos = Vector3.zero;	
	
	// Use this for initialization
	protected void Setup (SessionType s) {
		main = this;
		
		sType = s;
		
		xml = new XmlManager();
		
		state = GameState.Title;	
		
		text.renderer.enabled = false;	
		
		screen = GameObject.Find("Screen").GetComponent<Renderer>();
	
		currentEventNum = 0;
		
		currentPractice = 0;
		
		practicing =true;
		
		practiceSessionCount = 0;
		
		startTime = 0;
		
		audioSource = GetComponent<AudioSource>();
		
		practice = new List<EventStats>();
		
		events = new List<EventStats>();
		
		border.renderer.material.mainTexture = simpleBorderImage;
		
		border.SetActive(true);
	}
	
	//Generate the main events
	protected virtual void generateEvents() {}

	//Generate practice pitches
	protected virtual void generatePractice() {}
	
	//Run the tutorial
	protected virtual IEnumerator runTutorial(){yield return null;}
	
	//Main method
	protected virtual IEnumerator runSession(){yield return null;}
	
	//Randomizes the list of events
	protected virtual void randomizeEvents(){}
	
	//Plays a audio cue to indicate the player pressed the screen
	public void playSound(float p){
		audioSource.pitch = p;
		
		audioSource.Play();
	}
	
	//Show the title card before a new section
	public IEnumerator showTitle(string title, float duration){
		
		state = GameManager.GameState.Title;
		
		screen.material.color = new Color(0f,0f,0f,0);
		
		text.text = title;
		
		screen.enabled = true;
		
		text.renderer.enabled = true;
					
		yield return new WaitForSeconds(duration);
		
		if(title != "Session Over") screen.enabled = false;
		
		text.renderer.enabled = false;
	}
}