using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ExplosionColoring : NetworkBehaviour {
	[SyncVar] public Vector3 originalPosition;
	[SyncVar] public Color particleColor;
	float timeToDestroy = 1.5f;

	// Use this for initialization
	void Start () {
		transform.position = originalPosition;
		foreach (ParticleSystem p in this.GetComponentsInChildren<ParticleSystem>()) {
			p.startColor = particleColor;
			p.Clear();
			p.startColor = particleColor;
			p.Play();
			p.startColor = particleColor;
		}
		//TODO make this a network object I guess?

	}

	void FixedUpdate() {
		if (!isServer)
			return;

		timeToDestroy -= Time.deltaTime;
		if (timeToDestroy <= 0) {
			NetworkServer.Destroy(this.gameObject);
			//Network.Destroy(this.gameObject);
		}

	}
}
