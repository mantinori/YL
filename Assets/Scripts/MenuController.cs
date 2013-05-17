using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using LumenWorks.Framework.IO.Csv;

//Script used to control all activity in the menu scene.
public class MenuController : MonoBehaviour {
	
	//List of the task button in the scene(Should be 6)
	public List<UIButton> taskButtons;
	
	public UIPanel menu;
	public UIPanel brightness;
	
	//List of other buttons on the screen by default
	public ButtonResponder abortButton;
	public ButtonResponder quitButton;
	
	//Buttons uesd in the warning message when the player presses the abortButton
	public ButtonResponder yesButton;
	public ButtonResponder noButton;
	
	//Texted used for the warning message
	public UILabel warning;
	//Name of the current player
	public UILabel userName;
	
	public UIScrollBar gammaBar;
	private Color ambience;
	
	public UILabel brightnessText;
	public ButtonResponder brightnessButton;
	
	public UILabel returnText;
	public ButtonResponder returnButton;
	
	//What language should be shown
	private string language ="english";
	
	// Use this for initialization
	void Start () {
		
		if(PlayerPrefs.HasKey("-ambience")){
			float val = PlayerPrefs.GetFloat("-ambience");
			
			ambience.b = val;
			ambience.r= val;
			ambience.g = val;
			
			gammaBar.scrollValue = val;
			
			RenderSettings.ambientLight = ambience;
		}
		
		if(PlayerPrefs.HasKey("-language"))
			language = PlayerPrefs.GetString("-language");
		
		//Set the response for all the buttons to methods in this class
		foreach(UIButton b in taskButtons){
			b.GetComponent<ButtonResponder>().response = beginTask;
			if(language == "spanish")
				b.GetComponentInChildren<UILabel>().text = b.GetComponentInChildren<UILabel>().text.Replace("Game", "Juego"); 
		}
		quitButton.response = quitbuttonPressed;
		abortButton.response = displayWarning;
		returnButton.response = returnToMenu;
		brightnessButton.response = goToBirghtness;
		
		if(language == "spanish"){
			abortButton.GetComponentInChildren<UILabel>().text = "Cambiar usuario"; 
			quitButton.GetComponentInChildren<UILabel>().text = "Salir"; 
			yesButton.GetComponentInChildren<UILabel>().text = "Sí";
			warning.text = " La sesión actual será borrada,\nesta seguro que quiere continuar?";
			((BoxCollider)abortButton.collider).size = new Vector3(200,40,0);
			abortButton.GetComponentInChildren<UISlicedSprite>().transform.localScale = new Vector3(215,35,1);
			returnText.text = "Volver";
			brightnessText.text = "Brillo";
		}
		
		yesButton.response = resetDevice;
		yesButton.gameObject.SetActive(false);
		noButton.response = removeWarning;
		noButton.gameObject.SetActive(false);
		
		warning.enabled =false;
		
		//Set up the scene
		setupScene();
	}
	
	private void returnToMenu(GameObject go){
		menu.gameObject.SetActive(true);
		brightness.gameObject.SetActive(false);
		
		PlayerPrefs.SetFloat("-ambience", ambience.b);
	}
	
	private void goToBirghtness(GameObject go){
		menu.gameObject.SetActive(false);
		brightness.gameObject.SetActive(true);
		
		gammaBar.scrollValue = ambience.b;
	}
	
