using UnityEngine;
using System.Collections;

[System.Serializable]
public class PerlinLayer {
	
	Vector2 offset;
	public float scale;
	public float power;
	public float threshold;

	public void Set() {
		offset = new Vector2 (1000000f * Random.value, 1000000f * Random.value);
	}
		
	public float GetAt(float x, float y) {
		return power*Mathf.PerlinNoise(offset.x + x*scale, offset.y + y*scale);
	}

	public float GetAvgHeight() {
		return GetAt(0,0);
	}
}