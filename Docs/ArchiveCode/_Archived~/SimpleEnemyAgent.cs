using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SimpleEnemyAgent : MonoBehaviour
{
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float detectionRadius = 8f;
    [SerializeField] private float loseTargetRadius = 13f;
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float patrolPause = 1f;
    [SerializeField] private int hitPoints = 3;

    private NavMeshAgent agent;
    private Transform player;
    private int currentPatrolIndex;
    private float waitTimer;
    private bool chasingPlayer;
    private float nextPlayerResolveTime;
    private float detectionRadiusSqr;
    private float loseTargetRadiusSqr;


    public void SetPatrolPoints(Transform[] points)
    {
        patrolPoints = points;
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        CacheDistanceThresholds();
        ResolvePlayerReference();
    }

    private void OnValidate()
    {
        CacheDistanceThresholds();
    }

    private void Start()
    {
        if (!agent.isOnNavMesh)
        {
            return;
        }

        agent.stoppingDistance = 0.1f;
        GoToCurrentPatrolPoint();
    }

    private void Update()
    {
        if (GameplayPauseFacade.IsPaused)
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }
            return;
        }

        if (agent != null)
        {
            agent.isStopped = false;
        }

        if (player == null)
        {
            if (Time.time >= nextPlayerResolveTime)
            {
                nextPlayerResolveTime = Time.time + 1f;
                ResolvePlayerReference();
            }
        }

        if (player == null || !agent.isOnNavMesh)
        {
            return;
        }

        float playerDistanceSqr = (transform.position - player.position).sqrMagnitude;

        if (chasingPlayer)
        {
            if (playerDistanceSqr > loseTargetRadiusSqr)
            {
                chasingPlayer = false;
                GoToCurrentPatrolPoint();
                return;
            }

            agent.stoppingDistance = attackRange;
            agent.SetDestination(player.position);
            return;
        }

        if (playerDistanceSqr <= detectionRadiusSqr)
        {
            chasingPlayer = true;
            return;
        }

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return;
        }

        if (agent.pathPending)
        {
            return;
        }

        if (agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= patrolPause)
            {
                waitTimer = 0f;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                GoToCurrentPatrolPoint();
            }
        }
    }

    public void RegisterHit()
    {
        hitPoints--;
        if (hitPoints <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            chasingPlayer = true;
        }
    }

    private void GoToCurrentPatrolPoint()
    {
        if (!agent.isOnNavMesh || patrolPoints == null || patrolPoints.Length == 0 || patrolPoints[currentPatrolIndex] == null)
        {
            return;
        }

        agent.stoppingDistance = 0.1f;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    private void ResolvePlayerReference()
    {
        if (player != null)
        {
            return;
        }

        if (HolstinSceneContext.TryGet(out HolstinSceneContext context) && context.PlayerTransform != null)
        {
            player = context.PlayerTransform;
            return;
        }

        PlayerMover mover = FindAnyObjectByType<PlayerMover>();
        if (mover != null)
        {
            player = mover.transform;
        }
    }

    private void CacheDistanceThresholds()
    {
        detectionRadiusSqr = Mathf.Max(0f, detectionRadius) * Mathf.Max(0f, detectionRadius);
        loseTargetRadiusSqr = Mathf.Max(detectionRadius, loseTargetRadius) * Mathf.Max(detectionRadius, loseTargetRadius);
    }
}
