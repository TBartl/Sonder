using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIRollIcon : MonoBehaviour {
	//Color originalColor;
	Image swirl;
	//Image frame;


	// Use this for initialization
	void Awake () {
		//frame = GetComponent<Image> ();
		swirl = transform.GetChild(0).GetComponent<Image> ();
	
	}

	public void SetVariable(float amount) {
		swirl.fillAmount = Mathf.Clamp (amount, 0, 1);

	}
}
