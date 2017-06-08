using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour 
{

	public static GridManager Instance; // Singleton



	public int GridHeight;
	public int GridWidth;

	public GameObject perimeterPrefab;

	// contains XY coordinates of all edges in scene
	public Dictionary<Vector2, Vector2[]> edges = new Dictionary<Vector2, Vector2[]>();

	void Awake()
	{
		RegisterSingleton(); // register this instance
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

			new1.transform.parent = transform;
			new2.transform.parent = transform;

			new1.name = "Perimeter: (" + 0 + ", " + i + ")";
			new2.name = "Perimeter: (" + GridWidth + ", " + i + ")";
		}
		for (int i = 0; i <= GridWidth; i++)
		{

			GameObject new1 = Instantiate(perimeterPrefab, new Vector3(i, 0, 0), Quaternion.identity);
			GameObject new2 = Instantiate(perimeterPrefab, new Vector3(i, GridHeight, 0), Quaternion.identity);

			new1.transform.parent = transform;
			new2.transform.parent = transform;

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
