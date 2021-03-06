using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

[RequireComponent (typeof (Controller))]
public class Player : MonoBehaviour {

	public Vector2 spawn;
	
	public bool canMove = true;
	public float maxJumpHeight = 4;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;
	public float moveSpeed = 6;
	float accelerationTimeAirborne = .2f;
	float accelerationTimeGrounded = .1f;

	public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallLeap;

	public float wallSlideSpeedMax = 3;
	public float wallStickTime = .25f;
	float timeToWallUnstick;

	float gravity;
	float maxJumpVelocity;
	float minJumpVelocity;
	Vector3 velocity;
	float velocityXSmoothing;
	bool oldCanMove;
	float minY;

	Controller controller;
	Animator animator;
	bool wallSliding = false;

	void Start() {
		controller = GetComponent<Controller> ();
		animator = GetComponent<Animator> ();

		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);

		transform.position = spawn;

		minY = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<CameraFollow> ().minYPosition;
	}

	void Update() {
		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		int wallDirX = (controller.collisions.left) ? -1 : 1;

		float targetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborne);

		if(controller.collisions.triggerHit)
			CheckForTrigger ();
		if (((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)) {
			wallSliding = true;

			if (velocity.y < -wallSlideSpeedMax) {
				velocity.y = -wallSlideSpeedMax;
			}

			if (timeToWallUnstick > 0) {
				velocityXSmoothing = 0;
				velocity.x = 0;

				if (input.x != wallDirX && input.x != 0) {
					timeToWallUnstick -= Time.deltaTime;
				} else {
					timeToWallUnstick = wallStickTime;
				}
			} else {
				timeToWallUnstick = wallStickTime;
			}

		}  else
			wallSliding = false;

		if (((controller.collisions.left || controller.collisions.right) && controller.collisions.below && velocity.y == 0) && (controller.collisions.hit)){
			if (controller.collisions.hit.collider.tag == "Stone" && velocity.y == 0) {
				controller.collisions.hit.collider.transform.gameObject.GetComponent<Stone> ().Move (controller.collisions.faceDir);
			}
		}

		if (Input.GetButtonDown ("Jump")) {
			if (wallSliding) {
				if (wallDirX == input.x) {
					velocity.x = -wallDirX * wallJumpClimb.x;
					velocity.y = wallJumpClimb.y;
				} else if (input.x == 0) {
					velocity.x = -wallDirX * wallJumpOff.x;
					velocity.y = wallJumpOff.y;
				} else {
					velocity.x = -wallDirX * wallLeap.x;
					velocity.y = wallLeap.y;
				}
			}

			if (controller.collisions.below) {
				velocity.y = maxJumpVelocity;
				GetComponent<AudioSource> ().Play ();
			}
		}
		if (Input.GetButtonUp ("Jump")) {
			if (velocity.y > minJumpVelocity) {
				velocity.y = minJumpVelocity;
			}
		}
			
		velocity.y += gravity * Time.deltaTime;
		if (oldCanMove != canMove)
			velocity.y = 0;
		if(canMove)
			controller.Move (velocity * Time.deltaTime, input);

		oldCanMove = canMove;

		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}

		if (transform.position.y + controller.boxCollider.bounds.extents.y < minY)
			Restart ();

		CheckAnimation ();
	}

	public void Restart(){
		transform.position = spawn;
	}

	void OnDrawGizmos(){
		Gizmos.color = new Color (0, 0, 1, .5f);
		Gizmos.DrawSphere (spawn, .2f);
	}

	void CheckAnimation(){
		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));

		if (input.x != 0 && !wallSliding && !Input.GetMouseButtonDown(0) && controller.collisions.below)
			GetComponent<Animator> ().SetBool ("Running", true);
		else 
			GetComponent<Animator> ().SetBool ("Running", false);
		if (input.x == 0 && velocity.y == 0 && !wallSliding && !Input.GetMouseButtonDown(0))
			GetComponent<Animator> ().SetBool ("Idle", true);
		else 
			GetComponent<Animator> ().SetBool ("Idle", false);
		if (wallSliding)
			GetComponent<Animator> ().SetBool ("WallSlide", true);
		else 
			GetComponent<Animator> ().SetBool ("WallSlide", false);
		if (!wallSliding && !controller.collisions.below)
			GetComponent<Animator> ().SetBool ("Jump", true);
		else 
			GetComponent<Animator> ().SetBool ("Jump", false);
		if (Input.GetMouseButtonDown(0) && controller.collisions.below)
			GetComponent<Animator> ().SetBool ("Shot", true);
		else 
			GetComponent<Animator> ().SetBool ("Shot", false);
	}

	void CheckForTrigger ()
	{
		Debug.Log ("fuck");
		if (controller.collisions.triggerHit.collider.tag == "Thorn") {
			controller.collisions.triggerHit.collider.gameObject.GetComponent<Thorn> ().Die ();
		}
		if (controller.collisions.triggerHit.collider.tag == "LifeUp") {
			controller.collisions.triggerHit.collider.gameObject.GetComponent<LifeUp> ().Up ();
		}
		if (controller.collisions.triggerHit.collider.tag == "Fairy") {
			controller.collisions.triggerHit.collider.gameObject.GetComponent<Fairy> ().Collect ();
		}
		if (controller.collisions.triggerHit.collider.tag == "LevelChange") {

			controller.collisions.triggerHit.collider.gameObject.GetComponent<LevelChange> ().Change ();
		}


	}
}