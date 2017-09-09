using UnityEngine;
using System.Collections;

public class OverrideForSandbox : MonoBehaviour {
	public Material skyboxMat;
	// Use this for initialization
	void Start () {
		if (!isActiveAndEnabled)
			return;
		GameObject mainGO = GameObject.Find ("Main");
		RotateSkybox rotateSkybox = mainGO.GetComponent<RotateSkybox> ();
		rotateSkybox.skyboxMat = this.skyboxMat;
	}
}
