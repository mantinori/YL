using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

//Script used to control all activity in the menu scene.
public class MenuController : MonoBehaviour {
	
	//List of the task button in the scene(Should be 6)
	public List<UIButton> taskButtons;
	
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
	
	// Use this for initialization
	void Start () {
		
		//Set the response for all the buttons to methods in this class
		foreach(UIButton b in taskButtons){
			b.GetComponent<ButtonResponder>().response = beginTask;
		}
		quitButton.response = quitbuttonPressed;
		abortButton.response = displayWarning;
		
		yesButton.response = resetDevice;
		yesButton.gameObject.SetActive(false);
		noButton.response = removeWarning;
		noButton.gameObject.SetActive(false);
		
		warning.enabled =false;
		
		//Set up the scene
		setupScene();
	}
	
	//Sets up the scene based on the saved PlayerPrefs
	private void setupScene(){
		
		//Set the userName text to the player's name
		userName.text = PlayerPrefs.GetString("-player");
		
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
				
				taskButtons[i].isEnabled = false;
			}
		}
		
		//If the player has completed all the tasks, change the "abort" button to the "done" button
		if(latestTask==6){
			abortButton.transform.GetComponentInChildren<UISlicedSprite>().color = Color.green;
			abortButton.transform.GetComponentInChildren<UILabel>().text = "DONE";
			abortButton.transform.localScale = new Vector3(1.5f,1.5f,1);
			abortButton.transform.localPosition = new Vector3(-250,-310,0);
		}
	}
	
	//If a task button was pressed, find out what game type it is then start the game
	private void beginTask(GameObject o){
		
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
	}
	
	//Reset PlayerPrefs and shut down
	private void resetDevice(GameObject o){
		
		NeuroLog.Log("Resetting the device");
		
		if(PlayerPrefs.GetString("-testing") !="t"){
			//Get the players file off of Dropbox
			XmlDocument xml = new XmlDocument();
			
			if(File.Exists(Path.Combine(XmlManager.PlayerSpecificPath,"players.xml"))){
				Debug.Log("Attempting to read from Dropbox folder");		
				xml.Load(Path.Combine(XmlManager.PlayerSpecificPath,"players.xml"));
			}
			//Can't find? Use the local bundle one
			else{
				Debug.Log("Attempting to read from local build folder");
				try{
					TextAsset sessionData = Resources.Load("players") as TextAsset;
			
					TextReader reader = new StringReader(sessionData.text);
				
					xml.Load(reader);
				}
				catch{
					NeuroLog.Error("Unable to find local task file");
					
					return;
				}
			}
			
			//Local the current player's node with his/her region info
			string state = PlayerPrefs.GetString("-state");
			string region = PlayerPrefs.GetString("-region");
			string subregion = PlayerPrefs.GetString("-subregion");
			string player = PlayerPrefs.GetString("-player");
			
			XmlNode playersNode = xml.SelectSingleNode("/players/"+state+"/"+ region +"/"+subregion);
			
			//Find the child in the subdivision and set his device back to " "
			foreach(XmlNode n in playersNode.ChildNodes){
				if(n.Attributes["name"].Value == player){
					n.Attributes["device"].Value = "";
					
					break;
				}
			}
			
			//Save the updated file
			xml.Save(Path.Combine(XmlManager.PlayerSpecificPath,"players.xml"));
		}
		//Delete all saved player prefs
		PlayerPrefs.DeleteAll();
		
		//Quit
		Application.Quit();
	}
	
	//If quit button is pressed, just simply end the program
	private void quitbuttonPressed(GameObject o){
		Application.Quit();
	}
}