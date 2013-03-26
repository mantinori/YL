using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Manager of a star game
public class StarManager : GameManager {
	
	//Lists of each type of object in a trial
	List<GameObject> littleStars = new List<GameObject>();
	List<GameObject> bigStars = new List<GameObject>();
	List<GameObject> dots = new List<GameObject>();
	List<GameObject> triangles = new List<GameObject>();
	
	//How many trials the player will have to do
	public int numTrials;
	
	//Return the current event
	public StarEvent CurrentEvent{
		get{
			//If were still practicing, return practice event
			if(practice.Count<=currentPractice) 
				return (StarEvent)events[currentEventNum]; 
			//otherwise return normal event
			else 
				return (StarEvent)practice[currentPractice];
		}
	}
	
	//If the skip arrow was pressed
	private bool skipTrial;
	
	//The Skip arrow object
	private GameObject arrow;
	
	// Use this for initialization
	void Awake () {
		
		skipTrial = false;
		
		base.Setup(GameManager.SessionType.Star);
		
		//Hide the arrow
		arrow = GameObject.Find("next");
		arrow.SetActive(false);
		
		for(int i=1; i< materials.Length;i++){
			materials[i].color = Color.black;
		}
		
		//Read in the event
		events =  xml.ReadInSession();
		
		//If read in failed, generate basic list of events
		if(events == null){
			
			events = new List<EventStats>();
			
			NeuroLog.Log("Generating Random events");
			
			numTrials = 3;
			
			for(int i =0;i<numTrials;i++){
				StarEvent eS = new StarEvent();
			
				eS.NumLittleStars = 34;
				eS.NumBigStars = 31;
				eS.NumTriangles= 7;
				eS.NumDots= 6;
			
				events.Add(eS);
			}
		}
		else{
			numTrials = events.Count;
		}
		
		//Find the max number of objects we'll need
		int maxTri = 2;
		int maxDot = 2;
		int maxLS = 3;
		int maxBS = 5;
		foreach(StarEvent sE in events){
			if(sE.NumBigStars>maxBS) maxBS = sE.NumBigStars;
			if(sE.NumLittleStars>maxLS) maxLS = sE.NumLittleStars;
			if(sE.NumDots>maxDot) maxDot = sE.NumDots;
			if(sE.NumTriangles>maxTri) maxTri = sE.NumTriangles;
		}
		
		//Have the game generate the maximum amount of objects needed for this session
		generateObjects(maxLS,maxBS,maxDot,maxTri);
		
		//Start the game
		StartCoroutine("runSession");

	}
	
	public void randomizeOrder(int maxLS,int maxBS,int maxT,int maxD){
	
		System.Random rand = new System.Random();
		
		//Apply a base randomization
		for(int i=0; i<maxLS-1;i++){
			GameObject originalObj = (GameObject)littleStars[i];
			
			int spotToMove = rand.Next(i+1,maxLS);
			
			littleStars[i] = littleStars[spotToMove];
			
			littleStars[spotToMove] = originalObj;
		}
		
		//Apply a base randomization
		for(int i=0; i<maxBS-1;i++){
			GameObject originalObj = (GameObject)bigStars[i];
			
			int spotToMove = rand.Next(i+1,maxBS);
			
			bigStars[i] = bigStars[spotToMove];
			
			bigStars[spotToMove] = originalObj;
		}
		
		//Apply a base randomization
		for(int i=0; i<maxD-1;i++){
			GameObject originalObj = (GameObject)dots[i];
			
			int spotToMove = rand.Next(i+1,maxD);
			
			dots[i] = dots[spotToMove];
			
			dots[spotToMove] = originalObj;
		}
		
		//Apply a base randomization
		for(int i=0; i<maxT-1;i++){
			GameObject originalObj = (GameObject)triangles[i];
			
			int spotToMove = rand.Next(i+1,maxT);
			
			triangles[i] = triangles[spotToMove];
			
			triangles[spotToMove] = originalObj;
		}
	}
	
