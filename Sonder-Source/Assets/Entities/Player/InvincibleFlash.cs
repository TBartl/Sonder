using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvincibleFlash : MonoBehaviour {

    Coroutine co;
    bool going = false;
    Renderer[] renderers;
    public float flashTime;

    public void TryUpdate(bool newGoing) {
        if (!going && newGoing) {
            going = true;
            co = StartCoroutine(Flash());
        }
        else if (going && !newGoing) {
            going = false;
            StopCoroutine(co);
            foreach (Renderer r in renderers) {
                r.enabled = true;
            }
        }

    }

    IEnumerator Flash() {
        renderers = this.GetComponentsInChildren<Renderer>();
        while (true) {
            foreach (Renderer r in renderers) {
                r.enabled = false;
            }
            yield return new WaitForSeconds(flashTime);
            foreach (Renderer r in renderers) {
                r.enabled = true;
            }
            yield return new WaitForSeconds(flashTime);
        }
    }
}
