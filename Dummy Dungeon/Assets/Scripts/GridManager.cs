using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour 
{

	public static GridManager Instance; // Singleton GridManager

	// for Gridspace organization
	[HideInInspector]
	public GameObject Perimeter;
	[HideInInspector]
	public GameObject Floor;


	public int GridHeight; // Height of Grid in editor units
	public int GridWidth; // Width of Grid in editor units

	public Vector2 start;
	public Vector2 goal;

	public GameObject perimeterPrefab;
	public GameObject floorPrefab;

	public GameObject startMarkerPrefab;
	public GameObject goalMarkerPrefab;
	public GameObject searchMarkerPrefab;

	// contains all GridSpaces of all GridSpace edges in scene
	public Dictionary<GridSpace, GridSpace[]> edges = new Dictionary<GridSpace, GridSpace[]>();
	public Dictionary<Vector2, GridSpace> trueGrid = new Dictionary<Vector2, GridSpace>();

	private List<GameObject> markers = new List<GameObject>();

	void Awake()
	{
		RegisterSingleton(); // register this instance

		Perimeter = new GameObject();
		Perimeter.transform.parent = transform;
		Perimeter.name = "Perimeter";
		Floor = new GameObject();
		Floor.transform.parent = transform;
		Floor.name = "Floor";
	}


	// Use this for initialization
	void Start () 
	{
		CreateWalledPerimeter();
		fillGrid();
	}


    // Update is called once per frame
    void Update () 
	{
		DrawDebugPerimeter();

		if (Input.GetButtonDown("Fire1"))
		{
			// do a pathfinding search
			BFSearch(start, goal);
		}
	}


    int BFSearch(Vector2 startPosition, Vector2 goalPosition)
	{
		clearMarkers();
		markers.Add(Instantiate(startMarkerPrefab, new Vector3(startPosition.x, startPosition.y, 0), Quaternion.identity)); // drop marker
		markers.Add(Instantiate(goalMarkerPrefab, new Vector3(goalPosition.x, goalPosition.y, 0), Quaternion.identity)); // drop marker

		Queue<Vector2> frontier = new Queue<Vector2>();
		frontier.Enqueue(startPosition);

		Dictionary<Vector2, GridSpace> cameFrom = new Dictionary<Vector2, GridSpace>();
		cameFrom.Add(startPosition, getGridSpace(startPosition));

		while (cameFrom.Keys.Count != 0)
		{
			try 
			{ 
				Vector2 currentPosition = frontier.Dequeue(); 

				if (currentPosition == goalPosition)
				{
					break; // got 'em
				}
				markers.Add(Instantiate(searchMarkerPrefab, new Vector3(currentPosition.x, currentPosition.y, 0), Quaternion.identity)); // drop marker

				foreach (Vector2 nextPosition in getNeighbourPositions(currentPosition))
				{
					if (!cameFrom.ContainsKey(nextPosition))
					{
						frontier.Enqueue(nextPosition);
						cameFrom.Add(nextPosition, getGridSpace(currentPosition));
					}
				}
			} catch (InvalidOperationException) 
			{
				Debug.Log("Couldn't reach the goal position.");
				break;
			}
			
			

		}
		

		return 1; // a path to the goal could not be found
	}


	private void clearMarkers()
	{
		foreach (GameObject marker in markers)
		{
			Destroy(marker);
		}

		markers.Clear();
	}


	private List<Vector2> getNeighbourPositions(Vector2 position) 
	{
		List<Vector2> results = new List<Vector2>();
		GridSpace origin = getGridSpace(position);
		float originX = position.x;
		float originY = position.y;

		GridSpace possibleNeighbour;

		possibleNeighbour = getGridSpace(new Vector2(originX + 1, originY));
		if (possibleNeighbour != null) { results.Add(possibleNeighbour.coords()); }
		possibleNeighbour = getGridSpace(new Vector2(originX - 1, originY));
		if (possibleNeighbour != null) { results.Add(possibleNeighbour.coords()); }

		possibleNeighbour = getGridSpace(new Vector2(originX, originY + 1));
		if (possibleNeighbour != null) { results.Add(possibleNeighbour.coords()); }
		possibleNeighbour = getGridSpace(new Vector2(originX, originY - 1));
		if (possibleNeighbour != null) { results.Add(possibleNeighbour.coords()); }

		return results;
	}



	private GridSpace getGridSpace(Vector2 position)
	{
		if (trueGrid.ContainsKey(position))
		{
			return trueGrid[position];
		} else
		{
			Debug.LogWarning("(" + position.ToString() + ") is not in GridManager's trueGrid Dictionary");
			return null;
		}
	}


	private void RegisterSingleton()
	{
		// Register singleton
		if (Instance != null)
		{
			Debug.LogError("Multiple instances of GridManager Singleton");
		}
		Instance = this;
	}


	/// <summary>
	/// Place impassible prefabs along the perimeter of the scene.
	/// </summary>
	private void CreateWalledPerimeter()
    {
        for (int i = 0; i <= GridHeight; i++)
		{
			GameObject new1 = Instantiate(perimeterPrefab, new Vector3(0, i, 0), Quaternion.identity);
			GameObject new2 = Instantiate(perimeterPrefab, new Vector3(GridWidth, i, 0), Quaternion.identity);

			new1.GetComponent<GridSpace>().impassible = true;
			new2.GetComponent<GridSpace>().impassible = true;
			new1.transform.parent = Perimeter.transform;
			new2.transform.parent = Perimeter.transform;
			new1.name = "Perimeter: (" + 0 + ", " + i + ")";
			new2.name = "Perimeter: (" + GridWidth + ", " + i + ")";
		}
		for (int i = 0; i <= GridWidth; i++)
		{
			GameObject new1 = Instantiate(perimeterPrefab, new Vector3(i, 0, 0), Quaternion.identity);
			GameObject new2 = Instantiate(perimeterPrefab, new Vector3(i, GridHeight, 0), Quaternion.identity);

			new1.GetComponent<GridSpace>().impassible = true;
			new2.GetComponent<GridSpace>().impassible = true;
			new1.transform.parent = Perimeter.transform;
			new2.transform.parent = Perimeter.transform;
			new1.name = "Perimeter: (" + i + ", " + 0 + ")";
			new2.name = "Perimeter: (" + i + ", " + GridHeight + ")";
		}
    }


	/// <summary>
	/// Place passible prefabs filling the grid of the scene.
	/// </summary>
	private void fillGrid()
    {
        for (int i = 1; i < GridWidth; i++)
		{
			for (int j = 1; j < GridHeight; j++)
			{
				GameObject newFloor = Instantiate(floorPrefab, new Vector3(i, j, 0), Quaternion.identity);
				newFloor.GetComponent<GridSpace>().impassible = false;
				newFloor.transform.parent = Floor.transform;
				newFloor.name = "Floor: (" + i + ", " + j + ")";
			}
		}
    }


	/// <summary>
	/// Draw the perimeter in the editor.
	/// </summary>
    private void DrawDebugPerimeter()
    {
        Debug.DrawLine(
			new Vector3(0, 0, 0),
			new Vector3(0, GridHeight, 0),
			Color.white
		);
		Debug.DrawLine(
			new Vector3(0, 0, 0),
			new Vector3(GridWidth, 0, 0),
			Color.white
		);
		Debug.DrawLine(
			new Vector3(0, GridHeight, 0),
			new Vector3(GridWidth, GridHeight, 0),
			Color.white
		);
		Debug.DrawLine(
			new Vector3(GridWidth, 0, 0),
			new Vector3(GridWidth, GridHeight, 0),
			Color.white
		);
    }


}
