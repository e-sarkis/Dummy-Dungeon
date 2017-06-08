using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSpace : MonoBehaviour 
{
	// Movement and pathfinding
	public bool impassible = false; // false if this location legally traversible
	public int moveCost = 1; // the movement cost to traverse this spot

	// Sprite representation
	private SpriteRenderer sr;
	//public bool Tall; //true if the sprite is 32x16 otherwise assumed to be 16x16

	private GridManager Grid;

	void Awake () 
	{
		sr = GetComponent<SpriteRenderer>();
	}

	void Start()
	{
		Grid = GridManager.Instance;
		if (!Grid.trueGrid.ContainsKey(new Vector2(transform.position.x, transform.position.y)))
		{
			// join the Dictionary representation of the grid
			Grid.trueGrid.Add(new Vector2(transform.position.x, transform.position.y), this);
		}


		if (!impassible)
		{
			sr.enabled = false;
		}
	}
}