	//Generate the objects to fill the screen
	private void generateObjects(int maxLS, int maxBS, int maxD, int maxT){
				
		//Load in the Prefab
		GameObject loadedObject = (GameObject)Resources.Load("Prefabs/Star/starItem");

		//Generate little target Stars
		for(int i=0;i <maxLS;i++){
			
			GameObject newStar = (GameObject)Instantiate(loadedObject,new Vector3(-225,.5f,0), Quaternion.identity);
			
			newStar.name = "littleStar";
			newStar.transform.localScale = new Vector3(1.4f,1,-1.4f);
			
			//Mark the first 5 as "example", for tutorial
			if(i<5) newStar.tag = "example";
			
			if(i<= maxLS/2) newStar.renderer.material = materials[1];
			else  newStar.renderer.material = materials[2];
			
			littleStars.Add(newStar);
		}
		
		//Generate large stars
		for(int i=0;i <maxBS;i++){
			GameObject newStar = (GameObject)Instantiate(loadedObject,new Vector3(-225,0,0), Quaternion.identity);
			newStar.name = "bigStar";
			newStar.transform.localScale = new Vector3(2.8f,1,-2.8f);
			
			//Mark the first 3 as "example", for tutorial
			if(i<3) newStar.tag = "example";
			
			if(i%2==0) newStar.renderer.material = materials[1];
			else  newStar.renderer.material = materials[2];
			
			bigStars.Add(newStar);
		}
		
		//Generate Dots
		for(int i=0;i <maxD;i++){
			GameObject newDot = (GameObject)Instantiate(loadedObject,new Vector3(-225,0,0), Quaternion.identity);
			newDot.name = "dot";
			newDot.transform.localScale = new Vector3(2.8f,1,-2.8f);
			newDot.renderer.material = materials[3];
			
			//Mark the first 2 as "example", for tutorial
			if(i<2) newDot.tag = "example";
			
			if(i%2==0) newDot.renderer.material = materials[3];
			else  newDot.renderer.material = materials[4];
			
			dots.Add(newDot);
		}
		
		//Generate Triangle
		for(int i=0;i <maxT;i++){
			GameObject newTri = (GameObject)Instantiate(loadedObject,new Vector3(-225,0,0), Quaternion.identity);
			newTri.name = "triangle";
			newTri.transform.localScale = new Vector3(1.4f,1,-1.4f);
			newTri.renderer.material = materials[5];
			
			//Mark the first 2 as "example", for tutorial
			if(i<2) newTri.tag = "example";
			
			if(i%2==0) newTri.renderer.material = materials[5];
			else  newTri.renderer.material = materials[6];
			
			triangles.Add(newTri);
		}
	}
	
