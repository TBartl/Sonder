using UnityEngine;
using System.Collections;

public class PlayerParent : MonoBehaviour {
	public float rotationSpeedFast = 18000;
	public float rotationSpeedSlow = 35;
	[HideInInspector] public Transform movableParent;
	LevelGen levelGen;
	public float fastRotationRemaining = 1;
	Quaternion targetRotation = Quaternion.identity;


	// Use this for initialization
	void Awake () {
		levelGen = GameObject.Find ("Main").GetComponent<LevelGen> ();
		movableParent = new GameObject().transform;
		transform.parent = movableParent;
		movableParent.localPosition = Vector3.zero;


	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (movableParent.parent == null) {
			transform.parent = null;
			targetRotation = Quaternion.FromToRotation(Vector3.down, transform.position.normalized);
			transform.parent = movableParent;
		}

		transform.parent = null;
		movableParent.rotation = Quaternion.RotateTowards (movableParent.rotation, targetRotation, Time.deltaTime*GetRotationSpeed());
		transform.parent = movableParent;

		fastRotationRemaining = Mathf.Max(0, fastRotationRemaining - Time.deltaTime);
	
	}

	public void SetTarget(Transform target) {
		transform.parent = null;

		if (target == null && movableParent.parent != null) {
			movableParent.parent = null;

		} else if (target != null && movableParent.parent == null) {
			targetRotation = Quaternion.FromToRotation(Vector3.down, target.position.normalized);

			movableParent.parent = target;
		}

		transform.parent = movableParent;
	}

	float GetRotationSpeed() {
		if (fastRotationRemaining > 0)
			return rotationSpeedFast;
		return rotationSpeedSlow;
	}

	public bool AttachedToSkybox() {
		if (movableParent.parent == null) 
			return false;
		return movableParent.parent.name == "SandBox";
	}
}
