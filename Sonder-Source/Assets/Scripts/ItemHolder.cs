using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ItemHolder : NetworkBehaviour {
    [SyncVar] public ItemInstance itemInst;
    public float timeElapsed;
    Vector3 origin;
    //float waveHeight = .15f;
    //float waveSpeed = 3f;
    bool setup = false;
    [SyncVar] public Vector3 targetTransform;
    public float resetTransformTime;
    //[SyncVar] public NetworkInstanceId targetNetworkID;
    [SyncVar] public GameObject islandGameObject;
    ParticleSystem particles;

    // Use this for initialization	
    void Start() {
        //gameObject.GetComponent<NetworkIdentity>().
        particles = GetComponent<ParticleSystem>();
        origin = transform.localPosition;
        transform.localRotation = Quaternion.identity;
        timeElapsed = Random.Range(0, 2 * Mathf.PI);
        Setup();
        FixedUpdate();


    }

    // Update is called once per frame
    void FixedUpdate() {

        //GameObject tryParent = NetworkServer.find (targetNetworkID);
        //if (setup == false && tryParent != null) {
        if (setup == false) {
            setup = true;
            //transform.parent = NetworkServer.FindLocalObject (targetNetworkID).transform.FindChild ("Close");
            transform.parent = islandGameObject.transform.Find("Close");
            //Debug.Log("Found it");
        }

        if (isServer) {
            targetTransform = transform.localPosition;
        }
        else {
            transform.localPosition = targetTransform;
        }

        resetTransformTime -= Time.deltaTime;
        if (resetTransformTime < 0) {
            resetTransformTime = .5f;
            ParticleSystem.EmissionModule emission = this.particles.emission;
            bool originalEmission = emission.enabled;
            emission.enabled = false;
            emission.enabled = true;
            emission.enabled = originalEmission;


            transform.position += 0.02f * Vector3.up;
        }


        if (isServer) {
            timeElapsed = (timeElapsed + Time.deltaTime) % (2 * Mathf.PI);
            //transform.localRotation = Quaternion.identity;
            //transform.localPosition = origin + Vector3.up * waveHeight * Mathf.Sin (waveSpeed * timeElapsed);
            transform.localPosition = origin;
            //transform.position = transform.position + Vector3.up * .02f;
        }
    }

    public void Setup() {
        if (!isServer) {
            //Debug.Log(itemInst.aNumber);
            //Debug.Log (itemInst.itemID);
        }


        gameObject.GetComponent<ParticleSystemRenderer>().material.mainTexture = itemInst.GetItem().icon.texture;
        gameObject.GetComponent<ParticleSystemRenderer>().material.SetTexture("_Outline", itemInst.GetItem().iconOutline.texture);
        gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_OutlineColor", itemInst.GetQualityColor());
        //Debug.Log (Shader.PropertyToID ("Outline Color")); // TODO Experiment with finding the int above for efficiency
        gameObject.SetActive(true);
        gameObject.name = itemInst.GetItem().baseName + " Drop";
        //ItemHolder itemHolder = gameObject.GetComponent<ItemHolder> ();
        //itemHolder.item = item;
    }

}
