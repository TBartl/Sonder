using UnityEngine;
using System.Collections;

[System.Serializable]
public enum MoveState {
    grounded, airborne, onLedge, rolling, gliding, hit
}

public class PlayerMovementController : MonoBehaviour {
    //TODO: Clean values
    //TODO Max Fallspeed
    //TODO: Adjust friction on sloped surface
    //TODO: Super friction on ledge.

    PlayerCameraController cameraController;
    PlayerAnimationController animController;
    Rigidbody rb;
    PlayerMain playerMain;

    public float acceleration;
    public float friction;
    public float airAcceleration;
    public float airFriction;
    public float maxSpeed;
    Vector3 accelerationDirection = Vector3.zero;

    public float dotProductForGrounded;
    MoveState moveState = MoveState.grounded;

    Vector3 velocity;
    public float maxGroundedDownVelocity;
    public float gravity;
    int jumpCharges;
    public float initialJumpPower;
    public float maxJumpPowerAdd;
    public float addWindow;
    float windowRemainig;
    float jumpResetRemaining;
    public float jumpReset;
    public float maxFallSpeed;
    public float aboveGroundCheck;

    public float ledgeHorizontalScale;
    public float ledgeVerticalScale;
    public float ledgeDistanceCheck;
    public float ledgeResetMax;
    float ledgeReset = 0;

    public float rollInitialSpeed;
    public float rollInfluence;
    public float rollDecay;
    public float rollStopSpeed;
    Vector3 rollDirection;
    float rollSpeed;

    public int maxInvincRolls;
    public float invincRollsRemaining;
    public float invincRollRechargeTime;

    //TODO start from here
    [HideInInspector] public float glideStamina;
    public float glideUpAcc;
    public float glideGravity;
    public float glideTurnSpeed;
    public float maxGlideDelay;
    public float glideMinFallSpeed;
    public float glideMaxRiseSpeed;
    float glideDelay = 0;
    bool glideCanStart = true;
    bool glideCanUp = true;

    float dazeTime = 0;
    public float dazeMovementConstant;



    LayerMask raycastMask = 1 << 8;


    // Use this for initialization
    void Start() {
        rb = GetComponent<Rigidbody>();
        cameraController = GetComponent<PlayerCameraController>();
        animController = GetComponent<PlayerAnimationController>();
        playerMain = GetComponent<PlayerMain>();

        SetupRolls();
        glideStamina = playerMain.maxGlideStamina;
    }

    // Update is called once per frame
    void FixedUpdate() {
        rb.velocity = Vector3.zero;
        //DEBUG 
        /*
		if (moveState == MoveState.airborne)
			GetComponentInChildren<Renderer> ().material.color = Color.red;
		else if (moveState == MoveState.grounded)
			GetComponentInChildren<Renderer> ().material.color = new Color (.5f, .5f, 1);
		else if (moveState == MoveState.rolling)
			GetComponentInChildren<Renderer> ().material.color = Color.green;
		else if (moveState == MoveState.gliding)
			GetComponentInChildren<Renderer> ().material.color = Color.magenta;
		*/

        HandleLedgePhysics();

        if (moveState != MoveState.onLedge && moveState != MoveState.rolling && moveState != MoveState.gliding && CheckAboveGround())
            moveState = MoveState.airborne;

        transform.localPosition += velocity * Time.deltaTime;

        if (moveState == MoveState.grounded && velocity.y < -maxGroundedDownVelocity)
            velocity.y = -maxGroundedDownVelocity;

        CheckOutOfBounds();


    }

    void Update() {


        invincRollsRemaining = Mathf.Min(invincRollsRemaining + Time.deltaTime / invincRollRechargeTime, maxInvincRolls);

        if (moveState == MoveState.grounded) {
            CheckRoll();
        }
        else if (moveState == MoveState.airborne || moveState == MoveState.gliding) {
            CheckGlide();
        }

        if (moveState == MoveState.rolling) {
            HandleRoll();

        }
        else if (moveState == MoveState.onLedge) {
            CheckUnhinge();
        }
        else if (moveState != MoveState.gliding) {
            HandleHorizontal();
        }
        if (moveState != MoveState.rolling && moveState != MoveState.gliding)
            HandleJumps();


        HandleGlide();

        if (moveState != MoveState.onLedge && moveState != MoveState.rolling)
            AddGravity();


        transform.localRotation = Quaternion.identity;
    }