	//Randomize the positions of the elements on the screen
	private void randomizePositions(){
		
		int k=0;
		
		//Randomize the order of items
		randomizeOrder(littleStars.Count,bigStars.Count,triangles.Count,dots.Count);
		
		//List of objects in the scene
		List<GameObject> starObjects = new List<GameObject>();
		
		//Get the required amount of little stars
		for(k=0;k<littleStars.Count;k++){
			littleStars[k].transform.rotation = Quaternion.identity;
			littleStars[k].transform.position = new Vector3(-250,0,0);
			if(k<CurrentEvent.NumLittleStars/2) littleStars[k].renderer.material = materials[1];
			else littleStars[k].renderer.material = materials[2];
			if(k<CurrentEvent.NumLittleStars){
				littleStars[k].renderer.enabled = true;
				starObjects.Add(littleStars[k]);
			}
			//Hide any unneeded ones
			else littleStars[k].renderer.enabled = false;
		}
		
		k = 0;
		
		//Get the required amount of big stars
		foreach(GameObject gO in bigStars){
			gO.transform.rotation = Quaternion.identity;
			gO.transform.position = new Vector3(-250,0,0);
			if(k<CurrentEvent.NumBigStars){
				gO.renderer.enabled = true;
				starObjects.Add(gO);
			}
			//Hide any unneeded ones
			else gO.renderer.enabled = false;
			k++;
		}
		
		k = 0;
		
		//Get the required amount of triangles
		foreach(GameObject gO in triangles){
			gO.transform.rotation = Quaternion.identity;
			gO.transform.position = new Vector3(-250,0,0);
			
			if(k<CurrentEvent.NumTriangles){
				gO.renderer.enabled = true;
				starObjects.Add(gO);
			}
			//Hide any unneeded ones
			else gO.renderer.enabled = false;
			k++;
		}
		
		k = 0;
		
		//Get the required amount of dots
		foreach(GameObject gO in dots){
			gO.transform.rotation = Quaternion.identity;
			gO.transform.position = new Vector3(-250,0,0);
			
			if(k<CurrentEvent.NumDots){
				starObjects.Add(gO);
				gO.renderer.enabled = true;
			}
			//Hide any unneeded ones
			else gO.renderer.enabled = false;
			k++;
		}
		
		System.Random rand = new System.Random();
		
		int area = rand.Next(1,5);
		
		Vector3 pos;
		
		//Accepted min/max 
		int minX;
		int maxX;
		int minY;
		int maxY;
		
		bool good;
		int loopCount;
		
		//Loop through the list of starObjects placing objects in different areas to try to evenly spread them out
		for(int i = 0;i<starObjects.Count;i++){
			float x;
			float y;
			//If were dealing with little stars
			if(i<CurrentEvent.NumLittleStars){
				//Determine the min's and max's
				if(area <=2){
					minX = -170;
					maxX=-90;
				}
				else{
					minX = -90;
					maxX = 0;
				}
				if(area%2==0){
					minY =-90;
					maxY = 0;
				}
				else{
					minY = 0;
					maxY = 90;
				}
				loopCount = 0;
				
				//Find an acceptable place on the screen that is not too close to another star(Maintain at least a distance of 25)
				//Stop looping after 20 tries and goes with current
				do{
					x = rand.Next(minX,maxX);
					
					y = rand.Next(minY,maxY);
					
					pos =new Vector3(x,.5f, y);
					
					good = true;
					for(int j = 0;j<CurrentEvent.NumLittleStars;j++){
						if(starObjects[j].transform.position.x>-200){
							if(Vector3.Distance(pos,starObjects[j].transform.position)<25){
								good = false;
								break;
							}
						}
					}
					loopCount++;
				}while(good==false && loopCount<20);
				
				starObjects[i].transform.position = pos;
				
				//Mirror the little Star's x position on the other side of the screen to maintain a balance
				if(i+1<CurrentEvent.NumLittleStars){
					
					i++;

					x = pos.x*-1;
					if(x>110) minY = -55;
					
					loopCount = 0;
					do{
					
						//Allow random y position
					 	y = rand.Next(minY,maxY);
						pos =new Vector3(x,.5f, y);
					
						good = true;
						//Loop til we find a good place
						for(int j = 0;j<CurrentEvent.NumLittleStars;j++){
							if(starObjects[j].transform.position.x>-200){
								if(Vector3.Distance(pos,starObjects[j].transform.position)<25){
									good = false;
									break;
								}
							}
						}
						loopCount++;
					}while(good==false && loopCount<20);
			
					starObjects[i].transform.position = pos;
				}
				
				//Only focus in the first 4 areas, as every other one will be mirrored automatically.
				area++;
				if(area>4) area=1;
			}
			else{
				//Get min and max's
				if(area <=2){
					minX = -160;
					maxX=-80;
				}
				else if(area <=4){
					minX = -80;
					maxX = 0;
				}
				else if(area <=6){
					minX = 0;
					maxX = 80;
				}
				else{
					minX = 80;
					maxX = 160;
				}
				
				if(area%2==0){
					minY =-80;
					maxY = 0;
				}
				else{
					minY = 0;
					maxY = 80;
				}
				
				
				//Creates a random position for the object
				x = rand.Next(minX,maxX);
					
				if(x>110) minY = -55;
				y = rand.Next(minY,maxY);
				
				pos = new Vector3(x,0, y);
				starObjects[i].transform.position = pos;
				
				//Use whole 8 areas
				area++;
				if(area>8) area=1;
			}
		}
		
		Vector3 moveTo;
		Vector3 p;
		
		float dist;
		
		loopCount=0;
		//For 50 tries, or until everything is spaced enough
		do{
			good = true;
			k=0;
			//Loop through the list of starObjects
			foreach(GameObject obj in starObjects){
				p = obj.transform.position;
				
				moveTo = Vector3.zero;
				
				int xMax = 165;
				int yMax = 90;
				if(obj.transform.localScale.x==2.8f){ 
					xMax = 155;
					yMax = 80;
				}
				
				int yMin = -80;
				
				//Check against all other objects to make sure they aren't close/on top of it
				foreach(GameObject other in starObjects){
					if(obj!=other){
						if(other.transform.localScale.x == 2.8f || obj.transform.localScale.x == 2.8f) dist = 28;
						else dist = 16;
						
						//If an objects to close, save the inverse of the vector to that object
						if(Vector3.Distance(p, other.transform.position)<dist){
							good = false;
							Vector3 v= (p - other.transform.position);	
							
							v.y = 0;
							
							if(Mathf.Abs(p.x)>=xMax)	v.x = 0;
							
							if(p.z<=yMin ||p.z>=yMax) v.z = 0;
							
							v.Normalize();
							
							moveTo+=v;
						}
					}
				}
				
				//If the object is found to be too close to another, move the object in the normalized combined vector away from those it is touching
				if(moveTo!=Vector3.zero){
					moveTo.y = 0;
				
					moveTo.Normalize();
				
					obj.transform.Translate(moveTo*1f);
					
					p = obj.transform.position;
					
					if(p.x>xMax)
						obj.transform.position = new Vector3(xMax,0,p.z);
					else if(p.x<(-1*xMax))
						obj.transform.position = new Vector3(-1*xMax,0,p.z);
				
					if(p.x>110) yMin =-55;
					else yMin = -80;
					
					if(p.z>yMax)
						obj.transform.position = new Vector3(p.x,0,yMax);
					else if(p.z<yMin)
						obj.transform.position = new Vector3(p.x,0,yMin);
			
				}
				k++;
			}
			loopCount++;
		}while(!good && loopCount<50);
		
		//Give all the shapes a random rotation
		foreach(GameObject gO in starObjects){
			gO.transform.rotation = Quaternion.Euler(0,rand.Next(0,360),0);
		}
	}
	
