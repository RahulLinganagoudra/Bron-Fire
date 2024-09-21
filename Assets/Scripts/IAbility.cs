public interface IAbility
{
	int MaxCharge { get; set; }
	float ChargeRate { get; set; }
	int StaminaCost { get; set; }
	void UseAbility(CombatScript combatScript);
}
