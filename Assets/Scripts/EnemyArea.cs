using MyUtils;
using UnityEngine;

[RequireComponent (typeof(Collider))]
public class EnemyArea : MonoBehaviour
{
    [SerializeField] EnemyAI[] enemies;

	private void OnTriggerEnter(Collider other)
	{
		if(!other.IsPlayer())
		{
			return;
		}

		foreach (var enemy in enemies)
		{
			enemy.AwakenEnemy();
		}
		enabled = false;
		gameObject.SetActive(false);
	}
}
