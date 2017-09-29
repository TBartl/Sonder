using UnityEngine;
using System.Collections;

public class MonsterCharger : LiveEntity {

	Rigidbody rb;
	MonsterAggroSphere aggroSphere;

	Vector3 originalPosition;

	Vector3 direction;
	Vector3 targetDirection;
	
	public float maxSpeed;
	public float rotationSpeed;
	float newDirectionTime;
	public float newDirectionTimeMax;
	public float minimumAwayDistance;


	GameObject skin;
	GameObject eye;

	//public float skinRotateSpeed;


	// Use this for initialization
	void Start () {
		rb = gameObject.GetComponent<Rigidbody> ();
		aggroSphere = transform.Find ("AggroSphere").gameObject.GetComponent<MonsterAggroSphere> ();

		originalPosition = transform.localPosition;
		newDirectionTime = Random.Range(0, newDirectionTimeMax);
		targetDirection = new Vector3 (Random.value -.5f, Random.value-.5f, Random.value-.5f).normalized;

		
		skin = transform.Find ("Skin").gameObject;
		eye = transform.Find ("Eye").gameObject;


	
	}
	
	// Update is called once per frame
	void Update () {	
		CheckForDeath ();
		UpdateDirection ();
		UpdatePosition ();
		UpdateEyesAndSkin ();
	}

	void CheckForDeath() {
		//TODO network server this probably.
		if (health <= 0) 
			Destroy (gameObject);

	}

	void UpdateDirection() {
		GameObject target = aggroSphere.GetTarget ();
		if (target == null) {
			newDirectionTime -= Time.deltaTime;
			if (newDirectionTime <= 0) {
				if (Vector3.Distance (transform.localPosition, originalPosition) <= minimumAwayDistance) {
					targetDirection = new Vector3 (Random.value - .5f, Random.value - .5f, Random.value - .5f).normalized;
				} else {
					targetDirection = (originalPosition - transform.localPosition).normalized;
				}
				newDirectionTime = newDirectionTimeMax;
			} 
		} else {
			targetDirection = transform.InverseTransformDirection((target.transform.position - transform.position).normalized);
		}
	}

	void UpdatePosition() {
		rb.velocity = Vector3.zero;

		direction = Vector3.RotateTowards (direction, targetDirection, rotationSpeed * Time.deltaTime, 10);

		transform.localPosition += direction * maxSpeed * Time.deltaTime;	
	}

	void UpdateEyesAndSkin() {
		eye.transform.LookAt (transform.position + transform.TransformDirection(targetDirection));
		
		//Quaternion currentLookAt = skin.transform.rotation;
		//Quaternion targetLookAt = eye.transform.rotation;
		//skin.transform.rotation = Quaternion.Slerp (currentLookAt, targetLookAt, skinRotateSpeed * Time.deltaTime);

		skin.transform.LookAt (transform.position + transform.TransformDirection (direction));
	}

	void OnCollisionEnter(Collision c) {

		if (c.gameObject.layer != 8 && c.gameObject.name != "MainPlayer(Clone)")
			return;

		Vector3 normal = c.contacts[0].normal;
		normal = transform.TransformVector (normal);
		direction = Vector3.Reflect (direction, normal);

	}

	//TODO when we start doing knockback, possible lower the maxspeed and rotation speed?


}
