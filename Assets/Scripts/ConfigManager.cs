using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using LumenWorks.Framework.IO.Csv;

//Config Controller
public class ConfigManager : MonoBehaviour {
	
	//The players file name
	private string playersFile = "players";
	private bool fileOnDropbox;
	
	//Popuplists on the screen
	public UIPopupList stateSelection;
	public UIPopupList subDivisionOne;
	public UIPopupList subDivisionTwo;
	public UIPopupList players;
	
	//Only need the english checkbox
	public UICheckbox english;
	public UICheckbox spanish;
	
	//Other elements on the screen
	public UITexture background;
	public UIButton confirmButton;
	public UILabel buttonText;
	public UILabel message;
	
	//For testing purposes, allow someone to just write in a name
	public UIInput testInput;
	public UILabel testingLabel;
	public UICheckbox testingCheckbox;
	
	//The actual dropdown list
	GameObject ddL;
	bool setDDL=false;
	
	//The XmlDocument
	private XmlDocument xml;
	
	//The final ok
	private bool finalGo;
	
	//If the config was properly saved
	private bool configSaved=false;
	
	private string testName="";
	
	// Use this for initialization
	void Start () {
		
		//Resize the window to fit the monitor
		if(!Screen.fullScreen) Screen.SetResolution(1366,768,true);
		
		//Have the static xmlmanager check to make sure the folders are properly set up
		if(!CsvManager.CheckFolders()){
			//If a folder(s) is missing, exit out of the game
			NeuroLog.Error("Unable to start program due to missing folder");
			Application.Quit();
		}
		
		//Remove any saved previously saved scene
		PlayerPrefs.DeleteKey("-currentTask");
		
		bool needConfig=false;
		
		bool foundFile = true;
		
		finalGo = false;
		//Hide all the elements on the screen until were sure the files are set up
		stateSelection.gameObject.SetActive(false);
		subDivisionOne.gameObject.SetActive(false);
		subDivisionTwo.gameObject.SetActive(false);
		english.gameObject.SetActive(false);
		spanish.gameObject.SetActive(false);
		players.gameObject.SetActive(false);
		testingCheckbox.gameObject.SetActive(false);
		
		testingLabel.enabled = false;
		confirmButton.gameObject.SetActive(false);
		confirmButton.GetComponent<ButtonResponder>().response = buttonPressed;
		
		message.enabled = false;
		
		background.color = Color.black;
		
		//Set up the XmlDocument variable
		xml = new XmlDocument();
		
		
		//Try to load the players file. If it fails, exit out
		try{
			//Try on Dropbox
			Debug.Log("Attempting to read from Dropbox folder");
			xml.Load(Path.Combine(XmlManager.PlayerSpecificPath, playersFile+".xml"));
		}
		catch{
			NeuroLog.Error("Unable to find dropbox player file.");
			//Try local bundle if not on dropbox
			try{		
				TextAsset sessionData = Resources.Load(playersFile) as TextAsset;
				
				TextReader reader = new StringReader(sessionData.text);
			
				xml.Load(reader);
			}
			catch{
				NeuroLog.Error("Unable to find local player file");
			
				foundFile =false;
			}
		}
		
		//If the program found the file
		if(foundFile){
			string p = PlayerPrefs.GetString("-player");
			
			string testing = PlayerPrefs.GetString("-testing");
			
			//If there is no player saved in PlayerPrefs, signal that we need to config the device
			if(p == ""){
				needConfig = true;
					
				NeuroLog.Log("No player set to the device.");
			}
			else if(testing=="t"){
				needConfig = false;
				
				NeuroLog.Log("Using Testing Player");
			}
			//Otherwise, first make sure the player hasn't been assigned to a different machine in the meantime
			else{
				XmlNode node = xml.SelectSingleNode("/players/"+PlayerPrefs.GetString("-state")+"/"+PlayerPrefs.GetString("-region")+"/"+PlayerPrefs.GetString("-subregion"));
		
				foreach(XmlNode x in node.ChildNodes){
					if(x.Attributes["name"].Value == p){
						node = x;
						break;
					}
				}
				
				//If the player's saved device matches the current machine, we don't need to configure
				if(node.Attributes["device"].Value == System.Environment.MachineName){
					needConfig = false;
					
					NeuroLog.Log(p+ " set to this device.");
				}
				//Otherwise we do
				else{
					needConfig = true;
					
					PlayerPrefs.DeleteAll();
					
					NeuroLog.Error(p+ " set to a different device. Removing him from this device");
				}
			}
			
			//If the program needs to be configured, make all the GUI elements appear
			if(needConfig){
				//Set up the players file
				if(SetupPlayersXML()){
					stateSelection.gameObject.SetActive(true);
					subDivisionOne.gameObject.SetActive(true);
					subDivisionTwo.gameObject.SetActive(true);
					players.gameObject.SetActive(true);
					
					testingLabel.enabled = true;
					testingCheckbox.gameObject.SetActive(true);
		
					english.gameObject.SetActive(true);
					spanish.gameObject.SetActive(true);
				
					background.color = Color.white;
				}
				else{
					NeuroLog.Error("Failed to initialize config scene.");
				
					message.text = "Error! Unable to Run Config Program!";
					message.enabled = true;
				}
			}
			//If the program doesn't need to be configured just go to the next scene
			else{
				if(!PlayerPrefs.HasKey("-language"))
					PlayerPrefs.SetString("-language", "english");
				
				Application.LoadLevel("menu");
			}
		}
		//Tell the player the file was unable to be loaded
		else{
			NeuroLog.Error("Failed to initialize config scene.");
				
			message.text = "Error! Unable to Run Config Program!";
			message.enabled = true;
		}
	}
	
