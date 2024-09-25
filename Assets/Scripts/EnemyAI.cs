using UnityEngine;
using Utilities;


public enum EnemyStates
{
	Idle,
	Strafing,
	Attacking,
	Retreating
}

[RequireComponent(typeof(CharacterController))]
public class EnemyAI : MonoBehaviour
{
	private const int MinSafeDistance = 3;
	private const int MaxSafeDistance = 7;
	private const float StoppingDistance = 0.4f;
	private const float ErrorMargin = .25f;
	public bool DisableMovement = false;

	private const string HorizontalBlendAnimatorParam = "HorizontalBlend";
	private const string BlendAnimatorParam = "Blend";
	private const string HitAnimatorParam = "Hit1";
	private const string PunchAnimatorParam = "NormalPunch1";
	private const string DeadAnimatorParam = "dead";
	[SerializeField] private float strafeSpeed = 5;
	[SerializeField] private int MaxHitPoints = 5;
	[SerializeField] private float AttackRadius = 1.2f;
	[SerializeField] private int damageAmmount;
	[SerializeField] private ParticleSystem InformAttackFX;
	[SerializeField] private AudioClip hitSFX;
	private int hitPoints;
	public static System.Action<EnemyAI> OnDead;
	private int lastStrafeDirection = 1;
	Transform playerTransform;
	CharacterController characterController;
	EnemyStates currentState;
	private Animator animator;
	private int strafeRadius;
	private int attackDelay = 2;
	private float lastTimeAttacked = float.MinValue;
	private float lastGotHit;
	private DamageInfo damageInfo;
	private float engageSpeed;

	public bool IsAttacking
	{
		get;
		private set;
	}
	public bool IsOnLowHealth => hitPoints < MaxHitPoints * .3f;

	// Start is called before the first frame update
	void Start()
	{
		engageSpeed = strafeSpeed * 3;
		damageInfo = new DamageInfo()
		{
			origin = gameObject,
			damageAmmount = damageAmmount
		};
		hitPoints = MaxHitPoints;
		animator = GetComponentInChildren<Animator>();
		playerTransform = GameObject.FindWithTag("Player").transform;
		characterController = GetComponent<CharacterController>();
	}

	// Update is called once per frame
	void Update()
	{
		if (DisableMovement) return;
		switch (currentState)
		{
			case EnemyStates.Idle:
				Idle();
				break;
			case EnemyStates.Strafing:
				Strafe(strafeRadius);
				break;
			case EnemyStates.Attacking:
				Attack();
				break;
			case EnemyStates.Retreating:
				Retreat();
				break;
			default:
				break;
		}
		transform.forward = (playerTransform.position - transform.position).XZ().normalized;
	}
	public void AwakenEnemy()
	{
		EnemyManager.instance.Subscribe(this);
	}
	void Dead()
	{
		EnemyManager.instance?.Unsubscribe(this);
		characterController.enabled = false;
		enabled = false;
		animator.SetTrigger(DeadAnimatorParam);
		OnDead?.Invoke(this);
	}
	public bool IsAttackable()
	{
		return true;
	}

	public void DealDamage(DamageInfo damageInfo)
	{
		hitPoints = Mathf.Clamp(hitPoints - (int)damageInfo.damageAmmount, 0, MaxHitPoints);

		SetCurrentEnemyState(EnemyStates.Attacking);
		animator.ResetTrigger(PunchAnimatorParam);
		animator.SetTrigger(HitAnimatorParam);
		if (IsOnLowHealth)
		{
			SetCurrentEnemyState(EnemyStates.Retreating);
		}
		else if (hitPoints <= 0)
		{
			Dead();
		}
	}
	public void SetCurrentEnemyState(EnemyStates enemyState)
	{
		strafeRadius = Random.Range(MinSafeDistance, MaxSafeDistance);
		lastStrafeDirection = Random.Range(0, 2) == 1 ? 1 : -1;
		ResetMovementAnimations();
		if (enemyState == EnemyStates.Attacking)
		{
			IsAttacking = true;
			InformAttackFX.Play();
		}
		else
		{
			IsAttacking = false;
		}
		currentState = enemyState;
	}
	void Idle()
	{
		if (transform.position.CompareDist(playerTransform.position, strafeRadius))
		{
			SetCurrentEnemyState(EnemyStates.Retreating);
		}
		ResetMovementAnimations();
	}
	void Retreat()
	{
		if (transform.position.CompareDist(playerTransform.position, strafeRadius))
		{
			Vector3 clampedDirection = (transform.position - playerTransform.position).normalized * strafeRadius;
			Vector3 directionToMove = playerTransform.position + clampedDirection - transform.position;
			MoveTo(directionToMove.XZ());
		}
		else
		{
			ResetMovementAnimations();
			currentState = EnemyStates.Idle;
		}
	}

