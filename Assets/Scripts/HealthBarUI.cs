using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HealthBarUI : MonoBehaviour
{
	public static HealthBarUI instance;
	public PlayerHealthAndStamina health;
	[SerializeField] private Slider primaryHealthSlider;
	[SerializeField] private Slider secondaryHealthSlider;
	[SerializeField] private Image secondaryHealthSliderFill;
	[SerializeField] private Slider primaryStaminaSlider;
	[SerializeField] private Slider secondaryStaminaSlider;
	[SerializeField] private Image secondaryStaminaSliderFill;
	
	[SerializeField] private Color staminaRestoreColor;
	[SerializeField] private Color staminaReducedColor;
	[SerializeField] private Color healthRestoredColor;
	[SerializeField] private Color healthDamageColor;
	[Header("Ability (Bro's Fire)")]
	[SerializeField] Image abilityFill;


	private void Awake()
	{
		instance = this;
	}
	private void Start()
	{
		health = GameObject.FindWithTag("Player").GetComponent<PlayerHealthAndStamina>();
		health.OnTakeDamage += UpdateHealth;
		health.OnStaminaChanged += UpdateStamina;
		UpdateHealth(health.GetHeathPercentage);
		UpdateStamina();
		secondaryHealthSlider.value = health.GetHeathPercentage;
	}
	public void UpdateBrosAbility(float percentage01)
	{
		abilityFill.fillAmount=percentage01;
	}
	internal void UnSubscribe(PlayerHealthAndStamina health)
	{
		this.health.OnDead -= OnDead;
		this.health.OnTakeDamage -= UpdateHealth;
		primaryHealthSlider.gameObject.SetActive(false);
		secondaryHealthSlider.gameObject.SetActive(false);
		this.health = null;
	}
	void UpdateHealth(float value)
	{
		secondaryHealthSliderFill.color = healthDamageColor;
		TakeDamage(primaryHealthSlider, secondaryHealthSlider, health.GetHeathPercentage);
	}
	private void UpdateHealthRestored(float oldValue, float newValue)
	{
		secondaryHealthSliderFill.color = healthRestoredColor;
		Restore(primaryHealthSlider, secondaryHealthSlider, health.GetHeathPercentage);
	}

	private void UpdateStamina()
	{
		if (primaryHealthSlider.value - health.GetStaminaPercentage > 0)
		{
			secondaryStaminaSliderFill.color = staminaReducedColor;
			TakeDamage(primaryStaminaSlider, secondaryStaminaSlider, health.GetStaminaPercentage);
		}
		else
		{
			secondaryStaminaSliderFill.color = staminaRestoreColor;
			Restore(primaryStaminaSlider, secondaryStaminaSlider, health.GetStaminaPercentage);
		}
	}

	private void TakeDamage(Slider primaryHealthSlider, Slider secondaryHealthSlider, float value)
	{
		primaryHealthSlider.DOValue(value, 0.1f).SetEase(Ease.InSine);

		if (secondaryHealthSlider.value - value > .3f)
		{
			secondaryHealthSlider.DOKill();
			secondaryHealthSlider.DOValue(value, 0.4f).SetEase(Ease.InSine);

		}
		else
		{
			secondaryHealthSlider.value = value;
		}
	}

	private void Restore(Slider primarySlider, Slider secondarySlider, float value)
	{
		if (secondarySlider.value - value < 1f)
		{
			secondarySlider.DORestart();
			primarySlider.DOValue(value, 0.8f).SetEase(Ease.InSine);
		}
		else
		{
			primarySlider.value = value;
		}
	}

	void OnDead()
	{
		health.OnTakeDamage -= UpdateHealth;
		health = null;
		primaryHealthSlider.gameObject.SetActive(false);
		secondaryHealthSlider.gameObject.SetActive(false);
	}

}

