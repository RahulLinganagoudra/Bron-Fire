using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

/*
 * This code is inspired from the free flowing combat from mix and jam. Video https://youtu.be/GFOpKcpKGKQ?si=grAcNnXIzx7lvewD 
 * 
 * This class is handles autofocusing enemies within range and it handles all the targeting stuff 
 * 
 * Basically the idea is to Find the closest enemy based on the player's input with respect to  Where the player is pointing his mouse pointer 
 *
 * Here, the view cone With Vector dot product is used to find the closest point where the player is pointing at based on the players input. 
 * 
 * If the enemy is targeted explicitly by the player, then we ignore self targeting 
 * 
 * When a explicit target is behind the player and player presses shoot button, then the explicit target should become null And the new target will be recalculated by auto targeting 
 */
public class AutoTargeting : MonoBehaviour
{


	private const float HardLockInputDelay = .5f;
	[SerializeField] protected virtual List<EnemyAI> enemiesAlive => EnemyManager.instance.activeEnemies;
	[SerializeField] private float autoFocusRadius = 10;

	protected Vector3 inputDirection;
	protected Vector2 movementAxis;
	protected Camera cam;
	protected EnemyAI explicitTarget;
	protected EnemyAI currentTarget;
	protected EnemyAI previousTarget;
	private float lastTimeHardTargetChecked;


	public Action<EnemyAI> OnTargetChanged;
	protected virtual void Start()
	{
		cam = Camera.main;
		currentTarget = GetTheEnemyInDirection(transform.forward);
	}
	private void OnEnable()
	{
		EnemyManager.instance.OnEnemyDead += (enemy) =>
		{
			if (enemiesAlive.Count == 0)
			{
				OnTargetChanged?.Invoke(null);
			}

			if (currentTarget == enemy)
			{
				currentTarget = null;
				explicitTarget = null;
			}
		};
	}
	protected virtual void Update()
	{
		//when player locks again the same hardlocked enemy, hardLock cancels
		if (Input.GetMouseButtonDown(2))
		{
			EnemyAI newHardTarget = GetTheEnemyInCameraDirection();
			if (explicitTarget == newHardTarget)
			{
				explicitTarget = null;
			}
			else
			{
				explicitTarget = newHardTarget;
			}

			ChangeTarget(explicitTarget);
			lastTimeHardTargetChecked = Time.time;
		}
		//allows to choose the right target by holding the mouse middle button
		//delay is added to prevent doing this task every frame which is not ideal for performance
		else if (Input.GetMouseButton(2) && Time.time - lastTimeHardTargetChecked > HardLockInputDelay)
		{
			EnemyAI newHardTarget = GetTheEnemyInCameraDirection();
			explicitTarget = newHardTarget;

			ChangeTarget(explicitTarget);
			lastTimeHardTargetChecked = Time.time;
		}


		if (enemiesAlive.Count == 0 || explicitTarget != null) return;
		
		
		movementAxis.x = Input.GetAxisRaw("Horizontal");
		movementAxis.y = Input.GetAxisRaw("Vertical");
		Vector3 forward = cam.transform.forward;
		Vector3 right = cam.transform.right;
		forward.y = 0f;
		right.y = 0f;

		forward.Normalize();
		right.Normalize();

		if (movementAxis.magnitude > .05f)
		{
			inputDirection = forward * movementAxis.y + right * movementAxis.x;
		}
		else
		{
			if (currentTarget == null)
			{
				inputDirection = forward + right;
			}
			else
			{
				return;
			}
		}
		inputDirection = inputDirection.normalized;
		var newTarget = GetTheEnemyInDirection(inputDirection);
		ChangeTarget(newTarget);
	}

	private void ChangeTarget(EnemyAI newTarget)
	{
		//TODO: Fighureout why i was checking  previous target check
		if (currentTarget != newTarget)//&& explicitTarget != previousTarget)
		{
			previousTarget = currentTarget;
			currentTarget = newTarget;
			OnTargetChanged?.Invoke(currentTarget);
		}
	}

	public EnemyAI GetCurrentTarget()
	{
		return currentTarget;
	}
	public void SetCurrentTarget(EnemyAI enemy)
	{
		explicitTarget = enemy;
		currentTarget = enemy;
	}
	//Here I have used the dot product to check the closest match to input direction (which depends on both mouse and keyboard input) and the direction to enemy
	//To closely approximate which enemy player is trying to engage with
	//I've used this method because its very cheap than raycasting and as we kill more enemies the active enemy pool gets lighter and the number of checks also become less
	//
	//Goes through All the enemies alive and gets the closest enemy and whose direction alligns with player
	EnemyAI GetTheEnemyInDirection(Vector3 direction)
	{
		if (enemiesAlive.Count == 0) return null;
		int closestIndex = 0;
		float closestToDirection = float.MinValue;
		for (int i = 0; i < enemiesAlive.Count; i++)
		{
			EnemyAI enemy = enemiesAlive[i];
			Vector3 directionToEnemy = GetDirectionTo(enemy.transform);
			if (directionToEnemy.sqrMagnitude > autoFocusRadius.SQ())
			{
				continue;
			}

			if (enemy.IsAttacking)
			{
				return enemy;
			}
			float ratioOfAllignmentWithDirection = Vector3.Dot(direction, directionToEnemy.normalized);
			if (ratioOfAllignmentWithDirection <= 0) continue;
			if (closestToDirection < ratioOfAllignmentWithDirection)
			{
				closestToDirection = ratioOfAllignmentWithDirection;
				closestIndex = i;
			}
		}
		return enemiesAlive[closestIndex];
	}
	public EnemyAI GetTheEnemyInCameraDirection()
	{
		if (enemiesAlive.Count == 0) return null;
		int closestIndex = 0;
		float closestToDirection = float.MinValue;
		for (int i = 0; i < enemiesAlive.Count; i++)
		{
			EnemyAI enemy = enemiesAlive[i];
			Vector3 directionToEnemy = GetDirectionTo(enemy.transform);
			if (directionToEnemy.sqrMagnitude > autoFocusRadius.SQ())
			{
				continue;
			}

			float ratioOfAllignmentWithDirection = Vector3.Dot(cam.transform.forward.XZ().normalized, directionToEnemy.normalized);
			if (ratioOfAllignmentWithDirection <= 0) continue;
			if (closestToDirection < ratioOfAllignmentWithDirection)
			{
				closestToDirection = ratioOfAllignmentWithDirection;
				closestIndex = i;
			}
		}
		return enemiesAlive[closestIndex];
	}


	// Gets the direction to the target from the current position	
	private Vector3 GetDirectionTo(Transform target)
	{
		return target.position - transform.position;
	}

	internal EnemyAI GetAttackingEnemy(float queryRadius)
	{
		if (enemiesAlive.Count == 0) return null;

		foreach (EnemyAI enemy in enemiesAlive)
		{
			if (GetDirectionTo(enemy.transform).sqrMagnitude >= queryRadius.SQ())
			{
				continue;
			}
			if (enemy.IsAttacking)
			{
				return enemy;
			}
		}
		return null;
	}
	private void OnDrawGizmos()
	{
		if (currentTarget == null) return;
		if (explicitTarget == null)
		{
			Gizmos.color = Color.blue;
		}
		else
		{
			Gizmos.color = Color.red;
		}

		Gizmos.DrawSphere(currentTarget.transform.position + Vector3.up * .8f, .5f);
		Gizmos.DrawRay(transform.position + Vector3.up, inputDirection);
	}
}