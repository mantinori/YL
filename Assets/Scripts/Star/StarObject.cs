using UnityEngine;
using System.Collections;

public class StarObject {
	
	//What type of object is this. 0=littleStar, 1=bigStar,2=dot,3=triangle
	private int type;
	public int Type{
		get{return type;}
	}
	
	//The location on the screen where this object should be placed
	private Vector2 position;
	public Vector2 Position{
		get{return position;}
	}
	
	//The rotation of the object
	private float rotation;
	public float Rotation{
		get{return rotation;}
	}
	
	//Constructor
	//t(int) = type
	//p(Vector2) = position
	//r(float) = rotation
	public StarObject(int t, Vector2 p, float r){
		type = t;
		position = p;
		rotation = r;
	}
}
