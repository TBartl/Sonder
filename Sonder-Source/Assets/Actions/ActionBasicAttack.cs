using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[System.Serializable]
[CreateAssetMenu]
public class ActionBasicAttack : Action {

	public override void StartInstance(PlayerMain owner) {
		base.StartInstance (owner);
		Vector3 viewDirection = owner.cameraControl.GetDirectionRaw ();
		owner.actionsController.CmdGenerateAttack (viewDirection, owner.GetWeaponInst(), owner.entityTarget.GetTargetGO());
		done = true;
	}
}
