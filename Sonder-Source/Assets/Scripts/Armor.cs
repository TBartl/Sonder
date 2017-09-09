using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
[CreateAssetMenu]
public class Armor: Item {	
	// Primary: defence;
	// Secondary: agility;
	public GameObject objectPrefab;

	public float GetPercentTaken(ItemQuality quality) {
		float stat = primaryStat + quality.primaryIncrease;
		stat = (stat + 4) / 4.0f;
		stat = Mathf.Pow(.5f, stat);
		return stat;
	}

	public int GetMaxRolls(ItemQuality quality) {
		int stat = secondaryStat + quality.secondaryIncrease;
		return stat + 5;
	}

	public float GetRollRechargeTime(ItemQuality quality) {
		int stat = secondaryStat + quality.secondaryIncrease;
		stat *= -1;
		stat += 5; //1 3 5 7 9
		stat += 3; //4 6 8 10 12
		float time = stat / 4.0f; // 1 1.5 2 2.5 3
		time = Mathf.Pow (2, time);
		return time;
	}

	/*
	new public Armor Clone() {
		//Armor t = (Armor)base.Clone();
		
		Armor t = ScriptableObject.CreateInstance<Armor>();
		
		t.icon = icon;
		t.iconOutline = iconOutline;
		t.baseName = baseName;
		t.type = type;
		
		t.primaryStat = primaryStat;
		t.secondaryStat = secondaryStat;
		t.objectPrefab = objectPrefab;
		return t;
	}
	*/
}