	//If the event has a preset setup, setup the objects where they are suppose to go
	private void setupPositions(){
		
		//Randomize the order of the objects
		randomizeOrder(littleStars.Count,bigStars.Count,triangles.Count,dots.Count);
		
		//Divide up the Event's StarObjects into different lists based on type
		List<StarObject> ls = new List<StarObject>();
		List<StarObject> bs = new List<StarObject>();
		List<StarObject> d = new List<StarObject>();
		List<StarObject> t = new List<StarObject>();
		
		foreach(StarObject sO in CurrentEvent.Objects){
			if(sO.Type == 0) ls.Add(sO);
			else if(sO.Type == 1) bs.Add(sO);
			else if(sO.Type == 2) d.Add(sO);
			else if(sO.Type == 3) t.Add(sO);
		}
		
		//Set up the little Stars
		for(int i =0;i<littleStars.Count;i++){
			if(i<ls.Count){
				littleStars[i].transform.rotation = Quaternion.Euler(new Vector3(0, ls[i].Rotation,0));
				littleStars[i].transform.position = new Vector3(ls[i].Position.x,0,ls[i].Position.y);
				if(i<ls.Count/2) littleStars[i].renderer.material = materials[1];
				else littleStars[i].renderer.material = materials[2];
				littleStars[i].renderer.enabled = true;
			}
			else{
				littleStars[i].transform.position = new Vector3(-250,0,0);
			 	littleStars[i].renderer.enabled = false;
			}
		}
		
		//Set up the big Stars
		for(int i =0;i<bigStars.Count;i++){
			if(i<bs.Count){
				bigStars[i].transform.rotation = Quaternion.Euler(new Vector3(0, bs[i].Rotation,0));
				bigStars[i].transform.position = new Vector3(bs[i].Position.x,0,bs[i].Position.y);
				bigStars[i].renderer.enabled = true;
			}
			else{
				bigStars[i].transform.position = new Vector3(-250,0,0);
			 	bigStars[i].renderer.enabled = false;
			}
		}
		
		//Set up the dots
		for(int i =0;i<dots.Count;i++){
			if(i<d.Count){
				dots[i].transform.rotation = Quaternion.Euler(new Vector3(0, d[i].Rotation,0));
				dots[i].transform.position = new Vector3(d[i].Position.x,0,d[i].Position.y);
				dots[i].renderer.enabled = true;
			}
			else{
				dots[i].transform.position = new Vector3(-250,0,0);
			 	dots[i].renderer.enabled = false;
			}
		}
		
		//Set up the triangles
		for(int i =0;i<triangles.Count;i++){
			if(i<t.Count){
				triangles[i].transform.rotation = Quaternion.Euler(new Vector3(0, t[i].Rotation,0));
				triangles[i].transform.position = new Vector3(t[i].Position.x,0,t[i].Position.y);
				triangles[i].renderer.enabled = true;
			}
			else{
				triangles[i].transform.position = new Vector3(-250,0,0);
			 	triangles[i].renderer.enabled = false;
			}
		}
	}
	
