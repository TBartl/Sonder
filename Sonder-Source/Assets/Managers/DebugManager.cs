using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
        //DEBUG
        if (Input.GetKeyDown(KeyCode.LeftBracket))
            Time.timeScale = .5f;
        if (Input.GetKeyDown(KeyCode.P))
            Time.timeScale = .25f;
        if (Input.GetKeyDown(KeyCode.O))
            Time.timeScale = Time.timeScale / 2f;
        if (Input.GetKeyDown(KeyCode.RightBracket))
            Time.timeScale = 1f;
        if (Input.GetKeyDown(KeyCode.L))
            Cursor.lockState = CursorLockMode.Locked;
        if (Input.GetKeyDown(KeyCode.U))
            Cursor.lockState = CursorLockMode.None;
        if (Input.GetKeyDown(KeyCode.T)) {
            foreach (PlayerMovementController movement in GetComponents<PlayerMovementController>())
            movement.SpawnPlayer();
        }
    }
}