	//Make sure there is at least one sub division
	public bool SetupPlayersXML()
	{
		XmlNode currentNode = (xml.SelectSingleNode("/players"));
		
		foreach(XmlNode node in currentNode.ChildNodes){
			stateSelection.items.Add(node.Name);
		}
		
		//Good Exit, found at least one subdivision
		if(stateSelection.items.Count>1){
			return true;
		}
		else return false;
	}
	
	//If a button is pressed
	public void buttonPressed(GameObject gO){
		//If the config has been saved, quit once the button is pressed
		if(configSaved){
			NeuroLog.Log("Exiting Now");
			
			Application.Quit();
		}
		//If the config hasn't been saved
		else{
			string prevDevice="";
			XmlNode playerNode=null;
			
			if(testName==""){
				//Reload the file to make sure there were no changes
				if(File.Exists(Path.Combine(XmlManager.PlayerSpecificPath,playersFile + ".xml"))){
					Debug.Log("Attempting to read from Dropbox folder");		
					xml.Load(Path.Combine(XmlManager.PlayerSpecificPath,playersFile + ".xml"));
				}
				else{
					Debug.Log("Attempting to read from local build folder");
					try{
						TextAsset sessionData = Resources.Load(playersFile) as TextAsset;
		
						TextReader reader = new StringReader(sessionData.text);
			
						xml.Load(reader);
					}
					catch{
						NeuroLog.Error("Unable to find local task file");
				
						return;
					}
				}
			
				//Get the player's node
				playerNode = (xml.SelectSingleNode("/players/"+stateSelection.selection+"/"+subDivisionOne.selection+"/"+subDivisionTwo.selection));
			
				foreach(XmlNode x in playerNode.ChildNodes){
					if(x.Attributes["name"].Value == players.selection){
						playerNode = x;
						break;
					}
				}
				prevDevice = playerNode.Attributes["device"].Value;
			}
			else{
				NeuroLog.Log("Using testName");
			}
			
			//If the player has been previously assigned to a different device, show a warning before saving
			if(!finalGo && prevDevice!= ""){
				
				message.text = players.selection + " already assigned to " + prevDevice+".\nSave Anyway?";
				
				confirmButton.gameObject.transform.localPosition = new Vector3(0,-50,0);
								
				buttonText.text = "YES";
				
				finalGo = true;
			}
			//If the warning has been shown or the player hasn't been set to a different device
			else{
				
				//Set up the task statuses
				if(testingCheckbox.isChecked){
					PlayerPrefs.SetString("-t1", "true");
					PlayerPrefs.SetString("-t2", "true");
					PlayerPrefs.SetString("-t3", "true");
					PlayerPrefs.SetString("-t4", "true");
					PlayerPrefs.SetString("-t5", "true");
					PlayerPrefs.SetString("-t6", "true");
					PlayerPrefs.SetString("-t7", "true");
				}
				else{
					PlayerPrefs.SetString("-t1", "false");
					PlayerPrefs.SetString("-t2", "false");
					PlayerPrefs.SetString("-t3", "false");
					PlayerPrefs.SetString("-t4", "false");
					PlayerPrefs.SetString("-t5", "false");
					PlayerPrefs.SetString("-t6", "false");
					PlayerPrefs.SetString("-t7", "false");
				}
				
				if(english.isChecked){
					PlayerPrefs.SetString("-language", "english");
				}
				else{
					PlayerPrefs.SetString("-language", "spanish");
				}
				
				string name ="";
				
				//Save the player's info
				if(testName==""){
					
					name = players.selection.Replace(" ", "");
					PlayerPrefs.SetString("-player", players.selection);
					PlayerPrefs.SetString("-testing", "f");
					PlayerPrefs.SetString("-state", stateSelection.selection);
					PlayerPrefs.SetString("-region", subDivisionOne.selection);
					PlayerPrefs.SetString("-subregion", subDivisionTwo.selection);
				
			
					//Update the players file to say the player is assigned to this device
					playerNode = (xml.SelectSingleNode("/players/"+stateSelection.selection+"/"+subDivisionOne.selection+"/"+subDivisionTwo.selection));
				
					foreach(XmlNode x in playerNode.ChildNodes){
						if(x.Attributes["name"].Value == players.selection){
							playerNode = x;
							break;
						}
					}
			
					playerNode.Attributes["device"].Value = System.Environment.MachineName;
				
					//Save the file
					xml.Save(Path.Combine(XmlManager.PlayerSpecificPath, "players.xml"));
				}
				else{
					name = testName.Replace(" ", "");
					
					PlayerPrefs.SetString("-player", testName);
					PlayerPrefs.SetString("-testing", "t");
					PlayerPrefs.SetString("-state", "n/a");
					PlayerPrefs.SetString("-region", "n/a");
					PlayerPrefs.SetString("-subregion", "n/a");
				}
				
				Debug.Log(Path.Combine(CsvManager.PlayerSpecificPath, name+"_Criterion.csv"));
				
				using(StreamWriter writer = new StreamWriter(Path.Combine(CsvManager.PlayerSpecificPath, name+"_" + System.Environment.MachineName+"_Criterion.csv"))){
					writer.WriteLine("TaskNum, CriterionScore, NumofPractice, NumofEvents, AccuracyofPresented , AccuracyofResponses, NumResponsesBasal");
					
					for(int i = 0;i<8;i++){
						writer.WriteLine(i+", 0, ., ., ., ., .");
					}
				}
				
				message.text = "Config info saved!";
				
				buttonText.text = "CLOSE";
				
				configSaved = true;
				//Hide the dropdown menus
				stateSelection.gameObject.SetActive(false);
				english.gameObject.SetActive(false);
				spanish.gameObject.SetActive(false);
				subDivisionOne.gameObject.SetActive(false);
				subDivisionTwo.gameObject.SetActive(false);
				testingCheckbox.gameObject.SetActive(false);
				players.gameObject.SetActive(false);
				testInput.gameObject.SetActive(false);	
				testingLabel.enabled = false;
			}
		}
	}
		
