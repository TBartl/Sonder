using UnityEngine;
using System.Collections;
using UnityEngine.Networking;


public class IslandTransformSync : NetworkBehaviour {
	[SyncVar] Vector3 targetTransform;
	[SyncVar] Quaternion targetRotation;

	// Use this for initialization
	void Start () {
		FixedUpdate ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (isServer) {
			targetTransform = transform.position;
			targetRotation = transform.rotation; 
		} else {
			transform.position = targetTransform;
			transform.rotation = targetRotation;
		}
	}
}
