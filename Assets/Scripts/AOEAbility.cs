using UnityEngine;
using System;

[Serializable]
public class AOEAbility : IAbility
{
	public int maxCharge;
	public float chargeRate;
	public int staminaCost;
	public RingOfFire FX;
	public int MaxCharge { get => maxCharge; set => maxCharge = value; }
	public float ChargeRate { get => chargeRate; set => chargeRate = value; }
	public int StaminaCost { get => staminaCost; set => staminaCost = value; }

	public void UseAbility(CombatScript combatScript)
	{
		var fx = GameObject.Instantiate(FX);
		fx.transform.position = combatScript.transform.position;
		GameObject.Destroy(fx.gameObject, 5f);
	}
}