	//Update the region list if the state changes
	void updateState(string selected){
		subDivisionOne.items.Clear();
		
		if(selected != "Select State"){
			subDivisionOne.items.Add("Select Region");
			
			XmlNode currentNode = (xml.SelectSingleNode("/players/"+stateSelection.selection));
		
			foreach(XmlNode node in currentNode.ChildNodes){
				subDivisionOne.items.Add(node.Name);
			}
			
			testName = "";
			
			testInput.text="";
			
			confirmButton.gameObject.SetActive(false);
			
			message.enabled = false;
		}
		else{
			subDivisionOne.items.Add("---");
		}
			
		subDivisionOne.selection = subDivisionOne.items[0];
		
		int scale=40;
		int length = stateSelection.selection.Length;
		if(length>15){
			scale -= (3*(length-15));
		}
		
	 	stateSelection.textLabel.transform.localScale = new Vector3(scale,scale,1);
	}
	
	//Update the subdivision list if the region changes
	void updateRegion(string selected){
		subDivisionTwo.items.Clear();
		
		if(selected != "---" && selected != "Select Region"){
			
			subDivisionTwo.items.Add("Select SubRegion");
			
			XmlNode currentNode = (xml.SelectSingleNode("/players/"+stateSelection.selection+"/"+subDivisionOne.selection));
		
			foreach(XmlNode node in currentNode.ChildNodes){
				subDivisionTwo.items.Add(node.Name);
			}
		}
		else{
			subDivisionTwo.items.Add("---");
		}
			
		subDivisionTwo.selection = subDivisionTwo.items[0];
		
		int scale=40;
		int length = subDivisionOne.selection.Length;
		if(length>15){
			scale -= (3*(length-15));
		}
		
		subDivisionOne.textLabel.transform.localScale = new Vector3(scale,scale,1);
	}
	
