using System.Collections;
using UnityEngine;

//This script requires you to have setup your animator with 3 parameters, "InputMagnitude", "InputX", "InputZ"
//With a blend tree to control the inputmagnitude and allow blending between animations.
[RequireComponent(typeof(CharacterController))]
public class MovementInput : MonoBehaviour
{

	public float Velocity;
	public float TiredVelocity;
	[Space]

	public float InputX;
	public float InputZ;
	public Vector3 desiredMoveDirection;
	public bool blockRotationPlayer;
	public float desiredRotationSpeed = 0.1f;
	public Animator anim;
	public float Speed;
	public float allowPlayerRotation = 0.1f;
	public Camera cam;
	public CharacterController controller;
	public PlayerHealthAndStamina health;
	public bool isGrounded;
	[SerializeField]
	AudioClip[] FootstepAudioClips;
	[Header("Animation Smoothing")]
	[Range(0, 1f)]
	public float HorizontalAnimSmoothTime = 0.2f;
	[Range(0, 1f)]
	public float VerticalAnimTime = 0.2f;
	[Range(0, 1f)]
	public float StartAnimTime = 0.3f;
	[Range(0, 1f)]
	public float StopAnimTime = 0.15f;

	public float verticalVel;
	private Vector3 moveVector;

	public float FootstepAudioVolume = 1;

	public bool CanDash { get; internal set; }

	// Use this for initialization
	void Start()
	{
		anim = this.GetComponent<Animator>();
		cam = Camera.main;
		controller = this.GetComponent<CharacterController>();
		health = this.GetComponent<PlayerHealthAndStamina>();

		Cursor.lockState = CursorLockMode.Locked;
	}

	// Update is called once per frame
	void Update()
	{
		InputMagnitude();

		isGrounded = controller.isGrounded;
		if (isGrounded)
		{
			verticalVel -= 0;
		}
		else
		{
			verticalVel -= 1;
		}
		moveVector = new Vector3(0, verticalVel * .2f * Time.deltaTime, 0);
		if (controller.enabled)
		{
			controller.Move(moveVector);
		}
	}

	void PlayerMoveAndRotation()
	{
		InputX = Input.GetAxis("Horizontal");
		InputZ = Input.GetAxis("Vertical");

		var camera = Camera.main;
		var forward = cam.transform.forward;
		var right = cam.transform.right;

		forward.y = 0f;
		right.y = 0f;

		forward.Normalize();
		right.Normalize();

		desiredMoveDirection = forward * InputZ + right * InputX;

		desiredMoveDirection.Normalize();
		if (blockRotationPlayer == false)
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), desiredRotationSpeed);
			float Velocity = health.IsOnLowStamina ? TiredVelocity : this.Velocity;
			if(controller.enabled)
			{
				controller.Move(desiredMoveDirection * Time.deltaTime * Velocity);
			}
		}
	}

	public void LookAt(Vector3 pos)
	{
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(pos), desiredRotationSpeed);
	}

	public void RotateToCamera(Transform t)
	{

		var camera = Camera.main;
		var forward = cam.transform.forward;
		var right = cam.transform.right;

		desiredMoveDirection = forward;

		t.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), desiredRotationSpeed);
	}

	void InputMagnitude()
	{
		//Calculate Input Vectors
		InputX = Input.GetAxis("Horizontal");
		InputZ = Input.GetAxis("Vertical");

		//anim.SetFloat ("InputZ", InputZ, VerticalAnimTime, Time.deltaTime * 2f);
		//anim.SetFloat ("InputX", InputX, HorizontalAnimSmoothTime, Time.deltaTime * 2f);

		//Calculate the Input Magnitude
		Speed = new Vector2(InputX, InputZ).normalized.sqrMagnitude;

		//Physically move player
		if (Speed > allowPlayerRotation)
		{
			anim.SetFloat("Blend", health.IsOnLowStamina ? 0.5f * Speed : Speed, StartAnimTime, Time.deltaTime);
			PlayerMoveAndRotation();
		}
		else if (Speed < allowPlayerRotation)
		{
			anim.SetFloat("Blend", health.IsOnLowStamina ? 0.5f * Speed : Speed, StopAnimTime, Time.deltaTime);
		}
	}
	public void BlockPlayerMovementFor(float seconds)
	{
		StartCoroutine(BlockPlayerMovement(seconds));
	}
	IEnumerator BlockPlayerMovement(float seconds)
	{
		controller.enabled = false;
		yield return new WaitForSeconds(seconds);
		controller.enabled = true;
	}
	//Animation Events
	private void OnFootstep(AnimationEvent animationEvent)
	{
		if (animationEvent.animatorClipInfo.weight > 0.5f)
		{
			if (FootstepAudioClips.Length > 0)
			{
				var index = Random.Range(0, FootstepAudioClips.Length);
				AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(controller.center), FootstepAudioVolume);
			}
		}
	}
}
