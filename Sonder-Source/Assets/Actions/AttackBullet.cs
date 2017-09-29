using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class AttackBullet : HurtBox {
    [SyncVar] protected Vector3 worldVelocity;
    protected Renderer colorRenderer;
    protected float elapsedLife;
    public float fadeInTime;
    public float speed;
    public float maxLifeSpan;
    AttackGroundHitDetector groundHitDetector;

    // Use this for initialization
    public void Setup(float damage, float dazeMovementTime, float dazeActionTime, float force, ForceApplyMode forceApply,
                              GameObject exclude, Vector3 origin, Vector3 direction) {
        base.Setup(damage, dazeMovementTime, dazeActionTime, force, forceApply, exclude);
        transform.position = origin;
        //worldVelocity = direction.normalized * (maxDistance / maxLifeSpan);
        worldVelocity = direction.normalized * speed;
        elapsedLife = 0;

    }

    public virtual void Start() {
        if (!isServer)
            elapsedLife = .2f;
        groundHitDetector = transform.GetChild(0).gameObject.GetComponent<AttackGroundHitDetector>();
    }

    // Update is called once per frame
    public virtual void FixedUpdate() {
        elapsedLife += Time.deltaTime;

        if (!isServer)
            return;

        //bool hitGround = groundHitDetector.hitGround;
        bool reachedLifeSpan = elapsedLife > maxLifeSpan;
        bool hitSomething = base.GetNumHit() > 0;

        if (groundHitDetector.hitGround == true || reachedLifeSpan || hitSomething) {
            this.DestroyThis();
        }

        transform.position += worldVelocity * Time.deltaTime;


    }

    protected Color ApplyAlphaReduction(Color c) {
        c = new Color(c.r, c.g, c.b, c.a * Mathf.Min(1, elapsedLife / fadeInTime));
        return c;
    }

    public override void OnTriggerStay(Collider c) {
        base.OnTriggerStay(c);
    }

    public virtual void DestroyThis() {

        NetworkServer.Destroy(this.gameObject);
    }
}