	//Generate a practice event
	protected override void generatePractice(){
		
		StarEvent prac = new StarEvent();
		
		prac.NumLittleStars = 5;
		prac.NumBigStars = 3;
		prac.NumTriangles= 2;
		prac.NumDots= 2;
		
		practice.Add(prac);
	}
	
	//Run the tutorial
	protected override IEnumerator runTutorial(){
		state = GameManager.GameState.Tutorial;
		
		yield return StartCoroutine(showTitle("Tutorial",3));
		
		screen.enabled = false;
		
		//Get all "example" objects
		GameObject[] practiceObjs = GameObject.FindGameObjectsWithTag("example");
		
		List<Vector3> tutPresses = new List<Vector3>();
		
		//Set up the objects
		practiceObjs[0].transform.position = new Vector3(60,0f,-65);
		practiceObjs[1].transform.position = new Vector3(-50,0f,75);
		practiceObjs[2].transform.position = new Vector3(-125,0f,-15);
		practiceObjs[3].transform.position = new Vector3(130,0f,50);
		practiceObjs[4].transform.position = new Vector3(-10,0f,5);
		practiceObjs[5].transform.position = new Vector3(25,0f,-25);
		practiceObjs[6].transform.position = new Vector3(15,0f,45);
		practiceObjs[7].transform.position = new Vector3(125,0f,-50);
		practiceObjs[8].transform.position = new Vector3(-55,0f,-50);
		practiceObjs[9].transform.position = new Vector3(85,0f,20);
		practiceObjs[10].transform.position = new Vector3(-80,0f,35);
		practiceObjs[11].transform.position = new Vector3(-30,.1f,20);
		
		foreach(GameObject gO in practiceObjs){
			gO.transform.rotation = Quaternion.Euler(0,0,0);
			if(gO.name == "littleStar"){
				gO.renderer.material = materials[1];
				tutPresses.Add(gO.transform.position);
			}
		}
		
		yield return new WaitForSeconds(1.5f);
		
		//Start the finger
		yield return StartCoroutine(tFinger.performAction(tutPresses, practiceObjs));
		
		yield return StartCoroutine(tFinger.exit());
		
		yield return new WaitForSeconds(.5f);
	}
	
