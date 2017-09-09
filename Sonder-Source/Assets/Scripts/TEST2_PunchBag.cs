using UnityEngine;
using System.Collections;

public class TEST2_PunchBag : MonoBehaviour {
	LiveEntity liveEntity;

	// Use this for initialization
	void Start () {
		liveEntity = GetComponent<LiveEntity> ();
	
	}
	
	// Update is called once per frame
	void Update () {
		liveEntity.health = Mathf.Min(liveEntity.health + 20 * Time.deltaTime, liveEntity.maxHealth);
		float val = liveEntity.health / liveEntity.maxHealth;
		GetComponent<Renderer> ().material.color = new Color (val, val, val);
	
	}
}
