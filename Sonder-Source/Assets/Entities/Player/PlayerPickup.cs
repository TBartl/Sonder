using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickup : MonoBehaviour {

    PlayerMain playerMain;

    // Use this for initialization
    void Start () {
        playerMain = this.GetComponentInParent<PlayerMain>();
	}

    void OnTriggerStay(Collider other) {
        ItemHolder holder = other.GetComponent<ItemHolder>();
        if (holder && playerMain)
            playerMain.SetItemHolder(holder);
    }

}
