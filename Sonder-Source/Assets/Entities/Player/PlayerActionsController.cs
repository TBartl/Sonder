using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

//TODO global CD?


public class PlayerActionsController : NetworkBehaviour {
	PlayerMain playerMain;

	float attackRecharge;
	public float maxAttackRecharge;

	Action weaponBasicAttack;
	Action weaponAbility;
	Action armorAbility;
	Action artifactAbility;
	//List<ActionInstance> spells;

	Action current;


	public GameObject basicAttackFlare;
	public GameObject chargeAttack;
	public GameObject leapKnockbackHurtbox;
	


	// Use this for initialization
	void Start () {
		if (!isLocalPlayer)
			return;
		playerMain = GetComponent<PlayerMain> ();
		weaponBasicAttack = playerMain.attackAction;
		SetWeaponBasicAttackSpeed ();
	}

	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer)
			return;
		
		UpdateCDIfNotNull (weaponBasicAttack);
		UpdateCDIfNotNull (weaponAbility);
		UpdateCDIfNotNull (armorAbility);
		UpdateCDIfNotNull (artifactAbility);

		if (current == null) {
			TryActionInstance(weaponBasicAttack, KeyCode.Mouse0);
			TryActionInstance(weaponAbility, KeyCode.Alpha1);
			TryActionInstance(armorAbility, KeyCode.Alpha2);
			TryActionInstance(artifactAbility, KeyCode.Alpha3);

		} else {
			current.UpdateInstance();
			if (current.CheckDone())
				current = null;
		}
	
	}

	public void UpdateCDIfNotNull(Action actionInstance) {
		if (actionInstance != null) {
			actionInstance.UpdateCooldown();
		}
	}

	void TryActionInstance(Action actionInstance, KeyCode input) {
		if (Input.GetKey (input) && actionInstance != null && actionInstance.CheckReady() && current == null){ //just another check to be certain
			current = actionInstance;
			current.StartInstance(playerMain);
		} 
	}

	public void SetWeaponBasicAttackSpeed() {
		float attackSpeed = playerMain.GetWeapon ().GetRealSpeed (playerMain.GetWeaponInst ().quality);
		weaponBasicAttack.maxCooldown = attackSpeed;

	}

	public void AddWeaponAbility() {
		weaponAbility = playerMain.GetWeapon ().action;
	}

	public void AddArmorAbility() {
		armorAbility = playerMain.GetArmor ().action;
	}

	public void AddArtifactAbility() {
		artifactAbility = playerMain.GetArtifact ().action;
	}

	public Action GetWeaponAction() {
		return weaponAbility;
	}
	public Action GetArmorAction() {
		return armorAbility;
	}
	public Action GetArtifactAction() {
		return artifactAbility;
	}

	//TODO investigate if owner is important
	[Command]
	public void CmdGenerateAttack(Vector3 direction, ItemInstance weapon, GameObject targetTransformGO) {
		GameObject temp = (GameObject) GameObject.Instantiate (basicAttackFlare, transform.position, Quaternion.identity);
		float damage = ((Weapon)weapon.GetItem ()).GetRealDamage (weapon.quality);
		temp.GetComponent<PlayerAttackBullet> ().Setup (damage, .8f, 0, 5, ForceApplyMode.posDiff, this.gameObject, transform.position, direction, weapon, targetTransformGO);
		NetworkServer.Spawn (temp);
	}

	[Command]
	public void CmdGenerateChargedAttack(float size) {
		GameObject temp = (GameObject) GameObject.Instantiate (chargeAttack, transform.position, Quaternion.identity);
		HurtBox hurtBox = temp.GetComponent<HurtBox> ();
		hurtBox.damage = 20;
		hurtBox.AddAlreadyHit (this.gameObject.GetComponent<LiveEntity> ());
		temp.GetComponent<SphereCollider> ().radius = size;
		NetworkServer.Spawn (temp);
	}

	[Command]
	public void CmdGenerateLeapKnockback(Quaternion rotation) {
		GameObject temp = (GameObject) GameObject.Instantiate (leapKnockbackHurtbox, transform.position, rotation);
		HurtBox hurtBox = temp.GetComponent<HurtBox> ();
		hurtBox.AddAlreadyHit (this.gameObject.GetComponent<LiveEntity> ());
		NetworkServer.Spawn (temp);
	}


}
