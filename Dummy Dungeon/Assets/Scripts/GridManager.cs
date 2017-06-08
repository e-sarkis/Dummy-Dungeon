using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour 
{

	public static GridManager Instance; // Singleton GridManager

	[HideInInspector]
	public GameObject Perimeter; // for Gridspace organization


	public int GridHeight; // Height of Grid in editor units
	public int GridWidth; // Width of Grid in editor units

	public GameObject perimeterPrefab;

	// contains all GridSpaces of all GridSpace edges in scene
	public Dictionary<GridSpace, GridSpace[]> edges = new Dictionary<GridSpace, GridSpace[]>();

	public Dictionary<Vector2, GridSpace> trueGrid = new Dictionary<Vector2, GridSpace>();

	void Awake()
	{
		RegisterSingleton(); // register this instance

		Perimeter = new GameObject();
		Perimeter.transform.parent = transform;
	}


	// Use this for initialization
	void Start () 
	{
		CreateWalledPerimeter();
	}


    // Update is called once per frame
    void Update () 
	{
		DrawDebugPerimeter();
	}


    int BFSearch()
	{
		return 1; // the 
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

	private void CreateWalledPerimeter()
    {
        for (int i = 0; i <= GridHeight; i++)
		{
			GameObject new1 = Instantiate(perimeterPrefab, new Vector3(0, i, 0), Quaternion.identity);
			GameObject new2 = Instantiate(perimeterPrefab, new Vector3(GridWidth, i, 0), Quaternion.identity);

			new1.transform.parent = Perimeter.transform;
			new2.transform.parent = Perimeter.transform;

			new1.name = "Perimeter: (" + 0 + ", " + i + ")";
			new2.name = "Perimeter: (" + GridWidth + ", " + i + ")";
		}
		for (int i = 0; i <= GridWidth; i++)
		{

			GameObject new1 = Instantiate(perimeterPrefab, new Vector3(i, 0, 0), Quaternion.identity);
			GameObject new2 = Instantiate(perimeterPrefab, new Vector3(i, GridHeight, 0), Quaternion.identity);

			new1.transform.parent = Perimeter.transform;
			new2.transform.parent = Perimeter.transform;

			new1.name = "Perimeter: (" + i + ", " + 0 + ")";
			new2.name = "Perimeter: (" + i + ", " + GridHeight + ")";
		}
    }


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
