
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[System.Serializable]
[CreateAssetMenu]
public class ActionGenerateFlair : Action {
	public GameObject flare;	
	public override void StartInstance(PlayerMain owner) {
		base.StartInstance (owner);
		GameObject.Instantiate (flare, owner.transform.position, owner.transform.rotation);
		done = true;
	}
}