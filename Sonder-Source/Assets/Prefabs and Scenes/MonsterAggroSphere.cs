using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterAggroSphere : MonoBehaviour {

	List<GameObject> inRange;

	void Start () {
		inRange = new List<GameObject> ();
	}

	void Update () {
		//May need this for aggro determination?
	}

	public GameObject GetTarget() {
		if (inRange.Count > 0) {
			//TODO Select based off aggro
			return inRange [0];
		} else {
			return null;
		}
	}

	void OnTriggerEnter(Collider c) {
		if (IsLegitTarget(c.gameObject))
			inRange.Add (c.gameObject);
	}

	void OnTriggerExit(Collider c) {
		if (IsLegitTarget(c.gameObject))
			inRange.Remove (c.gameObject);
	}

	bool IsLegitTarget(GameObject g) {
		//TODO Make this work a bit better
		if (g.name == "MainPlayer(Clone)") {
			return true;
		}
		return false;
	}


}
