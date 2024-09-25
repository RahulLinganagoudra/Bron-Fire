using UnityEngine;
using DG.Tweening;
using System.Collections;
using Utilities;
using Unity.VisualScripting;

public class AssassinsFocus : MonoBehaviour
{
	private const float StoppingDistance = 1f;
	[SerializeField] AssasinsFocusUI assassinsFocusUI;
	private AutoTargeting enemyDetection;
	private CombatScript combatScript;
	private Animator animator;
	private EnemyAI currentEnemy;
	private System.Collections.Generic.Queue<EnemyAI> focusedEnemies = new();


	private bool IsUsingFocus { get; set; } = false;
	private bool IsPerformingFocusAttack { get; set; }

	private void Start()
	{
		enemyDetection = GetComponent<AutoTargeting>();
		combatScript = GetComponent<CombatScript>();
		animator = GetComponent<Animator>();
	}
	private void Update()
	{

		if (Input.GetKeyDown(KeyCode.V) && !IsPerformingFocusAttack)
		{
			IsUsingFocus = !IsUsingFocus;
			if (IsUsingFocus)
			{
				Time.timeScale = 0.07f;
				combatScript.enabled = false;
				EnemyManager.instance.stopUpdate = true;
				assassinsFocusUI.StartFocus();
			}
			else
			{
				EndFocusState();
				EnemyManager.instance.stopUpdate = false;

				if (!IsPerformingFocusAttack)
				{
					if (focusedEnemies != null)
					{
						focusedEnemies.Clear();
					}
				}
			}
		}
		else if (IsUsingFocus)
		{
			if (focusedEnemies != null && focusedEnemies.Count > 0)
			{
				assassinsFocusUI.Repaint(focusedEnemies.ToArray());
			}
		}

		if (IsUsingFocus && !IsPerformingFocusAttack && Input.GetKeyDown(KeyCode.F))
		{
			IsPerformingFocusAttack = true;
			Time.timeScale = 1;
			PerformFocusAttack();
		}
		if (Input.GetMouseButtonDown(0) && IsUsingFocus && !IsPerformingFocusAttack)
		{

			focusedEnemies ??= new();
			EnemyAI enemy = enemyDetection.GetTheEnemyInCameraDirection();
			//if (focusedEnemies.Contains(enemy))
			//{
			//	//TODO: Remove marker
			//	focusedEnemies.Dequeue();
			//}
			if (focusedEnemies.Count < 5 && !focusedEnemies.Contains(enemy))
			{
				enemy.DisableMovement = true;
				focusedEnemies.Enqueue(enemy);
			}
		}
	}

	private void EndFocusState()
	{
		combatScript.enabled = true;
		Time.timeScale = 1;
		assassinsFocusUI.EndFocus();
	}

	private void PerformFocusAttack()
	{
		GetComponent<CharacterController>().enabled = false;
		StartCoroutine(StartFocusAttack());
	}
	IEnumerator StartFocusAttack()
	{
		while (focusedEnemies.Count > 0)
		{
			currentEnemy = focusedEnemies.Dequeue();
			transform.DOLookAt(currentEnemy.transform.position, 0.1f);
			Vector3 directionToEnemy = (currentEnemy.transform.position - transform.position);
			Vector3 negetiveDirection = -directionToEnemy.normalized * StoppingDistance;
			Vector3 stoppingPoint = transform.position + (directionToEnemy + negetiveDirection).XZ();
			transform.DOMove(stoppingPoint, 0.2f).onComplete += () =>
			{
				currentEnemy.DealDamage(int.MaxValue);
				animator.SetTrigger("Assassinate");
			};
			yield return new WaitForSeconds(1.4f);
		}
		GetComponent<CharacterController>().enabled = true;
		IsUsingFocus = false;
		combatScript.enabled = true;
		assassinsFocusUI.EndFocus();
		IsPerformingFocusAttack = false;
		EnemyManager.instance.stopUpdate = false;
	}
	private void OnAssassinate()
	{
	}
}
