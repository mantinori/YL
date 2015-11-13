using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using LumenWorks.Framework.IO.Csv;

public class CsvManager {

	public static string dashboardFolderName = Path.Combine("YL_tasks", "yl_test");
	
	//Folder names
	public static string logFilesFolderName = "log_files";
	public static string traceFilesFolderName = "trace_files";
	public static string playerSpecificFolderName = "player_specific";
	public static string sessionFilesName = "session_files";
	public static string playermeasuresName = "indiv_measures";
	public static string pitchFilesName = "pitch_files";
	public static string unfinishedSessionFolderName = "unfinished_files";
	
	//The delimiter used for Path
	//public static char delim = Path.DirectorySeparatorChar;
	
	//The game manager class
	private GameManager gm;
	
	//The username
	private string mUserName ="testGuy";
	//The file name of the write out file
	public string statsXML = "";
	//The file name of the read in file
	public string sessionXML = "randomList";

	// Assumed to be under $HOME/Dropbox/YL_tasks/
	public static string DropboxPath
	{
		get
		{
			if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) {
				string home = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
				return Path.Combine(home, "Dropbox");
			} else if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor) {
				string home = Environment.GetEnvironmentVariable("HOME");
				return Path.Combine(home, "Dropbox");
			} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
				return Application.persistentDataPath;
			} else {
				//In the default case, just return whatever persistent data path the platform has.
				return Application.persistentDataPath;
			}
		}
	}

	//Methods that return the path to specific folder names
	public static string DashboardPath{
		get { return Path.Combine(DropboxPath,dashboardFolderName); }
	}

	public static string LogFilesPath{
		get { return Path.Combine(Path.Combine(DropboxPath,dashboardFolderName), logFilesFolderName); }
	}

	public static string TraceFilesPath{
		get { return Path.Combine(Path.Combine(DropboxPath, dashboardFolderName), traceFilesFolderName); }
	}

	public static string PlayerSpecificPath{
		get { return Path.Combine(Path.Combine(DropboxPath, dashboardFolderName), playerSpecificFolderName); }
	}
	
	public static string PitchFilesPath{
		get { return Path.Combine(Path.Combine(DropboxPath, dashboardFolderName), pitchFilesName); }
	}
	
	public static string SessionFilesPath{
		get { return Path.Combine(Path.Combine(DropboxPath, dashboardFolderName), sessionFilesName); }
	}

	public static string PlayerMeasuresPath{
		get { return Path.Combine(Path.Combine( DropboxPath, dashboardFolderName), playermeasuresName); }
	}

	public static string UnfinishedSessionPath{
		get { return Path.Combine(Path.Combine(DropboxPath, dashboardFolderName), unfinishedSessionFolderName); }
	}

	// Write out exit code file to let know the dashboard if we exited well or not
	// Use code 1 for error, code 0 for everything ok.
	public static void WriteExitCode(string module, int code){
 		NeuroLog.Log("writing exit code " + code + " for module " + module);
		string fn = System.Environment.MachineName + "_" + module + "_exit_code.txt";
		using (StreamWriter sw = new StreamWriter(Path.Combine(TraceFilesPath, fn))) {
			sw.WriteLine(code);
		}
	}
	
	//Constructor for CsvManager
	public CsvManager(){

		gm = GameManager.main;

	}

	/*
	 * Checks to see if dir exists, and if not tries to create. If all goes
	 * well returns true. If something goes wrong, false is returned.
	 */
	private static bool CheckAndCreateDir(string dirname)
	{
		DirectoryInfo dir = new DirectoryInfo(dirname);
		//If it doesn't exist, try to create the folder
		if(!dir.Exists) {
			try {
				System.IO.Directory.CreateDirectory(dir.FullName);
			}
			//If it can't create the folder, return false
			catch (System.Exception e) {
				NeuroLog.Log("Couldn't create directory " + dir.FullName + ": " + e);
				return false;
			}
		}
		return true;
	}

	/*
	 * Checks that all the folders we define here are accessible, and if not
	 * creates them. If something goes wrong returns False.
	 */
	public static bool CheckFolders()
	{
		if (!CheckAndCreateDir(DashboardPath)) return false;

		if (!CheckAndCreateDir(TraceFilesPath)) return false;

		if (!CheckAndCreateDir(LogFilesPath)) return false;
		
		if (!CheckAndCreateDir(PlayerSpecificPath)) return false;
		
		if (!CheckAndCreateDir(UnfinishedSessionPath)) return false;
		
		if (!CheckAndCreateDir(SessionFilesPath)) return false;
		
		if (!CheckAndCreateDir(PitchFilesPath)) return false;
		
		if(!CheckAndCreateDir(PlayerMeasuresPath)) return false;
		
		return true;
	}
	
	
	//Base method used to start the read in process, returns true if successful
	public List<EventStats> ReadInSession()
	{
		int tNum = PlayerPrefs.GetInt("-currentTask");
		
		//Check to see if the task number is in the correct range of 1-7
		if(tNum<1 ||tNum>6){
			
			NeuroLog.Log("Invalid task number given: " + tNum.ToString());
			
			sessionXML = "randomList";
			
			return null;
		}
		
		//Set the user and session file
		sessionXML = "task"+tNum.ToString();
		mUserName = PlayerPrefs.GetString("-childID").Replace(" ","");
		
		//Add an xml extension
		string fn = sessionXML;
		if (!fn.EndsWith(".xml")) fn += ".csv";
		
		//Set up the XmlDocument variable
		//StreamReader sR;
		
		TextReader reader;
		
		//Try to load the file. If it fails, exit out
		try{
			if(File.Exists(Path.Combine(CsvManager.SessionFilesPath, fn))){
				NeuroLog.Log("Attempting to Load " + Path.Combine(CsvManager.SessionFilesPath, fn));
				reader = new StreamReader(Path.Combine(CsvManager.SessionFilesPath, fn));
			}
			else{
				NeuroLog.Log("Attempting to read from local Resources: " + "session_files/" + fn);
					
				TextAsset sessionData = Resources.Load("session_files/" + fn) as TextAsset;
				
				reader = new StringReader(sessionData.text);
			}
		}
		catch(Exception e){
			NeuroLog.Log("Unable to find sessionFile! Error: " + e);
			
			sessionXML = "randomList";
			
			return null;
		}
		
		List<EventStats> eS = new List<EventStats>();
		
		//Check the headers to make sure its the right type of file
		using (CsvReader csv = new CsvReader(reader,true)){
        	List<string> headers = new List<string>(csv.GetFieldHeaders());
			
			for(int i=0;i<headers.Count;i++){
				headers[i]= headers[i].ToLower();
			}
			
			bool exit = false;
			
			if(gm.SType == GameManager.SessionType.Spatial){
				if(!headers.Contains("dots")){
					NeuroLog.Log("Invalid read in file for current scene. File is missing header 'Dots' for Spatial Scene");
					exit =true;
				}
				else if(!headers.Contains("delay")){
					NeuroLog.Log("Invalid read in file for current scene. File is missing header 'Delay' for Spatial Scene");
					exit =true;
				}
				else 	
					eS = ReadSpatialEvents(csv);
			}
			else if(gm.SType == GameManager.SessionType.Inhibition){
				if(!headers.Contains("side")){
					NeuroLog.Log("Invalid read in file for current scene. File is missing header 'Side' for Inhibition Scene");
					exit =true;
				}
				else if(!headers.Contains("color")){
					NeuroLog.Log("Invalid read in file for current scene. File is missing header 'Color' for Inhibition Scene");
					exit =true;
				}
				else 
					eS = ReadInhibEvents(csv);
			}
			else if(gm.SType == GameManager.SessionType.Star){
				if(!headers.Contains("blocknum")){
					NeuroLog.Log("Invalid read in file for current scene. File is missing header 'BlockNum' for Star Scene");
					exit =true;
				}
				else if(!headers.Contains("type")){
					NeuroLog.Log("Invalid read in file for current scene. File is missing header 'Type' for Star Scene");
					exit =true;
				}
				else if(!headers.Contains("position")){
					NeuroLog.Log("Invalid read in file for current scene. File is missing header 'Position' for Star Scene");
					exit =true;
				}
				else if(!headers.Contains("rotation")){
					NeuroLog.Log("Invalid read in file for current scene. File is missing header 'Rotation' for Star Scene");
					exit =true;
				}
				else
					eS = ReadStarEvents(csv);
			}
			else if(gm.SType == GameManager.SessionType.Implicit){
				if(!headers.Contains("blocknum")){
					NeuroLog.Log("Invalid read in file for current scene. File is missing header 'BlockNum' for Implicit Scene");
					exit =true;
				}
				else if(!headers.Contains("dot")){
					NeuroLog.Log("Invalid read in file for current scene. File is missing header 'Dot' for Implicit Scene");
					exit =true;
				}
				else
					eS = ReadImplicitEvents(csv);
			}
			else if(gm.SType == GameManager.SessionType.Stopping){
				if(!headers.Contains("go")){
					NeuroLog.Log("Invalid read in file for current scene. File is missing header 'Go' for Stopping Scene");
					exit =true;
				}
				else if(!headers.Contains("dot")){
					NeuroLog.Log("Invalid read in file for current scene. File is missing header 'Dot' for Stopping Scene");
					exit =true;
				}
				else 
					eS = ReadStoppingEvents(csv);
			}
			else if(gm.SType == GameManager.SessionType.Associate ){
				if(!headers.Contains("target")){
					NeuroLog.Log("Invalid read in file for current scene. File is missing header 'Target' for Associate Scene");
					exit =true;
				}
				else if(!headers.Contains("stimuli")){
					NeuroLog.Log("Invalid read in file for current scene. File is missing header 'Stimuli' for Associate Scene");
					exit =true;
				}
				else
					eS = ReadAssociateEvents(csv);
			}
			else if(gm.SType == GameManager.SessionType.MemAttentEnc1 || gm.SType == GameManager.SessionType.MemAttentEnc2 ){
				if(!headers.Contains("quadrant")){
					NeuroLog.Log("Invalid read in file for current scene. File is missing header 'Quadrant' for Associate Scene");
					exit =true;
				}
				else if(!headers.Contains("stimulus")){
					NeuroLog.Log("Invalid read in file for current scene. File is missing header 'Stimuli' for Associate Scene");
					exit =true;
				}
				else
					eS = ReadMemAttEncEvents(csv);
			}
			
			if(exit){
				sessionXML = "randomList";
			
				return null;
			}
		}
		
		//Make sure it actually read in something
		if(eS == null || eS.Count == 0){
			NeuroLog.Error("No events found. Generating random set");
			sessionXML = "randomList";
			return null;
		}
		
		NeuroLog.Log("Finished Reading Trials");
		
		//Good Exit
		return eS;
	}
	
	//Method used to read in all the trials of the game
	private List<EventStats> ReadSpatialEvents(CsvReader csv){
		
		List<EventStats> events = new List<EventStats>();
		
    	int fieldCount = csv.FieldCount;

    	string[] headers = csv.GetFieldHeaders();
		
		int i =2;
		
    	while (csv.ReadNextRecord()){
			
			List<int> dots = new List<int>();
			float delay=.1f;
			
			for(int j = 0;j<fieldCount;j++){
			
				if(headers[j].ToLower() == "dots"){
					int num;
					
					string[] d = csv[j].Split(';');
					for(int k =0;k<d.Length;k++){
						if(int.TryParse(d[k],out num)){
							if(num<1 || num>9){
								NeuroLog.Error("Invalid Value for '"+ d[k] +"' value for 'dots' of line #" + i.ToString() + ". Needs to be between 1 and 9.");
							}
							else{
								dots.Add(num);
							}
						}
						else{
							NeuroLog.Error("Invalid ValueType for '"+ d[k] +"' value for 'dots' of line #" + i.ToString() + ". Needs to be a float.");
						}
					}
				}
				//Delay
				else if(headers[j].ToLower() == "delay"){

					if(float.TryParse(csv[j],out delay)){
						if(delay!= .1f && delay != 3){
							NeuroLog.Log("Invalid value for 'delay' at line #" + i.ToString() + ". Needs to be either .1 or 3.");
							delay = .1f;
						}
					}
					else NeuroLog.Log("Invalid value for 'delay' at line #" + i.ToString() + ". Needs to be a float.");
				}
				else{
					NeuroLog.Log(headers[j].ToLower());
				}
			}
			
			if(dots.Count!=0){
				SpatialEvent sE = new SpatialEvent(dots);
				sE.Delay = delay;
			
				//Add the newly compiled event to the event list
				events.Add(sE);
			}
			
			i++;
		}
	
		//Set the event list
		return events;
	}
	
	//Method used to read in all the trials of the game
	private List<EventStats> ReadInhibEvents(CsvReader csv){
		
		List<EventStats> inhibEvents = new List<EventStats>();
	
    	int fieldCount = csv.FieldCount;

    	string[] headers = csv.GetFieldHeaders();
		
		int i =2;
		
    	while (csv.ReadNextRecord()){
			
			char charVar='0';
			string color="";
			
			for(int j = 0;j<fieldCount;j++){
				if(headers[j].ToLower() == "side"){	
					if(char.TryParse(csv[j].ToLower(),out charVar)){
						if(charVar !='l'  && charVar !='r'){
							NeuroLog.Log("Invalid value for 'side' at event #" + i.ToString() + ". Needs to be either 'l' or 'r'.");
							charVar = '0';
						}
					}
					else 
						NeuroLog.Log("Invalid value for 'side' at event #" + i.ToString() + ". Needs to be a char.");
				}
				//Color
				else if(headers[j].ToLower() == "color"){	
					color = csv[j].ToLower();
					if(color!="yellow" && color!="purple"){	
						NeuroLog.Log("Invalid value for 'color' at line #" + i.ToString() + ". Needs to be either 'yellow' or 'purple'.");
						color = "";
					}
				}
			}
			
			if(charVar!='0' && color !="") inhibEvents.Add(new InhibitionEvent(charVar,color));
			
			i++;
		}
	
		return inhibEvents;
	}
	
	//Method used to read in all the trials of the game
	private List<EventStats> ReadStarEvents(CsvReader csv){
		
		List<EventStats> starEvents = new List<EventStats>();
		
    	int fieldCount = csv.FieldCount;

    	string[] headers = csv.GetFieldHeaders();
		
		int i =2;
		
		StarEvent sE = new StarEvent();
		
		int lS=0;
		int bS=0;
		int d=0;
		int t=0;
			
		int block =0;
		
		List<StarObject> objs = new List<StarObject>();
		
    	while (csv.ReadNextRecord()){
			int type = -1;
			int b=-1;
			Vector2 pos = new Vector2(Mathf.Infinity,Mathf.Infinity);
			float rotation = 0;
			
			for(int j = 0;j<fieldCount;j++){
				//Type
				if(headers[j].ToLower() == "type"){	
					if(int.TryParse(csv[j].ToLower(),out type)){
						if(type <0  && type>3){
							NeuroLog.Log("Invalid value for 'type' at line #" + i.ToString() + ". Needs to be between 0 and 4.");
							type = -1;
						}
						else{
							if(type == 0)
								lS++;
							else if(type ==1)
								bS++;
							else if(type ==2)
								d++;
							else if(type ==3)
								t++;
						}
					}
					else NeuroLog.Log("Invalid value for 'type' at line #" + i.ToString() + ". Needs to be a int.");
				}
				//Position
				else if(headers[j].ToLower() =="position"){
					string[] input = csv[j].Split(';');
					
					float x = Mathf.Infinity;
					float y = Mathf.Infinity;
					
					if(float.TryParse(input[0],out x)){
						if(x!=Mathf.Infinity){
							if(x>170 || x<-170){
								x = Mathf.Infinity;
								NeuroLog.Log("Invalid value for 'position.x' at line #" + i.ToString() + ". Needs to be between -170 and 170.");
							}
						}
					}
					else NeuroLog.Log("Invalid value for 'position.x' at line #" + i.ToString() + ". Needs to be a float.");
					
					if(float.TryParse(input[1],out y)){
						if(y!=Mathf.Infinity){
							if(y>90 || y<-75){
								y = Mathf.Infinity;
								NeuroLog.Log("Invalid value for 'position.y' at line #" + i.ToString() + ". Needs to be between -75 and 90.");
							}
						}
					}
					else NeuroLog.Log("Invalid value for 'position.y' at line #" + i.ToString() + ". Needs to be a float.");
					
					pos = new Vector2(x,y);
				}
				//Rotation
				else if(headers[j].ToLower() == "rotation"){	
					if(!float.TryParse(csv[j].ToLower(),out rotation))
						NeuroLog.Log("Invalid value for 'rotation' at line #" + i.ToString() + ". Needs to be a float.");
				}
				//Block Num
				else if (headers[j].ToLower() =="blocknum"){
					if(int.TryParse(csv[j],out b)){
						if(block ==0){
							block = b;
						}
						else if(b <block){
							NeuroLog.Log("Invalid value for 'dot' at line #" + i.ToString() + ". Needs to be greater than "+block +" at this point. Please keep block num's together");
							b = -1;
						}
						else if(b> block+1){
							NeuroLog.Log("Invalid value for 'dot' at line #" + i.ToString() + ". Needs to be less than "+(block+1).ToString() 
								+" at this point. Do not jump ahead.");
							b = -1;
						}
					}
					else NeuroLog.Log("Invalid value for 'BlockNum' at line #" + i.ToString() + ". Needs to be a int.");
				}
			}
			if(b != -1){
				if(b != block){
					
					block = b;
					sE.Objects = objs;
					
					if(type == 0) lS--;
					else if(type ==1) bS--;
					else if(type ==2) d--;
					else if(type ==3) t--;
			
					sE.NumBigStars = bS;
					sE.NumLittleStars=lS;
					sE.NumTriangles = t;
					sE.NumDots = d;
					
					starEvents.Add(sE);
					
					lS=0;
					bS=0;
					d=0;
					t=0;
					if(type == 0) lS++;
					else if(type ==1) bS++;
					else if(type ==2) d++;
					else if(type ==3) t++;
			
					objs = new List<StarObject>();
					
					sE = new StarEvent();
				}
				
				if(type!=-1 && pos.x != Mathf.Infinity && pos.y != Mathf.Infinity){
					StarObject sO = new StarObject(type,pos,rotation);
						
					objs.Add(sO);
				}
			}
			
			i++;
		}
		
		if(objs.Count>0){
			sE.Objects = objs;
			
			sE.NumBigStars = bS;
			sE.NumLittleStars=lS;
			sE.NumTriangles = t;
			sE.NumDots = d;
					
			starEvents.Add(sE);
		}
		
		return starEvents;
	}
	
	//Method used to read in all the trials of the game
	private List<EventStats> ReadImplicitEvents(CsvReader csv){
		
		List<EventStats> implicitEvents = new List<EventStats>();
		
    	int fieldCount = csv.FieldCount;

    	string[] headers = csv.GetFieldHeaders();
		
		int i =2;
		
		int block = 0;
		
    	while (csv.ReadNextRecord()){
			
			int dot = -1;
			
			int b=-1;
			
			for(int j = 0;j<fieldCount;j++){
				if(headers[j].ToLower() == "dot"){	
					if(int.TryParse(csv[j],out dot)){
						if(dot <1  && dot>4){
							NeuroLog.Log("Invalid value for 'dot' at line #" + i.ToString() + ". Needs to be between 0 and 4.");
							dot = -1;
						}
					}
					else NeuroLog.Log("Invalid value for 'dot' at line #" + i.ToString() + ". Needs to be a int.");
				}
				else if (headers[j].ToLower() =="blocknum"){
					if(int.TryParse(csv[j],out b)){
						if(block ==0){
							block = b;
						}
						else if(b <block){
							NeuroLog.Log("Invalid value for 'BlockNum' at line #" + i.ToString() + ". Needs to be greater than "+block +" at this point. Please keep block num's together");
							b = -1;
						}
						else if(b> block+1){
							NeuroLog.Log("Invalid value for 'BlockNum' at line #" + i.ToString() + ". Needs to be less than "+(block+1).ToString() 
								+" at this point. Do not jump ahead.");
							b = -1;
						}
					}
					else NeuroLog.Log("Invalid value for 'BlockNum' at line #" + i.ToString() + ". Needs to be a int.");
				}
			}
			
			if(b != block && b!= -1) block = b;
			
			if(dot!=-1 && b !=-1) implicitEvents.Add(new ImplicitEvent(dot,block));
			
			i++;
		}
	
		return implicitEvents;
	}
	
	//Method used to read in all the trials of the game
	private List<EventStats> ReadStoppingEvents(CsvReader csv){
		
		List<EventStats> stoppingEvents = new List<EventStats>();
		
    	int fieldCount = csv.FieldCount;

    	string[] headers = csv.GetFieldHeaders();
		
		int i =2;
		
    	while (csv.ReadNextRecord()){
			
			int dot=-1;
			bool go =true;
			
			for(int j = 0;j<fieldCount;j++){
				if(headers[j].ToLower() == "dot"){	
					if(int.TryParse(csv[j],out dot)){
						if(dot <1  && dot>4){
							NeuroLog.Log("Invalid value for 'dot' at line #" + i.ToString() + ". Needs to be between 0 and 4.");
							dot = -1;
						}
					}
					else NeuroLog.Log("Invalid value for 'dot' at line #" + i.ToString() + ". Needs to be a int.");
				}
				else if (headers[j].ToLower() =="go"){
					if(!bool.TryParse(csv[j],out go))
						NeuroLog.Log("Invalid value for 'go' at line #" + (i+1).ToString() + ". Needs to be an bool.");
				}
			}
			
			if(dot!=-1) stoppingEvents.Add(new StoppingEvent(dot,go));
			
			i++;
		}
		
		return stoppingEvents;
	}
	
	//Method used to read in all the trials of the game
	private List<EventStats> ReadAssociateEvents(CsvReader csv){
		
		List<EventStats> associateEvents = new List<EventStats>();
		
    	int fieldCount = csv.FieldCount;

    	string[] headers = csv.GetFieldHeaders();
		
		int i =2;			
		
    	while (csv.ReadNextRecord()){

			int target = -1;
			List<int> stim = new List<int>();
			
			for(int j = 0;j<fieldCount;j++){
				if(headers[j].ToLower() == "target"){	
					if(int.TryParse(csv[j].ToLower(),out target)){
						if(target <1  && target>12){
							NeuroLog.Log("Invalid value for 'target' at line #" + i.ToString() + ". Needs to be between 1 and 12.");
							target = -1;
						}
					}
					else NeuroLog.Log("Invalid value for 'target' at line #" + i.ToString() + ". Needs to be a int.");
				}
				//Stimuli
				else if(headers[j].ToLower() =="stimuli"){
					string[] input = csv[j].Split(';');
					
					int output;
					
					//For the first four values
					for(int s = 0;s<4;s++){
						if(int.TryParse(input[s], out output)){
							if(output<1  && output>12)
								NeuroLog.Log("Invalid value for 'stimuli["+s.ToString()+"]' at event #" + (i+1).ToString() + ". Needs to be between 1 and 12.");
							else
								stim.Add(output);
						}
						else
							NeuroLog.Log("Invalid value for stimuli["+s.ToString()+"]' at event #" + (i+1).ToString() + ". Needs to be a int.");
					}
				}
			}
			
			if(target != -1 && stim.Count==4){
				
				int correspondingNum=0;
				
				switch(target){
					case 1: correspondingNum = 12;
							break;
	 				case 2: correspondingNum = 9;
							break;
	 				case 3: correspondingNum = 11;
							break;
	 				case 4: correspondingNum = 7;
							break;
	 				case 5: correspondingNum = 8;
							break;
					case 6: correspondingNum = 10;
							break;
					case 7: correspondingNum = 4;
							break;
					case 8: correspondingNum = 5;
							break;
					case 9: correspondingNum = 2;
							break;
					case 10: correspondingNum = 6;
							break;
					case 11: correspondingNum = 3;
							break;
					case 12: correspondingNum = 1;
							break;
				}
				
				if(stim.Contains(correspondingNum))
					associateEvents.Add(new AssociateEvent(target,stim));
				else
					NeuroLog.Log("Invalid trial. One stimuli value must be '"+ correspondingNum + "' to match the target value of '" + target + "'.");
			}
			
			i++;
		}
	
		
		return associateEvents;
	}

	private List<EventStats> ReadMemAttEncEvents(CsvReader csv){
		
		List<EventStats> events = new List<EventStats>();
		
		int fieldCount = csv.FieldCount;
		
		string[] headers = csv.GetFieldHeaders();
		
		int i = 2; // start on second line		

		while (csv.ReadNextRecord()){
			
			int quadrant = -1;
			
			string stimulus = "";
			
			for(int j = 0;j<fieldCount;j++){
				if(headers[j].ToLower() == "quadrant"){	
					if(int.TryParse(csv[j],out quadrant)){
						if(quadrant <1  && quadrant>4){
							NeuroLog.Log("Invalid value for 'quadrant' at line #" + i.ToString() + ". Needs to be between 0 and 4.");
							quadrant = -1;
						}
					}
					else NeuroLog.Log("Invalid value for 'quadrant' at line #" + i.ToString() + ". Needs to be a int.");
				}
				else if(headers[j].ToLower() == "stimulus"){	
					stimulus = csv[j].ToLower();
				}
			}

			if(quadrant != -1 && stimulus != "") events.Add(new MemAttentionEvent(quadrant,stimulus));
			
			i++;
		}
		
		return events;
	}
	
	//Method used to write out the log file once the game has completed
	//takes  bool that indicates whether the session was properly completed
	public void WriteOut(bool completed){
		
		string filePath="";
		
		//Generate the file name
		GenerateNewTimestamp();
		
		if(completed)
			filePath = Path.Combine(LogFilesPath, statsXML);
		else
			filePath = Path.Combine(UnfinishedSessionPath, statsXML);
		
		if(gm.SType == GameManager.SessionType.Spatial) WriteOutSpatial(filePath);
		else if(gm.SType == GameManager.SessionType.Inhibition) WriteOutInhibition(filePath);
		else if (gm.SType == GameManager.SessionType.Star) WriteOutStar(filePath);
		else if (gm.SType == GameManager.SessionType.Implicit) WriteOutImplicit(filePath);
		else if (gm.SType == GameManager.SessionType.Associate) WriteOutAssociate(filePath);
		else if (gm.SType == GameManager.SessionType.Stopping) WriteOutStopping(filePath);
		else if (gm.SType == GameManager.SessionType.MemAttentEnc2) WriteOutMemAttentionEnc2(filePath);

		if(completed){

			try{
				//Path.Combine(CsvManager.PlayerSpecificPath, name+"_" + System.Environment.MachineName+"_Criterion.csv"
				filePath = Path.Combine(CsvManager.PlayerSpecificPath, mUserName+"_" + System.Environment.MachineName+"_Criterion.csv");
				Debug.Log("saving criterion to: " + filePath + "," + gm.SType);

				WriteOutCriterion(filePath, gm.SType);
			}catch(Exception e){
				NeuroLog.Log("Unable to update the player's Criterion file");
				NeuroLog.Log(e.Message);
			}
		}
	}
	
	private void WriteOutSpatial(string filePath){
				
		using(StreamWriter writer = new StreamWriter(filePath)){
			string newValue = "";
			string newLine = "Dot Locations";
			
			for(float i =1; i<10;i++){
			
				newValue = " Dot " + i+", ";
				
				Vector2 dotPos = Vector2.zero;
				
				if(i%3 == 0) dotPos.x = Screen.width*(5f/6f);
				else if((i+1)%3==0) dotPos.x = Screen.width*(3f/6f);
				else dotPos.x = Screen.width/6f;
			
				if(i/3f>2)dotPos.y = Screen.height*(5f/6f);
				else if(i/3f>1) dotPos.y = Screen.height*(3f/6f);
				else dotPos.y = Screen.height/6f;
				
				newValue += dotPos.x +"; "+dotPos.y;
				
				newLine += newValue;
				if(i<9)
					newLine+= ",";
			}
			
	   		writer.WriteLine(newLine);
			
			newLine = "Practice, TrialNum, NumDots, ShownDots, Delay, TimeOut,  EarlyResponse, DotPressed, ReactionTime, TouchPosition, DistanceFromCenter";
	   		
			writer.WriteLine(newLine);
			
			int index = 1;
			//Loop through all the events and write them out
			foreach(SpatialEvent eS in gm.Practice){
				
				if(eS.Completed){
					if(eS.Responses.Count ==0 && eS.BadResponses.Count ==0){
						newLine = " ,";
							
						newLine += index.ToString()+"," + eS.Dots.Count.ToString() + ",";
						
						if(eS.Dots.Count>2)
							newLine += eS.Dots[0].ToString()+";"+eS.Dots[1].ToString()+";"+eS.Dots[2].ToString()+",";
						else if(eS.Dots.Count>1)
							newLine += eS.Dots[0].ToString()+";"+eS.Dots[1].ToString()+",";
						else
							newLine += eS.Dots[0].ToString()+",";
						
						newLine += eS.Delay.ToString()+",";
							
						newLine += eS.TimedOut.ToString()+", ., ., ., ., .";
							
						writer.WriteLine(newLine);
					}
					else{
						foreach(Response r in eS.BadResponses){
							
							newLine = " ,";
							
							newLine += index.ToString()+"," + eS.Dots.Count.ToString() + ",";
							if(eS.Dots.Count>2)
								newLine += eS.Dots[0].ToString()+";"+eS.Dots[1].ToString()+";"+eS.Dots[2].ToString()+",";
							else if(eS.Dots.Count>1)
								newLine += eS.Dots[0].ToString()+";"+eS.Dots[1].ToString()+",";
							else
								newLine += eS.Dots[0].ToString()+",";
							
							newLine += eS.Delay.ToString()+",";
							
							newLine += "False,";
							
							newLine += "True,";
							
							newLine += r.DotPressed.ToString()+",";
							
							newLine += r.ResponseTime.ToString()+",";
							
							newLine += r.TouchLocation.x.ToString()+ ";"+ r.TouchLocation.y.ToString()+",";
							
							newLine += r.DistanceFromCenter.ToString();
							
							writer.WriteLine(newLine);
						}
					
						foreach(Response r in eS.Responses){
							
							newLine = " ,";
							
							newLine += index.ToString()+"," + eS.Dots.Count.ToString() + ",";;
							if(eS.Dots.Count>2)
								newLine += eS.Dots[0].ToString()+";"+eS.Dots[1].ToString()+";"+eS.Dots[2].ToString()+",";
							else if(eS.Dots.Count>1)
								newLine += eS.Dots[0].ToString()+";"+eS.Dots[1].ToString()+",";
							else
								newLine += eS.Dots[0].ToString()+",";
							
							newLine += eS.Delay.ToString()+",";
							
							newLine += "False,";
							
							newLine += "False,";
							
							newLine += r.DotPressed.ToString()+",";
							
							newLine += r.ResponseTime.ToString()+",";
							
							newLine += r.TouchLocation.x.ToString()+ ";"+ r.TouchLocation.y.ToString()+",";
							
							newLine += r.DistanceFromCenter.ToString();
							
							writer.WriteLine(newLine);
						}
						if(eS.Responses.Count< eS.Dots.Count){
							int i = eS.Responses.Count;
							while(i<eS.Dots.Count){
								newLine = " ,";
							
								newLine += index.ToString()+"," + eS.Dots.Count.ToString() + ",";
								if(eS.Dots.Count>2)
									newLine += eS.Dots[0].ToString()+";"+eS.Dots[1].ToString()+";"+eS.Dots[2].ToString()+",";
								else if(eS.Dots.Count>1)
									newLine += eS.Dots[0].ToString()+";"+eS.Dots[1].ToString()+",";
								else
									newLine += eS.Dots[0].ToString()+",";
						
								newLine += eS.Delay.ToString()+",";
								
								newLine +="True, ., ., ., ., .";
								
								writer.WriteLine(newLine);
								
								i++;
							}
						}
					}
				}
				index++;
			}
			
			newLine = "Task, TrialNum, NumDots, ShownDots, Delay, TimeOut, EarlyResponse, DotPressed, ReactionTime, TouchPosition, DistanceFromCenter";
		   		
			writer.WriteLine(newLine);
				
			index = 1;
			
			foreach(SpatialEvent eS in gm.Events){
				
				if(eS.Completed){
					if(eS.Responses.Count ==0 && eS.BadResponses.Count ==0){
						newLine = " ,";
						
						newLine += index.ToString()+"," + eS.Dots.Count.ToString() +",";
						if(eS.Dots.Count>2)
							newLine += eS.Dots[0].ToString()+";"+eS.Dots[1].ToString()+";"+eS.Dots[2].ToString()+",";
						else if(eS.Dots.Count>1)
							newLine += eS.Dots[0].ToString()+";"+eS.Dots[1].ToString()+",";
						else
							newLine += eS.Dots[0].ToString()+",";
						
						newLine += eS.Delay.ToString()+",";
							
						newLine += eS.TimedOut.ToString()+", ., ., ., ., .";
							
						writer.WriteLine(newLine);
					}
					else{
						foreach(Response r in eS.BadResponses){
							
							newLine = " ,";
							
							newLine += index.ToString()+"," + eS.Dots.Count.ToString() + ",";
							if(eS.Dots.Count>2)
								newLine += eS.Dots[0].ToString()+";"+eS.Dots[1].ToString()+";"+eS.Dots[2].ToString()+",";
							else if(eS.Dots.Count>1)
								newLine += eS.Dots[0].ToString()+";"+eS.Dots[1].ToString()+",";
							else
								newLine += eS.Dots[0].ToString()+",";
							
							newLine += eS.Delay.ToString()+",";
							
							newLine += "False,";
							
							newLine += "True,";
							
							newLine += r.DotPressed.ToString()+",";
							
							newLine += r.ResponseTime.ToString()+",";
							
							newLine += r.TouchLocation.x.ToString()+ ";"+ r.TouchLocation.y.ToString()+",";
							
							newLine += r.DistanceFromCenter.ToString();
							
							writer.WriteLine(newLine);
						}
					
						foreach(Response r in eS.Responses){
							
							newLine = " ,";
							
							newLine += index.ToString()+"," + eS.Dots.Count.ToString() + ",";
							if(eS.Dots.Count>2)
								newLine += eS.Dots[0].ToString()+";"+eS.Dots[1].ToString()+";"+eS.Dots[2].ToString()+",";
							else if(eS.Dots.Count>1)
								newLine += eS.Dots[0].ToString()+";"+eS.Dots[1].ToString()+",";
							else
								newLine += eS.Dots[0].ToString()+",";
							
							newLine += eS.Delay.ToString()+",";
							
							newLine += "False,";
							
							newLine += "False,";
							
							newLine += r.DotPressed.ToString()+",";
							
							newLine += r.ResponseTime.ToString()+",";
							
							newLine += r.TouchLocation.x.ToString()+ ";"+ r.TouchLocation.y.ToString()+",";
							
							newLine += r.DistanceFromCenter.ToString();
							
							writer.WriteLine(newLine);
						}
						if(eS.Responses.Count< eS.Dots.Count){
							int i = eS.Responses.Count;
							while(i<eS.Dots.Count){
								newLine = " ,";
							
								newLine += index.ToString()+"," + eS.Dots.Count.ToString() + ",";
								if(eS.Dots.Count>2)
									newLine += eS.Dots[0].ToString()+";"+eS.Dots[1].ToString()+";"+eS.Dots[2].ToString()+",";
								else if(eS.Dots.Count>1)
									newLine += eS.Dots[0].ToString()+";"+eS.Dots[1].ToString()+",";
								else
									newLine += eS.Dots[0].ToString()+",";
						
								newLine += eS.Delay.ToString()+",";
								
								newLine +="True, ., ., ., ., .";
								
								writer.WriteLine(newLine);
								
								i++;
							}
						}
					}
				}
				index++;
			}
		}
	}
	
	private void WriteOutInhibition(string filePath){
		using(StreamWriter writer = new StreamWriter(filePath)){
			string newLine = "Dot Locations, Left, " + (Screen.width/4f).ToString() +";"+(Screen.height/2f).ToString()+
								", Right, " +(Screen.width*.75f).ToString() +";"+(Screen.height/2f).ToString();
			
			writer.WriteLine(newLine);
			
			newLine = "Practice, TrialNum, TargetSide, TimeOut, ReactionTime, TouchPosition, DistanceFromCenter, PressedSide, Correct";
	   		
			writer.WriteLine(newLine);
			
			int index =1;
			
			foreach(InhibitionEvent eS in gm.Practice){
				if(eS.Completed){
					newLine = " ,";
					
					if(eS.Response ==null){
							
						newLine += index.ToString()+",";
						
						if(eS.DotColor =="yellow") newLine += "Same,";
						else newLine += "Opposite,";
							
						newLine += "True, ., ., ., ., 0";
							
						writer.WriteLine(newLine);
					}
					else{
							
						newLine = " ,";
						
						newLine += index.ToString()+",";
					
						if(eS.DotColor =="yellow") newLine += "Same,";
						else newLine += "Opposite,";
						
						newLine += "False,";
						
						newLine += eS.Response.ResponseTime.ToString()+",";
						
						newLine += eS.Response.TouchLocation.x.ToString()+ ";"+ eS.Response.TouchLocation.y.ToString()+",";
						
						newLine += eS.Response.DistanceFromCenter.ToString()+",";
					
						if(eS.Response.DotPressed ==1) newLine +=" Right,";
						else newLine +=" Left,";
					
						if(eS.respondedCorrectly()) newLine +="1";
						else newLine +="0";
						
						writer.WriteLine(newLine);
					
					}
				}
				index++;
			}
			
			newLine = "Task, TrialNum, TargetSide, TimeOut, ReactionTime, TouchPosition, DistanceFromCenter, PressedSide, Correct";
	   		
			writer.WriteLine(newLine);
			
			index =1;
			
			foreach(InhibitionEvent eS in gm.Events){
				if(eS.Completed){
					newLine = " ,";
					
					if(eS.Response ==null){
							
						newLine += index.ToString()+",";
						
						if(eS.DotColor =="yellow") newLine += "Same,";
						else newLine += "Opposite,";
							
						newLine += "True, ., ., ., ., 0";
							
						writer.WriteLine(newLine);
					}
					else{
							
						newLine = " ,";
						
						newLine += index.ToString()+",";
					
						if(eS.DotColor =="yellow") newLine += "Same,";
						else newLine += "Opposite,";
						
						newLine += "False,";
						
						newLine += eS.Response.ResponseTime.ToString()+",";
						
						newLine += eS.Response.TouchLocation.x.ToString()+ ";"+ eS.Response.TouchLocation.y.ToString()+",";
						
						newLine += eS.Response.DistanceFromCenter.ToString()+",";
					
						if(eS.Response.DotPressed ==1) newLine +=" Right,";
						else newLine +=" Left,";
					
						if(eS.respondedCorrectly()) newLine +="1";
						else newLine +="0";
						
						writer.WriteLine(newLine);
					
					}
				}
				index++;
			}
		}
	}
	
	private void WriteOutStar(string filePath){
		
		float avggood=0;
		float avgbad=0;
		float avgrepeat=0;
		float avgtimetarget=0;
		float avgtimeaction=0;
		float avgstd=0;
		List<float> avgperArea=new List<float>(){0,0,0,0,0,0,0,0};
		float avglocation=0;
		float avgfirst=0;
		float avglast=0;
		float avgdist=0;
		
		foreach(StarEvent eS in gm.Events){
			if(eS.Completed){
				float good=eS.NumGoodTouches;
				avggood +=good;
				float bad=eS.NumBadTouches;
				avgbad+=bad; 
				float repeat=eS.RepeatTouches;
				avgrepeat +=repeat;
				float timetarget=eS.AvgTimePerTarget();
				avgtimetarget += timetarget;
				float timeaction=eS.AvgTimePerAction();
				avgtimeaction += timeaction;
				float std=eS.StD();
				avgstd+= std;
				List<int> perArea= eS.correctPerArea();
				for(int i =0;i<perArea.Count;i++){
					avgperArea[i] += perArea[i];
				}
				float location=eS.AvgDistanceofTargets();
				avglocation+= location;
				float first=eS.AvgTimeStart();
				avgfirst+= first;
				float last=eS.AvgTimeLast();
				avglast+= last;
				float dist=eS.AvgDistance();
				avgdist+= dist;	
			}
		}
		
		avggood/=(gm.Events.Count);
		avgbad/=(gm.Events.Count);
		avgrepeat/=(gm.Events.Count);
		avgtimetarget/=(gm.Events.Count);
		avgtimeaction/=(gm.Events.Count);
		avgstd/=(gm.Events.Count);
		string avgArea="(";
		for(int i =0;i<avgperArea.Count;i++){
			avgperArea[i] /= (gm.Events.Count);
			
			avgArea += avgperArea[i];
			if(i<avgperArea.Count-1)
				avgArea+=";";
			else
				avgArea+=")";
		}
		avglocation/=(gm.Events.Count );
		avgfirst/=(gm.Events.Count );
		avglast/=(gm.Events.Count );
		avgdist/=(gm.Events.Count);
		
		using(StreamWriter writer = new StreamWriter(filePath)){
			
			int eventIndex = 1;
			int responseIndex = 1;
			string newLine ="Global Averages, AvgGoodTouches, " + avggood.ToString() + ", AvgBadTouches, " + avgbad.ToString() + ", AvgRepeats, "
							+ avgrepeat.ToString() + ", AvgTimePerTarget, " + avgtimetarget.ToString() + ", AvgStandardDeviation, " + avgstd.ToString() 
							+ ", AvgTimePerAction, " + avgtimeaction.ToString() + ", AvgTargetsPerArea, " + avgArea + ", AvgLocation, " + avglocation.ToString()
							+ ", AvgFirstTen, " +  avgfirst.ToString() + ", AvgLastTen, " + avglast.ToString() +", AvgDistancePerTarget,"+ avgdist.ToString();
			writer.WriteLine(newLine);
			
			foreach(StarEvent eS in gm.Practice){
				responseIndex = 1;
				if(eS.Completed){
					newLine = "Practice " +eventIndex+", EndCondition, "+ eS.EndCondition+ ", Duration, " + eS.Duration.ToString()
						+ ", NumGoodTouches, " + eS.NumGoodTouches.ToString() + ", NumBadTouches, " + eS.NumBadTouches.ToString()
							+ ", NumRepeats, " + eS.RepeatTouches.ToString() + ", AvgTimePerTarget, " + eS.AvgTimePerTarget().ToString()
							+ ", StandardDeviation, " + eS.StD().ToString() + ", AvgTimePerAction, " + eS.AvgTimePerAction().ToString() + ", AvgTargetsPerArea";
					
					List<int> perArea = eS.correctPerArea();
					string pArea=", (";
					for(int i =0;i<perArea.Count;i++){
						pArea += perArea[i];
						if(i<perArea.Count-1) pArea+=";";
						else pArea+="),";
					}
					
					newLine += (pArea + " AvgLocation, " + eS.AvgDistanceofTargets().ToString() + ", AvgFirstTen, " +eS.AvgTimeStart().ToString()
						+", AvgLastTen, " +  eS.AvgTimeLast().ToString() + ", AvgDistancePerTarget, " + eS.AvgDistance().ToString());
					
					writer.WriteLine(newLine);
					
					newLine = "ResponseNum, ResponseType, ResponseTime, TouchPosition";
					
					writer.WriteLine(newLine);
					
					foreach(Response r in eS.Responses){
						newLine = responseIndex+", ";
						
						string t ="";
						if(r.ResponseType==0)
							t="GOOD";
						else if(r.ResponseType==1)
							t="BAD";
						else if(r.ResponseType==2)
							t="REPEAT";
						
						newLine += (t + ", " +r.ResponseTime.ToString() +", (" + r.TouchLocation.x.ToString()+";"+r.TouchLocation.y.ToString()+")");
						
						writer.WriteLine(newLine);
						
						responseIndex++;
					}
				}
				eventIndex++;
			}
			
			newLine ="";
			eventIndex = 1;
		
			foreach(StarEvent eS in gm.Events){
				responseIndex = 1;
				if(eS.Completed){
					newLine = "Task " +eventIndex+", EndCondition, "+ eS.EndCondition+ ", Duration, " + eS.Duration.ToString()
						+ ", NumGoodTouches, " + eS.NumGoodTouches.ToString() + ", NumBadTouches, " + eS.NumBadTouches.ToString()
							+ ", NumRepeats, " + eS.RepeatTouches.ToString() + ", AvgTimePerTarget, " + eS.AvgTimePerTarget().ToString()
							+ ", StandardDeviation, " + eS.StD().ToString() + ", AvgTimePerAction, " + eS.AvgTimePerAction().ToString() + ", AvgTargetsPerArea";
					
					List<int> perArea = eS.correctPerArea();
					string pArea=", (";
					for(int i =0;i<perArea.Count;i++){
						pArea += perArea[i];
						if(i<perArea.Count-1) pArea+=";";
						else pArea+="),";
					}
					
					newLine += (pArea + " AvgLocation, " + eS.AvgDistanceofTargets().ToString() + ", AvgFirstTen, " +eS.AvgTimeStart().ToString()
						+", AvgLastTen, " +  eS.AvgTimeLast().ToString() + ", AvgDistancePerTarget, " + eS.AvgDistance().ToString());
					
					writer.WriteLine(newLine);
					
					newLine = "ResponseNum, ResponseType, ResponseTime, TouchPosition";
					
					writer.WriteLine(newLine);
					
					foreach(Response r in eS.Responses){
						newLine = responseIndex+", ";
						
						string t ="";
						if(r.ResponseType==0)t="GOOD";
						else if(r.ResponseType==1)t="BAD";
						else if(r.ResponseType==2)t="REPEAT";
						
						newLine += (t + ", " +r.ResponseTime.ToString() +", " + ", (" + r.TouchLocation.x.ToString()+";"+r.TouchLocation.y.ToString()+")");
						
						writer.WriteLine(newLine);
						
						responseIndex++;
					}
				}
				eventIndex++;
			}
		}
	}
	
	private void WriteOutImplicit(string filePath){
		
		using(StreamWriter writer = new StreamWriter(filePath)){
			
			Vector2[] pos = ((ImplicitManager)gm).StimPositions;
			
			for(int i = 0;i<pos.Length;i++){
				pos[i].x = ((pos[i].x + 26.7f)/53.4f) * Screen.width;
						
				pos[i].y = ((pos[i].y - 15f)/-30f) * Screen.height;
			}
			
			
			string newLine = "Dot Locations, Dot1, " + pos[0].x.ToString() + "; " + pos[0].y.ToString()
								+ ", Dot2, " + pos[1].x.ToString() + "; " + pos[1].y.ToString()
								+ ", Dot3, " + pos[2].x.ToString() + "; " + pos[2].y.ToString()
								+ ", Dot4, " + pos[3].x.ToString() + "; " + pos[3].y.ToString();
			
			writer.WriteLine(newLine);
			
			newLine = "Practice, TrialNum, Correct, ResponseTime, TouchPosition, DistanceFromCenter";
			
			writer.WriteLine(newLine);
			
			int index = 1;
			
			//Loop through all the events and write them out
			foreach(ImplicitEvent eS in gm.Practice){
				if(eS.Completed){
					newLine = ", "+index+", ";
			
					if(eS.Response==null)
						newLine+= "False, ., ., .";
					else
						newLine+= "True, " +eS.Response.ResponseTime.ToString()+", ("+eS.Response.TouchLocation.x.ToString()+
							";" +eS.Response.TouchLocation.y.ToString()+ "), "+eS.Response.DistanceFromCenter.ToString();
			
					writer.WriteLine(newLine);
				}
				index++;
			}
			
			index = 1;
			
			List<List<ImplicitEvent>> b = new List<List<ImplicitEvent>>();
		
			b.Add(new List<ImplicitEvent>());
		
			int blockInt=0;
		
			//Shift out the event
			foreach(ImplicitEvent iE in gm.Events){
				if(b[blockInt].Count == 0)
					b[blockInt].Add(iE);
				else{
					if(iE.BlockNum != b[blockInt][0].BlockNum){
						b.Add(new List<ImplicitEvent>());
						blockInt++;
					}
				
					b[blockInt].Add(iE);
				}
			}
			
			blockInt = 1;
			
			foreach(List<ImplicitEvent> iEs in b){
			
				float avgTime=0;
				float avgDist=0;
				float responseCount=0;
				
				foreach(ImplicitEvent iE in iEs){
					if(iE.Completed){
						if(iE.Response!=null){
							responseCount++;
							avgTime += iE.Response.ResponseTime;
							avgDist += iE.Response.DistanceFromCenter;
						}
					}
				}
				
				avgTime= avgTime / (responseCount==0? 1 : responseCount);
				avgDist = avgDist / (responseCount==0? 1 : responseCount);
				responseCount = (responseCount / (float)iEs.Count) *100;
			
				newLine = "Block " + blockInt +" Calculations, PercentCorrect, " + responseCount +"%, AvgDistanceFromCenter, "
						+ avgDist.ToString()+", AvgResponseTime, " + avgTime.ToString();
			
				writer.WriteLine(newLine);
				
				newLine = ", TrialNum, Correct, ResponseTime, TouchPosition, DistanceFromCenter";
			
				writer.WriteLine(newLine);
				
				//Loop through all the events and write them out
				foreach(ImplicitEvent iE in iEs){
					
					if(iE.Completed){
						
						newLine =" , " +index+", ";
					
						if(iE.Response==null)
							newLine+= "False, ., ., .";
						else
							newLine+= "True, " +iE.Response.ResponseTime.ToString()+", ("+iE.Response.TouchLocation.x.ToString()+
								";" +iE.Response.TouchLocation.y.ToString()+ "), "+iE.Response.DistanceFromCenter.ToString();
						
						writer.WriteLine(newLine);
					}
					
					index++;
				}
				
				blockInt++;
			}
		}	
	}
	
	private void WriteOutStopping(string filePath){
		
		float avgTime=0;
		float avgDist=0;
		float responseCount=0;
		float blueCount=0;
		float avgCorrectBlue=0;
		float orangeCount=0;
		float avgCorrectOrange=0;
		//float avgTurningTime=0;
		
		//Loop through all the events and get averages/percentages
		foreach(StoppingEvent eS in gm.Events){
			if(eS.Completed){
				if(eS.Go) blueCount++;
				else orangeCount++;
				
				//avgTurningTime += eS.TurningTime;
				
				if((eS.Response==null && !eS.Go) || (eS.Response!=null && eS.Go)){
					if(eS.Go)
						avgCorrectBlue++;
					else{
						avgCorrectOrange++;
					}
				}
				
				if(eS.Response!=null ){
					if(eS.Go){
						responseCount++;
						avgTime += eS.Response.ResponseTime;
						avgDist += eS.Response.DistanceFromCenter;
					}
				}
			}
		}
		
		using(StreamWriter writer = new StreamWriter(filePath)){
			
			Vector2[] pos = ((StoppingManager)gm).StimPositions;
			
			for(int i = 0;i<pos.Length;i++){
				pos[i].x = ((pos[i].x + 26.7f)/53.4f) * Screen.width;
						
				pos[i].y = ((pos[i].y - 15f)/-30f) * Screen.height;
			}
			
			
			string newLine = "Dot Locations, Dot1, " + pos[0].x.ToString() + "; " + pos[0].y.ToString()
								+ ", Dot2, " + pos[1].x.ToString() + "; " + pos[1].y.ToString()
								+ ", Dot3, " + pos[2].x.ToString() + "; " + pos[2].y.ToString()
								+ ", Dot4, " + pos[3].x.ToString() + "; " + pos[3].y.ToString();
			
			writer.WriteLine(newLine);
			
			newLine = "Calculations, GoPercentCorrect, " + (avgCorrectBlue/(blueCount==0? 1 :blueCount)).ToString()
						+ ", AvgDistanceFromCenter, " + (avgDist / (responseCount==0? 1 : responseCount)).ToString()
						+ ", AvgResponseTime, " + (avgTime / (responseCount==0? 1 : responseCount)).ToString()
						+ ", StopPercentCorrect, " + (avgCorrectOrange/(orangeCount==0? 1 : orangeCount)).ToString()
						/*+ ", AvgTurningTime: " + (avgTurningTime/(orangeCount==0? 1 : orangeCount)).ToString()*/;
			
			writer.WriteLine(newLine);
			
			newLine = "Practice, TrialNum, TurnedOrange, Correct, ReactionTime, TouchPosition, DistanceFromCenter";
	   	
			writer.WriteLine(newLine);
			
			int index = 1;
			
			//Loop through all the events and write them out
			foreach(StoppingEvent eS in gm.Practice){
				
				newLine =", " +index +", ";
				
				if(eS.Completed){
					newLine += (!eS.Go).ToString()+", "/*+ eS.TurningTime.ToString()+", "*/;
					
					if((eS.Response==null && !eS.Go) || (eS.Response!=null && eS.Go))
						newLine += "True,";
					else  newLine += "False,";
					
					if(eS.Response!=null ){
						newLine +=eS.Response.ResponseTime.ToString() +", (" + eS.Response.TouchLocation.x.ToString()
								+"; " +eS.Response.TouchLocation.y.ToString()+"), " +eS.Response.DistanceFromCenter.ToString();
					}
					else{
						newLine +=".,.,.";
					}
					
					writer.WriteLine(newLine);
				}
				index++;
			}
			
			newLine = "Trial, TrialNum, TurnedOrange, TurningTime, Correct, ReactionTime, TouchPosition, DistanceFromCenter";
	   	
			writer.WriteLine(newLine);
			
			index = 1;
			//Loop through all the events and write them out
			foreach(StoppingEvent eS in gm.Events){
				
				newLine =", " +index +", ";
				
				if(eS.Completed){
					newLine += (!eS.Go).ToString()+", "/*+ eS.TurningTime.ToString()+", "*/;
					
					if((eS.Response==null && !eS.Go) || (eS.Response!=null && eS.Go))
						newLine += "True,";
					else  newLine += "False,";
					
					if(eS.Response!=null ){
						newLine +=eS.Response.ResponseTime.ToString() +", (" + eS.Response.TouchLocation.x.ToString()
								+"; " +eS.Response.TouchLocation.y.ToString()+"), " +eS.Response.DistanceFromCenter.ToString();
					}
					else{
						newLine +=".,.,.";
					}
					
					writer.WriteLine(newLine);
				}
				index++;
			}
		}
	}
	
	private void WriteOutAssociate(string filePath){
		
		float avgBadTouch =0;
		int indexOfLastSpike=-1;
		int countOfRightCategory=0;
		int indexOfFiftyPercent=-1;
		float probabilityCorrectTouch = 0;
		
		int index =0;
		
		//Loop through all the events and calculate stuff
		foreach(AssociateEvent eS in gm.Events){
			if(eS.Completed){
			
				avgBadTouch +=(eS.Responses.Count-1);
			
				if(index>=1)
					if(((AssociateEvent)gm.Events[index-1]).Responses.Count<eS.Responses.Count) indexOfLastSpike = index;
			
				if(eS.Responses.Count>1){
					int target = eS.TargetImage;
					
					if(target<7){
						if(eS.Responses[0].DotPressed>=7) countOfRightCategory++;
					}
					else{
						if(eS.Responses[0].DotPressed<7) countOfRightCategory++;
					}
				}
				else countOfRightCategory++;
			
				probabilityCorrectTouch = ((float)countOfRightCategory)/((float)(index+1));
			
				if(probabilityCorrectTouch<.5 &&indexOfFiftyPercent !=-1)
					indexOfFiftyPercent = -1;
				else if(probabilityCorrectTouch>=.5 &&indexOfFiftyPercent ==-1)
					indexOfFiftyPercent = index;
			}
			index++;
		}
		
		avgBadTouch = avgBadTouch/(float)gm.Events.Count;
		
		using(StreamWriter writer = new StreamWriter(filePath)){
			
			AssociateManager aM = (AssociateManager)(gm);
			
			float[] xPos = new float[4];
			
			float yPos = ((-7.5f - 15f)/-30f) * Screen.height;
			
			for(int i = 0;i<4;i++){
				xPos[i] = (((-18+(12*i)) + 26.7f)/53.4f) * Screen.width;
			}
			
			string newLine = "Stim Locations, Stim1, " + xPos[0].ToString() + "; " + yPos.ToString()
								+ ", Stim2, " + xPos[1].ToString() + "; " + yPos.ToString()
								+ ", Stim3, " + xPos[2].ToString() + "; " + yPos.ToString()
								+ ", Stim4, " + xPos[3].ToString() + "; " + yPos.ToString();
			
			writer.WriteLine(newLine);
			
			newLine = "Calculations, AvgNumBadTouch," + avgBadTouch.ToString()
						+ ", LearningRateItems, "+ (indexOfLastSpike==-1? "NaN" : (indexOfLastSpike+1).ToString())
						+ ", AvgProbabilityCorrectCategory, " + Mathf.Round(probabilityCorrectTouch *100).ToString() 
						+"%, LearningRateCategory, " + (indexOfFiftyPercent==-1 ? "NaN" : (indexOfFiftyPercent+1).ToString());
			
			writer.WriteLine(newLine);
				
			index = 1;
			
			if(gm.Practice.Count>0){
				newLine = "Practice, TrialNum, TargetID, Stimuli, TouchedID, TouchPosition, NumBadTouches, Score";
			
				writer.WriteLine(newLine);
			}
			
			//Loop through all the practice trials
			foreach(AssociateEvent eS in gm.Practice){
				if(eS.Completed){
					int target=eS.TargetImage;
					string availableStim=Mathf.Abs(eS.Stimuli[0]) +";" +Mathf.Abs(eS.Stimuli[1]) +";" +Mathf.Abs(eS.Stimuli[2]) +";" +Mathf.Abs(eS.Stimuli[3]);
					
					foreach(Response r in eS.Responses){
						int score = 0;
						
						if(aM.getMatchingNum(target) == r.DotPressed) 
							score = 0;
						else{
							if(target<7){
								if(r.DotPressed>=7) score =1;
								else score =2;
							}
							else{
								if(r.DotPressed>=7) score =2;
								else score =1;
							}
						}
						
						newLine=", " + index + ", " + target +", " + availableStim+ ", " + Mathf.Abs(r.DotPressed) +", "
							+ r.TouchLocation.x +";" + r.TouchLocation.y+", " +(eS.Responses.Count-1).ToString()+", " + score;
					
						writer.WriteLine(newLine);
					}
				}
				index++;
			}
			
			index =1;
			
			newLine = "Task, TrialNum, TargetID, Stimuli, TouchedID, TouchPosition, NumBadTouches, Score";
			
			writer.WriteLine(newLine);
			//Loop through all the real trials
			foreach(AssociateEvent eS in gm.Events){
				if(eS.Completed){
					int target=eS.TargetImage;
					string availableStim=eS.Stimuli[0] +";" +eS.Stimuli[1] +";" +eS.Stimuli[2] +";" +eS.Stimuli[3];
					
					foreach(Response r in eS.Responses){
						int score = 0;
						
						if(aM.getMatchingNum(target) == r.DotPressed) 
							score = 0;
						else{
							if(target<7){
								if(r.DotPressed>=7) score =1;
								else score =2;
							}
							else{
								if(r.DotPressed>=7) score =2;
								else score =1;
							}
						}
						
						newLine=", " + index + ", " + target +", " + availableStim+ ", " + Mathf.Abs(r.DotPressed) +", "
							+ r.TouchLocation.x +";" + r.TouchLocation.y+", " +(eS.Responses.Count-1).ToString()+", " + score;
					
						writer.WriteLine(newLine);
					}
				}
				index++;
			}
		}
	}


	private void WriteOutMemAttentionEnc2(string filePath){
		using(StreamWriter writer = new StreamWriter(filePath)){

			int leftX = Mathf.RoundToInt(Screen.width / 4f);
			int rightX = Mathf.RoundToInt(Screen.width * .75f);
			int topY = Mathf.RoundToInt(Screen.height * .75f);
			int bottomY = Mathf.RoundToInt(Screen.height / 4f);

			string newLine = "Quadrant1, " + rightX.ToString() + ";" + topY.ToString();
			writer.WriteLine(newLine);

			newLine = "Quadrant2, " + rightX.ToString() + ";" + bottomY.ToString();
			writer.WriteLine(newLine);

			newLine = "Quadrant3, " + leftX.ToString() + ";" + bottomY.ToString();
			writer.WriteLine(newLine);

			newLine = "Quadrant4, " + leftX.ToString() + ";" + topY.ToString();
			writer.WriteLine(newLine);

			newLine = "Practice, TrialNum, Screen, TouchTime, TouchPosition, CorrectPosition, TouchedQuadrant, CorrectQuadrant, Correct/Incorrect";
			
			writer.WriteLine(newLine);
			
			int index = 1;
			
			foreach(MemAttentionEvent eS in gm.Practice){

				if(eS.Completed){

					foreach(Response r in eS.Responses){

						newLine = " ,";
						
						newLine += index.ToString()+",";

						newLine += r.ScreenIndex.ToString()+",";

						newLine += RoundFloat(r.ResponseTime, 3).ToString()+",";

						newLine += Mathf.RoundToInt(r.TouchLocation.x).ToString()+ ";"+ Mathf.RoundToInt(r.TouchLocation.y).ToString()+",";

						newLine += Mathf.RoundToInt(r.StimulusLocation.x).ToString()+ ";"+ Mathf.RoundToInt(r.StimulusLocation.y).ToString() + ",";

						newLine += r.QuadrantTouched.ToString() + ",";

						newLine += eS.Quadrant.ToString() + ",";

						newLine += r.QuadrantTouched == eS.Quadrant ? "Correct" : "Incorrect";

						writer.WriteLine(newLine);
						
					}
				}
				index++;
			}
			
			newLine = "Study, TrialNum, Screen, TouchTime, TouchPosition, CorrectPosition, TouchedQuadrant, CorrectQuadrant, Correct/Incorrect";
			
			writer.WriteLine(newLine);
			
			index = 1;
			
			foreach(MemAttentionEvent eS in gm.Events){
				if(eS.Completed){
					foreach(Response r in eS.Responses){
						
						newLine = " ,";
						
						newLine += index.ToString()+",";

						newLine += r.ScreenIndex.ToString()+",";

						newLine += RoundFloat(r.ResponseTime, 3).ToString()+",";

						newLine += Mathf.RoundToInt(r.TouchLocation.x).ToString()+ ";"+ Mathf.RoundToInt(r.TouchLocation.y).ToString()+",";
						
						newLine += Mathf.RoundToInt(r.StimulusLocation.x).ToString()+ ";"+ Mathf.RoundToInt(r.StimulusLocation.y).ToString() + ",";
						
						newLine += r.QuadrantTouched.ToString() + ",";
						
						newLine += eS.Quadrant.ToString() + ",";
						
						newLine += r.QuadrantTouched == eS.Quadrant ? "Correct" : "Incorrect";

						writer.WriteLine(newLine);
						
					}
				}
				index++;
			}
		}
	}

	private void WriteOutCriterion(string criterionPath, GameManager.SessionType type){

		Debug.Log("WriteOutCriterion");

		string[] lines = System.IO.File.ReadAllLines(criterionPath);
		
		int taskNum = int.Parse(sessionXML.Replace("task",""));
		
		string newLine = taskNum+",";
		
		bool practicePassed=false;
		
		float responseCount = 0;
		float numberOfEarlyPresses=0;
		int practiceCount=gm.Practice.Count;
		
		if(type == GameManager.SessionType.Spatial){
			
			foreach(SpatialEvent e in gm.Practice){
				if(e.Responses.Count>=1){
					responseCount++;
				
					foreach(Response r in e.Responses ){
						if(r.ResponseTime<.2f)
							numberOfEarlyPresses++;
					}
				}
			}
		
			if((responseCount/practiceCount)>.7f && (numberOfEarlyPresses/responseCount)<.1f) practicePassed = true;
		}
		else if (type == GameManager.SessionType.Inhibition){
			foreach(InhibitionEvent e in gm.Practice){
				if(e.Response != null){
					responseCount++;
				
					if(e.Response.ResponseTime<.2f){
						numberOfEarlyPresses++;
					}
				}
			}
		
			if((responseCount/practiceCount)>.7f && (numberOfEarlyPresses/responseCount)<.1f) practicePassed = true;
		}
		else if (type == GameManager.SessionType.Star){
			practiceCount=0;
			
			foreach(StarEvent e in gm.Practice){
				practiceCount+= e.NumLittleStars;
				foreach(Response r in e.Responses){
					if(r.ResponseType ==0)
						responseCount++;
				}
			}
		
			if((responseCount/practiceCount)>.7f) practicePassed = true;
		}
		else if (type == GameManager.SessionType.Implicit){
			foreach(ImplicitEvent e in gm.Practice){
				if(e.Response!= null){
					responseCount++;
				
					if(e.Response.ResponseTime<.2f)	numberOfEarlyPresses++;
				}
			}
		
			if((responseCount/practiceCount)>.7f && (numberOfEarlyPresses/responseCount)<.1f) practicePassed = true;
		}
		else if (type == GameManager.SessionType.Stopping){
			float go = 0;
			foreach(StoppingEvent e in gm.Practice){
				if(e.Go) go++;
				
				if(e.Response!= null){
					responseCount++;
				
					if(e.Response.ResponseTime<.2f)	numberOfEarlyPresses++;
				}
			}
		
			if((responseCount/go)>.7f && (numberOfEarlyPresses/responseCount)<.1f) practicePassed = true;
		}
		else if (type == GameManager.SessionType.Associate){
			
			float numResponses=gm.Practice.Count;
			
			foreach(AssociateEvent e in gm.Practice){
				if(e.Responses[e.Responses.Count-1].ResponseTime<.2f || e.Responses[e.Responses.Count-1].ResponseTime>8f) numberOfEarlyPresses++;	
			}
		
			if((numberOfEarlyPresses/numResponses)<.1f) practicePassed = true;
		} 
		else if (type == GameManager.SessionType.MemAttentEnc1){			
			practicePassed = true;
		} 
		else if (type == GameManager.SessionType.MemAttentEnc2){			
			foreach(MemAttentionEvent e in gm.Practice){
				foreach(Response r in e.Responses ){
					if(r.ResponseTime<.2f) numberOfEarlyPresses++;
				}
			}
			
			if((numberOfEarlyPresses/gm.Practice.Count)<.1f) practicePassed = true;

		}

		int criterion = 0;
		
		float correctResponse =0;
		responseCount = 0;
		numberOfEarlyPresses=0;
		float totalResponseCount = gm.Events.Count;
		
		if(type == GameManager.SessionType.Spatial){
			
			foreach(SpatialEvent e in gm.Events){
				if(e.Responses.Count>=1){
					responseCount++;
					
					if(e.respondedCorrectly()) correctResponse++;
				
					foreach(Response r in e.Responses ){
						if(r.ResponseTime<.2f)
							numberOfEarlyPresses++;
					}
				}
			}
			
			if(practicePassed && (responseCount/gm.Events.Count)>.7f && (numberOfEarlyPresses/responseCount)<.1f)	criterion = 1;
		}
		else if (type == GameManager.SessionType.Inhibition){
			foreach(InhibitionEvent e in gm.Events){
				if(e.Response != null){
					responseCount++;
					
					if(e.respondedCorrectly()) correctResponse++;
				
					if(e.Response.ResponseTime<.2f)	numberOfEarlyPresses++;
				}
			}
			
			if(practicePassed && (responseCount/gm.Events.Count)>.7f && (numberOfEarlyPresses/responseCount)<.1f)	criterion = 1;
		}
		else if (type == GameManager.SessionType.Star){
			int numLittleStars=0;
			
			foreach(StarEvent e in gm.Events){
				numLittleStars+= e.NumLittleStars;
				for(int i = 0;i<e.Responses.Count;i++){
					
					float responseTime=e.Responses[i].ResponseTime;
					if(i>0) responseTime = e.Responses[i].ResponseTime - e.Responses[i-1].ResponseTime;
					if(responseTime<.2f || responseTime>8f) numberOfEarlyPresses++;
					
					responseCount++;
					if(e.Responses[i].ResponseType ==0){
						correctResponse++;
					}
				}
				
				totalResponseCount += e.NumLittleStars;
			}
			
			if(practicePassed && correctResponse>10 && (numberOfEarlyPresses/responseCount)<.1f)	criterion = 1;
		}
		else if (type == GameManager.SessionType.Implicit){
			foreach(ImplicitEvent e in gm.Events){
				if(e.Response!= null){
					responseCount++;
					
					if(e.respondedCorrectly()) correctResponse++;
				
					if(e.Response.ResponseTime<.2f) numberOfEarlyPresses++;
				}
			}
		
			if(practicePassed && (responseCount/gm.Events.Count)>.7f && (numberOfEarlyPresses/responseCount)<.1f)	criterion = 1;
		}
		else if (type == GameManager.SessionType.Stopping){
			totalResponseCount = 0;
			
			foreach(StoppingEvent e in gm.Events){
				
				if(e.Go)totalResponseCount++;

				if(e.Response!= null){
						responseCount++;
						
					if(e.respondedCorrectly()) correctResponse++;
					if(e.Response.ResponseTime<.2f) numberOfEarlyPresses++;
				}	
			}
			
			if(practicePassed && (responseCount/totalResponseCount)>.7f && (numberOfEarlyPresses/responseCount)<.1f)	criterion = 1;
			totalResponseCount = gm.Events.Count;
		}
		else if (type == GameManager.SessionType.Associate){
			
			correctResponse = gm.Events.Count;
			responseCount = correctResponse;
			
			foreach(AssociateEvent e in gm.Events){
				if(e.Responses[e.Responses.Count-1].ResponseTime<.2f || e.Responses[e.Responses.Count-1].ResponseTime>8f) numberOfEarlyPresses++;	
			}
			
			if(practicePassed && (numberOfEarlyPresses/gm.Events.Count)<.1f) criterion = 1;
		}
		else if (type == GameManager.SessionType.MemAttentEnc1){
			criterion = 1;
		}
		else if (type == GameManager.SessionType.MemAttentEnc2){

			foreach(MemAttentionEvent e in gm.Events){
					
				if(e.Responses.Count >= 1) responseCount++;

				if(e.respondedCorrectly()) correctResponse++;

				foreach(Response r in e.Responses ){
					if(r.ResponseTime<.2f) numberOfEarlyPresses++;
				}
			}
			
			if(practicePassed && (numberOfEarlyPresses/totalResponseCount)<.1f) criterion = 1;
		}

		float responseAccuracy = correctResponse/totalResponseCount;
		float correctAccuracy = responseCount > 0 ? correctResponse/responseCount : 0f;

		Debug.Log(correctResponse + "/" + totalResponseCount);
		Debug.Log(correctResponse + "/" + responseCount);


		newLine+= criterion + "," + practiceCount + "," + totalResponseCount +"," + responseAccuracy + "," + correctAccuracy+"," + numberOfEarlyPresses;
		
		bool acrossAll=true;
					
		float totalPracCount=0;
		float totalEventCount=0;
		float totalRightperPresented=0;
		float totalRightperResponse=0;
		float totalEarlyResponse=0;
		
		practicePassed = true;
		
		for(int i =2;i<lines.Length;i++){
			if(lines[i].StartsWith(taskNum.ToString())){
				lines[i] = newLine;
			}
			else if(acrossAll){
				int count = 0;
				
				foreach(char c in lines[i]){
					if(c=='.') count++;
				}
				
				if(count==5) acrossAll = false;
			}
			
			if(acrossAll){
				
				string line = lines[i].Replace(" " ,"");
				
				string[] values = line.Split(',');
				
				float tPC = -1;
				float tEC = -1;
				float tRP = -1;
				float tRR = -1;
				float tER = -1;
				
				if(values[1] =="0") practicePassed = false;
				
				if(!float.TryParse(values[2],out tPC)) acrossAll = false;
				if(!float.TryParse(values[3],out tEC)) acrossAll = false;
				if(!float.TryParse(values[4],out tRP)) acrossAll = false;
				if(!float.TryParse(values[5],out tRR)) acrossAll = false;
				if(!float.TryParse(values[6],out tER)) acrossAll = false;
				
				if(acrossAll){
					totalPracCount += tPC;
					totalEventCount += tEC;
					totalRightperPresented += (tRP * tEC);
					totalRightperResponse += ((tRP * tEC) * tRR);
					totalEarlyResponse += tER;
				}
			}	
		}
		
		if(acrossAll){
			if(practicePassed && (totalRightperPresented /totalEventCount)>.7f && (totalEarlyResponse/totalRightperPresented)<.1f)	criterion = 1;
			else criterion = 0;
			
			float totalRpR = totalRightperResponse/totalRightperPresented;
			
			float totalRpP = totalRightperResponse/totalEventCount;
			
			newLine= "0," +criterion + "," + totalPracCount + "," + totalEventCount +"," + totalRpP + "," + totalRpR +"," + totalEarlyResponse;
			
			lines[1] = newLine;
		}

		System.IO.File.WriteAllLines(criterionPath,lines);

	}
	
	//Generates the time the file was written out, and writeout file name
	public void GenerateNewTimestamp(){
		DateTime Now = DateTime.Now;
		string theTime = Now.ToShortDateString().Replace('/', '-') + "-" + Now.ToString("HH:mm:ss").Replace(':', '-');
		
		string[] filePaths = Directory.GetFiles(CsvManager.LogFilesPath);
		
		int count =1;
		
		foreach(string s in filePaths){
			if(s.Contains((mUserName+"_"+sessionXML))) count++;
		}
		
		try{
			//Regex noXML = new Regex(".xml", RegexOptions.IgnoreCase);
			statsXML = mUserName +"_" + System.Environment.MachineName + "_"+sessionXML+"_" + theTime;
			
			if(count>1 && sessionXML != "randomList")	statsXML += ("_try" + count.ToString());
			
			statsXML+=".csv";
		} catch(ArgumentOutOfRangeException e) {
			// If the player quits the game before a gameType is chosen
			NeuroLog.Log(e.Message);
			statsXML = mUserName +"_" + System.Environment.MachineName +"_None_" + theTime + ".csv";
		}
	}

	float RoundFloat(float num, int places) {
		float rounded = Mathf.Round(Mathf.Pow(10f, places) * num) / Mathf.Pow(10f, places);
		
		return rounded;
	}
}