	//Update the player list if the subregion changes
	void updateSubRegion(string selected){
		players.items.Clear();
		
		if(selected != "---" && selected != "Select SubRegion"){
			players.items.Add("Select Player");
			
			XmlNode currentNode = (xml.SelectSingleNode("/players/"+stateSelection.selection+"/"+subDivisionOne.selection+"/"+subDivisionTwo.selection));
		
			foreach(XmlNode node in currentNode.ChildNodes){
					
				string name = node.Attributes["name"].Value;
			
				players.items.Add(name);
			}
		}
		else{
			players.items.Add("---");
		}
			
		players.selection = players.items[0];
		
		int scale=40;
		int length = subDivisionTwo.selection.Length;
		if(length>15){
			scale -= (3*(length-15));
		}
		
		subDivisionTwo.textLabel.transform.localScale = new Vector3(scale,scale,1);
	}
	
	//Either hide or show the save button if player gets set
	void updatePlayer(string selected){	
		finalGo = false;
		
		confirmButton.gameObject.transform.localPosition = new Vector3(0,-225,0);

		buttonText.text = "Save";
			
		if(selected != "---"&& selected != "Select Player"){
			
			confirmButton.gameObject.SetActive(true);
		
			message.text = "Save " + players.selection + " to this Device?";
			
			message.enabled = true;
		}
		else{ 
			if(testInput.text==""){
				confirmButton.gameObject.SetActive(false);
			
				message.enabled = false;
				
				testName = "";
			}
		}
		
		int scale=40;
		int length = players.selection.Length;
		if(length>15){
			scale -= (3*(length-15));
		}
		
		players.textLabel.transform.localScale = new Vector3(scale,scale,1);
	}
	
	void OnSubmit(){
		testName = testInput.text;
		
		if(testName !=""){
			stateSelection.selection = stateSelection.items[0];
			
			confirmButton.gameObject.SetActive(true);
		
			message.text = "Save " + testName + " to this Device?";
			
			message.enabled = true;
		}
	}
	
	//Update will make sure that no dropdown lists will appear outside the screen
	void Update(){
		if(players.isOpen && !setDDL){
			if(ddL == null) ddL = GameObject.Find("Drop-down List");
			else{			
				float width =  ddL.transform.FindChild("Sprite").transform.localScale.x;
				
				if(width>210){
	
					Vector3 pos = ddL.transform.localPosition;
					
					pos.x -= width-210;
	
					ddL.transform.localPosition = pos;
					setDDL = true;
				}
			}
		}
		else if(!players.isOpen) setDDL = false;
		
		
		//Check for escape button
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
	}
}