using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

[System.Serializable]
public enum ItemID {
	none, weaponDefault, armorDefault, artifactDefault, hammer, axe, dagger, plates, scales, tunic, tome, gem, skull
}

[System.Serializable]
public class SubQualities {
	public List<ItemQuality> qualities;

	ItemQuality GetRandom() {
		return qualities [Random.Range (0, qualities.Count)];
	}
	
}

public class ItemDatabase : NetworkBehaviour {
	public List<Weapon> weapons;
	public List<Armor> armors;
	public List<Artifact> artifacts;
	public List<SubQualities> weaponQualities;
	public List<SubQualities> armorQualities;
	public List<SubQualities> artifactQualities;
	public List<Color> qualityColors;
	public GameObject itemHolderPrefab;
	public List<int> PseudoRandomQualities;
	public List<Item> lookUpIDs;


	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	}

	public GameObject CreateItemHolder(Vector3 location, ItemInstance itemInst, GameObject islandGameObject) {
		GameObject temp = (GameObject)GameObject.Instantiate (itemHolderPrefab, location, Quaternion.identity);
		temp.GetComponent<ItemHolder> ().itemInst = itemInst.Clone ();
		temp.GetComponent<ItemHolder> ().islandGameObject = islandGameObject;
		//temp.GetComponent<ItemHolder> () = islandGameObject;
		//temp.GetComponent<ItemHolder> ().targetNetworkID = ID;
		return temp;
	}

	public void NetworkInitItemHolder(GameObject itemObject) {
		NetworkServer.Spawn (itemObject);
	}


	
	
}
