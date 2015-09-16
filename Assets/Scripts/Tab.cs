using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tab: MonoBehaviour {
	
	//The index of whatever element is at the top of the list
	private int clusterStart;
	private int playerStart;
	
	//scroll bars for each category
	public UIScrollBar clusterScroll;
	public UIScrollBar playerScroll;
	
	//The backgrounds for each category
	public UITexture clusterBackground;
	public UITexture playerBackground;
	
	//Lists of the uilabels for each category
	public List<UILabel> clusterLabels;
	public List<UILabel> playerLabels;
	
	//The selected variables
	private string selectedCluster;
	public string SelectedCluster{
		get{return selectedCluster;}
	}
	private string selectedPlayer;
	public string SelectedPlayer{
		get{return selectedPlayer;}
	}
	
	private ConfigManager cM;
	
	//List of all the different variables
	private List<ConfigManager.Child> demographics;
	private List<string> clusters;
	private List<string> players;
	private Dictionary<string , bool> completed;
	
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
		
		clusterStart = 0;
		playerStart= 0;
		selectedCluster ="";
		selectedPlayer ="";
		
		demographics = list;
		
		clusterScroll.scrollValue = 0;
		playerScroll.scrollValue = 0;
		
		completed = new Dictionary<string, bool>();
		
		foreach(ConfigManager.Child c in demographics){
			bool complete =false;
			
			if(c.lastCompleted>=6) complete = true;
			
			completed.Add(c.Cluster+c.ID, complete);
		}
		
		updateLists();
	}
	
	public void Reset(){
		selectedPlayer = "";
		selectedCluster = "";
		players = new List<string>();
		
		updateLists();
	}
	
	//Updates the contents of the lists based on the selected values
	private void updateLists(){
		if(selectedPlayer ==""){
			if(selectedCluster!=""){
				
				playerStart = 0;
				
				selectedPlayer = "";
				players =new List<string>();
				foreach(ConfigManager.Child c in demographics){
					if(c.Cluster == selectedCluster){
						if(!players.Contains(c.ID)){
							players.Add(c.ID);
						}
					}
				}
				players.Sort();
			
			}
			else{
				clusterStart =0;
				selectedCluster="";
				selectedPlayer = "";
				players = new List<string>();
				clusters =new List<string>();
				foreach(ConfigManager.Child c in demographics){
					if(!clusters.Contains(c.Cluster)){
						clusters.Add(c.Cluster);
					}
				}
				clusters.Sort();
			}
		}
		
		if(clusters.Count<=9){
			clusterScroll.scrollValue = 0;
			clusterScroll.barSize = 1;
			clusterScroll.alpha =0f;
			clusterBackground.mainTexture = standardTab;
			clusterScroll.enabled = false;
		}
		else{
			clusterScroll.alpha =1f;
			clusterBackground.mainTexture = scrollBarTab;
			clusterScroll.barSize = 1f/(clusters.Count-8f);
			clusterScroll.enabled = true;
		}
		
		if(players.Count<=9){
			playerScroll.scrollValue = 0;
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

		for(int i=0;i<clusterLabels.Count;i++){
			if(clusters.Count>i){
				clusterLabels[i].text = clusters[i+clusterStart];
				if(selectedCluster == clusters[i+clusterStart])
					clusterLabels[i].color = Color.yellow;
				else
					clusterLabels[i].color = Color.white;
				clusterLabels[i].enabled = true;
			}
			else{
				clusterLabels[i].text ="";
				clusterLabels[i].enabled = false;
			}
		}
		
		for(int i=0;i<playerLabels.Count;i++){
			if(players.Count>i){
				playerLabels[i].text = players[i+playerStart];
				
				bool c = completed[selectedCluster+players[i+playerStart]];
				
				
				if(selectedPlayer == players[i+playerStart]){
					playerLabels[i].GetComponent<Collider>().enabled = true;
					playerLabels[i].color = Color.yellow;
				}
				else if(c){
					playerLabels[i].color = Color.gray;
					playerLabels[i].GetComponent<Collider>().enabled = false;
				}
				else{
					playerLabels[i].color = Color.white;
					playerLabels[i].GetComponent<Collider>().enabled = true;
				}
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
		
		if(clusterScroll.enabled){
			int i = Mathf.CeilToInt(clusterScroll.scrollValue /clusterScroll.barSize);
			
			if(i>0) i-=1;
			
			if(i>=0 && i != clusterStart){
				clusterStart = i;
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
					if(hit.collider.transform.parent.name == "Cluster"){
						if(clusterLabels[labelNum].text != ""){
							if(selectedCluster != clusterLabels[labelNum].text){
								selectedCluster = clusterLabels[labelNum].text;
								
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