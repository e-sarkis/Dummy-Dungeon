using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eppy;
using ESarkis;

public class GridManager : MonoBehaviour 
{

	public static GridManager Instance; //  GridManager is a Singleton

	// GameObjects for Gridspace organization in hierarchy
	[HideInInspector]
	public GameObject Perimeter;
	[HideInInspector]
	public GameObject Floor;
	[HideInInspector]
	public GameObject Markers;

	public int GridHeight; // Height of scene Grid in editor units
	public int GridWidth; // Width of scene Grid in editor units

	public GameObject perimeterPrefab; // The GridSpace prefab for Grid Perimeter walls.
	public GameObject floorPrefab; // The GridSpace prefab for Grid floors.

	// The Grid used in logic operations. Maps Vector2 -> GridSpace objects.
	public Dictionary<Vector2, GridSpace> trueGrid = new Dictionary<Vector2, GridSpace>();
	
	// PATHFINDING
	public bool useGameObjects = false; // true if using GameObjects for pathfinding Start / Goal locations
	public GameObject startGameObject = null;
	public GameObject goalGameObject = null;
	public Vector2 start; // the in-scene X-Y coordinates of the start position
	public Vector2 goal; // the in-scene X-Y coordinates of the goal position

	public GameObject startMarkerPrefab; // in-editor start Marker prefab placed in the scene
	public GameObject goalMarkerPrefab; // in-editor goal Marker prefab placed in the scene
	public GameObject pathMarkerPrefab; // in-editor path Marker prefab placed in the scene
	private List<GameObject> markersList = new List<GameObject>(); // List of all the Markers in the scene


	void Awake()
	{
		RegisterSingleton(); // Register this instance

		// Initialize Hierarchy organization GameObjects
		Perimeter = new GameObject();
		Perimeter.transform.parent = transform;
		Perimeter.name = "Perimeter";
		Floor = new GameObject();
		Floor.transform.parent = transform;
		Floor.name = "Floor";
		Markers = new GameObject();
		Markers.transform.parent = transform;
		Markers.name = "Markers";

	}


	void Start () 
	{
		CreateWalledPerimeter(); // Wall off the grid
		fillGrid(); // Populate available GridSpaces with Flooring.
	}


    // Update is called once per frame
    void Update () 
	{
		DrawDebugPerimeter(); // Draw the Grid perimeter in the editor viewport.

		if (Input.GetButtonDown("Fire1")) 
		{
			// Determine if pathfinding will use assigned GameObjects or set Vector2 values.
			if (useGameObjects && ( startGameObject != null && goalGameObject != null ))
			{
				start = new Vector2(startGameObject.transform.position.x, startGameObject.transform.position.y);
				goal = new Vector2(goalGameObject.transform.position.x, goalGameObject.transform.position.y);
			}

			FindPath(start, goal); // Execute pathfinding search		
		}
	}


	/// <summary>
	/// 
	/// </summary>
	/// <param name="startPosition"></param>
	/// <param name="goalPosition"></param>
	/// <returns></returns>
	int FindPath(Vector2 startPosition, Vector2 goalPosition)
	{
		clearMarkers();
		placeMarker(startPosition, "start");  // drop Start marker
		placeMarker(goalPosition, "goal");  // drop Goal marker
		// Check legality of start and goal positions
		if (outOfGridBounds(startPosition) || outOfGridBounds(goalPosition))
		{
			Debug.Log("Start or Goal position is out of Grid bounds.");
			return 1; // No Path
		} else if (getGridSpace(startPosition).impassible || getGridSpace(goalPosition).impassible) 
		{
			Debug.Log("Start or Goal position an impassible Gridspace.");
			return 1; // No Path
		}
		return AStarSearch(startPosition, goalPosition); // 0 - Found Path, 1 - No Path
	}

