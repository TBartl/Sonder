using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerCameraController : NetworkBehaviour {
	//TODO: Hide mouse from elsewhere
	//TODO: Allow users to change their zoom/rotate speed/sharpness.
	//TODO: Bob up and down along the y-axis
	//TODO: Adjust camera curves/values to make the feel smoother.
	//TODO: Only stop movement back if it hits a wall out (see cube world)
	//TODO: Investigate Sensitivity weirdness (check on built version)


	Transform pivotPoint;
	Transform expectedHolder;

	Camera mainCamera;
	Transform mainCameraTransform;

	LayerMask raycastMask;
	
	public float startHeightAbovePlayer;
	public float breathingRatio;
	//PlayerMovementController movementController; TODO

	public float initialZoom;
	public float minimumZoom;
	public float maximumZoom;

	public float zoomSensitivity;
	public AnimationCurve zoomVsRemainingZoom;	
	public float zoomXAxis; 
	public float zoomYAxis; // Increase to make quicker adjustments
	float zoomChange;
	float zoom;

	public float rotateSensitivity;
	public AnimationCurve rotateVsRemainingRotate;
	public float rotateXAxis;
	public float rotateZAxis; // Increase to make quicker adjustments
	Vector3 rotateChange;
	Vector3 rotate;
	Vector3 rotation = Vector3.zero;


	// Use this for initialization
	void Start () {
        if (!isLocalPlayer) {
            this.enabled = false;
            return;
        }

		// Create a pivot point that will be above the player's head.
		GameObject temp = new GameObject ("Camera Pivot");
		pivotPoint = temp.GetComponent<Transform> ();
		pivotPoint.parent = transform;
		pivotPoint.localPosition = new Vector3 (0, startHeightAbovePlayer, 0);

		// Set the initial value of the zoom in on the player.
		zoom = initialZoom;

		// Create a point that extends from the pivot point where the camera will be expected.
		temp = new GameObject ("Target Camera Holder");
		expectedHolder = temp.GetComponent<Transform> ();
		expectedHolder.parent = pivotPoint;
		expectedHolder.localPosition = new Vector3 (0, 0, -zoom);

		// Locate the camera
		mainCamera = Camera.main;
		mainCameraTransform = mainCamera.GetComponent<Transform> ();
		mainCameraTransform.parent = pivotPoint;
		mainCameraTransform.localPosition = new Vector3 (0, 0, -zoom);

		// MISC
		raycastMask = 1 << 8;
		Cursor.visible = false; //TODO
		//movementController = GetComponent<PlayerMovementController> (); TODO
	}

	void Update() {
		//DEBUG
		if (Input.GetKeyDown (KeyCode.M)) {
			rotateSensitivity += 50;
		}
		if (Input.GetKeyDown (KeyCode.N)) {
			rotateSensitivity -= 50;
		}

	}
	
	// Update is called once per frame
	void FixedUpdate () {

		GetInput ();
		SmoothlyMove ();
		MoveCamera ();
	}

	void GetInput()
	{
		zoomChange += Input.GetAxis ("Mouse ScrollWheel")*zoomSensitivity*Time.deltaTime;
		rotateChange += new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0)*rotateSensitivity;
	}

	void SmoothlyMove()
	{
		// Handle the zooming.
		float thisZoom = zoomVsRemainingZoom.Evaluate( Mathf.Abs(zoomChange)/zoomXAxis )*zoomYAxis;
		thisZoom *= Mathf.Sign(zoomChange)*Time.deltaTime;
		zoomChange -= thisZoom;
		zoom = Mathf.Clamp(zoom - thisZoom, minimumZoom, maximumZoom);
		expectedHolder.localPosition = new Vector3(0,0, -zoom);

		//Handle the rotating.
		for (int index = 0; index < 10; index += 1) {//ITERATED TO MAKE SMOOTH ON LOWER SPEED COMPS, TODO adjust
			Vector3 thisRotate = new Vector3 (rotateVsRemainingRotate.Evaluate (Mathf.Abs (rotateChange.x) / rotateXAxis),
		                                  rotateVsRemainingRotate.Evaluate (Mathf.Abs (rotateChange.y) / rotateXAxis), 0) * rotateZAxis;
			thisRotate = thisRotate/10;
			thisRotate = new Vector3 (thisRotate.x * Mathf.Sign (rotateChange.x), thisRotate.y * Mathf.Sign (rotateChange.y), 0) * Time.deltaTime;
			rotateChange -= thisRotate;
			rotation = new Vector3(Mathf.Min(Mathf.Max (-90, rotation.x + thisRotate.x), 90), rotation.y + thisRotate.y, 0);
		}
		pivotPoint.localRotation = Quaternion.Euler (rotation);

		// Allow the camera to breathe along the y axis.
		//pivotPoint.localPosition = new Vector3 (0, startHeightAbovePlayer -breathingRatio * movementController.GetVelocity().y, 0); TODO
		pivotPoint.localPosition = new Vector3 (0, startHeightAbovePlayer , 0);
	}

	// Move the camera to the correct position.
	void MoveCamera()
	{
		RaycastHit ray = new RaycastHit ();
		Physics.Linecast (pivotPoint.position, expectedHolder.position, out ray, raycastMask);
		if (ray.distance == 0)
			ray.distance = zoom;
		mainCameraTransform.localPosition = new Vector3 (0, 0, -ray.distance + .10f);
	}


	public Vector3 GetRotation()
	{
		return rotation;
	}

	public Vector3 GetDirectionRaw() {
		Vector3 direction = pivotPoint.position - expectedHolder.position;
		direction = direction.normalized;
		return direction;
	}

	public Vector3 GetDirectionAdjustedDirty() {
		Vector3 direction = GetDirectionRaw() + pivotPoint.TransformDirection(Vector3.up) * .175f;
		direction = direction.normalized;
		return direction;
	}

	public Quaternion GetFacingRotation() {
		Vector3 direction = GetDirectionRaw ();
		Quaternion rotation = Quaternion.LookRotation (direction, transform.up);
		return rotation;
	}

	public Vector3 GetDirectionOnMovementPlane() {
		Vector3 direction = pivotPoint.position - expectedHolder.position;
		direction = Vector3.ProjectOnPlane(direction, transform.up);
		direction = direction.normalized;
		return direction;
	}

	public Quaternion GetFacingRotationOnMovementPlane() {
		Vector3 direction = GetDirectionOnMovementPlane ();
		Quaternion rotation = Quaternion.LookRotation (direction, transform.up);
		return rotation;
	}

	public Vector3 GetPivotPointPos() {
		return pivotPoint.position;
	}
	

}
