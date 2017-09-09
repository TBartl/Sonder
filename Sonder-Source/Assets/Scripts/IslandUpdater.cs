using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class IslandUpdater : NetworkBehaviour {
	IslandGen islandGen;

	int expectedNumOfWeapons;

	public float itemsPerVolume; 
	public float maxTimeUntilItemSpawn;
	
	float timeUntilItemSpawn;
	List<GameObject> items;

	ItemDatabase itemDatabase;



	// Use this for initialization
	void Awake () {
		islandGen = GetComponent<IslandGen> ();	
		timeUntilItemSpawn = Random.Range(0,maxTimeUntilItemSpawn);
		items = new List<GameObject> ();
		itemDatabase = GameObject.Find ("Main").GetComponent<ItemDatabase> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!isServer)
			return;
		timeUntilItemSpawn = Mathf.Max (0, timeUntilItemSpawn - Time.deltaTime);

		if (timeUntilItemSpawn <= 0 && items.Count < expectedNumOfWeapons) {
			SpawnItem();
		}

	
	}

	public void SetExpectedNumOfWeapons() {
		//float height = 2 * islandGen.iStats.layerStats.Count + 4 * islandGen.iStats.cityLayerStats.Count;
		float height = 1;
		height += .3f * islandGen.iStats.cityLayerStats.Count;
		float volume = Mathf.PI * islandGen.radius * islandGen.radius * height;
		expectedNumOfWeapons = (int)(volume * itemsPerVolume) + 1;
		//expectedNumOfWeapons *= 15; //TODO ERASE
		//expectedNumOfWeapons = 0; //TODO ERASE
		//Debug.Log (expectedNumOfWeapons);
	}

	void SpawnItem() {
		//TODO Add quality, may have to create a clone function for items
		int itemType = Random.Range (0, 3);
		ItemQuality quality = new ItemQuality();
		ItemID id = ItemID.none;

		int thisQuality = itemDatabase.PseudoRandomQualities[ Random.Range(0, itemDatabase.PseudoRandomQualities.Count)];
		thisQuality += 3;
		if (itemType == 0) {
			id = itemDatabase.weapons[Random.Range(0,3)].itemID;
			int subQuality = Random.Range(0, itemDatabase.weaponQualities[thisQuality].qualities.Count);
			quality = itemDatabase.weaponQualities[thisQuality].qualities[subQuality].Clone() ;
		} else if (itemType == 1) {
			id = itemDatabase.armors[Random.Range(0,3)].itemID;
			int subQuality = Random.Range(0, itemDatabase.armorQualities[thisQuality].qualities.Count);
			quality = itemDatabase.armorQualities[thisQuality].qualities[subQuality].Clone() ;
		} else if (itemType == 2) {
			id = itemDatabase.artifacts[Random.Range(0,3)].itemID;
			int subQuality = Random.Range(0, itemDatabase.artifactQualities[thisQuality].qualities.Count);
			quality = itemDatabase.artifactQualities[thisQuality].qualities[subQuality].Clone();
		}

		ItemInstance iInst = new ItemInstance ();
		iInst.itemID = id;
		iInst.quality = quality;

		//TODO make it go on the thingey
		GameObject temp = itemDatabase.CreateItemHolder(Vector3.zero, iInst, this.gameObject);
		temp.transform.parent = this.transform;
		temp.transform.localPosition = islandGen.GetLegitSpot (.7f, true);
		Vector2 randomCircle = Random.insideUnitCircle * 3;
		temp.transform.localPosition += new Vector3 (randomCircle.x, 0, randomCircle.y);
		temp.transform.localRotation = Quaternion.identity;
		items.Add (temp);
		temp.GetComponent<ItemHolder> ().targetTransform = temp.transform.localPosition;
		itemDatabase.NetworkInitItemHolder (temp);
	}
}