	//Sets up the scene based on the saved PlayerPrefs
	private void setupScene(){
		
		//Set the userName text to the player's name
		userName.text = PlayerPrefs.GetString("-childID");
		
		//Check to see if were coming back from a session
		if(PlayerPrefs.HasKey("-currentTask")){
			
			//If so update the PlayerPrefs to reflect it
			string task ="-t" + PlayerPrefs.GetInt("-currentTask").ToString();
		
			PlayerPrefs.SetString(task,"true");
			
			PlayerPrefs.DeleteKey("-currentTask");
		}
		
		int latestTask=0;
		//See how far the player has gotten so far
		if(PlayerPrefs.GetString("-t1")=="true"){
			latestTask++;
			if(PlayerPrefs.GetString("-t2")=="true"){
				latestTask++;
				if(PlayerPrefs.GetString("-t3")=="true"){
					latestTask++;
					if(PlayerPrefs.GetString("-t4")=="true"){
						latestTask++;
						if(PlayerPrefs.GetString("-t5")=="true"){
							latestTask++;
							if(PlayerPrefs.GetString("-t6")=="true"){
								latestTask++;
								if(PlayerPrefs.GetString("-t7")=="true")latestTask++;
							}
						}
					}
				}
			}
		}
		
		//Update the list of task buttons
		for(int i =0; i<7;i++){
			//If the player's latest task is greater than i, mark it as complete by turning it green but still keeping it active
			if(i<latestTask){
				taskButtons[i].transform.GetComponentInChildren<UILabel>().color = Color.white;
				
				taskButtons[i].transform.GetComponentInChildren<UISlicedSprite>().color = Color.green;
				
				taskButtons[i].isEnabled = true;
			}
			//If the player's latest task is equal to i, keep it gray and active
			else if(i==latestTask){
				
				taskButtons[i].isEnabled = true;
				
				taskButtons[i].transform.GetComponentInChildren<UILabel>().color = Color.white;
			}
			//Otherwise, make it transparent and disabled
			else{
				taskButtons[i].transform.GetComponentInChildren<UILabel>().color = new Color(1f,1f,1f,.5f);
				
				taskButtons[i].transform.GetComponentInChildren<UISlicedSprite>().color =  new Color(.75f,.75f,.75f,.25f);
				
				taskButtons[i].isEnabled = false;
			}
		}
		
		//If the player has completed all the tasks, change the "abort" button to the "done" button
		if(latestTask==7){
			abortButton.transform.GetComponentInChildren<UISlicedSprite>().color = Color.green;
			((BoxCollider)abortButton.collider).size = new Vector3(165,40,0);
			abortButton.GetComponentInChildren<UISlicedSprite>().transform.localScale = new Vector3(175,35,1);
			if(language == "spanish")
				abortButton.transform.GetComponentInChildren<UILabel>().text ="Cambiar ID";
			else
				abortButton.transform.GetComponentInChildren<UILabel>().text = "Change User";
			abortButton.transform.localScale = new Vector3(1f,1f,1);
			//abortButton.transform.localPosition = new Vector3(-250,-310,0);
		}
	}
	
	//Old xml version: If a task button was pressed, find out what game type it is then start the game 
	/*private void beginTask(GameObject o){
		
		//String the fileName
		string fileName = o.name +".xml";
		
		//Get the taskNumber
		int num = int.Parse(o.name.Replace("task",""));
		
		XmlDocument xml = new XmlDocument();
		
		//Check to see if the file exists in Dropbox
		if(File.Exists(Path.Combine(XmlManager.SessionFilesPath, fileName))){
			Debug.Log("Attempting to read from Dropbox folder");
			xml.Load(Path.Combine(XmlManager.SessionFilesPath, fileName));
		}
		//If not, try the local bundle
		else{
			NeuroLog.Error("Attempting to read from local Resources");
			
			string path = "session_files/" + o.name;
			
			Debug.Log(path);
			
			try{		
				TextAsset sessionData = Resources.Load(path) as TextAsset;
		
				TextReader reader = new StringReader(sessionData.text);
			
				xml.Load(reader);
			}
			//Don't try to load another scene if we can't find the file
			catch{
				NeuroLog.Error("Unable to find local task file");
				
				return;
			}
		}
		
		//Get the session type of the task
		string type = xml.SelectSingleNode("/session").Attributes["type"].Value;
		
		PlayerPrefs.SetInt("-currentTask", num);
		
		//Spatial
		if(type =="0")
			Application.LoadLevel("spatial");
		//Inhibition
		else if(type =="1")
			Application.LoadLevel("inhibition");
		//Star
		else if(type=="2")
			Application.LoadLevel("star");
		//Implicit
		else if(type =="3")
			Application.LoadLevel("implicit");
		//Associate
		else if(type =="4")
			Application.LoadLevel("associate");
		//Stopping
		else if(type =="5")
			Application.LoadLevel("stopping");
	}*/
	
