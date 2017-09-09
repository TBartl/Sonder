using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 
[System.Serializable]
[CreateAssetMenu]
public class Action : ScriptableObject{
	protected PlayerMain owner;
	public float maxCooldown;
	float cooldownRemaining;
	public Sprite sprite;
	protected bool done = false;

	virtual public void StartInstance(PlayerMain owner) {
		this.owner = owner;
		done = false;
		cooldownRemaining = maxCooldown;
	}

	virtual public void UpdateInstance() {

	}

	virtual public void InterruptInstance() {

	}

	virtual public bool CheckDone() {
		return done;
	}

	virtual public bool CheckReady() {
		return cooldownRemaining <= 0;
	}

	public void UpdateCooldown() {
		cooldownRemaining -= Time.deltaTime;
	}

	public float GetRemainingCooldown() {
		return cooldownRemaining;
	}

	public float GetMaxCooldown() {
		return maxCooldown;
	}



	


}
/*
[System.Serializable]
public class ActionInstance {
	int reached;
	float timeUntilExecute;
	float maxTime;
	Action action;
	bool done;
	public float maxCooldown;
	float cooldownRemaining;

	public ActionInstance(Action a) {
		SetupNew ();
		done = true;
		action = a;
		maxTime = a.GetTotalTime();	
		maxCooldown = a.baseCooldown;
	}

	public void SetupNew() {
		timeUntilExecute = 0;
		reached = -1;
		done = false;
	}

	public void UpdateCooldown() {
		if (done)
			cooldownRemaining -= Time.deltaTime;
	}


	public void Interrupt() {
		//TODO Implement
	}

	public bool CheckFinished() {
		return done;
	}

	public bool CheckReady() {
		return (done && cooldownRemaining <= 0);
	}
	public float GetRemainingCD() {
		return cooldownRemaining;
	}


	public void UpdateAction(PlayerMain playerMain) {
		timeUntilExecute += Time.deltaTime;
		
		while (timeUntilExecute >= action.executions[reached + 1].timeToExecute) { 
			timeUntilExecute -= action.executions[reached + 1].timeToExecute;
			action.executions[reached + 1].subAction.Execute(playerMain);
			reached += 1;
			if (reached + 1 >= action.executions.Count){
				done = true;
				cooldownRemaining = maxCooldown;
				break;
			}
		}
		
	}

}
*/





