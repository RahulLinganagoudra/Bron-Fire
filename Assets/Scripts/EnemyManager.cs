using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;
/*
 * Enemy manager manages the current available enemies and puts them into the active enemy's pool 
 * Enemies subscribe themself when they become active and unsubscribes themselves when they become inactive or become dead 
 * This runs a enemy loop, which runs for certain times within second, which is way less than the update loop,
 * It updates the enemy states and manages the enemy behaviour 
*/
public class EnemyManager : Singleton<EnemyManager>
{
	public List<EnemyAI> activeEnemies = new();
	public EnemyAI attackingEnemy;
	public System.Action<EnemyAI> OnEnemyDead;

	[Header("Enemy Update Loop")]
	[SerializeField]
	[Tooltip("While other enemy is getting hit, Chance of sending backup enemy for his help")]
	private int getBackupPercentage=15;
	[SerializeField]
	[Min(0.5f)]
	float updatesPerSecond = 5;


	private float deltaTime => 1 / updatesPerSecond;
	private float timePassed = 0;
	public bool stopUpdate;

	private void Start()
	{
		DOTween.Init();
		print($"Deltatime = {deltaTime}");
	}
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			DOTween.KillAll();
			DOTween.Clear(true);
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}
		if (activeEnemies.Count == 0 || stopUpdate) return;
		//Enemy Update Loop
		if (timePassed >= deltaTime)
		{
			timePassed = 0;
			UpdateEnemyStates();
		}
		else
		{
			timePassed += Time.deltaTime;
		}
	}

	//this method calls in a specified enemy update loop Which only call one to two times in a second 
	//or when a attacking enemy dies
	void UpdateEnemyStates()
	{
		bool foundAttackingEnemy = false;
		foreach (var enemy in activeEnemies)
		{
			if (enemy.IsAttacking)
			{
				foundAttackingEnemy = true;
				continue;
			}

			EnemyStates enemyState = Random.Range(0, 2) == 1 ? EnemyStates.Idle : EnemyStates.Strafing;
			enemy.SetCurrentEnemyState(enemyState);
		}
		if (!foundAttackingEnemy || MyUtils.Utilities.GetChance(getBackupPercentage))
		{
			while (true)
			{
				EnemyAI enemyAI = activeEnemies[Random.Range(0, activeEnemies.Count)];
				if (!enemyAI.IsAttacking && !enemyAI.IsOnLowHealth)
				{
					enemyAI.SetCurrentEnemyState(EnemyStates.Attacking);
					break;
				}
			}
		}
	}
	public void ForceUpdateEnemies()
	{
		UpdateEnemyStates();
	}
	public void Subscribe(EnemyAI enemy)
	{
		activeEnemies.Add(enemy);
	}
	public void Unsubscribe(EnemyAI enemy)
	{
		if (enemy == attackingEnemy)
		{
			attackingEnemy = null;
		}
		activeEnemies.Remove(enemy);
		OnEnemyDead?.Invoke(enemy);
	}
}