	//If a task button was pressed, find out what game type it is then start the game
	private void beginTask(GameObject o){
		
		//String the fileName
		string fileName = o.name +".csv";
		
		//Get the taskNumber
		int num = int.Parse(o.name.Replace("task",""));
		
		StreamReader sR;
		
		try{
			if(File.Exists(Path.Combine(CsvManager.SessionFilesPath, fileName))){
				sR = new StreamReader(Path.Combine(CsvManager.SessionFilesPath, fileName));
			}
			else{
				NeuroLog.Error("Unable to find task file");
				
				return;
			}/*
			else{
				NeuroLog.Log("Attempting to read from local Resources");
					
				TextAsset sessionData = Resources.Load("session_files/" + sessionXML) as TextAsset;
		
				TextReader reader = new StringReader(sessionData.text);
				
				sR = new StreamReader();
			}
			*/
		}
		catch{
			
			NeuroLog.Error("Unable to find task file");
				
			return;
		}
		
		PlayerPrefs.SetInt("-currentTask", num);
		
		//Check the headers to make sure its the right type of file
		using (CsvReader csv = new CsvReader(sR,true)){
        	List<string> headers = new List<string>(csv.GetFieldHeaders());
			
			for(int i=0; i<headers.Count;i++){
				headers[i] = headers[i].ToLower();
			}
			
			if(headers.Contains("dots") && headers.Contains("delay")){
				NeuroLog.Log("Loading Spatial game");
				Application.LoadLevel("spatial");
			}
			else if(headers.Contains("side") && headers.Contains("color")){
				NeuroLog.Log("Loading Inhibition game");
				Application.LoadLevel("inhibition");
			}
			else if(headers.Contains("blocknum") && headers.Contains("type") && headers.Contains("position") && headers.Contains("rotation")){
				NeuroLog.Log("Loading Star game");
				Application.LoadLevel("star");
			}
			else if(headers.Contains("blocknum") && headers.Contains("dot")){
				NeuroLog.Log("Loading Implicit game");
				Application.LoadLevel("implicit");
			}
			else if(headers.Contains("go") && headers.Contains("dot")){
				NeuroLog.Log("Loading Stopping game");
				Application.LoadLevel("stopping");
			}
			else if(headers.Contains("target") && headers.Contains("stimuli")){
				NeuroLog.Log("Loading Associate game");
				Application.LoadLevel("associate");
			}
			else{
				NeuroLog.Log("Listed headers in task " +num + "do not match up with any of the current programs");
			}
		}
	}
	
	
	//Bring up warning message
	private void displayWarning(GameObject o){
		
		//Show the warning components
		warning.enabled =true;
		yesButton.gameObject.SetActive(true);
		noButton.gameObject.SetActive(true);
		
		//Hide the regular components 
		foreach(UIButton b in taskButtons){
			b.gameObject.SetActive(false);
		}
		
		quitButton.gameObject.SetActive(false);
		abortButton.gameObject.SetActive(false);
		brightnessButton.gameObject.SetActive(false);
	}
	
	//Hides the warning, and resets the scene
	private void removeWarning(GameObject o){
		//Hide the warning components
		warning.enabled =false;
		yesButton.gameObject.SetActive(false);
		noButton.gameObject.SetActive(false);
		
		//Make the regular components reappear
		foreach(UIButton b in taskButtons){
			b.gameObject.SetActive(true);
		}
		
		quitButton.gameObject.SetActive(true);
		abortButton.gameObject.SetActive(true);
		brightnessButton.gameObject.SetActive(true);
	}
	
	//Reset PlayerPrefs and shut down
	private void resetDevice(GameObject o){
		
		NeuroLog.Log("Resetting the device");
		
		//Delete all saved player prefs
		PlayerPrefs.DeleteAll();
		
		//Quit
		Application.Quit();
	}
	
	//If quit button is pressed, just simply end the program
	private void quitbuttonPressed(GameObject o){
		Application.Quit();
	}
	
	//Check for escape button
	void LateUpdate () {	
		if(Input.GetKeyDown(KeyCode.Escape)){
			if(Screen.fullScreen){
				int width= Screen.currentResolution.width-200;
				int height = Screen.currentResolution.height;
		
				height = Mathf.RoundToInt(width/1.77777778f);
		
				Screen.SetResolution(width,height,false);
			}
			else{
				int width= 1366;
				int height = 768;
				
				Screen.SetResolution(width,height,true);
			}
		}
		if(ambience.b!= gammaBar.scrollValue){
			ambience.b = gammaBar.scrollValue;
			ambience.r = gammaBar.scrollValue;
			ambience.g = gammaBar.scrollValue;
			
			RenderSettings.ambientLight = ambience;
		}
	}
}