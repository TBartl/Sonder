using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
//TODO Generate island hexes twice as big

public class IslandGen : NetworkBehaviour {
	[HideInInspector] public LevelGen parentGen;
	[HideInInspector] public IslandUpdater islandUpdater;
	[SyncVar]
	[HideInInspector] public float radius;
	[SyncVar]
	[HideInInspector] public int size;
	[SyncVar]
	public int rings;
	float timeRecalc;
	
	Vector3 pseudoVel;
	public float forceConstant;
	public float neglibleDistance;

	public float maxBringInTime;
	public float bringInDistance;
	[HideInInspector] public float bringInTime;
	[HideInInspector] public bool gettingDestroyed = false;

	[HideInInspector] float spin;
	public float maxSpinSpeed;
	float spinSpeed;

	public List<GameObject> hexagonPrefabs;
	[SyncVar]
	[HideInInspector] public int iStatsIndex;
	[HideInInspector] public IslandStats iStats;
	HexType[,,] tiles;
	[HideInInspector] public Transform farHolder;
	[HideInInspector] public Transform closeHolder;
	Transform cloudHolder;
	ParticleSystem cloudCover;

	PerlinLayer baseHeightStats;

	float timeUntilSetDirty;

	[SyncVar]
	[HideInInspector] public int randomSeed;

	public float maxPhysicUpdateTime;
	float physicsUpdateTime;






	// Use this for initialization
	void Start () {
		Random.seed = randomSeed;
		
		parentGen = GameObject.Find ("Main").GetComponent<LevelGen> ();
		iStats = parentGen.islandStats[iStatsIndex];
		SetBaseHeight(iStats.baseLayerHeight);


		islandUpdater = GetComponent<IslandUpdater> ();

		transform.position = Random.onUnitSphere;
		farHolder = transform.Find ("Far");
		closeHolder = transform.Find ("Close");

		cloudHolder = transform.Find ("CloudHolder");
		cloudHolder.localScale = Vector3.one * radius / 2; 
		cloudCover = cloudHolder.GetComponentInChildren<ParticleSystem> ();
		cloudHolder.gameObject.SetActive (false); //TODO Re enable if this ever works well enough
		cloudCover.startSize *= radius;


		spin = Random.Range (0.0f, 360.0f);
		spinSpeed = Random.Range (-maxSpinSpeed, maxSpinSpeed);
		timeRecalc = Random.Range (0, parentGen.islandMoveIntervals);

		foreach (PerlinLayer layer in iStats.layerStats) {
			layer.Set(); //TODO maybe save seperatley incase we need to reaccess this (it will get overwritten by the next island)	
		}

		GenerateLowQuality ();		
		tiles = new HexType[iStats.layerStats.Count + iStats.cityLayerStats.Count, size,size];
		GenerateBaseHexes ();
		//TODO put in a loop
		if (iStats.cityLayerStats.Count != 0) {
			for (int index = 0; index < iStats.cityLayerStats.Count; index++){
				iStats.cityLayerStats [index].GenerateHuts (iStats.layerStats.Count + index, tiles);
				iStats.cityLayerStats [index].GenerateConnections (iStats.layerStats.Count + index, tiles);
				iStats.cityLayerStats [index].RemoveSingleHuts (iStats.layerStats.Count + index, tiles);
				iStats.cityLayerStats [index].FillHoles (iStats.layerStats.Count + index, tiles);
				iStats.cityLayerStats [index].RemoveIfNoBottom (iStats.layerStats.Count + index, tiles);
				if (index != 0) 
					iStats.cityLayerStats [index-1].GenerateStairs (iStats.layerStats.Count + index - 1, tiles);
				//iStats.cityLayerStats [index].RemoveSingleHuts (iStats.layerStats.Count + index, tiles);
			}

		}
		DrawBaseHexes ();
		if (iStats.cityLayerStats.Count != 0)
			DrawCityHexes ();

		CapsuleCollider cTemp = gameObject.GetComponent<CapsuleCollider> ();
		cTemp.radius = radius + 5;
		cTemp.height = cTemp.radius*2f;

		OcclusionArea oTemp = gameObject.GetComponent<OcclusionArea> ();
		oTemp.size = Vector3.one * (radius + 1) * 2;
		
		islandUpdater.SetExpectedNumOfWeapons ();

		//TODO Fix to allow more or less islands added.
		if (!isServer) {
			parentGen.islandStats [iStatsIndex].islands.Add (this);
			parentGen.AddOneIsland(this);
		}

		
		SetFar (true);


		Random.seed = (int)System.DateTime.Now.Ticks;

		
		if (isServer)
			UpdateToSpherePosition (true);	
	}

