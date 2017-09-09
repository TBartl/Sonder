using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
[CreateAssetMenu]
public class Weapon: Item {
	//Primary: damage;
	//Secondary: speed;
	public int actionNumber;
	public Action attackDetails;
	public List<ObjectAndAttachment> objectAndAttachments;
	public Mesh bulletMesh;
	
	public float GetRealDamage(ItemQuality quality) {
		float stat = primaryStat + quality.primaryIncrease;
		stat = (stat + 6) / 2.0f;
		stat = 4.0f * Mathf.Pow(2.0f, stat);
		return stat;

		//return .5f * primaryStat * primaryStat + 2.5f * primaryStat + 8;
		//TODO Make better, using {-2,5}, {0,8}, and {2,15}
	}
	
	public float GetRealSpeed(ItemQuality quality) {
		float stat = secondaryStat + quality.secondaryIncrease;
		stat = (stat + 6) / 2.0f;
		float bpm = 21.5f * Mathf.Pow(2.0f, stat);
		float bps = bpm / 60.0f;
		return 1.0f / bps;
	}

	/*
	//TODO use the base clone function
	new public Weapon Clone() {
		//Item i = (Weapon) base.Clone ();
		Weapon t = ScriptableObject.CreateInstance<Weapon>();
		t.itemID = itemID;
		
		t.icon = icon;
		t.iconOutline = iconOutline;
		t.baseName = baseName;
		t.type = type;
		
		t.primaryStat = primaryStat;
		t.secondaryStat = secondaryStat;
		t.actionNumber = actionNumber;
		t.attackDetails = attackDetails;
		t.objectAndAttachments = objectAndAttachments;
		t.hurtBox = hurtBox;
		return t;
	}
	*/
}