	//Main Method 
	protected override IEnumerator runSession(){
		
		//Run the tutorial
		yield return StartCoroutine(runTutorial());
		
		//Show Practice card
		yield return StartCoroutine(showTitle("Practice",3));
		
		//generate a practice event
		generatePractice();
		
		state = GameState.Delay;
		
		//Loop til all the events are done
		while(currentEventNum <numTrials){
			
			//Determine how long the player has based on if its a practice or real trial
			float endTime = 180;

			if(practicing)	endTime = 30;
			
			//Either setup the set positions of the objects, or randomize it
			if(CurrentEvent.Objects.Count>0) setupPositions();
			else randomizePositions();
			
			yield return new WaitForSeconds(.5f);
			
			screen.enabled = false;
			
			state = GameState.Probe;
			
			startTime = Time.time;
			
			//Loop until the skip arrow is pressed, the player gets all the stars, or the time limit is reached
			while(Time.time - startTime<endTime && skipTrial==false && CurrentEvent.NumGoodTouches< CurrentEvent.NumLittleStars){
				yield return new WaitForFixedUpdate();
				if(!practicing && Time.time - startTime> endTime/2 && !arrow.activeSelf) arrow.SetActive(true);
			}
			
			state = GameState.Delay;
			//Record how the trial ended
			if(skipTrial){
				CurrentEvent.Duration = Time.time - startTime;
				CurrentEvent.EndCondition ="Skipped";
				NeuroLog.Log("Skipped");
			}
			else if(CurrentEvent.NumGoodTouches>= CurrentEvent.NumLittleStars){
				
				CurrentEvent.Duration = Time.time - startTime;
				CurrentEvent.EndCondition ="Completed";
				
				NeuroLog.Log("Completed");
				
				yield return new WaitForSeconds(.25f);
			}
			else {
				NeuroLog.Log("TimedOut");
				CurrentEvent.Duration = endTime;
				CurrentEvent.EndCondition ="TimedOut";
			}
			
			//Hide the arrow
			arrow.SetActive(false);
			
			screen.enabled = true;
						
			skipTrial = false;
			
			//Get the next trial
			nextEvent();
		
			if(currentEventNum%3==0){
				for(int i=1; i< materials.Length;i++){
					materials[i].color = Color.black;
				}
			}
			else if(currentEventNum%3==1){
				for(int i=1; i< materials.Length;i++){
					if(i%2==0) materials[i].color =  new Color(.6078f,.7255f,.3529f);
					else materials[i].color = new Color(.4980f,.3922f,.6313f);
				}
			}
			else if(currentEventNum%3==2){
				for(int i=1; i< materials.Length;i++){
					if(i%2==0) materials[i].color =  new Color(.9216f,.7089f,.7089f);
					else materials[i].color =  new Color(.2157f,.7843f,.2157f);
				}
			}
			
			//If we reached the end of the practice list, check to see if the player passed
			if(practicing && currentPractice>=practice.Count){
				
				practiceSessionCount++;
				int numCorrect=0;
				for(int i = currentPractice-1; i< currentPractice;i++){
					if(practice[i].respondedCorrectly()) numCorrect+=1;
				}
				//If the player correct did the practice trial, or has tried 4 times, continue on
				if(numCorrect>=1f || practiceSessionCount>=4){
					NeuroLog.Log("Continuing to MainSession");
					
					yield return StartCoroutine(showTitle("Test",3));
					
					border.SetActive(false);
					
					practicing = false;
				}
				else{
					NeuroLog.Log("Redo Practice");
					
					yield return StartCoroutine(runTutorial());
					
					yield return StartCoroutine(showTitle("Practice",3));
					
					generatePractice();
		
				}
			}
		}
		
		//Write out the Log file
		xml.WriteOut();
		
		yield return StartCoroutine(showTitle("Session Over",3));
		
		NeuroLog.Log("GAME OVER, Returning to menu");
		
		//Return to the menu
		Application.LoadLevel("menu");
	}
	
	// Constantly check for player input
	void Update () {	
		bool currentTouch;
			
		//Get the touch location based on the platform type
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
			
		//Not touching
		if(!currentTouch)
			touching = false;
		//Not holding, touch
		else if(!touching && currentTouch){		
			touching = true;
			//If were in the probe state
			if(state == GameState.Probe && !skipTrial){
				Ray ray = camera.ScreenPointToRay(touchPos);
				RaycastHit hit = new RaycastHit();
				//If the raycast of the touch hit something
				if(Physics.Raycast(ray, out hit)) {
					
					//Next Arrow, signal to skip the event
					if(hit.collider.name == "next"){
						skipTrial = true;
					}
					//Otherwise
					else{
						touchPos.y = Screen.height - touchPos.y;
					
						float responseTime = Time.time - startTime;
						
						Response r;
					
						//LittleStar
						if(hit.collider.name == "littleStar"){
							//The star has not been touched yet
							if(!hit.collider.renderer.material.name.Contains("Touched")){
						
								r = new Response(sType, responseTime,new Vector2(touchPos.x,touchPos.y),0);
							
								hit.collider.renderer.material = materials[0];
								
								StartCoroutine(spot.fadeFinger(Vector3.zero, 1));
							}
							//The star has been touched
							else{
								r = new Response(sType, responseTime,new Vector2(touchPos.x,touchPos.y),2);
							}
						}
						//Any other object
						else{
							r = new Response(sType, responseTime,new Vector2(touchPos.x,touchPos.y),1);
						}
					
						//Add the response
						CurrentEvent.AddResponse(r);
					}
				}
			}
		}
	}
}