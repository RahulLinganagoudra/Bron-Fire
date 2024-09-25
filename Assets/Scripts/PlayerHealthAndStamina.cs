using System;
using UnityEngine;

public class PlayerHealthAndStamina : MonoBehaviour
{
	private const float EightyPercentage = 0.8f;
	private const int OneSecond = 1;
	[SerializeField] private HealthBarUI healthBarUI;

	[SerializeField] private int _MaxHealthPoints;
	[SerializeField] private float _healthReductionSpeed;
	[SerializeField] private int _MaxStaminaPoints;
	[SerializeField] private float _staminaReductionSpeed;
	[SerializeField] private float _staminaRetentionTime;
	[SerializeField] private float _lowStaminaThreshold;
	[SerializeField] private float broFireLifetime;

	private float _health;
	private float _stamina;




	float lastTimeStaminaGained = 0;

	public Action<float> OnTakeDamage;
	public Action OnStaminaChanged;
	public Action OnDead;
	private bool isOnFire;
	private float lastTimeBroOnFire;
	private float lastTimeHealthReduced = float.MinValue;
	private float lastTimeStaminaReduced;
	private float lastTimeHealthGained;
	private float HeathRestoreOnFullHealth = 5;

	public float Health
	{
		get
		{
			return _health;
		}
		private set
		{
			_health = Mathf.Clamp(value, 0, _MaxHealthPoints);
			OnTakeDamage?.Invoke(Health);
		}
	}
	public int MaxHealth => _MaxHealthPoints;
	public float HealthReductionSpeed => _healthReductionSpeed;
	public float Stamina
	{
		get
		{
			return _stamina;
		}
		private set
		{
			_stamina = Mathf.Clamp(value, 0, _MaxStaminaPoints);
			OnStaminaChanged?.Invoke();
		}
	}
	public int MaxStamina => _MaxStaminaPoints;
	public float StaminaReductionSpeed => _staminaReductionSpeed;
	public float GetHeathPercentage => Health * 100 / _MaxHealthPoints;
	public float GetStaminaPercentage => _stamina * 100 / _MaxStaminaPoints;
	public bool IsOnLowStamina => _stamina < _lowStaminaThreshold;
	public bool IsOnFullStamina => _stamina >= _MaxStaminaPoints;


	private void OnEnable()
	{
		lastTimeBroOnFire = 0;
		lastTimeHealthReduced = 0;
		lastTimeStaminaReduced = 0;
		lastTimeHealthGained = 0;
		HeathRestoreOnFullHealth = 0;
		Health = _MaxHealthPoints;
		Stamina = _MaxStaminaPoints * 0.8f;
		lastTimeStaminaGained = Time.time;
		EnemyAI.OnDead += (e) => GainStamina(10);
	}


	// Update is called once per frame
	void Update()
	{
		if (IsOnFullStamina)
		{
			if (Time.time - lastTimeHealthGained >= OneSecond && Health <= MaxHealth * EightyPercentage)
			{
				Health += HeathRestoreOnFullHealth;
				lastTimeHealthGained = Time.time;
			}

			if (isOnFire && Time.time - lastTimeBroOnFire > broFireLifetime)
			{
				isOnFire = false;
				Stamina -= _staminaReductionSpeed;
				lastTimeStaminaReduced = Time.time;
			}
		}
		else if (!IsOnLowStamina)
		{
			if (Time.time - lastTimeStaminaGained > _staminaRetentionTime && Time.time - lastTimeStaminaReduced >= OneSecond)
			{
				Stamina -= _staminaReductionSpeed;
				lastTimeStaminaReduced = Time.time;
			}
		}
		else
		{
			if (Time.time - lastTimeHealthReduced > 1)
			{
				Health -= _healthReductionSpeed;
				lastTimeHealthReduced = Time.time;
			}
		}
	}

	// Public API
	public int GetCurrentHealth()
	{
		return 0;
	}
	public void GainHeath(float healthGain)
	{
		Stamina += healthGain;
		lastTimeHealthGained = Time.time ;
	}
	public void GainStamina(float ammount)
	{
		Stamina += ammount;
		lastTimeStaminaGained = Time.time;
		if (Stamina >= _MaxStaminaPoints)
		{
			isOnFire = true;
			lastTimeBroOnFire = Time.time;
		}
	}
	public void DealDamage(DamageInfo info)
	{
		Health -= info.damageAmmount;
	}
	

	public void UseStamina(int cost)
	{
		Stamina -= cost;
		if (!IsOnFullStamina && isOnFire)
		{
			isOnFire = false;
		}
	}
	/// <summary>
	/// After Increasing Max_health also restores health
	/// </summary>
	/// <param name="ammount">new max_health</param>
	public void IncreaseMaxHealthTo(int ammount)
	{
		_MaxHealthPoints = ammount;
		Health = _MaxHealthPoints;
	}
	/// <summary>
	/// After Increasing Max_Stamina also restores stamina to 80%
	/// </summary>
	/// <param name="ammount">new max_stamina</param>
	public void IncreaseMaxStaminaTo(int ammount)
	{
		_MaxStaminaPoints = ammount;
		Stamina = _MaxStaminaPoints * EightyPercentage;
	}
	public void UpdateHealthReductionSpeed(float newSpeed)
	{
		_healthReductionSpeed = newSpeed;
	}
	public void UpdateStaminaReductionSpeed(float newSpeed)
	{
		_staminaReductionSpeed = newSpeed;
	}
	public void UpdateStaminaRetentionTime(float newRetentionTime)
	{
		_staminaRetentionTime = newRetentionTime;
	}
	public void UpdateBroFireLifetime(float newLifetime)
	{
		broFireLifetime = newLifetime;
	}
}
public class DamageInfo
{
	public GameObject origin;
	public float damageAmmount;
}
