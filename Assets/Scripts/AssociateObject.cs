using UnityEngine;
using System.Collections;

public class AssociateObject : MonoBehaviour {
	
	//Whether the object is still dancing
	private bool dancing=false;
	public bool Dancing{
		get{return dancing;}
	}
	
	//The num of the object
	private int num;
	public int Num{
		get{return num;}
		set{num = value;}
	}
	
	private AssociateBox box;
	
	void Start(){
		box = GameObject.Find("box").GetComponent<AssociateBox>();
	}
	
	//Reset the orientation/position of the object
	public void Reset(){
		transform.rotation = Quaternion.identity;
		
		transform.localScale = new Vector3(-.75f,1,-.75f);
		
		if(num>0)
			transform.position = new Vector3(-30 + (num *12), -3.5f,-20);
		else
			transform.position = new Vector3(-4, -4, 20);
	}
	
	// Use this for initialization
	public IEnumerator matchUp () {
		Vector3 finalPos = new Vector3(4,-4,7.5f);
		Vector3 startPos = transform.position;
		
		float totalDist = Vector3.Distance(finalPos,startPos);
		Vector3 toSpot = Vector3.zero;
		float percentage =0;
		//Loop til were there
		do{
			//Get the Vector to the next spot
			toSpot = finalPos - transform.position;
			
			toSpot.y = 0;
			toSpot.Normalize();
			
			transform.Translate(toSpot* .75f,Space.World);
			
			percentage = Vector3.Distance(startPos,transform.position)/ totalDist;
			
			if(percentage>1) percentage = 1;
			
			transform.rotation = Quaternion.Euler(0,percentage*360f,0);
			
			box.updateLines(percentage);
			
			yield return new WaitForFixedUpdate();
		
		}while(Vector3.Distance(finalPos,transform.position)>1f);
		
		percentage = 1;
		
		box.updateLines(percentage);
		
		transform.rotation = Quaternion.identity;
		
		yield return new WaitForSeconds(.25f);
	}
	
	// Update is called once per frame
	public IEnumerator dance () {
		
		dancing = true;
		float radian = 0;
		
		Vector3 startPos = transform.position;
		do{
			radian+=.1f;
			float x = Mathf.Sin(radian) * 2;
			
			float z = -Mathf.Pow(Mathf.Abs(x),2)+ Mathf.Abs(x)*2;
			
			transform.position = startPos + new Vector3(x,0,z);
			
			float rot = 30 * (x/2);
			
			transform.rotation= Quaternion.Euler(0,rot,0);
			
			yield return new WaitForFixedUpdate();
		}while(radian<Mathf.PI*4);
		
		transform.rotation = Quaternion.identity;
		transform.position = startPos;
		
		yield return new WaitForSeconds(.25f);
		
		dancing = false;
	}
}
