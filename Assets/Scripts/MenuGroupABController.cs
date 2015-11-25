using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Script used to control all activity in the menu scene.
public class MenuGroupABController : MonoBehaviour {
	
	//List of the task button in the scene(Should be 6)
	public List<UIButton> groupButtons;

	private string language ="english";

	// Use this for initialization
	void Start () {

		if(PlayerPrefs.HasKey("-language"))
			language = PlayerPrefs.GetString("-language");

		//Set the response for all the buttons to methods in this class
		foreach(UIButton b in groupButtons){
			b.GetComponent<ButtonResponder>().response = groupButtonPressed;
			if(language == "spanish")
				b.GetComponentInChildren<UILabel>().text = b.GetComponentInChildren<UILabel>().text.Replace("Group", "Grupo"); 
		}
	}

	private void groupButtonPressed(GameObject o){

		string group = o.name.Replace("Group","");

		CsvManager.sessionFilesName = "session_files" + group;

		Application.LoadLevel(2);

	}

}
