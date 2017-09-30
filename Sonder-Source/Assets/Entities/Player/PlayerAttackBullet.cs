using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

//TODO Fix if target has been destroyed

public class PlayerAttackBullet : AttackBullet {
    Transform targetTransform;
    public float spinSpeed;
    [SyncVar] protected ItemInstance weaponInst;
    public GameObject launcherPrefab;
    public GameObject explosionPrefab;
    Vector3 spin;
    public float maxDistance;

    public float curveAnglesPerSecond;
    public float randomisationAmount;


    public void Setup(float damage, float dazeMovementTime, float dazeActionTime, float force, ForceApplyMode forceApply,
                      GameObject exclude, Vector3 origin, Vector3 direction,
                      ItemInstance weaponInst, GameObject targetTransformGO) {
        base.Setup(damage, dazeMovementTime, dazeActionTime, force, forceApply,
                   exclude, origin, direction);
        this.weaponInst = weaponInst;

        if (targetTransformGO == null) {
            GameObject tempGO = new GameObject("Target");
            tempGO.transform.position = transform.position + direction * maxDistance;
            this.targetTransform = tempGO.transform;
            this.maxLifeSpan = maxDistance / speed;
            Destroy(tempGO, 12);
        }
        else {
            this.targetTransform = targetTransformGO.transform;
            this.maxLifeSpan = 10;
            //Randomize extremely, up life
        }

        worldVelocity = (worldVelocity.normalized + Random.onUnitSphere * randomisationAmount).normalized * speed;
        transform.position += .25f * worldVelocity.normalized;
    }

    public override void Start() {
        base.Start();

        spin = Random.onUnitSphere * spinSpeed;

        Quaternion launcherRotation = Quaternion.FromToRotation(Vector3.up, worldVelocity.normalized);
        GameObject temp = (GameObject)GameObject.Instantiate(launcherPrefab, transform.position, launcherRotation);
        temp.transform.Find("Quad").GetComponent<Renderer>().material.SetColor("_TintColor", weaponInst.GetLowAlphaColor());
        foreach (ParticleSystem p in temp.GetComponentsInChildren<ParticleSystem>()) {
            ParticleSystem.MainModule pMain = p.main;
            pMain.startColor = weaponInst.GetLowAlphaColor();
        }
        this.GetComponent<TrailRenderer>().material.SetColor("_TintColor", weaponInst.GetLowAlphaColor());
        Destroy(temp, 1);

        this.GetComponent<MeshFilter>().mesh = ((Weapon)(weaponInst.GetItem())).bulletMesh;
        colorRenderer = this.GetComponent<Renderer>();
        colorRenderer.material.SetColor("_RimColor", ApplyAlphaReduction(weaponInst.GetQualityColor()));
        colorRenderer.material.SetColor("_InteriorColor", ApplyAlphaReduction(weaponInst.GetLowAlphaColor()));

    }

    public override void FixedUpdate() {
        base.FixedUpdate();
        transform.rotation *= Quaternion.Euler(spin * Time.deltaTime);
        colorRenderer.material.SetColor("_RimColor", ApplyAlphaReduction(weaponInst.GetQualityColor()));
        colorRenderer.material.SetColor("_InteriorColor", ApplyAlphaReduction(weaponInst.GetLowAlphaColor()));

        if (isServer) {
            Vector3 currentDirection = worldVelocity.normalized;
            Vector3 targetDirection = (targetTransform.position - transform.position).normalized;
            worldVelocity = Vector3.RotateTowards(currentDirection, targetDirection, curveAnglesPerSecond * Time.deltaTime, 10) * speed;

            //worldVelocity = (worldVelocity + direction * curveForce * Time.deltaTime).normalized * speed;
        }
    }

    public override void DestroyThis() {
        MakeExplosion();
        base.DestroyThis();
        //I have a feeling something is crashing unity here

    }

    void MakeExplosion() {
        GameObject temp = (GameObject)GameObject.Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Color tempColor = weaponInst.GetLowAlphaColor();
        temp.GetComponent<ExplosionColoring>().originalPosition = transform.position;
        temp.GetComponent<ExplosionColoring>().particleColor = tempColor;

        NetworkServer.Spawn(temp);

    }

}
