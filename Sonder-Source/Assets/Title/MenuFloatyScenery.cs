using UnityEngine;
using System.Collections;

public class MenuFloatyScenery : MonoBehaviour {
	Vector3 originalPosition;
	public float oscillateSpeed;
	float posInSinWave;
	public float oscillateHeight;

	public float spinSpeed;
	float spin;




	// Use this for initialization
	void Start () {
		originalPosition = transform.localPosition;
		posInSinWave = Random.Range(0, 2 * Mathf.PI) ;
		spin = 0;
	}
	
	// Update is called once per frame
	void Update () {
		posInSinWave = (posInSinWave + Time.deltaTime * oscillateSpeed) % (2 * Mathf.PI);
		transform.localPosition = originalPosition + Vector3.up * oscillateHeight * Mathf.Sin (posInSinWave);

		spin = (spin + spinSpeed * Time.deltaTime) % 360;
		transform.localRotation = Quaternion.Euler (new Vector3 (0, spin, 0));
	
	}
}
