using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tab: MonoBehaviour {
	
	//The index of whatever element is at the top of the list
	private int departmentStart;
	private int provinceStart;
	private int districtStart;
	private int playerStart;
	
	//scroll bars for each category
	public UIScrollBar departmentScroll;
	public UIScrollBar provinceScroll;
	public UIScrollBar districtScroll;
	public UIScrollBar playerScroll;
	
	//The backgrounds for each category
	public UITexture departmentBackground;
	public UITexture provinceBackground;
	public UITexture districtBackground;
	public UITexture playerBackground;
	
	//Lists of the uilabels for each category
	public List<UILabel> departmentLabels;
	public List<UILabel> provinceLabels;
	public List<UILabel> districtLabels;
	public List<UILabel> playerLabels;
	
	//The selected variables
	private string selectedDepartment;
	public string SelectedDepartment{
		get{return selectedDepartment;}
	}
	private string selectedProvince;
	public string SelectedProvince{
		get{return selectedProvince;}
	}
	private string selectedDistrict;
	public string SelectedDistrict{
		get{return selectedDistrict;}
	}
	private string selectedPlayer;
	public string SelectedPlayer{
		get{return selectedPlayer;}
	}
	
	private ConfigManager cM;
	
	//List of all the different variables
	private List<ConfigManager.Child> demographics;
	private List<string> departments;
	private List<string> provinces;
	private List<string> districts;
	private List<string> players;
	
	public Texture scrollBarTab;
	public Texture standardTab;
	
	//The main camera
	private Camera cam;
	//If the player is currently touching the screen
	private bool touching=false;
	//Where did the player touch the screen
	private Vector3	 touchPos = Vector3.zero;
	
	// Use this for initialization
	public void setupTab (ConfigManager config, List<ConfigManager.Child> list) {
		
		cM = config;
		
		cam = Camera.main;
		
		departmentStart = 0;
		provinceStart = 0;
		districtStart=0;
		playerStart= 0;
		selectedDepartment ="";
		selectedDistrict ="";
		selectedProvince ="";
		selectedPlayer ="";
		
		demographics = list;
		
		updateLists();
	}
	
	public void Reset(){
		selectedPlayer = "";
		selectedDepartment = "";
		selectedDistrict ="";
		selectedProvince ="";
		provinces = new List<string>();
		players = new List<string>();
		districts = new List<string>();
		
		updateLists();
	}
	
	//Updates the contents of the lists based on the selected values
	private void updateLists(){
		
		if(selectedPlayer ==""){
			if(selectedDepartment!="" && selectedProvince != "" && selectedDistrict != ""){
				
				playerStart = 0;
				
				selectedPlayer = "";
				players =new List<string>();
				foreach(ConfigManager.Child c in demographics){
					if(c.Department == selectedDepartment && c.Province == selectedProvince && c.District == selectedDistrict){
						if(!players.Contains(c.ID)){
							players.Add(c.ID);
						}
					}
				}
				players.Sort();
			
			}
			else if(selectedDepartment!="" && selectedProvince != ""){
				
				districtStart = 0;
				selectedPlayer = "";
				selectedDistrict = "";
				players = new List<string>();
				districts =new List<string>();
				foreach(ConfigManager.Child c in demographics){
					if(c.Department == selectedDepartment && c.Province == selectedProvince){
						if(!districts.Contains(c.District)){
							districts.Add(c.District);
						}
					}
				}
				districts.Sort();
			}
			else if(selectedDepartment!=""){
				provinceStart = 0;
				selectedPlayer = "";
				selectedDistrict = "";
				selectedProvince ="";
				players = new List<string>();
				districts =new List<string>();
				provinces =new List<string>();
				
				foreach(ConfigManager.Child c in demographics){
					if(c.Department == selectedDepartment){
						if(!provinces.Contains(c.Province)){
							provinces.Add(c.Province);
						}
					}
				}
				provinces.Sort();
			}
			else{
				departmentStart =0;
				selectedDepartment="";
				selectedPlayer = "";
				selectedDistrict = "";
				selectedProvince ="";
				players = new List<string>();
				districts =new List<string>();
				provinces =new List<string>();
				departments =new List<string>();
				foreach(ConfigManager.Child c in demographics){
					if(!departments.Contains(c.Department)){
						departments.Add(c.Department);
					}
				}
				departments.Sort();
			}
		}
		
		departmentScroll.scrollValue = 0;
		if(departments.Count<=9){
			departmentScroll.barSize = 1;
			departmentScroll.alpha =0f;
			departmentBackground.mainTexture = standardTab;
			departmentScroll.enabled = false;
		}
		else{
			departmentScroll.alpha =1f;
			departmentBackground.mainTexture = scrollBarTab;
			departmentScroll.barSize = 1f/(departments.Count-8f);
			departmentScroll.enabled = true;
		}
		
		provinceScroll.scrollValue = 0;
		if(provinces.Count<=9){
			provinceScroll.barSize = 1;
			provinceScroll.alpha =0f;
			provinceBackground.mainTexture = standardTab;
			provinceScroll.enabled = false;
		}
		else{
			provinceScroll.alpha =1f;
			provinceScroll.barSize = 1f/(provinces.Count-8f);
			provinceBackground.mainTexture = scrollBarTab;
			provinceScroll.enabled = true;
		}
		
		districtScroll.scrollValue = 0;
		if(districts.Count<=9){
			districtScroll.barSize = 1;
			districtScroll.alpha =0f;
			districtBackground.mainTexture = standardTab;
			districtScroll.enabled = false;
		}
		else{
			districtScroll.alpha =1f;
			districtScroll.barSize = 1f/(districts.Count-8f);
			districtBackground.mainTexture = scrollBarTab;
			districtScroll.enabled = true;
		}
		
		playerScroll.scrollValue = 0;
		if(players.Count<=9){
			playerScroll.barSize = 1;
			playerScroll.alpha =0f;
			playerBackground.mainTexture = standardTab;
			playerScroll.enabled = false;
		}
		else{
			playerScroll.alpha =1f;
			playerScroll.barSize = 1f/(players.Count-8f);
			playerBackground.mainTexture = scrollBarTab;
			playerScroll.enabled = true;
		}
		
		updateVisuals();
	}
	
	private void updateVisuals(){
		for(int i=0;i<departmentLabels.Count;i++){
			if(departments.Count>i){
				departmentLabels[i].text = departments[i+departmentStart];
				if(selectedDepartment == departments[i+departmentStart])
					departmentLabels[i].color = Color.yellow;
				else
					departmentLabels[i].color = Color.white;
				departmentLabels[i].enabled = true;
			}
			else{
				departmentLabels[i].text ="";
				departmentLabels[i].enabled = false;
			}
		}
		
		//If the operator chose a department
		for(int i=0;i<provinceLabels.Count;i++){
			if(provinces.Count>i){
				provinceLabels[i].text = provinces[i+provinceStart];
				if(selectedProvince == provinces[i+provinceStart])
					provinceLabels[i].color = Color.yellow;
				else
					provinceLabels[i].color = Color.white;
				provinceLabels[i].enabled = true;
			}
			else{
				provinceLabels[i].text ="";
				provinceLabels[i].enabled = false;
			}
		}
		
		for(int i=0;i<districtLabels.Count;i++){
			if(districts.Count>i){
				districtLabels[i].text = districts[i+districtStart];
				if(selectedDistrict == districts[i+districtStart])
					districtLabels[i].color = Color.yellow;
				else
					districtLabels[i].color = Color.white;
				districtLabels[i].enabled = true;
			}
			else{
				districtLabels[i].text ="";
				districtLabels[i].enabled = false;
			}
		}
		
		for(int i=0;i<playerLabels.Count;i++){
			if(players.Count>i){
				playerLabels[i].text = players[i+playerStart];
				
				if(selectedPlayer == players[i+playerStart])
					playerLabels[i].color = Color.yellow;
				else
					playerLabels[i].color = Color.white;
				playerLabels[i].enabled = true;
			}
			else{
				playerLabels[i].text ="";
				playerLabels[i].enabled = false;
			}
		}
	}
	
	// Constantly check for player input
	void Update () {
		
		if(departmentScroll.enabled){
			int i = Mathf.CeilToInt(departmentScroll.scrollValue /departmentScroll.barSize);
			
			if(i>0) i-=1;
			
			if(i>=0 && i != departmentStart){
				departmentStart = i;
				updateVisuals();
			}
		}
		
		if(provinceScroll.enabled){
			int i = Mathf.CeilToInt(provinceScroll.scrollValue /provinceScroll.barSize);
			
			if(i>0) i-=1;
			
			if(i>=0 && i != provinceStart){
				provinceStart = i;
				updateVisuals();
			}
		}
		if(districtScroll.enabled){
			int i = Mathf.CeilToInt(districtScroll.scrollValue /districtScroll.barSize);
			
			if(i>0) i-=1;
			
			if(i>=0 && i != districtStart){
				districtStart = i;
				updateVisuals();
			}
		}
		if(playerScroll.enabled){
			int i = Mathf.CeilToInt(playerScroll.scrollValue /playerScroll.barSize);
			
			if(i>0) i-=1;
			
			if(i>=0 && i != playerStart){
				playerStart = i;
				updateVisuals();
			}
		}
		
		
			
		bool currentTouch;
			
		//Get the touch location based on the platform
		if(Application.platform == RuntimePlatform.IPhonePlayer){
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
		if(!currentTouch){
			touching = false;
		}
		//If a player has touched the screen, not holding
		else if(!touching && currentTouch){	
			
			touching = true;
			
			Ray ray = cam.ScreenPointToRay(touchPos);
			RaycastHit hit = new RaycastHit();
			
			//If the raycast of the touch hit something
			if(Physics.Raycast(ray, out hit)) {	
				//Next Arrow, signal to skip the event
				if(hit.collider.name.Contains("name")){
					
					int labelNum =int.Parse(hit.collider.name.Replace("name","")) - 1;
					if(hit.collider.transform.parent.name == "Department"){
						if(departmentLabels[labelNum].text != ""){
							if(selectedDepartment != departmentLabels[labelNum].text){
								selectedDepartment = departmentLabels[labelNum].text;
								
								cM.hideConfirmation();
								selectedProvince ="";
								selectedDistrict ="";
								selectedPlayer ="";
								
								updateLists();
							}
						}
					}
					else if(hit.collider.transform.parent.name == "Province"){
						if(provinceLabels[labelNum].text != ""){
							if(selectedProvince != provinceLabels[labelNum].text){
								selectedProvince = provinceLabels[labelNum].text;
							
								cM.hideConfirmation();

								selectedDistrict ="";
								selectedPlayer ="";
								
								updateLists();
							}
						}
					}
					else if(hit.collider.transform.parent.name == "District"){
						if(districtLabels[labelNum].text != ""){
							if(selectedDistrict!= districtLabels[labelNum].text){
								selectedDistrict = districtLabels[labelNum].text;
								
								cM.hideConfirmation();
								
								selectedPlayer ="";
								
								updateLists();
							}
						}
					}
					else if(hit.collider.transform.parent.name == "Player"){
						if(playerLabels[labelNum].text != ""){
							if(selectedPlayer != playerLabels[labelNum].text){
								selectedPlayer = playerLabels[labelNum].text;
								
								cM.askForConfirmation();
								
								updateLists();
							}
						}
					}
				}
			}
		}
	}
}