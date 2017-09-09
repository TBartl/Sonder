using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkDestroyOnTime : NetworkBehaviour {
	public float maxTime;
	float time = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (!isServer)
			return;
		time += Time.deltaTime;
		if (time >= maxTime) {
			NetworkServer.Destroy(this.gameObject);
		}
	
	}
}
