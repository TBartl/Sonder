using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerIslandRadar : MonoBehaviour {
	public float maxDistance;
	LevelGen levelGen;

	public float maxTimeBetweenRadar;
	[HideInInspector] public float timeUntilRadar;

	// Use this for initialization
	void Start () {
		levelGen = GameObject.Find ("Main").GetComponent<LevelGen> ();
		timeUntilRadar = maxTimeBetweenRadar;
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		timeUntilRadar -= Time.deltaTime;
		if (timeUntilRadar <= 0) {
			timeUntilRadar = maxTimeBetweenRadar;
			for (int iStatIndex = 0; iStatIndex < levelGen.islandStats.Count; iStatIndex++) {
				IslandStats iStat = levelGen.islandStats[iStatIndex];

				for (int iIndex = 0; iIndex < iStat.islands.Count; iIndex++) {
					IslandGen i = iStat.islands[iIndex];
					if (Vector3.Distance(transform.position, i.transform.position) >= maxDistance)
						i.SetFar(true);
					else 
						i.SetFar(false);
				}
			}
		}	
	}
}
