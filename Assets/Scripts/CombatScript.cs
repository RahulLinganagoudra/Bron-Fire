using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Cinemachine;
using Utilities;
using UI;

public class CombatScript : MonoBehaviour
{
	private const int defaultCombo = 1;
	private const float comboTimeoutDelay = 1f;
	private const float InputDeadZone = 0.1f;
	private const float AttackIdleThresholdTime = 10f;
	private const float StoppingDistance = 1f;
	private const int AttackDistance = 2;
	private const float HalfSecond = 0.5f;
	private const float NoStaminaCostTeliportDistance = 2f;
	private const string CombatStanceAnimationParam = "CombatStance";
	private const string NormalPunchAnimationParam = "NormalPunch";
	private const string TiredAnimationParam = "Tired";
	private const string HitAnimatorTrigger = "Hit1";
	[SerializeField] TrailRenderer rightHand, LefHand, Leg;
	[SerializeField] ParticleSystem rightHandShock, leftHandShock, legShock;
	[SerializeField] private int maxCombo = 2;
	[SerializeField] private int StaminaGainPerHit = 2;
	[SerializeField] private int StaminaCostPerTeliport = 5;
	[SerializeField] private float TeliportDuration = 0.2f;
	[SerializeField] private AudioClip[] onAirPunchSFX;
	[SerializeField] private AudioClip[] punch1SFX;
	[SerializeField] private AudioClip[] punch2SFX;
	[SerializeField] private AudioClip[] punch3SFX;
	[SerializeField] EnemyFocusUI enemyFocusUI;
	[SerializeField] private float targetUiOffset;
	[SerializeField] bool isTired = false;
	[SerializeField] AOEAbility ability;
	[Space]

	//Events
	public UnityEvent<EnemyAI> OnTrajectory;
	public UnityEvent<EnemyAI> OnHit;
	public UnityEvent<EnemyAI> OnCounterAttack;

	private EnemyAI currentTarget;
	private PlayerHealthAndStamina health;
	private AutoTargeting enemyDetection;
	private MovementInput movementInput;
	private Animator animator;
	private CinemachineImpulseSource impulseSource;
	private AudioSource audioSource;
	private int currentCombo;
	private float lastTimeFireInputReceived;
	private bool isInCombatStance = false;
	private float aoeCharge;
	private bool isTeleporting;
	private float teliportLerpTimeDelta;
	private float lastTimeAOERecharged;


