using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class PlayerInitializer : NetworkBehaviour {

    public List<MonoBehaviour> toEnable;
	
	// Use this for initialization
	void Start () {
		if (isLocalPlayer) {
            foreach (MonoBehaviour b in toEnable) {
                b.enabled = true;
            }
            GameObject uiGameObject = GameObject.Find("UI");
            if (uiGameObject.activeSelf) {
                uiGameObject.SetActive(true);
                uiGameObject.GetComponent<GameUIControl>().enabled = true;
                uiGameObject.GetComponent<GameUIControl> ().mainPlayer = this.gameObject;
            }
		}
	}

}
