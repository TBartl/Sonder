using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerInitializer : NetworkBehaviour {

	
	// Use this for initialization
	void Start () {
		if (isLocalPlayer) {
			GetComponent<PlayerMovementController> ().enabled = true;
			GetComponent<PlayerActionsController> ().enabled = true;
			//GetComponent<PlayerAnimationController> ().enabled = true; //Now is a Network Script
			GetComponent<PlayerMain> ().enabled = true;
			transform.Find("EntityTargetingSystem").gameObject.SetActive (true);
			GameObject.Find ("UI").SetActive (true);
			GameObject.Find ("UI").GetComponent<GameUIControl> ().enabled = true;
			GameObject.Find ("UI").GetComponent<GameUIControl> ().mainPlayer = this.gameObject;
		}
	}

}
