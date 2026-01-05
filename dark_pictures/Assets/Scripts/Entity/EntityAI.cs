using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EntityAI : MonoBehaviour
{
    [Header("Smart Patrol")]
    public int maxPatrolMemory = 5;
    private System.Collections.Generic.List<Vector3> visitedPatrolPoints = new System.Collections.Generic.List<Vector3>();

    [Header("Sound Detection")]
    public float soundDetectionRange = 15f;
    public float minPlayerSpeedToHear = 5f; // Sprint speed threshold

    [Header("References")]
    public Transform player;
    public Light playerFlashLight;

    [Header("Left and right eye of entity")]
    public Renderer[] eyeRenderers;
    public Light eyeLight;

    [Header("Settings")]
    public float patrolSpeed = 3f;
    public float chaseSpeed = 8f;

    [Header("Detection")]
    public float darkDetectionRange = 8f;
    public float lightDetectionRange = 20f;
    public LayerMask visionMask;

    [Header("Patrol Settings")]
    public float patrolRadius = 50f;

    [Header("Investigation")]
    public float investigationTime = 5f;
    public float investigationRadius = 10f;

    private bool isInvestigating = false;
    private Vector3 lastKnownPlayerPosition;
    private float investigateTimer = 0f;

    private NavMeshAgent agent;
    private bool isChasing = false;
    private Vector3 patrolDestination;
    private float stuckTimer = 0f;
    private bool isStunned = false;

    private Color originalColor;
    private Color stunColor = Color.yellow;
    public float stunBrightness = 5f;

    [SerializeField] GameManager gameManager;
    [SerializeField] AudioClip screamClip;
    private AudioSource audioSource;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (visionMask == 0) visionMask = -1;

        // save original eye color
        if (eyeRenderers.Length > 0)
        {
            originalColor = eyeRenderers[0].material.GetColor("_EmissionColor");

            if (eyeLight != null)
            {
                eyeLight.color = originalColor;
                eyeLight.intensity = 2f;
                eyeLight.range = 2f;
            }
        }
        audioSource = GetComponent<AudioSource>();
        SetNewPatrolPoint();
    }

    void Update()
    {
        if (isStunned) return;

        if (CanSeePlayer())
        {
            lastKnownPlayerPosition = player.position;
            ChasePlayer();
        }
        else if (CanHearPlayer())
        {
            // Heard the player! Investigate sound
            Debug.Log("Heard player movement!");
            StartInvestigation();
        }
        else if (isChasing)
        {
            StartInvestigation();
        }
        else
        {
            Patrol();
        }
    }

    bool CanHearPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > soundDetectionRange) return false;

        // Check if player is moving fast (sprinting)
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            // If player is sprinting, AI can hear them
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null && cc.velocity.magnitude > minPlayerSpeedToHear)
            {
                lastKnownPlayerPosition = player.position;
                return true;
            }
        }

        return false;
    }

    public void StunEntity(float duration)
    {
        if (isStunned) return;
        StartCoroutine(StunRoutine(duration));
    }

    IEnumerator StunRoutine(float duration)
    {
        isStunned = true;

        // last known position at stun (Where the player is right now)
        Vector3 lastKnownPos = player.position;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        UpdateEyeVisuals(stunColor);

        yield return new WaitForSeconds(duration);

        isStunned = false;
        agent.isStopped = false;
        UpdateEyeVisuals(originalColor);

        // If we can see the player NOW, chase them immediately.
        if (CanSeePlayer())
        {
            ChasePlayer();
        }
        else
        {
            // If we CAN'T see them (they hid behind a wall), 
            // go to where we last saw them (Investigate).
            agent.SetDestination(lastKnownPos);
        }
    }

    void UpdateEyeVisuals(Color targetColor)
    {
        if (eyeLight != null)
        {
            float maxColorComponent = targetColor.maxColorComponent;
            if (maxColorComponent > 1f)
            {
                eyeLight.color = targetColor / maxColorComponent;
            }
            else
                eyeLight.color = targetColor;
        }

        if (eyeRenderers != null)
        {
            Color finalMaterialColor = targetColor;

            if (targetColor.maxColorComponent <= 1f)
            {
                finalMaterialColor = targetColor * stunBrightness;
            }

            foreach (Renderer r in eyeRenderers)
            {
                r.material.EnableKeyword("_EMISSION");
                r.material.SetColor("_EmissionColor", finalMaterialColor);
            }
        }
    }

    bool CanSeePlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float currentRange = (playerFlashLight.intensity > 1f) ? lightDetectionRange : darkDetectionRange;

        if (distanceToPlayer > currentRange) return false;

        Vector3 origin = transform.position + Vector3.up * 1.0f;
        Vector3 target = player.position + Vector3.up * 1.0f;
        Vector3 direction = (target - origin).normalized;

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, currentRange, visionMask))
        {
            if (hit.transform == player || hit.transform.IsChildOf(player))
            {
                return true;
            }
        }
        return false;
    }

    void ChasePlayer()
    {
        isChasing = true;
        stuckTimer = 0f;
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    void Patrol()
    {
        isChasing = false;
        agent.speed = patrolSpeed;

        // If investigating, stay at the spot for a bit
        if (isInvestigating)
        {
            investigateTimer -= Time.deltaTime;
            if (investigateTimer <= 0f)
            {
                isInvestigating = false;
                SetNewPatrolPoint();
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            SetNewPatrolPoint();
            stuckTimer = 0f;
        }

        if (agent.velocity.sqrMagnitude < 0.1f && agent.remainingDistance > 0.5f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 2.0f)
            {
                SetNewPatrolPoint();
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    void StartInvestigation()
    {
        isChasing = false;
        isInvestigating = true;
        investigateTimer = investigationTime;

        // Go to last known position
        agent.SetDestination(lastKnownPlayerPosition);

        Debug.Log("Lost player! Investigating last known position...");
    }

    void SetNewPatrolPoint()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
            randomDirection += transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
            {
                // Check if we've been here recently
                bool tooCloseToRecent = false;
                foreach (Vector3 visited in visitedPatrolPoints)
                {
                    if (Vector3.Distance(hit.position, visited) < 5f)
                    {
                        tooCloseToRecent = true;
                        break;
                    }
                }

                if (tooCloseToRecent) continue;

                NavMeshPath path = new NavMeshPath();
                agent.CalculatePath(hit.position, path);

                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    patrolDestination = hit.position;
                    agent.SetDestination(patrolDestination);

                    // Remember this point
                    visitedPatrolPoints.Add(hit.position);
                    if (visitedPatrolPoints.Count > maxPatrolMemory)
                    {
                        visitedPatrolPoints.RemoveAt(0);
                    }

                    return;
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, darkDetectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lightDetectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, soundDetectionRange);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!gameManager.isGameOver)
                audioSource.PlayOneShot(screamClip);
            gameManager.GameOver();
        }
    }
}