	/*
	 
		if we choose to move in the perependicular direction from the direction to player, it is possible we dont strafe in the strafe radius and go past it ( trace a spiral path )
		instead we grab the right perpendicular point get direction to that right perpendicular, normalize it to strafe radius.
		now we move to that normalized point

	 */
	void Strafe(float strafeRadius)
	{
		const int MaxDirections = 2;
		for (int i = 0; i < MaxDirections; i++)
		{
			Vector3 dirToPlayer = playerTransform.position - transform.position;
			var rightPerpendicular = Vector3.Cross(dirToPlayer.normalized, Vector3.up);

			Vector3 dirToRPerpendicular = transform.position + (rightPerpendicular * lastStrafeDirection) - playerTransform.position;

			var dirToMove = playerTransform.position + (dirToRPerpendicular.normalized * strafeRadius) - transform.position;

			Ray ray = new Ray(transform.position + Vector3.up, dirToMove.normalized);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, 2f) &&
				hit.transform != transform
				&& hit.transform.gameObject.layer != LayerMask.GetMask("IgnoreCameraCollision"))
			{
				lastStrafeDirection = -lastStrafeDirection;
				if (i >= MaxDirections - 1)
				{
					SetCurrentEnemyState(EnemyStates.Idle);
				}
				continue;
			}

			MoveTo(dirToMove.XZ());
		}
	}
	private void Attack()
	{
		if (transform.position.CompareDist(playerTransform.position, AttackRadius))
		{
			if (Time.time - lastTimeAttacked > attackDelay)
			{
				animator.SetTrigger(PunchAnimatorParam);
				IsAttacking = false;
				lastTimeAttacked = Time.time;
				ResetMovementAnimations();
			}
		}
		else
		{
			Vector3 clampedDirection = (transform.position - playerTransform.position).normalized * (AttackRadius * .90f - StoppingDistance);
			Vector3 directionToMove = playerTransform.position + clampedDirection - transform.position;
			MoveTo(directionToMove.XZ(), engageSpeed);
		}

	}
	void MoveTo(Vector3 direction, float? speed = null)
	{
		if (direction.sqrMagnitude < StoppingDistance.SQ())
		{
			ResetMovementAnimations();
			return;
		}
		characterController.Move(direction.normalized * (speed == null ? strafeSpeed : speed.Value) * Time.deltaTime);

		if (animator != null)
		{
			Vector3 velocity = transform.InverseTransformDirection(characterController.velocity);
			velocity /= engageSpeed;
			animator.SetFloat(BlendAnimatorParam, velocity.z);
			animator.SetFloat(HorizontalBlendAnimatorParam, velocity.x);
		}
	}

	public void ResetMovementAnimations()
	{
		if (animator != null && (animator.GetFloat(HorizontalBlendAnimatorParam) != 0 || animator.GetFloat(BlendAnimatorParam) != 0))
		{
			animator.SetFloat(BlendAnimatorParam, 0);
			animator.SetFloat(HorizontalBlendAnimatorParam, 0);
		}
	}
	private void OnDrawGizmosSelected()
	{
		Vector3 dirToPlayer = playerTransform.position - transform.position;
		float strafeRadius = 4;
		var rightPerpendicular = Vector3.Cross(dirToPlayer.normalized, Vector3.up);

		Vector3 rightPoint = transform.position + (rightPerpendicular * lastStrafeDirection);
		Vector3 dirToRPerpendicular = rightPoint - playerTransform.position;

		Vector3 desiredPoint = playerTransform.position + (dirToRPerpendicular.normalized * strafeRadius);
		var dirToMove = desiredPoint - transform.position;

		Gizmos.DrawSphere(desiredPoint, .2f);
		Gizmos.DrawSphere(rightPoint, .2f);
		Gizmos.DrawRay(transform.position, dirToMove.normalized);

		Gizmos.DrawWireSphere(transform.position, AttackRadius + ErrorMargin);
	}

	//Animation Events
	void OnPunch1()
	{
		foreach (var collider in Physics.OverlapSphere(transform.position, AttackRadius + ErrorMargin))
		{
			if (collider.TryGetComponent(out CombatScript playerHealth))
			{
				playerHealth.DealDamage(damageInfo);
				AudioSource.PlayClipAtPoint(hitSFX, transform.position);
			}
		}
		currentState = EnemyStates.Retreating;
	}
	//dummy methods as they are animation events set for player animator they should be included 
	void EnableTrail() { }
	void DisableTrail() { }


}

