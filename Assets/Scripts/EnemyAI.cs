using UnityEngine;
using Utilities;


public enum EnemyStates
{
	Idle,
	Strafing,
	Attacking,
	Retreating,
	Dead
}

[RequireComponent(typeof(CharacterController))]
public class EnemyAI : MonoBehaviour
{
	private const float AttackRadius = 1.5f;
	private const int MinSafeDistance = 3;
	private const int MaxSafeDistance = 7;
	[SerializeField] private float strafeSpeed = 5;
	[SerializeField] private int MaxHitPoints = 5;
	[SerializeField] ParticleSystem InformAttackFX;
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

	public bool IsAttacking
	{
		get;
		private set;
	}

	// Start is called before the first frame update
	void Start()
	{
		hitPoints = MaxHitPoints;
		animator = GetComponentInChildren<Animator>();
		playerTransform = GameObject.FindWithTag("Player").transform;
		characterController = GetComponent<CharacterController>();
		AwakenEnemy();
	}

	// Update is called once per frame
	void Update()
	{
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
			case EnemyStates.Dead:
				break;
			default:
				break;
		}
		transform.forward = (playerTransform.position - transform.position).XZ().normalized;
	}
	void AwakenEnemy()
	{
		EnemyManager.instance.Subscribe(this);
	}
	void Dead()
	{
		animator.SetTrigger("dead");
		EnemyManager.instance?.Unsubscribe(this);
		//Disable this component
		enabled = false;
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
		
	}
	void Retreat()
	{
		if (!(transform.position - playerTransform.position).IsBetween(3, 7))
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
	void Strafe(float strafeRadius)
	{
		Vector3 dirToPlayer = playerTransform.position - transform.position;
		var rightPerpendicular = Vector3.Cross(dirToPlayer.normalized, Vector3.up);

		Vector3 dirToRPerpendicular = transform.position + (rightPerpendicular * lastStrafeDirection) - playerTransform.position;

		var dirToMove = playerTransform.position + (dirToRPerpendicular.normalized * strafeRadius) - transform.position;
		Ray ray = new Ray(transform.position + Vector3.up, dirToMove.normalized);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit) &&
			hit.transform != transform)// && hit.transform.gameObject.layer != LayerMask.GetMask("AIUnwalkable"))
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
				currentState = EnemyStates.Retreating;
			}
		}
		else
		{
			Vector3 clampedDirection = (transform.position - playerTransform.position).normalized * AttackRadius;
			Vector3 directionToMove = playerTransform.position + clampedDirection - transform.position;
			MoveTo(directionToMove.XZ(),strafeSpeed*3);
		}

	}
	void MoveTo(Vector3 deltaPos, float speed = -1)
	{
		if (deltaPos.sqrMagnitude <= 0.4f.SQ())
		{
			return;
		}
		characterController.Move(deltaPos.normalized * (speed == -1 ? strafeSpeed : speed) * Time.deltaTime);
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
	}

	//Animation Events
	void OnPunch1()
	{
		print("playergot hit");
	}
	//dummy methods as they are animation events set for player animator they should be included 
	void EnableTrail() { }
	void DisableTrail() { }


}

