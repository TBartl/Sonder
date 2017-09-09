using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
[CreateAssetMenu]
public class Artifact: Item {	
	//Primary: control;
	//Secondary: haste;
	public ObjectAndAttachment ojectAndAttachment;

	/*
	new public Artifact Clone() {
		//Artifact t = (Artifact)base.Clone();
		
		Artifact t = ScriptableObject.CreateInstance<Artifact>();
		
		t.icon = icon;
		t.iconOutline = iconOutline;
		t.baseName = baseName;
		t.type = type;
		
		t.primaryStat = primaryStat;
		t.secondaryStat = secondaryStat;
		t.ojectAndAttachment = ojectAndAttachment;
		return t;
	}
	*/
}