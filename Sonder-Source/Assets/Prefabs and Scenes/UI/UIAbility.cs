using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIAbility : MonoBehaviour {
	Image iconImage;
	Image backGroundImage;
	Image cooldownImage;
    

	// Use this for initialization
	void Awake () {
		iconImage = transform.Find("Icon").gameObject.GetComponent<Image> ();
		backGroundImage = transform.Find("Background").gameObject.GetComponent<Image> ();
		cooldownImage = transform.Find("Cooldown").gameObject.GetComponent<Image> ();

	}

	public void UpdateIcon(Action a) {
		if (a == null) {
			gameObject.SetActive(false);
		} else {
			gameObject.SetActive(true);			
			iconImage.sprite = a.sprite;
		}

	}

	public void UpdateCooldown(float timeRemaining, float maxTime) {
		if (timeRemaining > 0) {
			cooldownImage.fillAmount = 1 - timeRemaining / maxTime;
			backGroundImage.color = Color.black;
			iconImage.color = Color.white;

		} else {
			cooldownImage.fillAmount = 1;
			backGroundImage.color = Color.white;
			iconImage.color = Color.black;
		}

	}
}