    void HandleHorizontal() {
        // Accelerate the player on the ground
        Vector3 adjustedInput = InCameraDirection(GetInputDirection());
        Vector3 groundMovement = adjustedInput;
        if (groundMovement.magnitude != 0) {
            if (moveState == MoveState.grounded)
                groundMovement = groundMovement * acceleration * Time.deltaTime;
            else
                groundMovement = groundMovement * airAcceleration * Time.deltaTime;
            velocity += groundMovement;

            // Add friction
        }
        else {
            //TODO Super friction if going off edge
            Vector3 groundVector = GetGroundedVelocity();
            float frictionForce;
            if (moveState == MoveState.grounded)
                frictionForce = Mathf.Min(groundVector.magnitude, friction * Time.deltaTime);
            else
                frictionForce = Mathf.Min(groundVector.magnitude, airFriction * Time.deltaTime);
            velocity -= groundVector.normalized * frictionForce;

        }
        // TODO Maybe consider throwing in a * playerMain.GetSpeedReduction()
        float tiltStrength = Mathf.Clamp((1 - Vector3.Dot(adjustedInput, velocity / (maxSpeed))), 0, 1);
        tiltStrength = 1 - 2 * Mathf.Abs(.5f - tiltStrength);
        accelerationDirection = adjustedInput * tiltStrength;



        // Limit the max speed of the player
        float groundSpeed = GetGroundedVelocity().magnitude;
        if (groundSpeed > maxSpeed)
            velocity = new Vector3(velocity.x / groundSpeed * maxSpeed, velocity.y, velocity.z / groundSpeed * maxSpeed);
    }

