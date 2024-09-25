using UnityEngine;
using Utilities;

public class RingOfFire : MonoBehaviour
{
	private const int OneSecond = 1;
	public float radius;
	public float healthGainOverTime;
	public float staminaGainOverTime;
	private PlayerHealthAndStamina player;

	private float time = 1;
	public bool IsPlayerInsideRing => transform.position.CompareDist(player.transform.position, radius);

	private void Start()
	{
		player=GameObject.FindWithTag("Player").GetComponent<PlayerHealthAndStamina>();
		transform.localScale= radius * Vector3.one;
	}
	private void Update()
	{
		if (IsPlayerInsideRing)
		{
			time += Time.deltaTime;
			if (time >= OneSecond)
			{
				player.GainHeath(healthGainOverTime);
				player.GainStamina(staminaGainOverTime);

				time = 0;
			}
		}
	}

}