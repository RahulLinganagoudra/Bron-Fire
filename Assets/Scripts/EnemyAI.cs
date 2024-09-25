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

	public bool IsAttacking
	{
		get;
		private set;
	}

	// Start is called before the first frame update
	void Start()
	{
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
		animator.SetTrigger("dead");
		OnDead?.Invoke(this);
	}
	public bool IsAttackable()
	{
		return true;
	}
	public void DealDamage()
	{
		hitPoints--;
		currentState = EnemyStates.Attacking;
		animator.ResetTrigger("NormalPunch1");
		animator.SetTrigger("Hit1");
		if (hitPoints <= 0)
		{
			Dead();
		}
	}
	public void DealDamage(int ammount)
	{
		hitPoints = Mathf.Clamp(hitPoints - ammount, 0, MaxHitPoints);
		currentState = EnemyStates.Attacking;
		animator.ResetTrigger("NormalPunch1");
		animator.SetTrigger("Hit1");
		if (hitPoints <= 0)
		{
			Dead();
		}
	}
	public void SetCurrentEnemyState(EnemyStates enemyState)
	{
		strafeRadius = Random.Range(MinSafeDistance, MaxSafeDistance);
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
		if (transform.position.CompareDist(playerTransform.position, MinSafeDistance))
		{
			SetCurrentEnemyState(EnemyStates.Retreating);
		}
	}
	void Retreat()
	{
		if (!(transform.position - playerTransform.position).IsMagnitudeBetween(3, 7))
		{
			Vector3 clampedDirection = (transform.position - playerTransform.position).normalized * Random.Range(MinSafeDistance, MaxSafeDistance);
			Vector3 directionToMove = playerTransform.position + clampedDirection - transform.position;
			MoveTo(directionToMove.XZ());
		}
		else
		{
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
		Vector3 dirToPlayer = playerTransform.position - transform.position;
		var rightPerpendicular = Vector3.Cross(dirToPlayer.normalized, Vector3.up);

		Vector3 dirToRPerpendicular = transform.position + (rightPerpendicular * lastStrafeDirection) - playerTransform.position;

		var dirToMove = playerTransform.position + (dirToRPerpendicular.normalized * strafeRadius) - transform.position;

		//TODO: try converting this to a loop so that you can check if the ray collides on both side if so then stop strafing
		Ray ray = new Ray(transform.position + Vector3.up, dirToMove.normalized);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 2f) &&
			hit.transform != transform && !hit.transform.TryGetComponent(out EnemyArea area))// && hit.transform.gameObject.layer != LayerMask.GetMask("AIUnwalkable"))
		{
			lastStrafeDirection = -lastStrafeDirection;
			dirToRPerpendicular = transform.position + (rightPerpendicular * lastStrafeDirection) - playerTransform.position;

			dirToMove = playerTransform.position + (dirToRPerpendicular.normalized * strafeRadius) - transform.position;
		}
		MoveTo(dirToMove.XZ());
	}
	private void Attack()
	{
		if (transform.position.CompareDist(playerTransform.position, AttackRadius))
		{
			if (Time.time - lastTimeAttacked > attackDelay)
			{
				animator.SetTrigger("NormalPunch1");
				IsAttacking = false;
				lastTimeAttacked = Time.time;
			}
		}
		else
		{
			Vector3 clampedDirection = (transform.position - playerTransform.position).normalized * (AttackRadius * .90f - StoppingDistance);
			Vector3 directionToMove = playerTransform.position + clampedDirection - transform.position;
			MoveTo(directionToMove.XZ(), strafeSpeed * 3);
		}

	}
	void MoveTo(Vector3 direction, float speed = -1)
	{
		if (direction.sqrMagnitude < StoppingDistance.SQ())
		{
			return;
		}
		characterController.Move(direction.normalized * (speed == -1 ? strafeSpeed : speed) * Time.deltaTime);
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
		//TODO: player damage is not registering may be its the issue of player health or the following code is messed up
		foreach (var collider in Physics.OverlapSphere(transform.position, AttackRadius + ErrorMargin))
		{
			if (collider.TryGetComponent(out PlayerHealthAndStamina playerHealth))
			{
				//TODO: Dont contact player health Directly from enemy

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

