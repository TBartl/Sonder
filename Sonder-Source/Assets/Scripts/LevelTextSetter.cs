using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelTextSetter : MonoBehaviour {
	Image single;
	Image double1;
	Image double2;
	public List<Sprite> sprites;




	// Use this for initialization
	void Start () {
		single = transform.Find ("SingleText").GetComponent<Image> ();
		double1 = transform.Find ("DoubleText1").GetComponent<Image> ();
		double2 = transform.Find ("DoubleText2").GetComponent<Image> ();
		SetLevel (24);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetLevel(int level) {
		if (level <= 9) {
			single.enabled = true;
			double1.enabled = false;
			double2.enabled = false;
			single.sprite = sprites [level];
		} else if (level <= 99) {
			single.enabled = false;
			double1.enabled = true;
			double2.enabled = true;
			double2.sprite = sprites [level % 10];
			double1.sprite = sprites [level / 10];
		} else if (level <= 999) {
			single.enabled = true;
			double1.enabled = true;
			double2.enabled = true;
			single.sprite = sprites [level / 100];
			double2.sprite = sprites [level % 10];
			double1.sprite = sprites [(level / 10) % 10];
		} else {
			single.enabled = true;
			double1.enabled = true;
			double2.enabled = true;
			single.sprite = sprites [0];
			double2.sprite = sprites [0];
			double1.sprite = sprites [0];


		}
	}
}
