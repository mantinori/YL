using UnityEngine;
using System;
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
	
	//The CsvManager class of the scene
	protected CsvManager csv;
	
	//The current event number
	protected int currentEventNum;
	
	//Is the player currently practicing
	protected bool practicing;
	
	//Total number of practice events
	protected int practiceSessionCount;
	
	//The current practice number
	protected int currentPractice;
	
	//What language the game should be presented in
	private string language = "english";
	
	//Increments either the event or practice number
	public void nextEvent(){
		if(!practicing){
			events[currentEventNum].Completed = true;
			currentEventNum++; 
		}
		else{
			practice[currentPractice].Completed = true;
			currentPractice++;
		}
	}
	
	//List of materials to be used in the scene
	public Material[] materials;
	
	//AudioSource to play sounds
	protected AudioSource audioSource;
	
	private List<Vector3> dragPoints;

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
	
	private float activeSlope;
	
	// Use this for initialization
	protected void Setup (SessionType s) {
		main = this;
		
		dragPoints = new List<Vector3>();
		
		if(PlayerPrefs.HasKey("-language")){
			language = PlayerPrefs.GetString("-language");
		}
		
		sType = s;
		
		activeSlope =0;
		
		xml = new XmlManager();
		
		csv = new CsvManager();
		
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
	public IEnumerator showTitle(string t, float duration){
		
		string title = t;
		
		state = GameManager.GameState.Title;
		
		screen.material.color = new Color(0f,0f,0f,0);
		
		if(language == "spanish"){
			if(title == "Tutorial") title = "Instrucciones";
			else if(title == "Practice") title ="Práctica";
			else if(title == "Test") title = "Juego";
			else if(title == "Session Over") title = "Sesión Completa";
		}
		
		text.text = title;
		
		screen.enabled = true;
		
		text.renderer.enabled = true;
					
		yield return new WaitForSeconds(duration);
		
		if(title != "Session Over") screen.enabled = false;
		
		text.renderer.enabled = false;
	}
	
	//Used to read in
	void LateUpdate () {	
		if(touching){
			if(dragPoints.Count == 0){
				dragPoints.Add(touchPos);
			}
			else{
				if(Vector3.Distance(dragPoints[dragPoints.Count-1], touchPos)>50){
					
					if(dragPoints.Count==1){
						dragPoints.Add(touchPos);
						activeSlope =  Vector3.Angle(new Vector3(1,0,0),
													new Vector3((dragPoints[dragPoints.Count-1].x - dragPoints[dragPoints.Count-2].x),
													(dragPoints[dragPoints.Count-1].y - dragPoints[dragPoints.Count-2].y),0));
					}
					else{
						float newAngle = Vector3.Angle(new Vector3(1,0,0), new Vector3( (touchPos.x-dragPoints[dragPoints.Count-1].x), (touchPos.y-dragPoints[dragPoints.Count-1].y),0));
					
						if(Mathf.Abs(activeSlope - newAngle)<60f){
							if(dragPoints.Count==4){
								if(Vector3.Distance(dragPoints[2], dragPoints[3])<(Vector3.Distance(dragPoints[0], dragPoints[1])+50))
									dragPoints[3] = touchPos;
							}
							else
								dragPoints[dragPoints.Count-1] = touchPos;
						}
						else{
							if(dragPoints.Count<4){
								dragPoints.Add(touchPos);
							
								activeSlope = Vector3.Angle(new Vector3(1,0,0),
														new Vector3((dragPoints[dragPoints.Count-1].x - dragPoints[dragPoints.Count-2].x),
														(dragPoints[dragPoints.Count-1].y - dragPoints[dragPoints.Count-2].y),0));
							}
						}
						
						//If there are 4 points
						if(dragPoints.Count==4){
							//Make sure the first and last line have about the same length (Margin of 100 pixels)
							if(Mathf.Abs(Vector3.Distance(dragPoints[0], dragPoints[1]) - Vector3.Distance(dragPoints[2], dragPoints[3]))<=100){
								
								//The 1st line of the Z (first drawn)
								float angOne = Vector3.Angle(new Vector3(1,0,0), new Vector3((dragPoints[1].x - dragPoints[0].x),(dragPoints[1].y - dragPoints[0].y),0));
								if((dragPoints[1].y - dragPoints[0].y)/(dragPoints[1].x - dragPoints[0].x) <0)
									angOne *= -1;
								//The 3rd line of the Z (last drawn)
								float angTwo = Vector3.Angle(new Vector3(1,0,0), new Vector3((dragPoints[3].x - dragPoints[2].x),(dragPoints[3].y - dragPoints[2].y),0));
								if((dragPoints[3].y - dragPoints[2].y)/(dragPoints[3].x - dragPoints[2].x) <0)
									angTwo *= -1;
								//The middle line of the Z
								float angSlash = Vector3.Angle(new Vector3(1,0,0), new Vector3((dragPoints[1].x - dragPoints[2].x),(dragPoints[1].y - dragPoints[2].y),0));
								if((dragPoints[1].y - dragPoints[2].y)/(dragPoints[1].x - dragPoints[2].x) <0)
									angSlash *= -1;
								
								//Make sure the first and last line are draw at about the same angle(margin of 30'),
								//and if the first and middle line are not given the sameish angle(margin of 10'), then its a Z
								if(Mathf.Abs(angOne - angTwo)<30 && Mathf.Abs(angOne - angSlash)>10){
									//Delete the current task
									PlayerPrefs.DeleteKey("-currentTask");
									
									NeuroLog.Debug("READ 'Z' SWIPE, HALTING TASK");
									
									//Write out the incompleted xml
									xml.WriteOut(false);
									
								//Go back to the menu
									Application.LoadLevel("menu");
								}
							}
						}
					}
				}
				
				//For debuging, will show swipe in scene screen
				/*
				Color c = Color.white;
				for(int i=0;i<dragPoints.Count;i++){
					if(i==1) c = Color.red;
					else if(i==2) c = Color.green;
					else if (i == 3) c = Color.blue;
					
					if(i+ 1 == dragPoints.Count){
						Debug.DrawLine(new Vector3(((dragPoints[i].x/Screen.width)*53 - 26.5f), 8,((dragPoints[i].y/Screen.height)*30 - 15f)),
										new Vector3(((touchPos.x/Screen.width)*53 - 26.5f), 8,((touchPos.y/Screen.height)*30 - 15f)), c);
					}
					else{
						Debug.DrawLine(new Vector3(((dragPoints[i].x/Screen.width)*53 - 26.5f), 8,((dragPoints[i].y/Screen.height)*30 - 15f)),
										new Vector3(((dragPoints[i+1].x/Screen.width)*53 - 26.5f), 8,((dragPoints[i+1].y/Screen.height)*30 - 15f)), c);
					}
				}
				*/
			}
		}
		//If the player isn't touching the screen, reset the dragPoints
		else if( dragPoints.Count>0){
			dragPoints = new List<Vector3>();
			activeSlope = 0;
		}
	}
}