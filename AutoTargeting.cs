using System.Collections.Generic;
using UnityEngine;

/*
 * This code is inspired from the free flowing combat from mix and jam. Video https://youtu.be/GFOpKcpKGKQ?si=grAcNnXIzx7lvewD 
 * 
 * This class is purely dedicated to autofocusing enemies within range and it handles all the targeting stuff 
 * 
 * Basically the idea is to Find the closest enemy based on the player's input with respect to  Where the player is pointing his mouse pointer 
 *
 * Enemies subscribes themselves when they become active and unsubscribes themselves when they become dead or inactive 
 *
 * Here, the view cone With Vector dot product is used to find the closest point where the player is pointing at based on the players input. 
 * 
 * If the enemy is targeted explicitly by the player, then we ignore self targeting 
 * 
 * When a explicit target is behind the player and player presses shoot button, then the explicit target should become null And the new target will be recalculated by auto targeting 
 */
public class AutoTargetingBase<T> : MonoBehaviour where T : MonoBehaviour
{

	[SerializeField] List<T> enemiesAlive = new();
	Vector3 inputDirection;

	Vector2 movementAxis;
	Camera cam;
	T currentTarget;


	void Start()
	{
		cam = Camera.main;
		currentTarget = GetTheEnemyInDirection(transform.forward);
	}
	void Update()
	{
		if (enemiesAlive.Count == 0) return;
		movementAxis.x = Input.GetAxis("Horizontal");
		movementAxis.y = Input.GetAxis("Vertical");
		Vector3 forward = cam.transform.forward;
		Vector3 right = cam.transform.right;
		forward.y = 0f;
		right.y = 0f;

		forward.Normalize();
		right.Normalize();

		if (movementAxis.magnitude > .2f)
		{
			inputDirection = forward * movementAxis.y + right * movementAxis.x;
		}
		else
		{
			if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
			{
				inputDirection = forward + right;
			}
		}
		inputDirection = inputDirection.normalized;
		//check for Explicit target
		currentTarget = GetTheEnemyInDirection(inputDirection);
	}
	public void Subscribe(T enemy)
	{
		enemiesAlive.Add(enemy);
	}
	public void Unsubscribe(T enemy)
	{
		if (currentTarget == enemy)
		{
			currentTarget = null;
		}

		enemiesAlive.Remove(enemy);
	}
	public T GetCurrentTarget()
	{
		return currentTarget;
	}
	public void SetCurrentTarget(T enemy)
	{
		currentTarget = enemy;
	}
	public void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		if (currentTarget == null) return;
		Gizmos.DrawSphere(currentTarget.transform.position, 0.5f);
		Gizmos.DrawRay(transform.position, inputDirection);
	}
	// Goes through All the enemies alive and gets the closest enemy and whose direction alligns with player
	T GetTheEnemyInDirection(Vector3 direction)
	{
		if (enemiesAlive.Count == 0) return null;
		int closestIndex = 0;
		float closestToDirection = float.MinValue;
		for (int i = 0; i < enemiesAlive.Count; i++)
		{
			T enemy = enemiesAlive[i];
			float ratioOfAllignmentWithDirection = Vector3.Dot(direction, GetDirection(enemy.transform).normalized);
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
	private Vector3 GetDirection(Transform target)
	{
		return target.position - transform.position;
	}
}

public class AutoTargeting : AutoTargetingBase<EnemyAI>
{

}
