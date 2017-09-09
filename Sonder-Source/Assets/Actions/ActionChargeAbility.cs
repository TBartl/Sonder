using UnityEngine;
using System.Collections;

[System.Serializable]
[CreateAssetMenu]
public class ActionChargeAbility : Action {
	public float chargeTime;
	float charge;
	public GameObject telegraphingSpherePrefab;
	GameObject telegraphingSphere;

	public float maxSize;



	public override void StartInstance(PlayerMain owner) {
		base.StartInstance (owner);
		telegraphingSphere = (GameObject)GameObject.Instantiate (telegraphingSpherePrefab, owner.transform.position, Quaternion.identity);
		telegraphingSphere.transform.parent = owner.transform;

		charge = 0;
	}

	public override void UpdateInstance() {
		charge = Mathf.Min(charge + Time.deltaTime, chargeTime); 
		telegraphingSphere.transform.localScale = Vector3.one * maxSize * charge / chargeTime;
		//TODO and input from 1 or left mouse
		if (charge >= chargeTime && (Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Alpha1)) ){
			Vector3 viewDirection = owner.cameraControl.GetDirectionRaw ();
			owner.actionsController.CmdGenerateChargedAttack (maxSize);
			CancelInstance();
		}

	}

	public void CancelInstance() {
		Destroy (telegraphingSphere);
		done = true;
	}
}