	int AStarSearch(Vector2 startPosition, Vector2 goalPosition)
	{
		PriorityQueue<Vector2> frontier = new PriorityQueue<Vector2>(); // Queue for Grid exploration
		frontier.Enqueue(startPosition, 0);

		Dictionary<Vector2, GridSpace> cameFrom = new Dictionary<Vector2, GridSpace>(); // GridSpaces visited: XY Position -> GridSpace
		Dictionary<Vector2, int> costSoFar = new Dictionary<Vector2, int>(); // Movement costs accumulated: XY Position -> integer
		cameFrom.Add(startPosition, getGridSpace(startPosition));
		costSoFar.Add(startPosition, 0); // No movement cost to get to starting GridSpace

		while (frontier.Count > 0)
		{
			try 
			{ 
				Vector2 currentPosition = frontier.Dequeue(); 

				if (currentPosition == goalPosition) // Have we reached our goal?
				{
					Debug.Log("The goal position is reachable.");
					// Draw Path to goal via dropping Path markers
					List<Vector2> path = new List<Vector2>();
					path.Add(currentPosition);
					while (currentPosition != startPosition)
					{
						currentPosition = cameFrom[currentPosition].coords();
						if (currentPosition != startPosition) { placeMarker(currentPosition, "path"); }
						path.Add(currentPosition);
					}
					return 0; // got 'em
				}
				
				// Iterate through currentPosition's neighbouring legal GridSpace's XY Positions
				foreach (Vector2 nextPosition in getNeighbourPositions(currentPosition))
				{
					int newCost = costSoFar[currentPosition] + getGridSpace(nextPosition).moveCost;
					
					if (!costSoFar.ContainsKey(nextPosition)) { costSoFar.Add(nextPosition, getGridSpace(nextPosition).moveCost); }
					if (!cameFrom.ContainsKey(nextPosition) || newCost < costSoFar[nextPosition])
					{
						costSoFar[nextPosition] = newCost;
						double priority = heuristicManhattan(goalPosition, nextPosition) + newCost;
						frontier.Enqueue(nextPosition, priority);
						if (!cameFrom.ContainsKey(nextPosition)) 
						{ 
							cameFrom.Add(nextPosition, getGridSpace(currentPosition)); 
						} else
						{
							cameFrom[nextPosition] = getGridSpace(currentPosition);
						}
						
					}
				}
			} catch (InvalidOperationException) 
			{
				break; // MIGHT BE IRRELEVANT IN CURRENT IMPLEMENTATION --- REVISIT ME ---
			}
		}
		Debug.Log("Goal position is not reachable.");
		return 1; // A path to the goal could not be found
	}


	// GRID FUNCTIONS

	/// <summary>
	/// 
	/// </summary>
	/// <param name="position"></param>
	/// <returns></returns>
	private List<Vector2> getNeighbourPositions(Vector2 position) 
	{
		List<Vector2> results = new List<Vector2>();
		GridSpace origin = getGridSpace(position);
		float originX = position.x;
		float originY = position.y;

		GridSpace possibleNeighbour;

		possibleNeighbour = getGridSpace(new Vector2(originX + 1, originY));
		if (possibleNeighbour != null && !possibleNeighbour.impassible){ results.Add(possibleNeighbour.coords()); }
		possibleNeighbour = getGridSpace(new Vector2(originX - 1, originY));
		if (possibleNeighbour != null && !possibleNeighbour.impassible) { results.Add(possibleNeighbour.coords()); }

		possibleNeighbour = getGridSpace(new Vector2(originX, originY + 1));
		if (possibleNeighbour != null && !possibleNeighbour.impassible) { results.Add(possibleNeighbour.coords()); }
		possibleNeighbour = getGridSpace(new Vector2(originX, originY - 1));
		if (possibleNeighbour != null && !possibleNeighbour.impassible) { results.Add(possibleNeighbour.coords()); }

		return results;
	}
	

	/// <summary>
	/// 
	/// </summary>
	/// <param name="position"></param>
	/// <returns></returns>
	private GridSpace getGridSpace(Vector2 position)
	{
		if (trueGrid.ContainsKey(position))
		{
			return trueGrid[position];
		} else
		{
			Debug.LogWarning("(" + position.ToString() + ") is not in GridManager's trueGrid Dictionary and getGridSpace returned null instead of a GridSpace.");
			return null;
		}
	}