    void HandleJumps() {

        // Check for jumps
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (jumpCharges != 0) {
                velocity = new Vector3(velocity.x, initialJumpPower, velocity.z);
                jumpCharges -= 1;
                windowRemainig = addWindow;
                jumpResetRemaining = jumpReset;
                if (moveState == MoveState.onLedge) {
                    moveState = MoveState.airborne;
                    ledgeReset = ledgeResetMax;
                }
            }

        }
        else if (Input.GetKey(KeyCode.Space)) {
            float timeThisFrame = Mathf.Min(Time.deltaTime, windowRemainig);
            windowRemainig -= timeThisFrame;
            velocity += Vector3.up * timeThisFrame * maxJumpPowerAdd / addWindow;
        }
        jumpResetRemaining = Mathf.Max(0, jumpResetRemaining - Time.deltaTime);





    }

    void CheckRoll() {
        Vector3 inputDir = InCameraDirection(GetInputDirection());
        if (Input.GetKeyDown(KeyCode.Mouse1) && inputDir.magnitude != 0 && rollSpeed <= rollStopSpeed) {
            if (invincRollsRemaining >= 1) {
                playerMain.AddRollInvincibility();
                invincRollsRemaining -= 1;
            }
            rollSpeed = rollInitialSpeed;
            rollDirection = inputDir.normalized;
            moveState = MoveState.rolling;
            //TODO maybe set jump charges to 0
        }
    }

    void HandleRoll() {
        rollDirection += InCameraDirection(GetInputDirection()) * Time.deltaTime * rollInfluence;
        velocity = rollDirection.normalized * rollSpeed;
        rollSpeed = Mathf.Max(rollSpeed - Time.deltaTime * rollDecay, 0);
        if (rollSpeed <= rollStopSpeed) {
            moveState = MoveState.airborne;
        }
    }

    void CheckGlide() {
        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            if (moveState == MoveState.gliding) {
                moveState = MoveState.airborne;
            }
            else if (moveState == MoveState.airborne && glideDelay <= 0) {
                moveState = MoveState.gliding;
                velocity = GetGroundedVelocity().normalized * maxSpeed + Vector3.up * velocity.y;//TODO: instead of setting v to 0, force the player to actively glide for some duration
                glideDelay = maxGlideDelay;
            }
        }
        glideDelay = Mathf.Max(0, glideDelay - Time.deltaTime);
    }

    void HandleGlide() {
        if (moveState == MoveState.gliding) {
            Vector3 currentDir = GetGroundedVelocity().normalized;
            Vector3 targetDir = InCameraDirection(Vector3.forward);
            accelerationDirection = targetDir * Mathf.Clamp(1 - Vector3.Dot(currentDir, targetDir), 0, 1);
            Vector3 intermediate = Vector3.Slerp(currentDir, targetDir, Time.deltaTime * glideTurnSpeed);

            float verticalSpeed = Mathf.Clamp(velocity.y, -glideMinFallSpeed, glideMaxRiseSpeed);
            if (Input.GetKey(KeyCode.Space)) {
                if (glideCanStart) {
                    StartCoroutine(BurnGlide());
                }
                if (glideCanUp) {
                    verticalSpeed += glideUpAcc * Time.deltaTime;
                }
            }

            velocity = intermediate * maxSpeed + Vector3.up * verticalSpeed;
        }
    }

    IEnumerator BurnGlide() {
        glideCanStart = false;
        glideCanUp = true;
        while (glideStamina > 0) {
            glideStamina -= Time.deltaTime;
            yield return null;
        }
        glideCanUp = false;
        while (glideStamina < playerMain.maxGlideStamina) {
            glideStamina += playerMain.glideRechargeSpeed * Time.deltaTime;
            yield return null;
        }

        glideCanStart = true;

    }

    void AddGravity() {
        if (moveState != MoveState.gliding) {
            if (!Input.GetKeyDown(KeyCode.Space))
                velocity += Vector3.down * gravity * Time.deltaTime;
        }
        else
            velocity += Vector3.down * glideGravity * Time.deltaTime;

        // Set Maximum Fallspeed
        velocity = new Vector3(velocity.x, Mathf.Max(velocity.y, -maxFallSpeed), velocity.z);
    }

    void HandleLedgePhysics() {

        RaycastHit hit = new RaycastHit();
        Vector3 inputDir = InCameraDirection(GetInputDirection());
        if (moveState == MoveState.gliding)
            inputDir = velocity.normalized;
        bool goodMoveState = (moveState == MoveState.airborne || moveState == MoveState.gliding) && moveState != MoveState.onLedge;
        Vector3 ledgePoint = transform.position + transform.TransformDirection(inputDir * ledgeHorizontalScale + Vector3.up * ledgeVerticalScale);
        bool nothingAbove = !Physics.Raycast(transform.position, transform.TransformDirection(Vector3.up), out hit, .6f, raycastMask);
        if (goodMoveState && Physics.Raycast(ledgePoint, transform.TransformDirection(Vector3.down), out hit, ledgeDistanceCheck, raycastMask) && nothingAbove && ledgeReset <= 0) {
            //GetComponentInChildren<Renderer> ().material.color = Color.yellow;
            moveState = MoveState.onLedge;
            velocity = Vector3.zero;
            jumpCharges = playerMain.maxJumpCharges; //TODO ASFD1
            animController.SetFaceDirection(Mathf.Rad2Deg * Mathf.Atan2(-inputDir.z, inputDir.x)); //TODO Move to anim controller or maybe not
        }
        if (moveState == MoveState.onLedge) {
            accelerationDirection = Vector3.zero;
            velocity = Vector3.zero;
        }

        //Debug.DrawLine (ledgePoint, ledgePoint + Vector3.down * ledgeDistanceCheck);
        ledgeReset = Mathf.Max(ledgeReset - Time.deltaTime, 0);
    }

    void CheckUnhinge() {
        if (moveState == MoveState.onLedge) {
            if (Vector3.Dot(Vector3.back, GetInputDirection()) >= .75) {
                moveState = MoveState.airborne;
                ledgeReset = ledgeResetMax;
            }
        }
    }

    void OnCollisionStay(Collision c) {
        //TODO test if this actually works
        if (!enabled) return;

        if (c.gameObject.layer == 8) {
            foreach (ContactPoint contact in c.contacts) {
                //float dotProduct = Vector3.Dot((contact.point - transform.position).normalized, Vector3.down);
                float dotProduct = Vector3.Dot(transform.TransformDirection(Vector3.up), contact.normal);
                if (dotProduct >= dotProductForGrounded) {
                    //Debug.DrawLine(contact.point, transform.position);
                    SetAsGrounded();
                }
            }
        }
    }

    bool CheckAboveGround() {
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), aboveGroundCheck, raycastMask)) {
            return false;
        }
        return true;

    }

    void SetAsGrounded() {
        if (jumpResetRemaining <= 0) {
            if (moveState != MoveState.rolling)
                moveState = MoveState.grounded;
            jumpCharges = playerMain.maxJumpCharges;
        }
    }

    void CheckOutOfBounds() {
        if (LevelGen.S == null) {
            Debug.Log("Can't check out of bounds");
            return;
        }
        if (Vector3.Distance(transform.position, Vector3.zero) >= LevelGen.S.smoothRadius + 15) {
            SpawnPlayer();
            playerMain.health /= 2;
        }
    }

    Vector3 GetInputDirection() {
        return new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
    }

    public Vector3 GetGroundedVelocity() {
        return new Vector3(velocity.x, 0, velocity.z);
    }

    public Vector3 InCameraDirection(Vector3 inVec) {
        float rotTemp = (0 - cameraController.GetRotation().y) * Mathf.Deg2Rad;
        return new Vector3(inVec.x * Mathf.Cos(rotTemp) - inVec.z * Mathf.Sin(rotTemp), 0, inVec.z * Mathf.Cos(rotTemp) + inVec.x * Mathf.Sin(rotTemp));
    }

    public Vector3 OutOfCameraDirection(Vector3 inVec) {
        float rotTemp = (0 + cameraController.GetRotation().y) * Mathf.Deg2Rad;
        //rotTemp = 180 - rotTemp;
        return new Vector3(inVec.x * Mathf.Cos(rotTemp) - inVec.z * Mathf.Sin(rotTemp), 0, inVec.z * Mathf.Cos(rotTemp) + inVec.x * Mathf.Sin(rotTemp));
    }

    public Vector3 GetVelocity() {
        return velocity;
    }

    public Vector3 GetAccelerationDirection() {
        return accelerationDirection;
    }

    public MoveState GetMoveState() {
        return moveState;
    }

    public float GetFlapping() {
        if (Input.GetKey(KeyCode.Space))
            return 1.0f;
        return 0.0f;
    }

    public void OnTriggerEnter(Collider c) {
        if (!enabled) return;
        //if (c.gameObject.layer == 9) {
        //	playerParent.SetTarget(c.transform);
        //}
    }

    void OnTriggerExit(Collider c) {
        if (!enabled) return;
        //if (c.gameObject.layer == 9) {
        //	playerParent.SetTarget(null);
        //}
    }

    public void SpawnPlayer() {

        if (LevelGen.S.GetAllIslands().Count > 0) {
            int island = Random.Range(0, LevelGen.S.GetAllIslands().Count);
            transform.position = LevelGen.S.GetAllIslands()[island].GetLegitSpot(.3f, false);
        }
    }

    bool GetDazed() {
        return (dazeTime > 0);
    }

    float GetDazedMult() {
        if (!GetDazed())
            return 1;
        return dazeMovementConstant;
    }//TODO implement DAZE


    public void AddForce(Vector3 worldVector, float force, float movementDazeTime) {
        velocity += transform.InverseTransformDirection(worldVector.normalized) * force;
        dazeTime += movementDazeTime;
    }

    public void SetupRolls() {
        maxInvincRolls = playerMain.GetArmor().GetMaxRolls(playerMain.GetArmorInst().quality);
        invincRollRechargeTime = playerMain.GetArmor().GetRollRechargeTime(playerMain.GetArtifactInst().quality);

    }

    //void MoveToSandbox() {
    //	GameObject sandBoxGO = GameObject.Find("SandBox");
    //	if (sandBoxGO != null) {
    //		playerParent.movableParent.parent = sandBoxGO.transform;
    //		transform.position = sandBoxGO.transform.position;
    //	}
    //}
}
