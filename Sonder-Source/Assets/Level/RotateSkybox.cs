using UnityEngine;
using System.Collections;

public class RotateSkybox : MonoBehaviour {
	public float skyboxRotSpeed;
	public Material skyboxMat;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		float skyboxRot = skyboxMat.GetFloat ("_Rotation");
		skyboxRot = (skyboxRot + skyboxRotSpeed * Time.deltaTime) % 360;
		skyboxMat.SetFloat( "_Rotation", skyboxRot );
		RenderSettings.skybox = skyboxMat;
	}
}
