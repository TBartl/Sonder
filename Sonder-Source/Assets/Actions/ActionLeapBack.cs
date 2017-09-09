using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[System.Serializable]
[CreateAssetMenu]
public class ActionLeapBack : Action {
	
	public override void StartInstance(PlayerMain owner) {
		base.StartInstance (owner);
		Quaternion rotation = owner.cameraControl.GetFacingRotationOnMovementPlane ();
		owner.actionsController.CmdGenerateLeapKnockback (rotation);
		//TODO Jump Back too
		done = true;
	}
}