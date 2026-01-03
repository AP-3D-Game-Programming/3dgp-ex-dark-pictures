using UnityEngine;
using UnityEngine.AI;

public class SecurityGuardAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;
    public float waitTimeAtPoint = 3f;
    
    [Header("Detection Settings")]
    public float detectionRange = 10f;
    public float fieldOfViewAngle = 90f;
    public float chaseSpeed = 4f;
    public LayerMask detectionLayers;
    
    [Header("Audio Settings")]
    public float hearingRange = 15f;
    public float investigateSpeed = 3f;
    
    private NavMeshAgent agent;
    private Transform player;
    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;
    private Vector3 investigatePosition;
    
    public enum GuardState { Patrolling, Investigating, Chasing, Waiting }
    public GuardState currentState = GuardState.Patrolling;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (player == null)
        {
            Debug.LogError("SecurityGuard: No Player found! Make sure your player has the 'Player' tag.");
        }
        
        if (patrolPoints.Length > 0)
        {
            agent.speed = patrolSpeed;
            GoToNextPatrolPoint();
        }
        else
        {
            Debug.LogWarning("SecurityGuard: No patrol points assigned!");
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        switch (currentState)
        {
            case GuardState.Patrolling:
                Patrol();
                CheckForPlayer();
                break;
            case GuardState.Investigating:
                Investigate();
                CheckForPlayer();
                break;
            case GuardState.Chasing:
                ChasePlayer();
                break;
            case GuardState.Waiting:
                Wait();
                break;
        }
    }
    
    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentState = GuardState.Waiting;
            waitTimer = waitTimeAtPoint;
        }
    }
    
    void Wait()
    {
        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0)
        {
            GoToNextPatrolPoint();
            currentState = GuardState.Patrolling;
        }
    }
    
    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        agent.speed = patrolSpeed;
    }
    
    void CheckForPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Line of sight check
        if (distanceToPlayer < detectionRange)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            
            if (angleToPlayer < fieldOfViewAngle / 2f)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, detectionRange, detectionLayers))
                {
                    if (hit.transform == player)
                    {
                        StartChasing();
                    }
                }
            }
        }
    }
    
    void StartChasing()
    {
        currentState = GuardState.Chasing;
        agent.speed = chaseSpeed;
        Debug.Log("Security Guard: Spotted the player!");
    }
    
    void ChasePlayer()
    {
        agent.SetDestination(player.position);
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Lost sight of player
        if (distanceToPlayer > detectionRange * 1.5f)
        {
            investigatePosition = player.position;
            currentState = GuardState.Investigating;
            agent.speed = investigateSpeed;
            Debug.Log("Security Guard: Lost sight, investigating...");
        }
        
        // Caught the player
        if (distanceToPlayer < 2f)
        {
            CatchPlayer();
        }
    }
    
    void Investigate()
    {
        agent.SetDestination(investigatePosition);
        
        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            // Give up search after reaching investigate point
            waitTimer = 2f;
            currentState = GuardState.Waiting;
        }
    }
    
    void CatchPlayer()
    {
        Debug.Log("Security Guard: You've been caught!");
        // For now just log, we'll add respawn later
        currentState = GuardState.Waiting;
        waitTimer = 5f;
    }
    
    // This will be called by PlayerController when player makes noise
    public void HearNoise(Vector3 noisePosition, float noiseIntensity)
    {
        float distanceToNoise = Vector3.Distance(transform.position, noisePosition);
        
        if (distanceToNoise < hearingRange * noiseIntensity && currentState != GuardState.Chasing)
        {
            investigatePosition = noisePosition;
            currentState = GuardState.Investigating;
            agent.speed = investigateSpeed;
            Debug.Log("Security Guard: Heard something!");
        }
    }
}
