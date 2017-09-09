using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class LiveEntity : NetworkBehaviour {
	public float targetSize;
	public float maxHealth;
	public float health;
	
	// Use this for initialization
	void Start () {
				
	}
	
	public void OnTriggerStay(Collider c) {
	}

	public virtual void ApplyDamage(float damage, float dazeMovementTime, float dazeActionTime, float force, Vector3 direction) {
		//Debug.Log ("GOT HIT BOYS");

		this.health = health - damage;
	}
}
