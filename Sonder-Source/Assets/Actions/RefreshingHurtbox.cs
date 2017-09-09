using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RefreshingHurtbox : HurtBox {
	public float timeToRefresh = .5f;
	float refresh = -5f;

	void Update() {
		refresh += Time.deltaTime;
		if (refresh >= timeToRefresh) {
			refresh = -5f;
			alreadyHit .Clear();
		}
	}

	protected override void HitSomething ()
	{
		base.HitSomething ();
		refresh = 0;
		

	}
}
