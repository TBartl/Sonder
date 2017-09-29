using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ExplosionColoring : NetworkBehaviour {
    [SyncVar] public Vector3 originalPosition;
    [SyncVar] public Color particleColor;
    float timeToDestroy = 1.5f;

    // Use this for initialization
    void Start() {
        transform.position = originalPosition;
        foreach (ParticleSystem p in this.GetComponentsInChildren<ParticleSystem>()) {
            ParticleSystem.MainModule particleModule = p.main;
            particleModule.startColor = particleColor;
            p.Clear();
            particleModule.startColor = particleColor;
            p.Play();
            particleModule.startColor = particleColor;
        }
        //TODO make this a network object I guess?

    }

    void FixedUpdate() {
        if (!isServer)
            return;

        timeToDestroy -= Time.deltaTime;
        if (timeToDestroy <= 0) {
            NetworkServer.Destroy(this.gameObject);
            //Network.Destroy(this.gameObject);
        }

    }
}