	// Update is called once per frame
	void FixedUpdate () {
		if (!isServer) 
			return;



		physicsUpdateTime = Mathf.Min (physicsUpdateTime - Time.deltaTime, 0);
		if (physicsUpdateTime <= 0) {
			UpdateVel ();
			UpdateToSpherePosition (false);
			physicsUpdateTime = maxPhysicUpdateTime;
		}
	}

	//TODO Optimise the hell out of this
	void UpdateVel() {
		timeRecalc += Time.deltaTime;
		if (parentGen.islandMoveTime > 0 && timeRecalc >= parentGen.islandMoveIntervals) {
			pseudoVel = Vector3.zero;
			foreach (IslandGen i in parentGen.GetAllIslands()) {	
				float distance = Vector3.Distance(GetPosition(), i.GetPosition()); //TODO Change to squared to save preformance
				distance = Mathf.Max(1, distance);
				if (distance < neglibleDistance) {
					Vector3 direction = (GetPosition() - i.GetPosition())/distance;
					if (distance != 0) {
						Vector3 thisForce = i.GetArea() / (Mathf.Pow(distance, 3f)) * direction;
						pseudoVel += thisForce;
					}
				}
			}
			timeRecalc = 0;
		}
		if (parentGen.islandMoveTime > 0)			
			transform.position += forceConstant * pseudoVel * Time.deltaTime;
	}

	//Moves the island to the correct radius,  
	void UpdateToSpherePosition(bool withCorrectSpinDEBUG) {
		/* Probably not needed anymore, delete this if you see it and still dont need it
		timeUntilSetDirty -= Time.deltaTime;
		if (timeUntilSetDirty <= 0) {
			timeUntilSetDirty = 5;
			transform.position += Vector3.up * 0.0001f;
			transform.rotation *= Quaternion.identity;
		}
		*/


		if (parentGen.islandMoveTime > 0) {
			float rToUse = Mathf.Sqrt (parentGen.smoothRadius * parentGen.smoothRadius - radius * radius);

			if (gettingDestroyed) {
				bringInTime = Mathf.Min (bringInTime + Time.deltaTime, maxBringInTime);
			} else {
				bringInTime = Mathf.Max (bringInTime - Time.deltaTime, 0);
				rToUse += Mathf.Pow (bringInTime / maxBringInTime * bringInDistance, 3);
			}
			transform.position = transform.position.normalized * rToUse;
		}

		if (withCorrectSpinDEBUG || parentGen.islandMoveTime > 0) {
			Quaternion nextRotation = transform.rotation;
			if (transform.position != Vector3.zero)
				nextRotation.SetLookRotation (Vector3.zero - transform.position, Vector3.up);
			//transform.LookAt (Vector3.zero, Vector3.up);

			nextRotation *= Quaternion.Euler (new Vector3 (90, 0, 0));
			//transform.localRotation *= Quaternion.Euler (new Vector3 (90, 0, 0));

			spin = (spin + spinSpeed * Time.deltaTime) % 360;
			nextRotation *= Quaternion.Euler (new Vector3 (0, spin, 0));
			//transform.localRotation *= Quaternion.Euler (new Vector3 (0, spin, 0));

			//TODO Uncomment below
			transform.localRotation = nextRotation; //TODO Optimise further by just setting all of the individual rotations to this.
		}
	}
	void GenerateLowQuality(){
		GameObject lowQuality = GameObject.Instantiate (iStats.farAwayObject);
		lowQuality.transform.parent = farHolder;
		lowQuality.transform.localPosition = Vector3.zero;
		lowQuality.transform.localRotation = Quaternion.identity;
		lowQuality.transform.localScale = new Vector3 (radius / 2, 1, radius / 2);

	}

