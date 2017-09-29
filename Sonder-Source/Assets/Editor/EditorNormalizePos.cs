using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class EditorNormalizePos : EditorWindow
{
    [MenuItem("Edit/Normalize Pos %3")]
    public static void PlayFromPrelaunchScene()
    {
        foreach (GameObject g in Selection.gameObjects) {
            g.transform.position = new Vector3(Mathf.Round(g.transform.position.x), 0, Mathf.Round(g.transform.position.z));
        }
    }
}
