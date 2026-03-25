using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Enhanced enemy AI. Uses CharacterStats + Damageable for proper RPG combat.
/// Patrols, detects player, chases, attacks in real-time.
/// Bosses are flagged and trigger turn-based combat.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Damageable))]
[RequireComponent(typeof(DeathRagdollController))]
public class EnemyController : MonoBehaviour
{
    [Header("AI Behavior")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float detectionRadius = 8f;
    [SerializeField] private float loseTargetRadius = 14f;
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private float attackDamage = 12f;
    [SerializeField] private DamageType attackDamageType = DamageType.Physical;
    [SerializeField] private float patrolPause = 1.5f;

    [Header("Loot")]
    [SerializeField] private int currencyDropMin = 5;
    [SerializeField] private int currencyDropMax = 20;
    [SerializeField] private string reputationFactionOnKill;
    [SerializeField] private int reputationDeltaOnKill = -5;

    private NavMeshAgent agent;
    private Damageable damageable;
    private Transform player;
    private int currentPatrolIndex;
    private float waitTimer;
    private float nextAttackTime;
    private bool chasingPlayer;
    private float detectionSqr;
    private float loseSqr;

    public void SetPatrolPoints(Transform[] points) => patrolPoints = points;

    private void Awake()
    {
        ProceduralHumanoidRig rig = GetComponent<ProceduralHumanoidRig>();
        if (rig == null)
        {
            rig = gameObject.AddComponent<ProceduralHumanoidRig>();
        }
        rig.ConfigureRendererVisibility(false, false);
        rig.EnsureBuilt();

        if (GetComponent<ActiveRagdollMotor>() == null)
        {
            gameObject.AddComponent<ActiveRagdollMotor>();
        }

        if (GetComponent<DeathRagdollController>() == null)
        {
            gameObject.AddComponent<DeathRagdollController>();
        }

        agent = GetComponent<NavMeshAgent>();
        damageable = GetComponent<Damageable>();

        detectionSqr = detectionRadius * detectionRadius;
        loseSqr = loseTargetRadius * loseTargetRadius;

        // Apply difficulty scaling to enemy health
        if (DifficultyManager.Instance != null)
        {
            float hpMult = DifficultyManager.Instance.EnemyHealthMultiplier;
        if (damageable != null && hpMult != 1f)
        {
            float scaledHp = damageable.HealthNormalized > 0f ? 50f * hpMult : 50f;
            damageable.Configure(scaledHp, damageable.IsBoss, false);
        }
        }

        if (damageable != null)
            damageable.Died += OnDied;
    }

    private void OnDestroy()
    {
        if (damageable != null)
            damageable.Died -= OnDied;
    }

    private void Start()
    {
        ResolvePlayer();
        if (agent.isOnNavMesh) GoToPatrolPoint();
    }

    private void Update()
    {
        if (damageable != null && damageable.IsDead) return;
        if (GameplayPauseFacade.IsPaused) { if (agent.isOnNavMesh) agent.isStopped = true; return; }
        if (agent.isOnNavMesh) agent.isStopped = false;

        if (player == null) { ResolvePlayer(); return; }
        if (!agent.isOnNavMesh) return;

        float distSqr = (transform.position - player.position).sqrMagnitude;

        if (chasingPlayer)
        {
            if (distSqr > loseSqr) { chasingPlayer = false; GoToPatrolPoint(); return; }

            agent.stoppingDistance = attackRange;
            agent.SetDestination(player.position);

            if (distSqr <= attackRange * attackRange && Time.time >= nextAttackTime)
                PerformAttack();
            return;
        }

        if (distSqr <= detectionSqr)
        {
            chasingPlayer = true;
            return;
        }

        // Patrol
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        if (agent.pathPending) return;

        if (agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= patrolPause)
            {
                waitTimer = 0f;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                GoToPatrolPoint();
            }
        }
    }

    private void PerformAttack()
    {
        nextAttackTime = Time.time + attackCooldown;

        float damage = attackDamage;
        if (DifficultyManager.Instance != null)
            damage *= DifficultyManager.Instance.EnemyDamageMultiplier;

        Damageable playerDamageable = player.GetComponent<Damageable>();
        if (playerDamageable != null)
            playerDamageable.ApplyDamage(damage, attackDamageType);
        else
        {
            CharacterStats pStats = player.GetComponent<CharacterStats>();
            if (pStats != null) pStats.TakeDamage(damage, attackDamageType);
        }
    }

    private void OnDied()
    {
        // Drop currency
        int drop = UnityEngine.Random.Range(currencyDropMin, currencyDropMax + 1);
        if (player != null)
        {
            CurrencyWallet wallet = player.GetComponent<CurrencyWallet>();
            if (wallet != null) wallet.Add("gold", drop);

            // Reputation impact
            if (!string.IsNullOrWhiteSpace(reputationFactionOnKill))
            {
                ReputationSystem rep = player.GetComponent<ReputationSystem>();
                if (rep != null) rep.ModifyReputation(reputationFactionOnKill, reputationDeltaOnKill);
            }

            // Combat XP
            SkillSystem skills = player.GetComponent<SkillSystem>();
            if (skills != null) skills.AddExperience("survival", 15);
        }
    }

    private void GoToPatrolPoint()
    {
        if (!agent.isOnNavMesh || patrolPoints == null || patrolPoints.Length == 0) return;
        if (patrolPoints[currentPatrolIndex] == null) return;
        agent.stoppingDistance = 0.1f;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    private void ResolvePlayer()
    {
        if (player != null) return;
        if (HolstinSceneContext.TryGet(out HolstinSceneContext ctx) && ctx.PlayerTransform != null)
            player = ctx.PlayerTransform;
        else
        {
            PlayerMover m = FindAnyObjectByType<PlayerMover>();
            if (m != null) player = m.transform;
        }
    }
}
