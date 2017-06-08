using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSpaceProperties : MonoBehaviour 
{
	// Movement and pathfinding
	public bool impassible = false; // false if this location legally traversible
	public int moveCost = 1; // the movement cost to traverse this spot

	// Sprite representation
	public SpriteRenderer sr;
	public bool 32x16; //true if the sprite is 32x16 otherwise assumed to be 16x16

	void Awake () 
	{
		sr = GetComponent<SpriteRenderer>();
	}
}
