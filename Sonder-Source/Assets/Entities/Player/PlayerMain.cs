using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerMain : LiveEntity {
    [HideInInspector] public PlayerMovementController movementController;
    [HideInInspector] public PlayerAnimationController animationController;
    [HideInInspector] public PlayerActionsController actionsController;
    [HideInInspector] public PlayerCameraController cameraControl;
    [HideInInspector] public EntityTargetingSystem entityTarget;
    [HideInInspector] public GameUIControl uiControl;

    //public Weapon defaultWeapon; TODO implement
    [HideInInspector] public int maxJumpCharges = 2;

    public float maxEnergy;
    [HideInInspector] public float energy;

    [HideInInspector] public float experience;
    [HideInInspector] public int level;

    public float glideRechargeSpeed;
    public float maxGlideStamina;

    public Weapon baseWeapon;
    public Armor baseArmor;
    public Artifact baseArtifact;
    public ItemQuality baseQuality;
    [SyncVar] ItemInstance weaponInst;
    [SyncVar] ItemInstance armorInst;
    [SyncVar] ItemInstance artifactInst;
    ItemHolder itemOnTopOf;
    public ItemInstance defaultItemOnTopOf;

    public Action attackAction;

    float remainingInvincFrames;
    [SyncVar] public bool invincible;
    bool lastInvincible;

    TrailRenderer rollTrail;


    void Awake() {
        movementController = GetComponent<PlayerMovementController>();
        animationController = GetComponent<PlayerAnimationController>();
        actionsController = GetComponent<PlayerActionsController>();
        cameraControl = GetComponent<PlayerCameraController>();
        entityTarget = transform.Find("EntityTargetingSystem").GetComponent<EntityTargetingSystem>();
        uiControl = GameObject.Find("UI").GetComponent<GameUIControl>();

        weaponInst = new ItemInstance();
        weaponInst.itemID = baseWeapon.itemID;
        weaponInst.quality = baseQuality;
        attackAction = GetWeapon().attackDetails;

        armorInst = new ItemInstance();
        armorInst.itemID = baseArmor.itemID;
        armorInst.quality = baseQuality;

        artifactInst = new ItemInstance();
        artifactInst.itemID = baseArtifact.itemID;
        artifactInst.quality = baseQuality;

        rollTrail = GetComponent<TrailRenderer>();
    }


    // Use this for initialization
    void Start() {
        health = maxHealth;
        energy = maxEnergy;
        level = 0;
        animationController.AttachWeapon();
        animationController.AttachArmor();
        animationController.AttachArtifact();

    }

    void FixedUpdate() {
        itemOnTopOf = null;
        transform.localRotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update() {
        rollTrail.enabled = invincible;

        if (!isLocalPlayer)
            return;

        if (Input.GetKeyDown(KeyCode.H)) {
            health = maxHealth;
        }

        if (experience >= RequiredExpTotal()) {
            experience = experience - RequiredExpTotal();
            level += 1;
        }
        health = Mathf.Min(health, maxHealth);
        energy = Mathf.Min(energy, maxEnergy);
        CheckGrabItem();


        remainingInvincFrames -= Time.deltaTime;
        lastInvincible = invincible;
        invincible = (remainingInvincFrames > 0);
        if (lastInvincible != invincible) {
            CmdSyncInvincibility(invincible);
        }
    }

    public float RequiredExpTotal() {
        return 100;
    }

    public void OnTriggerEnter(Collider c) {
    }

    new void OnTriggerStay(Collider c) {
        //Debug.Log (c.gameObject.name);
        base.OnTriggerStay(c);

        if (c.tag == "Item") {
            itemOnTopOf = c.gameObject.GetComponent<ItemHolder>();
        }
    }

    void CheckGrabItem() {

        if (itemOnTopOf != null && Input.GetKeyDown(KeyCode.E)) {
            if (itemOnTopOf.itemInst.GetItem().type == ItemType.weapon) {
                if (weaponInst.GetItem().baseName != "") {
                    DropItem(weaponInst.Clone());
                }
                //TODO make a function so there isn't so much copy pasta
                //TODO interrupt this perhaps?
                weaponInst = (itemOnTopOf.itemInst).Clone();
                attackAction = GetWeapon().attackDetails;
                CmdUpdateWeapon(weaponInst);
                uiControl.UpdateAbilityIconFromItem(ItemType.weapon);
                actionsController.SetWeaponBasicAttackSpeed();
                actionsController.AddWeaponAbility();

                DestroyServerItem(itemOnTopOf.gameObject);
            }
            else if (itemOnTopOf.itemInst.GetItem().type == ItemType.armor) {
                if (armorInst.GetItem().baseName != "") {
                    DropItem(armorInst.Clone());
                }
                armorInst = (itemOnTopOf.itemInst).Clone();
                movementController.SetupRolls();
                CmdUpdateArmor(armorInst);
                uiControl.UpdateAbilityIconFromItem(ItemType.armor);
                actionsController.AddArmorAbility();

                DestroyServerItem(itemOnTopOf.gameObject);

            }
            else if (itemOnTopOf.itemInst.GetItem().type == ItemType.artifact) {
                if (artifactInst.GetItem().baseName != "") {
                    DropItem(artifactInst.Clone());
                }
                artifactInst = (itemOnTopOf.itemInst).Clone();
                CmdUpdateArtifact(artifactInst);
                uiControl.UpdateAbilityIconFromItem(ItemType.artifact);
                actionsController.AddArtifactAbility();

                DestroyServerItem(itemOnTopOf.gameObject);
            }
        }

    }

    public override void ApplyDamage(float damage, float dazeMovementTime, float dazeActionTime, float force, Vector3 direction) {
        //base.ApplyDamage (damage, dazeMovementTime, dazeActionTime, force, direction);
        if (!invincible) {
            movementController.AddForce(direction, force, dazeMovementTime);
            this.health = health - damage * GetArmor().GetPercentTaken(GetArmorInst().quality);
        }

        //TODO knock up and shit
        //this.health = health - damage;
    }

    [ClientRpc]
    public void RpcApplyDamage(float damage, float dazeMovementTime, float dazeActionTime, float force, Vector3 direction) {
        //Debug.Log ("Reached RPC Call");
        if (isLocalPlayer)
            this.ApplyDamage(damage, dazeMovementTime, dazeActionTime, force, direction);
    }

    void DropItem(ItemInstance itemInst) {
        Debug.Log("Tried to drop item, need to reimplement");
        //GameObject islandGameObject = playerParent.movableParent.parent.gameObject;
        //CmdDropItem (itemInst, islandGameObject);
    }


    [Command]
    void CmdUpdateWeapon(ItemInstance i) {
        weaponInst = i;
        RpcUpdateWeapon(i);
    }

    [ClientRpc]
    void RpcUpdateWeapon(ItemInstance i) {
        weaponInst = i;
        animationController.AttachWeapon();
    }

    [Command]
    void CmdUpdateArmor(ItemInstance i) {
        armorInst = i;
        RpcUpdateArmor(i);
    }
    [ClientRpc]
    void RpcUpdateArmor(ItemInstance i) {
        armorInst = i;
        animationController.AttachArmor();
    }

    [Command]
    void CmdUpdateArtifact(ItemInstance i) {
        artifactInst = i;
        RpcUpdateArtifact(i);
    }
    [ClientRpc]
    void RpcUpdateArtifact(ItemInstance i) {
        artifactInst = i;
        animationController.AttachArtifact();
    }

    [Command]
    void CmdDropItem(ItemInstance itemInst, GameObject islandGameObject) {
        //NetworkServer.FindLocalObject (island);
        //itemInst = itemInst;
        //Debug.Log ("Tried to drop item: " + itemInst.GetFullName ());
        //GameObject temp = Item.GetItemDatabase().CreateItemHolder (transform.position+ .6f * transform.TransformDirection(Vector3.up), itemInst);
        GameObject temp = Item.GetItemDatabase().CreateItemHolder(transform.position, itemInst, islandGameObject);
        temp.transform.parent = islandGameObject.transform.Find("Close");
        ParticleSystem.EmissionModule particleEmission = temp.GetComponent<ParticleSystem>().emission;
        particleEmission.enabled = true;
        //TODO Reimplement when island moving is fixed

        temp.transform.localPosition += Vector3.up * .4f;
        temp.GetComponent<ItemHolder>().targetTransform = temp.transform.localPosition;
        Item.GetItemDatabase().NetworkInitItemHolder(temp);

    }

    void DestroyServerItem(GameObject toDestroy) {
        CmdDestroyServerItem(toDestroy);
        //GameObject.Destroy(toDestroy);
    }

    [Command]
    void CmdDestroyServerItem(GameObject toDestroy) {
        NetworkServer.Destroy(toDestroy);
    }

    public ItemInstance GetWeaponInst() {
        return weaponInst;
    }
    public ItemInstance GetArmorInst() {
        return armorInst;
    }
    public ItemInstance GetArtifactInst() {
        return artifactInst;
    }

    public Weapon GetWeapon() {
        return (Weapon)GetWeaponInst().GetItem();
    }
    public Armor GetArmor() {
        return (Armor)GetArmorInst().GetItem();
    }
    public Artifact GetArtifact() {
        return (Artifact)GetArtifactInst().GetItem();
    }

    public ItemInstance GetItemOnTopOf() {
        if (itemOnTopOf == null)
            return defaultItemOnTopOf;
        return itemOnTopOf.itemInst;
    }

    public void AddRollInvincibility() {
        remainingInvincFrames = 1.5f;
    }

    [Command]
    public void CmdSyncInvincibility(bool newInvincible) {
        this.invincible = newInvincible;
    }


}
