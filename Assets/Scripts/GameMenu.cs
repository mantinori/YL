using UnityEngine;
using System.Collections;

public class GameMenu : MonoBehaviour {

	[SerializeField]
	ButtonResponder continueBtn, redoPracticeBtn;

	//[SerializeField]
	//ButtonResponder redoPracticeBtn;

	// Use this for initialization
	void Start () {
		continueBtn.response = menuButtonPressed;
		redoPracticeBtn.response = menuButtonPressed;
	}
	
	// Update is called once per frame
	void Show (bool showRedo) {

		redoPracticeBtn.gameObject.SetActive(showRedo);


		// set text?
	
	}

	private void menuButtonPressed(GameObject o){
		//Debug.Log("btn pressed: " + o);

		if(o == redoPracticeBtn.gameObject) {
			Camera.main.gameObject.SendMessage("resetPractice");
		} 

		gameObject.SetActive(false);

	}
}
