using UnityEngine;
using System.Collections;

public class NonPhysicsPlayerTester : MonoBehaviour
{
	// movement config
	public float gravity = -25f;
	public float runSpeed = 8f;
	public float groundDamping = 20f; // how fast do we change direction? higher means faster
	public float inAirDamping = 5f;
	public float jumpHeight = 3f;
	[HideInInspector]
	private float
		normalizedHorizontalSpeed = 0;
	private CharacterController2D _controller;
	private Animator _animator;
	private RaycastHit2D _lastControllerColliderHit;
	private Vector3 _velocity;
	
	void Awake ()
	{
		_animator = GetComponent<Animator> ();
		_controller = GetComponent<CharacterController2D> ();
		
		// listen to some events for illustration purposes
		_controller.onControllerCollidedEvent += onControllerCollider;
		_controller.onTriggerEnterEvent += onTriggerEnterEvent;
		_controller.onTriggerExitEvent += onTriggerExitEvent;
	}
	
	
	#region Event Listeners
	
	void onControllerCollider (RaycastHit2D hit)
	{
		// bail out on plain old ground hits cause they arent very interesting
		if (hit.normal.y == 1f)
			return;
		
		// logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
		//Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
	}
	
	void onTriggerEnterEvent (Collider2D col)
	{
		Debug.Log ("onTriggerEnterEvent: " + col.gameObject.name);
	}
	
	void onTriggerExitEvent (Collider2D col)
	{
		Debug.Log ("onTriggerExitEvent: " + col.gameObject.name);
	}
	
	#endregion
	
	
	// the Update loop contains a very simple example of moving the character around and controlling the animation
	void Update ()
	{
		// grab our current _velocity to use as a base for all calculations
		_velocity = _controller.velocity;

		if (_controller.isGrounded)
		{
			_velocity.y = 0;
			_animator.SetBool("Jumping", false);
			_animator.SetBool("Falling", false);
		}
		
		float h = Input.GetAxis ("Horizontal");
		float v = Input.GetAxisRaw ("Vertical");
		
		if (Mathf.Abs (h) > 0)
		{
			if (h > 0)
			{
				if (transform.localScale.x < 0f)
					transform.localScale = new Vector3 (-transform.localScale.x, transform.localScale.y, transform.localScale.z);
			} else if (h < 0)
			{
				if (transform.localScale.x > 0f)
					transform.localScale = new Vector3 (-transform.localScale.x, transform.localScale.y, transform.localScale.z);
			}
			
			if (_controller.isGrounded)
				_animator.SetFloat ("Speed", Mathf.Abs (h));
		} else
		{
			if (_controller.isGrounded)
				_animator.SetFloat ("Speed", 0);
		}
		
		// we can only jump whilst grounded
		if (_controller.isGrounded && Input.GetKeyDown (KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
		{
			_velocity.y = Mathf.Sqrt (2f * jumpHeight * -gravity);
			_animator.SetBool ("Jumping", true);
		}



		
		if (!_controller.isGrounded && _velocity.y < 0)
		{
			_animator.SetBool("Jumping", false);
			_animator.SetBool("Falling", true);
		}

		if (_animator.GetBool ("Ducking"))
			h *= .5f;

		// apply horizontal speed smoothing it
		var smoothedMovementFactor = _controller.isGrounded ? groundDamping : inAirDamping; // how fast do we change direction?
		_velocity.x = Mathf.Lerp (_velocity.x, h * runSpeed, Time.deltaTime * smoothedMovementFactor);
		
		// apply gravity before moving
		_velocity.y += gravity * Time.deltaTime;
		
		_controller.move (_velocity * Time.deltaTime);

		// we can only duck while grounded
		if (_controller.isGrounded && v == -1)
		{
			
			_animator.SetBool ("Ducking", true);
			_controller.boxCollider.size = new Vector2(0.5f, 0.39f);
			_controller.boxCollider.center = new Vector2(0, 0.19f);
			_controller.recalculateDistanceBetweenRays();
			h *= .75f;
			//_controller.totalHorizontalRays = 5;
		} 
		else
		{
			if(_controller.canModifyCollisionSize(new Vector2(0.5f, 0.71f)))
			{
				_animator.SetBool ("Ducking", false);
				_controller.boxCollider.size = new Vector2(0.4f, 0.71f);
				_controller.boxCollider.center = new Vector2(0, 0.355f);
				_controller.recalculateDistanceBetweenRays();
				//_controller.totalHorizontalRays = 8;
			}
		}
	}
	
}
