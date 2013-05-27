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
		public string Cluster {get;set;}
		public int lastCompleted {get;set;}
	}
	
	//The players file name
	private string playersFile = "players";
	private bool fileOnDropbox;
	
	//Tab controller
	public Tab playerSelector;
	
	//Only need the english checkbox
	public UICheckbox english;
	public UICheckbox spanish;
	
	private bool currentlyEnglish;
	
	//Other elements on the screen
	public UITexture background;
	public UIButton customIDButton;
	public UISlicedSprite customBackground;
	public UILabel customIDLabel;
	public UIButton confirmButton;
	public UISlicedSprite confirmBackground;
	public UILabel buttonText;
	public UILabel message;
	public ButtonResponder quitButton;
	
	public UILabel clusterLabel;
	
	//For testing purposes, allow someone to just write in a name
	public UIInput testInput;
	public UILabel testingLabel;
	public UICheckbox testingCheckbox;
	public UILabel checkBoxLabel;

	//If the config was properly saved
	private bool configSaved=false;
	
	//Custom keyboard
	private bool windowsTab = false;
	private bool displayKeyboard =false;
	private bool shifting = false;
	public UIPanel keyboard;
	public UITexture shiftLight;
	public ButtonResponder back;
	public UILabel backText;
	public UILabel inputLabel;
	//If the player is currently touching the screen
	protected bool touching=false;
	private Camera cam;
	
	private bool needConfig;
	
	//Where did the player touch the screen
	protected Vector3 touchPos = Vector3.zero;	
	
	private string testName="";
	
	//List of child info
	private List<Child> demographics;
	
	// Use this for initialization
	void Start () {
		
		cam = Camera.main;
		
		currentlyEnglish = true;
		
		if(Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor){
			windowsTab = true;
			testInput.collider.enabled =false;
		}
		
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
		
		needConfig=false;
		
		bool foundFile = true;
		
		playerSelector.gameObject.SetActive(true);
		keyboard.gameObject.SetActive(false);
		
		//Hide all the elements on the screen until were sure the files are set up
		english.gameObject.SetActive(false);
		spanish.gameObject.SetActive(false);
		playerSelector.gameObject.SetActive(false);
		testingCheckbox.gameObject.SetActive(false);
		
		testingLabel.enabled = false;
		confirmButton.gameObject.SetActive(false);
		confirmButton.GetComponent<ButtonResponder>().response = buttonPressed;
		
		customIDButton.GetComponent<ButtonResponder>().response = showInputField;
		customIDButton.gameObject.SetActive(false);
		
		//Set up the quit button, it should be the only thing on the screen at the start
		quitButton.gameObject.SetActive(true);
		quitButton.response = quitbuttonPressed;
		
		back.response = showPlayerSelector;
		
		message.enabled = false;
		
		background.color = Color.black;
		
		TextReader reader=null;
		
		//Try to load the players file. If it fails, exit out
		try{
			//Try on Dropbox
			Debug.Log("Attempting to read from Dropbox folder: " + CsvManager.PlayerSpecificPath );
			
			Debug.Log(Path.Combine(CsvManager.PlayerSpecificPath, playersFile+".csv"));
			
			reader = new StreamReader(Path.Combine(CsvManager.PlayerSpecificPath, playersFile+".csv"));
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
			
			string customID = PlayerPrefs.GetString("-customID");
			
			//If there is no player saved in PlayerPrefs, signal that we need to config the device
			if(p == ""){
				needConfig = true;
					
				NeuroLog.Log("No player set to the device.");
			}
			else if(customID=="t"){
				needConfig = false;
				
				NeuroLog.Log("Using customID Player");
			}
			
			//If the program needs to be configured, make all the GUI elements appear
			if(needConfig){
				
				//Remove any previously set brightness value
				PlayerPrefs.DeleteKey("-ambience");
				
				//Set up the players file
				if(SetupPlayersCSV(reader)){
					playerSelector.setupTab(this,demographics);
					playerSelector.gameObject.SetActive(true);
					
					
					customIDButton.gameObject.SetActive(true);
					
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
				
			needConfig =false;
			
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
					c.Cluster = csv["cluster"];
					c.ID = csv["childID"];
					int lastcompleted = -1;
					
					int.TryParse(csv["lastcompleted"], out lastcompleted);
					
					if(c.lastCompleted<0)
						NeuroLog.Log("Child " + c.ID +"("+ c.Cluster+") has an invalid value for last Completed. Needs to be between 0-7.");
					else{
						c.lastCompleted = lastcompleted;
						
						demographics.Add(c);
					}
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
				PlayerPrefs.SetString("-testingMode", "true");
				PlayerPrefs.SetString("-t1", "true");
				PlayerPrefs.SetString("-t2", "true");
				PlayerPrefs.SetString("-t3", "true");
				PlayerPrefs.SetString("-t4", "true");
				PlayerPrefs.SetString("-t5", "true");
				PlayerPrefs.SetString("-t6", "true");
				PlayerPrefs.SetString("-t7", "true");
			}
			else{
				PlayerPrefs.SetString("-testingMode", "false");
				
				//Get the last completed of the player
				int lastCompleted =0;
				
				foreach(Child c in demographics){
					if(playerSelector.SelectedPlayer == c.ID && playerSelector.SelectedCluster == c.Cluster){
						lastCompleted = c.lastCompleted;
					}
				}
				
				//Save the t values
				for (int i =1 ; i<8;i++){
					if(lastCompleted<i)
						PlayerPrefs.SetString("-t"+i, "false");
					else
						PlayerPrefs.SetString("-t"+i, "true");
				}
			}
			
			if(english.isChecked) PlayerPrefs.SetString("-language", "english");
			else PlayerPrefs.SetString("-language", "spanish");
			
			string name ="";
			
			//Save the player's info
			if(testName==""){
				
				name = playerSelector.SelectedPlayer.Replace(" ", "");
				PlayerPrefs.SetString("-childID", playerSelector.SelectedPlayer);
				PlayerPrefs.SetString("-customID", "f");
				PlayerPrefs.SetString("-cluster", playerSelector.SelectedCluster);					
			}
			else{
				//testName = testName.Replace(" ", "");
				
				PlayerPrefs.SetString("-childID", testName);
				PlayerPrefs.SetString("-customID", "t");
				PlayerPrefs.SetString("-cluster", "n/a");
			}
			
			using(StreamWriter writer = new StreamWriter(Path.Combine(CsvManager.PlayerSpecificPath, name+"_" + System.Environment.MachineName+"_Criterion.csv"))){
				writer.WriteLine("TaskNum, CriterionScore, NumofPractice, NumofEvents, AccuracyofPresented , AccuracyofResponses, NumResponsesBasal");
				
				for(int i = 0;i<8;i++){
					writer.WriteLine(i+", 0, ., ., ., ., .");
				}
			}
			
			if(currentlyEnglish){
				message.text = "Config info saved";
			
				buttonText.text = "CLOSE";
			}
			else{
				message.text = "La configuración ha sido guardada";
				buttonText.text = "CERRAR";
				confirmBackground.transform.localScale = new Vector3(225,60,1);
			}
			
			configSaved = true;
			english.gameObject.SetActive(false);
			spanish.gameObject.SetActive(false);
			testingCheckbox.gameObject.SetActive(false);
			playerSelector.gameObject.SetActive(false);
			keyboard.gameObject.SetActive(false);
			testInput.gameObject.SetActive(false);	
			customIDButton.gameObject.SetActive(false);
			quitButton.gameObject.SetActive(false);
			testingLabel.enabled = false;
		}
	}
	
	//When the operator selects a player, show the button
	public void askForConfirmation(){
		confirmButton.gameObject.SetActive(true);
		
		if(currentlyEnglish)
			message.text = "Save " + playerSelector.SelectedPlayer + " to this Device?";
		else
			message.text = "Asignar "+playerSelector.SelectedPlayer+ " a este PC";
		
		message.enabled = true;	
	}
	
	//If the operator changes any setting in the selector, hide the message and button
	public void hideConfirmation(){
		if(message.enabled ==true){
			confirmButton.gameObject.SetActive(false);
			message.enabled = false;	
		}
		
		hideInputField();
	}
	
	//If the player presses the customIDButton, show the input field
	void showInputField(GameObject o){
		if(message.enabled ==true){
			confirmButton.gameObject.SetActive(false);
			message.enabled = false;	
		}
		
		testInput.gameObject.SetActive(true);
		testingLabel.enabled = true;
		
		customIDButton.gameObject.SetActive(false);
		
		playerSelector.Reset();
		
		if(windowsTab){
			testInput.text ="|";
			keyboard.gameObject.SetActive(true);
			playerSelector.gameObject.SetActive(false);
			displayKeyboard=true;
		}
	}
	
	//Hides the input field
	void hideInputField(){
		testInput.text = "";
		
		testInput.gameObject.SetActive(false);
		testingLabel.enabled = false;
		
		customIDButton.gameObject.SetActive(true);
	}
	
	//If the player presses the back button on the keyboard screen
	void showPlayerSelector(GameObject go){
		
		if(message.enabled ==true){
			testInput.text = "";
			
			confirmButton.gameObject.SetActive(false);
			message.enabled = false;	
		}
		
		displayKeyboard=false;
		
		playerSelector.gameObject.SetActive(true);
		keyboard.gameObject.SetActive(false);
		
		testInput.gameObject.SetActive(false);
		testingLabel.enabled = false;
		
		customIDButton.gameObject.SetActive(true);
	}
	
	void OnSubmit(){
		
		if(testInput.text.Length>0){
			if(testInput.text[testInput.text.Length-1] =='|'){
				testName = testInput.text.Substring(0,testInput.text.Length-1);
			}
			else testName = testInput.text;
		}
		
		if(testName !=""){
			playerSelector.Reset();
			
			confirmButton.gameObject.SetActive(true);
		
			if(currentlyEnglish)
				message.text = "Save " + testName + " to this Device?";
			else 
				message.text = "Asignar "+testName+ " a este PC";
			
			message.enabled = true;
		}
	}
	
	//If quit button is pressed, just simply end the program
	private void quitbuttonPressed(GameObject o){
		Application.Quit();
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
		if(needConfig){
			if(spanish.isChecked && currentlyEnglish){
				currentlyEnglish = false;
				customIDLabel.text = "ID no se encuentra en la lista, escribirla a mano";
				customBackground.transform.localScale = new Vector3(575,40,1);
				((BoxCollider)customIDButton.collider).size = new Vector3(575,40,0);
				testingLabel.text = "Escribir ID a mano:";
				clusterLabel.text = "Grupo";
				checkBoxLabel.text = "Modo de Prueba";
				quitButton.GetComponentInChildren<UILabel>().text = "Salir"; 
				if(testName!="")
					message.text = "Asignar "+testName+ " a este PC";
				else
					message.text = "Asignar "+playerSelector.SelectedPlayer+ " a este PC";
				buttonText.text ="Salvar";
				backText.text = "Volver";
			}
			else if(english.isChecked && !currentlyEnglish){
				currentlyEnglish = true;
				customIDLabel.text = "Can't find ID, enter it by hand";
				customBackground.transform.localScale = new Vector3(400,40,1);
				((BoxCollider)customIDButton.collider).size = new Vector3(400,40,0);
				testingLabel.text = "Input ID by hand:";
				clusterLabel.text = "Cluster";
				checkBoxLabel.text = "Testing Mode";
				quitButton.GetComponentInChildren<UILabel>().text = "Quit"; 
				if(testName!="")
					message.text = "Save "+testName+ " to this Device?";
				else
					message.text = "Save "+playerSelector.SelectedPlayer+ " to this Device?";
				buttonText.text ="Save";
				backText.text = "Back";
			}
		}
		
		//Show the save button if the test input ever changes
		if(testName != testInput.text){
			
			if(testInput.text!= "" && testInput.text!= "|" && testName != testInput.text){
				testName = testInput.text;
				
				OnSubmit();
			}
			else if(testInput.text== "" || testInput.text== "|"){
				testName = "";
				
				confirmButton.gameObject.SetActive(false);
				message.enabled = false;
			}
		}
		
		if(windowsTab && displayKeyboard){
			if(testInput.selected){
				if(testInput.text.Length >0){
					if(testInput.text[testInput.text.Length-1] =='|')
						testInput.text = testInput.text.Substring(0,testInput.text.Length-1);
				}
				inputLabel.color = new Color(.95f,1f,0f,1);
			}
			
			bool currentTouch =false;
			//Get the touch location based on the platform
			if(SystemInfo.deviceType == DeviceType.Handheld){
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
				Ray ray = cam.ScreenPointToRay(touchPos);
				RaycastHit hit = new RaycastHit();
				//If the raycast of the touch hit something
				if(Physics.Raycast(ray, out hit)) {
					//Hit the Stimulus
					if(hit.collider.name.Contains("Key")){
						
						inputLabel.color = Color.black;
						
						string keyChar = hit.collider.name.Replace("Key","");
						
						if(keyChar =="shift") shifting = !shifting;
						else{
							if(keyChar == "enter"){
								if(testInput.text.Length>0){
									if(testInput.text[testInput.text.Length-1] =='|')
										testInput.text= testInput.text.Substring(0,testInput.text.Length-1);
									
									OnSubmit();
								}
							}
							else{
								if(keyChar =="space"){
									if(testInput.text.Length==0)
										testInput.text =" |";
									if(testInput.text[testInput.text.Length-1] !='|')
										testInput.text += " |";
									else
										testInput.text= testInput.text.Substring(0,testInput.text.Length-1) + " |";
								}
								else if(keyChar =="backspace"){
									if(testInput.text.Length==0)
										testInput.text ="|";
									else if(testInput.text[testInput.text.Length-1] !='|')
										testInput.text = testInput.text.Substring(0,testInput.text.Length-1) + "|";
								 	else if(testInput.text.Length>1)
										testInput.text = testInput.text.Substring(0,testInput.text.Length-2) + "|";
								}
								else{
									if(shifting) keyChar = keyChar.ToUpper();
							
									if(testInput.text.Length==0)
										testInput.text = (keyChar+"|");
									else if(testInput.text[testInput.text.Length-1] !='|')
										testInput.text += (keyChar+"|");
									else
										testInput.text = testInput.text.Substring(0,testInput.text.Length-1) + keyChar +"|"; 
								}
							}
							
							shifting = false;
						}
						
						if(shifting) shiftLight.color = Color.green;
						else shiftLight.color = new Color(.2f,.2f,.2f,1);
					}
				}
			}
		}
	}
}