	void GenerateBaseHexes() {
		int layerCount = iStats.layerStats.Count;
		for (int layerIndex = 0; layerIndex < layerCount; layerIndex++) {			
			Vector2 point = HexHelp.GetCenter (rings);
			tiles[layerIndex, (int)point.y,(int)point.x] = HexType.Earth;
			for (int ring = 1; ring < rings; ring++) {
				point = HexHelp.MoveInDirection (point, HexDir.Up);
				for (int direction = 0; direction < 6; direction++) {
					for (int distance = 0; distance < ring; distance++) {
						bool inThreshold = iStats.layerStats [layerIndex].GetAt ((int)point.x, (int)point.y) > iStats.layerStats [layerIndex].threshold;
						bool hasBelow = HexHelp.CheckBelow((int)point.x, (int)point.y, layerIndex, tiles) ;
						if ((layerIndex == 0 || hasBelow) && inThreshold) 
							tiles [layerIndex, (int)point.y, (int)point.x] = HexType.Earth;
						point = HexHelp.MoveInDirection (point, (HexDir)direction);
					}
				}			
			}
		}
	}
	void DrawBaseHexes() {
		int numLayers = iStats.layerStats.Count;
		//numLayers = Mathf.Min (iStats.layerStats.Count, 3);
		for (int layer = 0; layer < numLayers; layer++) {
			for (int y = 0; y < size; y++) {
				for (int x = 0; x < size; x++) {
					HexType thisType = tiles [layer, y, x];
					if (thisType == HexType.Earth) {
						bool emptyBelow = !HexHelp.CheckBelow(x, y, layer, tiles);
						bool emptyAbove = !HexHelp.CheckAbove(x, y, layer, tiles);
						int prefabNum = (int)HexDraw.middle;
						if (emptyBelow && emptyAbove)
							prefabNum = (int)HexDraw.both;
						else if (emptyBelow && !emptyAbove)
							prefabNum = (int)HexDraw.bottom;
						else if (emptyAbove && !emptyBelow)
							prefabNum = (int)HexDraw.top;

						float height = 2*layer + baseHeightStats.GetAt (x, y) - baseHeightStats.GetAvgHeight ();
						GenerateSingleHex(x, y, height, prefabNum);

					}
				}
			}
		}
	}

	void DrawCityHexes() {
		for (int cityLayer = 0; cityLayer < iStats.cityLayerStats.Count; cityLayer++) {
			for (int y = 0; y < tiles.GetLength(1); y++) {
				for (int x = 0; x < tiles.GetLength(2); x++) {
					int actualLayer = iStats.layerStats.Count + cityLayer;
					HexType thisType = tiles [actualLayer, y, x];
					float height = 4 * cityLayer + 2 * iStats.layerStats.Count + baseHeightStats.GetAt (x, y) - baseHeightStats.GetAvgHeight ();
					if (thisType == HexType.StoneFoundation) {
						int prefabNum = (int)HexDraw.cityHut0 + cityLayer;
						GameObject temp = GenerateSingleHex(x, y, height, prefabNum);
						iStats.cityLayerStats[cityLayer].DeleteFramesAndDoors(temp, cityLayer, actualLayer, y, x, tiles);

					} else if (thisType == HexType.Stairs0) {						
						int prefabNum = (int)HexDraw.stairs1 + cityLayer;
						GameObject temp = GenerateSingleHex(x, y, height, prefabNum);
						temp.transform.localRotation *= Quaternion.Euler(0,60,0);
					} else if (thisType == HexType.Stairs90) {						
						int prefabNum = (int)HexDraw.stairs1 + cityLayer;
						GameObject temp = GenerateSingleHex(x, y, height, prefabNum);
						temp.transform.localRotation *= Quaternion.Euler(0,180,0);
					} else if (thisType == HexType.Stairs180) {						
						int prefabNum = (int)HexDraw.stairs1 + cityLayer;
						GameObject temp = GenerateSingleHex(x, y, height, prefabNum);
						temp.transform.localRotation *= Quaternion.Euler(0,240,0);
					} else if (thisType == HexType.Stairs270) {						
						int prefabNum = (int)HexDraw.stairs1 + cityLayer;
						GameObject temp = GenerateSingleHex(x, y, height, prefabNum);
						//temp.transform.localRotation *= Quaternion.Euler(0,180+60+240,0);
					}
				
				}
			}
		}

	}

