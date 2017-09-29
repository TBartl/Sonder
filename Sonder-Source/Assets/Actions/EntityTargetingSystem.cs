using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EntityTargetingSystem : MonoBehaviour {
	public GameObject targetingParticlesPrefab;
	GameObject targetingParticlesGO;
	ParticleSystem targetingParticlesReal;
	ParticleSystem.Particle[] targetingParticlesRealParts;
	LiveEntity target;
	List<LiveEntity> potentialTargets;
	public float maxRange;
	public float minimumDotProduct;
	PlayerMain playerMain;

	void Awake() {		
		potentialTargets = new List<LiveEntity> (); //If we don't do this here OnTriggerEnter can be called before Start
	}

	// Use this for initialization
	void Start () {
		targetingParticlesGO = (GameObject)GameObject.Instantiate (targetingParticlesPrefab, transform.position, Quaternion.identity);
		targetingParticlesGO.SetActive (false);
		targetingParticlesReal = targetingParticlesGO.GetComponent<ParticleSystem> ();
		targetingParticlesRealParts = new ParticleSystem.Particle[targetingParticlesReal.main.maxParticles];
		playerMain = transform.GetComponentInParent<PlayerMain> ();	
		
	
	}

	void Update() {		
		if (target != null) {
			targetingParticlesGO.transform.position = target.transform.position;
		} 

	}

	void FixedUpdate () {
		//target = null;
		LiveEntity newLiveEntity = null;
		//Debug.Log (potentialTargets.Count);
		float bestDotProduct = 0;
		for (int index = 0; index < potentialTargets.Count; index++) {
			//Debug.Log(potentialTargets[index].name);
			if (potentialTargets[index] == null) {
				potentialTargets.RemoveAt(index);
				continue;
				//WE PROBABLY NEED TO BE MORE CAREFUL HERE, KEEP AN EYE OUT
			} else {
				Vector3 cameraHolderPos = playerMain.cameraControl.GetPivotPointPos();
				float dotProduct = Vector3.Dot(playerMain.cameraControl.GetDirectionRaw().normalized, (potentialTargets[index].transform.position - cameraHolderPos).normalized);
				float distance = Vector3.Distance(cameraHolderPos, potentialTargets[index].transform.position);
				if (distance <= maxRange + potentialTargets[index].targetSize / 2.5f  && dotProduct >= minimumDotProduct) {
					if (newLiveEntity == null) {
						newLiveEntity = potentialTargets[index];
						bestDotProduct = dotProduct;
					} else if (dotProduct > bestDotProduct) {
						newLiveEntity = potentialTargets[index];
						bestDotProduct = dotProduct;
					}
				}
			}
		}	

		if (newLiveEntity != target) {
			SetNewTarget(newLiveEntity);
		}
	
	}

	void SetNewTarget(LiveEntity newLiveEntity) {
		target = newLiveEntity;
		if (target != null) {
			targetingParticlesReal.startSize = target.targetSize;
			int numCurrent = targetingParticlesReal.GetParticles(targetingParticlesRealParts);
			for (int index = 0; index < numCurrent; index++){
				targetingParticlesRealParts[index].size = target.targetSize;
			}
			targetingParticlesReal.SetParticles(targetingParticlesRealParts, numCurrent);
			
			if (targetingParticlesGO.activeSelf == false)
				targetingParticlesGO.SetActive (true);	
		} 
		else {
			if (targetingParticlesGO.activeSelf == true)
				targetingParticlesGO.SetActive (false);
		}

	}

	void OnTriggerEnter(Collider c) {
		if (c.tag == "LiveEntity") {
			potentialTargets.Add(c.GetComponent<LiveEntity>());
		}
	}

	//TODO this might fuck up for things not destroyed, use caution
	void OnTriggerExit(Collider c) {
		if (c.tag == "LiveEntity") {
			potentialTargets.Remove(c.GetComponent<LiveEntity>());			
		}
	}

	public Transform GetTarget() {
		if (target == null)
			return null;
		return target.transform;
	}

	public GameObject GetTargetGO() {
		if (target == null)
			return null;
		return target.gameObject;
	}
}
