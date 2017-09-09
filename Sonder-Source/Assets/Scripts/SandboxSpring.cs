using UnityEngine;
using System.Collections;

[System.Serializable]
public enum SandboxSpringType {
	health, energy, experience
}


public class SandboxSpring : MonoBehaviour {
	public SandboxSpringType type;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerStay(Collider c){
		if (c.name == "MainPlayer(Clone)") {
			if (type == SandboxSpringType.health) 
				c.gameObject.GetComponent<PlayerMain> ().health += 45 * Time.deltaTime;
			else if (type == SandboxSpringType.energy)
				c.gameObject.GetComponent<PlayerMain> ().energy += 45 * Time.deltaTime;
			else 
				c.gameObject.GetComponent<PlayerMain> ().experience += 200 * Time.deltaTime;
		}

//Debug.Log ("Stayed");
	}
}