	void Start()
	{
		health = GetComponent<PlayerHealthAndStamina>();
		animator = GetComponent<Animator>();
		audioSource = GetComponent<AudioSource>();
		enemyDetection = GetComponent<AutoTargeting>();
		movementInput = GetComponent<MovementInput>();
		impulseSource = GetComponentInChildren<CinemachineImpulseSource>();
		enemyDetection.OnTargetChanged += OnTargetChanged;
		EnemyManager.instance.OnEnemyDead += (x) =>
		{
			if (x == enemyDetection.GetCurrentTarget())
			{
				OnTargetChanged(null);
			}
		};
		OnTargetChanged(null);
	}
	private void OnDisable()
	{
		enemyDetection.OnTargetChanged -= OnTargetChanged;
	}
	private void Update()
	{
		if (health.IsOnFullStamina && aoeCharge < ability.MaxCharge && Time.time - lastTimeAOERecharged >= 1)
		{
			aoeCharge += ability.ChargeRate;
			HealthBarUI.instance.UpdateBrosAbility(aoeCharge / ability.MaxCharge);
			lastTimeAOERecharged = Time.time;
		}
		if (Input.GetKeyDown(KeyCode.Q) && aoeCharge >= ability.MaxCharge)
		{
			health.UseStamina(ability.StaminaCost);
			ability.UseAbility(this);
			aoeCharge = 0;
		}

		if (isInCombatStance && Input.GetKeyDown(KeyCode.Space))
		{
			EnemyAI attackingEnemy = enemyDetection.GetAttackingEnemy(queryRadius: 5);
			DodgeIncommingAttackFrom(attackingEnemy);
		}
		if (Input.GetMouseButtonDown(0) && Time.time - lastTimeFireInputReceived > HalfSecond)
		{
			currentTarget = enemyDetection.GetCurrentTarget();
			Attack();
			if (!isInCombatStance)
			{
				movementInput.CanDash = false;
				isInCombatStance = true;
				animator.SetBool(CombatStanceAnimationParam, isInCombatStance);
			}
			CalculateCombo();
			lastTimeFireInputReceived = Time.time;

		}

		if (Time.time - lastTimeFireInputReceived > AttackIdleThresholdTime || new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).magnitude > InputDeadZone)
		{
			if (isInCombatStance)
			{
				isInCombatStance = false;
				movementInput.CanDash = true;
				animator.SetBool(CombatStanceAnimationParam, isInCombatStance);
			}
		}
	}

	private void Attack()
	{
		if (currentTarget != null && !transform.position.CompareDist(currentTarget.transform.position, StoppingDistance))
		{
			if (!transform.position.CompareDist(currentTarget.transform.position, NoStaminaCostTeliportDistance))
			{
				health.UseStamina(StaminaCostPerTeliport);
				if (movementInput != null)
				{
					movementInput.BlockPlayerMovementFor(TeliportDuration);
				}
			}

			MoveTorwardsTarget(target: currentTarget, duration: TeliportDuration);
			transform.forward = (currentTarget.transform.position - transform.position).XZ().normalized;
		}
	}

	void DodgeIncommingAttackFrom(EnemyAI enemyAI)
	{
		if (enemyAI == null)
		{
			return;
		}

		enemyDetection.SetCurrentTarget(enemyAI);
		Vector3 oppositeDirectionFromEnemy = transform.position - enemyAI.transform.position;
		oppositeDirectionFromEnemy.Normalize();

		transform.DOLookAt(enemyAI.transform.position, .2f);
		transform.DOMove(transform.position + oppositeDirectionFromEnemy, 1).SetEase(Ease.InSine);
		CalculateCombo();
		lastTimeFireInputReceived = Time.time;
	}
	private void OnTargetChanged(EnemyAI newTarget)
	{
		currentTarget = newTarget;
		if (enemyFocusUI == null)
		{
			enemyFocusUI = FindAnyObjectByType<EnemyFocusUI>();
		}
		if (currentTarget != null)
		{
			enemyFocusUI.Focus(currentTarget.transform, Vector3.up * targetUiOffset);
		}
		else
		{
			enemyFocusUI.Focus(null, Vector3.up * targetUiOffset);
		}
		ResetCombo();
	}

	private void CalculateCombo()
	{
		if (Time.time - lastTimeFireInputReceived >= comboTimeoutDelay || currentCombo > maxCombo)
		{
			currentCombo = defaultCombo;
		}
		if (isTired != health.IsOnLowStamina)
		{
			isTired = health.IsOnLowStamina;
			animator.SetBool(TiredAnimationParam, isTired);
		}
		animator.SetTrigger(NormalPunchAnimationParam + currentCombo);

		currentCombo++;
	}

	void MoveTorwardsTarget(EnemyAI target, float duration)
	{
		OnTrajectory.Invoke(target);
		transform.DOLookAt(target.transform.position, .2f);
		transform.DOMove(TargetOffset(target.transform), duration);
		isTeleporting = true;
	}
	float TargetDistance(EnemyAI target)
	{
		return Vector3.Distance(transform.position, target.transform.position);
	}

	public Vector3 TargetOffset(Transform target)
	{
		Vector3 position;
		position = target.position;
		return Vector3.MoveTowards(position, transform.position, .95f);
	}


	public void OnPunch1(AnimationEvent animationEvent)
	{
		audioSource.clip = punch1SFX[Random.Range(0, punch1SFX.Length)];
		HitEnemy();
		rightHandShock.Play();
	}

	public void OnPunch2(AnimationEvent animationEvent)
	{
		audioSource.clip = punch2SFX[Random.Range(0, punch2SFX.Length)];
		HitEnemy();
		leftHandShock.Play();
	}
	public void OnPunch3(AnimationEvent animationEvent)
	{
		audioSource.clip = punch3SFX[Random.Range(0, punch3SFX.Length)];
		HitEnemy();
		legShock.Play();
	}
	private void HitEnemy()
	{
		if (currentTarget != null && transform.position.CompareDist(currentTarget.transform.position, AttackDistance))
		{
			audioSource.Play();
			currentTarget.DealDamage(new DamageInfo { origin=gameObject,damageAmmount=1});
			health.GainStamina(StaminaGainPerHit);
		}
	}
	void EnableTrail(AnimationEvent animationEvent)
	{
		audioSource.clip = onAirPunchSFX[Random.Range(0, onAirPunchSFX.Length)];
		audioSource.Play();
		if (health.IsOnLowStamina) return;
		//switch (animationEvent.intParameter)
		//{
		//	case 0:
		//		rightHand.enabled = true;
		//		break;
		//	case 1:
		//		LefHand.enabled = true;
		//		break;
		//	case 2:
		//		Leg.enabled = true;
		//		break;
		//	default:
		//		break;
		//}
	}
	void DisableTrail(AnimationEvent animationEvent)
	{
		//switch (animationEvent.intParameter)
		//{
		//	case 0:
		//		rightHand.enabled = false;
		//		break;
		//	case 1:
		//		LefHand.enabled = false;
		//		break;
		//	case 2:
		//		Leg.enabled = false;
		//		break;
		//	default:
		//		break;
		//}
	}
	// void LerpCharacterAcceleration()
	// {
	//     movementInput.acceleration = 0;
	//     DOVirtual.Float(0, 1, .6f, ((acceleration) => movementInput.acceleration = acceleration));
	// }
	void ResetCombo()
	{
		currentCombo = defaultCombo;
	}
	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawSphere(transform.position + Vector3.up * targetUiOffset, 0.1f);
	}

	internal void DealDamage(DamageInfo damageInfo)
	{
		animator.SetTrigger(HitAnimatorTrigger);
	health.DealDamage(damageInfo);
	}
}
