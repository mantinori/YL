using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using LumenWorks.Framework.IO.Csv;



//Config Controller
public class ConfigManager : MonoBehaviour {
	
	//Struct for child info
	public struct Child{
		public string ID {get;set;}
		public string Department {get;set;}
		public string Province {get;set;}
		public string District {get;set;}
	}
	
	//The players file name
	private string playersFile = "players";
	private bool fileOnDropbox;
	
	//Tab controller
	public Tab playerSelector;
	
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
	public UILabel testingSubscript;
	public UICheckbox testingCheckbox;

	//If the config was properly saved
	private bool configSaved=false;
	
	private string testName="";
	
	//List of child info
	private List<Child> demographics;
	
	// Use this for initialization
	void Start () {
		
		demographics = new List<Child>();
		
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
		
		//Hide all the elements on the screen until were sure the files are set up
		english.gameObject.SetActive(false);
		spanish.gameObject.SetActive(false);
		playerSelector.gameObject.SetActive(false);
		testingCheckbox.gameObject.SetActive(false);
		
		testingLabel.enabled = false;
		testingSubscript.enabled = false;
		confirmButton.gameObject.SetActive(false);
		confirmButton.GetComponent<ButtonResponder>().response = buttonPressed;
		
		message.enabled = false;
		
		background.color = Color.black;
		
		TextReader reader=null;
		
		//Try to load the players file. If it fails, exit out
		try{
			//Try on Dropbox
			Debug.Log("Attempting to read from Dropbox folder");
			reader = new StreamReader(Path.Combine(XmlManager.PlayerSpecificPath, playersFile+".csv"));
		}
		catch{
			NeuroLog.Error("Unable to find dropbox player file.");
			//Try local bundle if not on dropbox
			try{		
				TextAsset sessionData = Resources.Load(playersFile) as TextAsset;
				
				reader = new StringReader(sessionData.text);
			}
			catch{
				NeuroLog.Error("Unable to find local player file");
			
				foundFile =false;
			}
		}
		
		//If the program found the file
		if(foundFile){
			string p = PlayerPrefs.GetString("-childID");
			
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
			
			//If the program needs to be configured, make all the GUI elements appear
			if(needConfig){
				//Set up the players file
				if(SetupPlayersCSV(reader)){
					playerSelector.setupTab(this,demographics);
					playerSelector.gameObject.SetActive(true);
					
					testingLabel.enabled = true;
					testingSubscript.enabled =true;
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
	public bool SetupPlayersCSV(TextReader reader)
	{
		try{
			using (CsvReader csv = new CsvReader(reader,true)){
				while (csv.ReadNextRecord()){
					Child c = new Child();
					c.Department = csv["department"];
					c.Province = csv["province"];
					c.District = csv["district"];
					c.ID = csv["childID"];
					
					demographics.Add(c);
				}
			}
		}
		catch(Exception e){
			NeuroLog.Log(e.Message);
			return false;
		}
		
		//Good Exit, found at least one subdivision
		if(demographics.Count>1) return true;
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
			
			if(english.isChecked) PlayerPrefs.SetString("-language", "english");
			else PlayerPrefs.SetString("-language", "spanish");
			
			string name ="";
			
			//Save the player's info
			if(testName==""){
				
				name = playerSelector.SelectedPlayer.Replace(" ", "");
				PlayerPrefs.SetString("-childID", playerSelector.SelectedPlayer);
				PlayerPrefs.SetString("-testing", "f");
				PlayerPrefs.SetString("-department", playerSelector.SelectedDepartment);
				PlayerPrefs.SetString("-province", playerSelector.SelectedProvince);
				PlayerPrefs.SetString("-district", playerSelector.SelectedDistrict);					
			}
			else{
				testName = testName.Replace(" ", "");
				
				PlayerPrefs.SetString("-childID", testName);
				PlayerPrefs.SetString("-testing", "t");
				PlayerPrefs.SetString("-department", "n/a");
				PlayerPrefs.SetString("-province", "n/a");
				PlayerPrefs.SetString("-district", "n/a");
			}
			
			using(StreamWriter writer = new StreamWriter(Path.Combine(CsvManager.PlayerSpecificPath, name+"_" + System.Environment.MachineName+"_Criterion.csv"))){
				writer.WriteLine("TaskNum, CriterionScore, NumofPractice, NumofEvents, AccuracyofPresented , AccuracyofResponses, NumResponsesBasal");
				
				for(int i = 0;i<8;i++){
					writer.WriteLine(i+", 0, ., ., ., ., .");
				}
			}
			
			message.text = "Config info saved!";
			
			buttonText.text = "CLOSE";
			
			configSaved = true;
			english.gameObject.SetActive(false);
			spanish.gameObject.SetActive(false);
			testingCheckbox.gameObject.SetActive(false);
			playerSelector.gameObject.SetActive(false);
			testInput.gameObject.SetActive(false);	
			testingLabel.enabled = false;
			testingSubscript.enabled =false;
		}
	}
	
	//When the operator selects a player, show the button
	public void askForConfirmation(){
		confirmButton.gameObject.SetActive(true);
		
		message.text = "Save " + playerSelector.SelectedPlayer + " to this Device?";
		
		message.enabled = true;	
	}
	
	//If the operator changes any setting in the selector, hide the message and button
	public void hideConfirmation(){
		if(message.enabled ==true){
			confirmButton.gameObject.SetActive(false);
			message.enabled = false;	
		}
		
		testInput.text = "";
	}
	
	void OnSubmit(){
		testName = testInput.text;
		
		if(testName !=""){
			playerSelector.Reset();
			
			confirmButton.gameObject.SetActive(true);
		
			message.text = "Save " + testName + " to this Device?";
			
			message.enabled = true;
		}
	}
	
	//Update will make sure that no dropdown lists will appear outside the screen
	void Update(){
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