using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

[System.Serializable]
public class IslandStats {
	public int avgLoops; 
	public int avgLoopsDev;
	public float ratio;
	[HideInInspector] public List<IslandGen> islands;
	[HideInInspector] public float areaUsed = 0;
	public PerlinLayer baseLayerHeight;
	public List<PerlinLayer> layerStats;	
	public List<CityLayer> cityLayerStats;
	public GameObject farAwayObject;
}


public class LevelGen : NetworkBehaviour {

    public static LevelGen S;

	// References to other things
	public GameObject islandPrefab;

	// Total Area Stuff	
	[SyncVar]
	public int totalPlayers;
	public float baseArea;
	public float areaPerPlayer;
	[SyncVar]
	[HideInInspector] public float radius;
	[SyncVar]
	[HideInInspector] public float smoothRadius;
	public float smoothRadiusSpeed;

	//Island Movement Stuff
	public float islandMoveIntervals;
	public float maxIslandMoveTime;
	[HideInInspector] public float islandMoveTime;

	public List<IslandStats> islandStats;

	List<IslandGen> allIslands = new List<IslandGen> ();

    void Awake() {
        S = this;
    }

    // Use this for initialization
    void Start () {
		islandMoveTime = maxIslandMoveTime;
		if (isServer) {
			FillWithIslands ();
			UpdatePlayers ();
			smoothRadius = radius;	
		}
		
	}
	
	// Update is called once per frame
	void Update () { 
		if (isServer) {
			islandMoveTime -= Time.deltaTime;
			if (smoothRadius < radius) {
				smoothRadius = Mathf.Min(smoothRadius + Time.deltaTime*smoothRadiusSpeed, radius); 

			} else if (smoothRadius > radius) {
				smoothRadius = Mathf.Max(smoothRadius - Time.deltaTime*smoothRadiusSpeed, radius); 
			}

			if (Input.GetKeyDown (KeyCode.Alpha9)) {
				totalPlayers += 1;
				FillWithIslands ();
				UpdatePlayers ();
			} else if (Input.GetKeyDown (KeyCode.Alpha0)) {
				totalPlayers -= 1;
				FillWithIslands ();
				UpdatePlayers ();
			}
		}
	}

	public void FillWithIslands( ) {
		float leftOverArea = 0;
		float totalArea = GetArea ();

		foreach (IslandStats i in islandStats) {
			float allocatedArea = totalArea * i.ratio+ leftOverArea;

			// Delete Islands if there are too many
			if (i.areaUsed > allocatedArea) {
				while (i.areaUsed > allocatedArea) {
					i.islands[0].gettingDestroyed = true;
					i.areaUsed -= i.islands[0].GetArea();
					GameObject.Destroy(i.islands[0].gameObject, i.islands[0].maxBringInTime);
					allIslands.Remove (i.islands[0]);
					i.islands.RemoveAt(0);
				}
				leftOverArea = allocatedArea - i.areaUsed;

			// Spawn Islands if there are too few
			} else if (i.areaUsed < allocatedArea){
				while (true) {
					int testRings = i.avgLoops + Random.Range(-i.avgLoopsDev, i.avgLoopsDev);
					float testRadius = HexHelp.radius + (testRings-1)*HexHelp.widthCutoff;
					float testArea = Mathf.PI * testRadius * testRadius;
					if (i.areaUsed + testArea < allocatedArea) {
						GenerateIsland(testRings, testRadius, testArea, i);

					} else {
						leftOverArea = allocatedArea - i.areaUsed;
						break;
					}
				}
			}
		}
	}

	public void GenerateIsland(int rings, float radius, float A, IslandStats i) {
		GameObject tempObject = (GameObject)GameObject.Instantiate(islandPrefab);
		tempObject.transform.parent = this.transform;

		IslandGen tempIsland = tempObject.GetComponent<IslandGen>();
		tempIsland.rings = rings;
		tempIsland.size = rings * 2 - 1;
		tempIsland.radius = radius;
		tempIsland.bringInTime = tempIsland.maxBringInTime;
		tempIsland.transform.rotation = Quaternion.Euler(new Vector3 (Random.Range (0, 360), Random.Range (0, 360), Random.Range (0, 360)));
		//tempIsland.iStats = i;
		tempIsland.iStatsIndex = islandStats.IndexOf (i);
		//tempIsland.SetBaseHeight (i.baseLayerHeight);
		i.islands.Add(tempIsland);	
		allIslands.Add (tempIsland);
		i.areaUsed += A;
		tempIsland.randomSeed = (int)System.DateTime.Now.Ticks;
		

		NetworkServer.Spawn (tempIsland.gameObject);


	}

	public void UpdatePlayers() {
		radius = Mathf.Sqrt (GetArea () / (4.0f * Mathf.PI));
		islandMoveTime = maxIslandMoveTime;
		/*
		allIslands = new List<IslandGen> ();
		foreach (IslandStats iStats in islandStats) {
			allIslands.AddRange(iStats.islands);
		}
		*/

	}
	public void ClientFindIslands() {

	}



	public float GetArea() {
		return baseArea + areaPerPlayer * totalPlayers;
	}

	public List<IslandGen> GetAllIslands () {
		return allIslands;
	}

	public void AddOneIsland(IslandGen i ) {
		allIslands.Add (i);
	}
	
}
