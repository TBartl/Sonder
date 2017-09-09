using UnityEngine;
using System.Collections;

public class AttackGroundHitDetector : MonoBehaviour {
	public bool hitGround;

	// Use this for initialization
	void Awake () {
		hitGround = false;
	}

	void OnTriggerStay(Collider c)
	{	
		//if (!enabled) return;
		if (c.gameObject.layer == 8) { //TODO continue from here
			hitGround = true;
		}
	}
}
