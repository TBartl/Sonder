using UnityEngine;
using System.Collections;

public class RotateOverTime : MonoBehaviour {
    Vector3 rotation;
    public Vector3 rotateAmt;

	// Use this for initialization
	void Start () {
        rotation = transform.localRotation.eulerAngles;
	
	}
	
	// Update is called once per frame
	void Update () {
        rotation += rotateAmt * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(rotation);
	}
}