	/// <summary>
	/// 
	/// </summary>
	/// <param name="gridPosition"></param>
	/// <returns></returns>
    private bool outOfGridBounds(Vector2 gridPosition)
    {
        return (gridPosition.x < 0 || gridPosition.x > GridWidth || gridPosition.y < 0 || gridPosition.y > GridHeight);
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
				if (!trueGrid.ContainsKey(new Vector2(i, j)))
				{
					GameObject newFloor = Instantiate(floorPrefab, new Vector3(i, j, 0), Quaternion.identity);
					newFloor.GetComponent<GridSpace>().impassible = false;
					newFloor.transform.parent = Floor.transform;
					newFloor.name = "Floor: (" + i + ", " + j + ")";
				}
				
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


	// HEURISTIC FUNCTIONS - FOR PATHFINDING ESTIMATES

	/// <summary>
	/// 
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns></returns>
	private double heuristicManhattan(Vector2 a, Vector2 b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }


	// MARKER FUNCTIONS

	/// <summary>
	/// 
	/// </summary>
	private void clearMarkers()
	{
		foreach (GameObject marker in markersList)
		{
			Destroy(marker);
		}

		markersList.Clear();
	}


	/// <summary>
	/// 
	/// </summary>
	/// <param name="type"></param>
	private void placeMarker(Vector2 position, string type)
	{
		GameObject newMarker;
		if (type == "path")
		{
			newMarker = Instantiate(pathMarkerPrefab, new Vector3(position.x, position.y, 0), Quaternion.identity);
			newMarker.transform.parent = Markers.transform; // file into Markers GameObject
			markersList.Add(newMarker); // join List of markers
		} else if (type == "goal")
		{
			newMarker = Instantiate(goalMarkerPrefab, new Vector3(position.x, position.y, 0), Quaternion.identity);
			newMarker.transform.parent = Markers.transform; // file into Markers GameObject
			markersList.Add(newMarker); // join List of markers
		} else if (type == "start")
		{
			newMarker = Instantiate(startMarkerPrefab, new Vector3(position.x, position.y, 0), Quaternion.identity);
			newMarker.transform.parent = Markers.transform; // file into Markers GameObject
			markersList.Add(newMarker); // join List of markers
		} else
		{
			Debug.LogError("placeMarker(Vector2 position, string type) type string arguement " + type + " invalid.");
		}
	}


    int GreedyBFSearch(Vector2 startPosition, Vector2 goalPosition)
	{
		clearMarkers();
		markersList.Add(Instantiate(startMarkerPrefab, new Vector3(startPosition.x, startPosition.y, 0), Quaternion.identity)); // drop marker
		markersList.Add(Instantiate(goalMarkerPrefab, new Vector3(goalPosition.x, goalPosition.y, 0), Quaternion.identity)); // drop marker
		if (getGridSpace(startPosition).impassible || getGridSpace(goalPosition).impassible) 
		{
			Debug.Log("Start or Goal position an impassible Gridspace.");
			return 1; 
		} 

		PriorityQueue<Vector2> frontier = new PriorityQueue<Vector2>();
		frontier.Enqueue(startPosition, 0);

		Dictionary<Vector2, GridSpace> cameFrom = new Dictionary<Vector2, GridSpace>();
		cameFrom.Add(startPosition, getGridSpace(startPosition));

		while (cameFrom.Keys.Count != 0)
		{
			try 
			{ 
				Vector2 currentPosition = frontier.Dequeue(); 

				if (currentPosition == goalPosition)
				{
					Debug.Log("Could reach the goal position.");
					return 0; // got 'em
				} else if ((currentPosition != startPosition))  // drop a search marker
				{
					markersList.Add(Instantiate(pathMarkerPrefab, new Vector3(currentPosition.x, currentPosition.y, 0), Quaternion.identity));
				}
				

				foreach (Vector2 nextPosition in getNeighbourPositions(currentPosition))
				{
					if (!cameFrom.ContainsKey(nextPosition))
					{
						frontier.Enqueue(nextPosition, heuristicManhattan(goalPosition, nextPosition));
						cameFrom.Add(nextPosition, getGridSpace(currentPosition));
					}
				}
			} catch (InvalidOperationException) 
			{
				break;
			}
		}
		Debug.Log("Couldn't reach the goal position.");
		return 1; // a path to the goal could not be found
	}

    int BFSearch(Vector2 startPosition, Vector2 goalPosition)
	{
		clearMarkers();
		markersList.Add(Instantiate(startMarkerPrefab, new Vector3(startPosition.x, startPosition.y, 0), Quaternion.identity)); // drop marker
		markersList.Add(Instantiate(goalMarkerPrefab, new Vector3(goalPosition.x, goalPosition.y, 0), Quaternion.identity)); // drop marker

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
					Debug.Log("Could reach the goal position.");
					return 0; // got 'em
				}
				markersList.Add(Instantiate(pathMarkerPrefab, new Vector3(currentPosition.x, currentPosition.y, 0), Quaternion.identity)); // drop marker

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
				break;
			}
		}
		Debug.Log("Couldn't reach the goal position.");
		return 1; // a path to the goal could not be found
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
}