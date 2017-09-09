using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

[System.Serializable]
public enum ItemType {
	none, weapon, armor, artifact, salve
}

[System.Serializable]
public struct ItemQuality {
	public string prefix;
	public int netQuality;
	public int primaryIncrease;
	public int secondaryIncrease;

	public void ApplyItemQuality(Item i, ItemType t) {
			i.primaryStat += primaryIncrease;
			i.secondaryStat += secondaryIncrease;
			i.baseName = prefix + " " + i.baseName;
	}

	public ItemQuality Clone() {
		ItemQuality temp;
		temp.prefix = prefix;
		temp.netQuality = netQuality;
		temp.primaryIncrease = primaryIncrease;
		temp.secondaryIncrease = secondaryIncrease;
		return temp;
	}
}

[System.Serializable]
public struct ItemInstance {
	//public Item item;
	public ItemQuality quality;
	public ItemID itemID;
	string fullName;


	public Item GetItem() {
		return Item.GetItemDatabase ().lookUpIDs [(int)itemID];
	}

	public int GetNetPrimary(){
		return GetItem().primaryStat + quality.primaryIncrease;
	}

	public int GetNetSecondary(){
		return GetItem().secondaryStat + quality.secondaryIncrease;
	}

	public string GetFullName() {
		return fullName = quality.prefix + " " + GetItem().baseName;
		if (fullName == "") {
			fullName = quality.prefix + " " + GetItem().baseName;
		}
		return fullName;
	}

	public Color GetQualityColor() {
		return Item.GetItemDatabase().qualityColors [quality.netQuality + 4];
	}
	public Color GetHalfQualityColor() {
		Color c = Item.GetItemDatabase().qualityColors [quality.netQuality + 4];
		c /= 2;
		c.a = 1;
		return c;
	}
	public Color GetLightQualityColor() {
		Color c = Item.GetItemDatabase().qualityColors [quality.netQuality + 4];
		c *= 2;
		c.a = 1;
		return c;
	}

	public Color GetLowAlphaColor() {
		Color c = Item.GetItemDatabase().qualityColors [quality.netQuality + 4];
		c.a = .45f;
		return c;
	}

	public ItemInstance Clone() {
		ItemInstance temp = new ItemInstance ();
		temp.itemID = itemID;
		temp.quality = quality.Clone ();
		temp.fullName = "";
		return temp;
	}

	public bool isSetup() {
		return this.GetItem ().type != ItemType.none;
	}

}

//TODO Add an equippable class
[System.Serializable]
[CreateAssetMenu]
public class Item: ScriptableObject{
	public ItemID itemID;
	public Sprite icon;
	public Sprite iconOutline;
	public string baseName;
	public ItemType type;
	public int primaryStat;
	public int secondaryStat;
	public Action action;
	

	static ItemDatabase itemDatabase;

	public Item Clone() {
		Item t = ScriptableObject.CreateInstance<Item>();
		t.icon = icon;
		t.iconOutline = iconOutline;
		t.baseName = baseName;
		t.type = type;
		return t;
	}


	public static ItemDatabase GetItemDatabase() {
		if (itemDatabase == null)
			itemDatabase = GameObject.Find ("Main").GetComponent<ItemDatabase> ();
		return itemDatabase;
	}

}


[System.Serializable]
public class ObjectAndAttachment {
	public GameObject objectPrefab;
	public string attachmentNameHold;

}
