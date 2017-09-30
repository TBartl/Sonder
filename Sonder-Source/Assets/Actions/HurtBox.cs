using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public enum ForceApplyMode {
	none, knockup, posDiff, inversePosDiff, hurtboxVelocity, hurtboxVelocityDiff

}

public class HurtBox : NetworkBehaviour {
	public float damage;
	public float dazeMovementTime;
	public float dazeActionTime;
	public float force;
	public ForceApplyMode forceApplyMode;
	protected List<LiveEntity> alreadyHit = new List<LiveEntity>();

	public virtual void Setup(float damage, float dazeMovementTime, float dazeActionTime, float force, ForceApplyMode forceApply, GameObject exclude) {
		this.damage = damage;
		this.dazeMovementTime = dazeMovementTime;
		this.dazeActionTime = dazeActionTime;
		this.force = force;
		forceApplyMode = forceApply;
		alreadyHit.Add (exclude.GetComponent<LiveEntity>());
	}

	public bool CanTakeDamage(LiveEntity liveEntity) {
		foreach (LiveEntity live in alreadyHit) {
			if (live == liveEntity)
				return false;
		}
		return true;
	}

	public void AddAlreadyHit(LiveEntity liveEntity) {
		alreadyHit.Add (liveEntity);
	}



	public virtual void OnTriggerStay(Collider c) {	
		if (!isServer)
			return;

		if (c.tag == "LiveEntity") {
			Vector3 direction = (c.transform.position - transform.position).normalized;

			if (c.name.StartsWith("MainPlayer")){
				PlayerMain player = c.GetComponent<PlayerMain>();
				if (CanTakeDamage(player)) {
					player.RpcApplyDamage(damage, dazeMovementTime, dazeActionTime, force, direction);
					alreadyHit.Add(player);
					HitSomething();
				}
			} else {
				//TODO for sustained hurtboxes, use something else for already hit
				LiveEntity h = c.GetComponent<LiveEntity>();
				if (CanTakeDamage(h)) {
					h.ApplyDamage(damage, dazeMovementTime, dazeActionTime, force, direction);
					alreadyHit.Add(h);
					HitSomething();
				}
			}


		}
	}

	protected virtual void HitSomething() {
	}

	protected int GetNumHit() {
		return alreadyHit.Count - 1;
	}

	
}