	GameObject GenerateSingleHex (int x, int y, float height, int prefabNum) {
		GameObject temp = (GameObject)GameObject.Instantiate (hexagonPrefabs [prefabNum]);
		temp.transform.parent = closeHolder;
		temp.transform.localRotation = Quaternion.identity;
		Vector2 toVec3 = HexHelp.GetRelativePosition (new Vector2 (y, x), rings);
		temp.transform.localPosition = new Vector3 (toVec3.x, height, toVec3.y);
		return temp;

	}

	public void SetFar(bool isFar) {
		if (isFar != farHolder.gameObject.activeSelf) {
			if (isFar) {
				//closeHolder.parent = transform.parent;
				cloudCover.emissionRate = 2;
			} else {
				//closeHolder.parent = transform;
				closeHolder.localPosition = Vector3.zero;
				closeHolder.localRotation = Quaternion.identity;
				cloudCover.emissionRate = 0;
			}
		
			foreach (MeshRenderer meshRenderer in closeHolder.GetComponentsInChildren<MeshRenderer>()){
				meshRenderer.enabled = !isFar;
			}

			foreach (ParticleSystem pSystem in closeHolder.GetComponentsInChildren<ParticleSystem>()){
				pSystem.enableEmission = !isFar;
			}

			farHolder.gameObject.SetActive (isFar);
			//closeHolder.gameObject.SetActive (!isFar);
		}

	}

	public void SetBaseHeight(PerlinLayer original) {
		baseHeightStats = new PerlinLayer ();
		baseHeightStats.power = original.power;
		baseHeightStats.scale = original.scale;
		baseHeightStats.threshold = original.threshold;
		baseHeightStats.Set ();
	}


	public Vector3 GetPosition() {
		return transform.position;
	}

	public float GetArea() {
		return Mathf.PI * radius * radius;
	}

	public Vector3 GetLegitSpot(float heightIncrease, bool localOffset) {
		while (true) {			
			int randX = Random.Range(0, tiles.GetLength(2));
			int randY = Random.Range(0, tiles.GetLength(1));
			int randLayer = Random.Range(0, tiles.GetLength(0) - 1);
			bool badFoundation = tiles[randLayer, randY, randX] == HexType.None ||
				tiles[randLayer, randY, randX] == HexType.Stairs0 ||
				tiles[randLayer, randY, randX] == HexType.Stairs90 ||
				tiles[randLayer, randY, randX] == HexType.Stairs180 ||
				tiles[randLayer, randY, randX] == HexType.Stairs270;

			if (!badFoundation && !HexHelp.CheckAboveMinusHuts(randX, randY, randLayer, tiles)) {
				Vector2 toVec3 = HexHelp.GetRelativePosition (new Vector2 (randY, randX), rings);
				// 
				float height = baseHeightStats.GetAt (randX, randY) - baseHeightStats.GetAvgHeight ();
				height += 2 * Mathf.Min(iStats.layerStats.Count, randLayer+1);
				height += 4 * Mathf.Max(0, randLayer + 1 - iStats.layerStats.Count); 
				height += heightIncrease;

				Vector3 toReturn = Vector3.zero;
				if (localOffset) {
					toReturn = new Vector3(toVec3.x, height, toVec3.y);
				} else {
					GameObject marker = new GameObject();
					marker.transform.parent = transform;
					marker.transform.localPosition = new Vector3(toVec3.x, height, toVec3.y);
					toReturn = marker.transform.position;
					GameObject.Destroy(marker);
				}			


				return toReturn;
			}

		}
	}


}
