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
	private struct Child{
		public string ID {get;set;}
		public string Department {get;set;}
		public string Province {get;set;}
		public string District {get;set;}
	}
	
	//The players file name
	private string playersFile = "players";
	private bool fileOnDropbox;
	
	//Popuplists on the screen
	public UIPopupList departmentSelection;
	public UIPopupList provinceSelection;
	public UIPopupList districtSelection;
	public UIPopupList playerSelection;
	
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
		departmentSelection.gameObject.SetActive(false);
		provinceSelection.gameObject.SetActive(false);
		districtSelection.gameObject.SetActive(false);
		english.gameObject.SetActive(false);
		spanish.gameObject.SetActive(false);
		playerSelection.gameObject.SetActive(false);
		testingCheckbox.gameObject.SetActive(false);
		
		testingLabel.enabled = false;
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
					departmentSelection.gameObject.SetActive(true);
					provinceSelection.gameObject.SetActive(true);
					districtSelection.gameObject.SetActive(true);
					playerSelection.gameObject.SetActive(true);
					
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
					
					if(!departmentSelection.items.Contains(c.Department)){
						departmentSelection.items.Add(c.Department);
					}	
				}
			}
		}
		catch{
			return false;
		}
		
		//Good Exit, found at least one subdivision
		if(departmentSelection.items.Count>1) return true;
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
				
				name = playerSelection.selection.Replace(" ", "");
				PlayerPrefs.SetString("-childID", playerSelection.selection);
				PlayerPrefs.SetString("-testing", "f");
				PlayerPrefs.SetString("-department", departmentSelection.selection);
				PlayerPrefs.SetString("-province", provinceSelection.selection);
				PlayerPrefs.SetString("-district", districtSelection.selection);					
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
			//Hide the dropdown menus
			departmentSelection.gameObject.SetActive(false);
			english.gameObject.SetActive(false);
			spanish.gameObject.SetActive(false);
			provinceSelection.gameObject.SetActive(false);
			districtSelection.gameObject.SetActive(false);
			testingCheckbox.gameObject.SetActive(false);
			playerSelection.gameObject.SetActive(false);
			testInput.gameObject.SetActive(false);	
			testingLabel.enabled = false;
		}
	}
		
	//Update the region list if the state changes
	void updateState(string selected){
		provinceSelection.items.Clear();
		
		if(selected != "Select State"){
			provinceSelection.items.Add("Select Region");
		
			foreach(Child c in demographics){
				if(c.Department == departmentSelection.selection){
					if(!provinceSelection.items.Contains(c.Province)){
						provinceSelection.items.Add(c.Province);
					}
				}
			}
			
			testName = "";
			
			testInput.text="";
			
			confirmButton.gameObject.SetActive(false);
			
			message.enabled = false;
		}
		else{
			provinceSelection.items.Add("---");
		}
			
		provinceSelection.selection = provinceSelection.items[0];
		
		int scale=40;
		int length = departmentSelection.selection.Length;
		if(length>15){
			scale -= (3*(length-15));
		}
		
	 	departmentSelection.textLabel.transform.localScale = new Vector3(scale,scale,1);
	}
	
	//Update the subdivision list if the region changes
	void updateRegion(string selected){
		districtSelection.items.Clear();
		
		if(selected != "---" && selected != "Select Region"){
			
			districtSelection.items.Add("Select SubRegion");
			
			foreach(Child c in demographics){
				if(c.Department == departmentSelection.selection && c.Province == provinceSelection.selection){
					if(!districtSelection.items.Contains(c.District)){
						districtSelection.items.Add(c.District);
					}
				}
			}
		}
		else{
			districtSelection.items.Add("---");
		}
			
		districtSelection.selection = districtSelection.items[0];
		
		int scale=40;
		int length = provinceSelection.selection.Length;
		if(length>15){
			scale -= (3*(length-15));
		}
		
		provinceSelection.textLabel.transform.localScale = new Vector3(scale,scale,1);
	}
	
	//Update the player list if the subregion changes
	void updateSubRegion(string selected){
		playerSelection.items.Clear();
		
		if(selected != "---" && selected != "Select SubRegion"){
			playerSelection.items.Add("Select Player");
			
			foreach(Child c in demographics){
				if(c.Department == departmentSelection.selection && c.Province == provinceSelection.selection && c.District == districtSelection.selection){
					if(!playerSelection.items.Contains(c.ID)){
						playerSelection.items.Add(c.ID);
					}
				}
			}
		}
		else{
			playerSelection.items.Add("---");
		}
			
		playerSelection.selection = playerSelection.items[0];
		
		int scale=40;
		int length = districtSelection.selection.Length;
		if(length>15){
			scale -= (3*(length-15));
		}
		
		districtSelection.textLabel.transform.localScale = new Vector3(scale,scale,1);
	}
	
	//Either hide or show the save button if player gets set
	void updatePlayer(string selected){	
		confirmButton.gameObject.transform.localPosition = new Vector3(0,-225,0);

		buttonText.text = "Save";
			
		if(selected != "---"&& selected != "Select Player"){
			
			confirmButton.gameObject.SetActive(true);
		
			message.text = "Save " + playerSelection.selection + " to this Device?";
			
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
		int length = playerSelection.selection.Length;
		if(length>15){
			scale -= (3*(length-15));
		}
		
		playerSelection.textLabel.transform.localScale = new Vector3(scale,scale,1);
	}
	
	void OnSubmit(){
		testName = testInput.text;
		
		if(testName !=""){
			departmentSelection.selection = departmentSelection.items[0];
			
			confirmButton.gameObject.SetActive(true);
		
			message.text = "Save " + testName + " to this Device?";
			
			message.enabled = true;
		}
	}
	
	//Update will make sure that no dropdown lists will appear outside the screen
	void Update(){
		if(playerSelection.isOpen && !setDDL){
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
		else if(!playerSelection.isOpen) setDDL = false;
		
		
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