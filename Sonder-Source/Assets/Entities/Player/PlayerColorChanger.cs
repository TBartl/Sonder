using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PlayerColorChange {
    public Texture body;
    public Texture hair;

}

public class PlayerColorChanger : MonoBehaviour {

    public SkinnedMeshRenderer body;
    public SkinnedMeshRenderer hair;

    public List<PlayerColorChange> changes;

    static int last = -1;

	// Use this for initialization
	void Start () {
        int num = -1;
        while (num == -1 || num == last) {
            num = Random.Range(0, changes.Count);
        }
        last = num;
        body.material.mainTexture = changes[num].body;
        hair.material.mainTexture = changes[num].hair;
    }
}
