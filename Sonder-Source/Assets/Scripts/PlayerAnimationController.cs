using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class PlayerAnimationController : NetworkBehaviour {
	// TODO literally everything
	// TODO Make acceleration tilt less choppy
	public float aSmallValue;
	public float maxTilt;
	public float maxTiltGlide;
	PlayerMain playerMain;
	PlayerMovementController movementController;


	Transform armatureFaceVel;

	[SyncVar] 
	public float faceDirAngle;

	Transform armatureTilt;
	Transform armatureRoot;
	Animator animator;
	SkinnedMeshRenderer baseBodyRenderer;

	 
	public List<GameObject> weaponObjects;
	public GameObject armorObject;
	public GameObject artifactObject = null;
	public List<Transform> attachmentPoints;


	void Awake() {
		//faceDirAngle = 0;
		movementController = GetComponent<PlayerMovementController> ();
		playerMain = GetComponent<PlayerMain> ();
		animator = GetComponent<Animator> ();
		armatureTilt = transform.Find ("ArmatureTilt");
		armatureFaceVel = armatureTilt.Find ("ArmatureFaceVel");
		armatureRoot = armatureFaceVel.Find ("Armature/root");
		baseBodyRenderer = this.transform.Find ("BaseBody").GetComponent<SkinnedMeshRenderer>();
		
		FindAttachmentPoints ();
	}

	// Use this for initialization
	void Start () {	
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (isLocalPlayer) {
			Vector3 groundVelocity = movementController.GetGroundedVelocity ();
			if (groundVelocity.magnitude >= aSmallValue)

				SetFaceDirection (Mathf.Rad2Deg * Mathf.Atan2 (-groundVelocity.z, groundVelocity.x));
			//if (false && movementController.GetMoveState () != MoveState.rolling) {
			//	Vector3 cameraDirection = movementController.InCameraDirection (Vector3.forward);
			//	SetFaceDirection (Mathf.Rad2Deg * Mathf.Atan2 (-cameraDirection.z, cameraDirection.x));
			//	Vector3 moveDirection = movementController.OutOfCameraDirection (movementController.GetGroundedVelocity ().normalized);
			//	if (moveDirection.z >= -.1f)
			//		animator.SetFloat ("VelCompX", moveDirection.x);
			//	else
			//		animator.SetFloat ("VelCompX", -moveDirection.x);
				
				
			//}
			Vector3 accTilt = movementController.GetAccelerationDirection ();
			if (movementController.GetMoveState () == MoveState.gliding)
				armatureTilt.localRotation = Quaternion.Euler (accTilt.z * maxTiltGlide, 0, -accTilt.x * maxTiltGlide);
			else
				armatureTilt.localRotation = Quaternion.Euler (accTilt.z * maxTilt, 0, -accTilt.x * maxTilt);

			animator.SetInteger ("MoveState", (int)movementController.GetMoveState ());
			//Debug.Log ((int)movementController.GetMoveState ());
			animator.SetFloat ("Velocity", groundVelocity.magnitude);
			animator.SetFloat ("Jump", -movementController.GetVelocity ().y / (movementController.initialJumpPower + movementController.maxJumpPowerAdd / 2.5f));
			animator.SetFloat ("Flapping", movementController.GetFlapping ());

			CmdSyncData(faceDirAngle);

		}

		armatureFaceVel.localRotation = Quaternion.Euler (new Vector3 (0, 90 + faceDirAngle, 0));

		
		//armRotation.x = 90 * accTilt.z;
		//armRotation.z = 90 * accTilt.x;

	}

	public void SetFaceDirection(float f){
		faceDirAngle = f;
		//armatureFaceVel.localRotation = Quaternion.Euler (v);
	}

	void FindAttachmentPoints() {
		Transform[] allChildren = armatureFaceVel.GetComponentsInChildren<Transform>();
		foreach (Transform child in allChildren) {
			if (child.name.StartsWith ("_"))
				attachmentPoints.Add (child);
		}
	}

	public void AttachWeapon() {
		foreach (GameObject oldObjects in weaponObjects) {
			Destroy(oldObjects);
		}
		foreach (ObjectAndAttachment oAndA in playerMain.GetWeapon().objectAndAttachments) {
			GameObject temp = (GameObject) GameObject.Instantiate(oAndA.objectPrefab);
			temp.GetComponent<Renderer>().material.color = playerMain.GetWeaponInst().GetHalfQualityColor();
			weaponObjects.Add(temp);

			temp.GetComponent<Renderer>().material.color = playerMain.GetWeaponInst().GetHalfQualityColor();
			foreach (Transform t in attachmentPoints) {
				if (string.Equals(t.name, "_" + oAndA.attachmentNameHold) )
					temp.transform.parent = t;
					temp.transform.localPosition = Vector3.zero;
					temp.transform.localRotation = Quaternion.Euler(0,0,90);
			}
			weaponObjects.Add(temp);
		}		
		//weapon.SetupHurtbox(weaponObjects [0].GetComponent<HurtBox>());
	}


	
	public void AttachArmor() {
		if (armorObject != null)
			Destroy (armorObject);
		GameObject temp = (GameObject) GameObject.Instantiate(playerMain.GetArmor().objectPrefab);
		temp.GetComponent<SkinnedMeshRenderer> ().rootBone = armatureRoot;
		temp.GetComponent<SkinnedMeshRenderer> ().bones = baseBodyRenderer.bones;
		temp.transform.parent = this.transform;
		temp.transform.localPosition = Vector3.zero;
		temp.GetComponent<Renderer> ().material.color = playerMain.GetArmorInst().GetHalfQualityColor ();
		armorObject = temp;
	}

	public void AttachArtifact() {
		if (artifactObject != null)
			Destroy (artifactObject);
		GameObject temp = (GameObject) GameObject.Instantiate(playerMain.GetArtifact().ojectAndAttachment.objectPrefab);
		temp.GetComponent<Renderer> ().material.color = playerMain.GetArtifactInst().GetHalfQualityColor ();

		foreach (Transform t in attachmentPoints) {
			if (string.Equals(t.name, "_" + playerMain.GetArtifact().ojectAndAttachment.attachmentNameHold) )
				temp.transform.parent = t;
		}		
		temp.transform.localPosition = Vector3.zero;
		temp.transform.localRotation = Quaternion.Euler(0,0,90);
		
		artifactObject = temp;

	}



	public void StartWeaponAttack() {
		/*
		animator.SetInteger ("ActionNumber", 1);
		animator.SetInteger ("WeaponNumber", playerMain.GetWeapon ().actionNumber);
		//animator.GetCurrentAnimatorStateInfo (1).speed = 1;
		

		float speed = playerMain.GetWeapon().GetRealSpeed();
		//Debug.Log (speed);
		//float animSpeed = .8f * speed * speed - 10.16f * speed + 9.47f;
		//float animSpeed = 0.494473f + 0.0514484f * speed - 0.0109465f * speed * speed;
		float animSpeed = speed;
		animator.SetFloat ("AttackSpeed", animSpeed); //TODO In unity 5, replace blend states with paremeters
		*/
		
		//TODO Set speed and stuff

	}

	public void EndWeaponAttack() {
		animator.SetInteger ("ActionNumber", 0);
	}



	[Command]
	void CmdSyncData(float faceDirAngle) {
		this.faceDirAngle = faceDirAngle;
	}


}
