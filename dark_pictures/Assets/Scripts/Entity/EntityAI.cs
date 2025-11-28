using UnityEngine;
using UnityEngine.AI;

public class EntityAI : MonoBehaviour
{
	[Header("References")]
	public Transform player;
	public Light playerFlashLight;

	[Header("Settings")]
	public float patrolSpeed = 3f;
	public float chaseSpeed = 8f;

	[Header("Detection")]
	public float darkDetectionRange = 8f;  // How close needs to be to see you in dark
	public float lightDetectionRange = 20f; // How far it sees you when you flash

	// LayerMask so it does not see through walls
	public LayerMask visionMask;

	[Header("Patrol Settings")]
	public float patrolRadius = 50f;

	private NavMeshAgent agent;
	private bool isChasing = false;
	private Vector3 patrolDestination;

	// Variable to detect if we are stuck standing still
	private float stuckTimer = 0f;

	void Start()
	{
		agent = GetComponent<NavMeshAgent>();

		if (visionMask == 0) visionMask = -1;

		SetNewPatrolPoint();
	}

	void Update()
	{
		// Check if can see player
		if (CanSeePlayer())
		{
			ChasePlayer();
		}
		else
		{
			Patrol();
		}
	}

	/// <summary>
	/// Function to check if the entity can see the player
	/// </summary>
	/// <returns>bool</returns>
	bool CanSeePlayer()
	{
		float distanceToPlayer = Vector3.Distance(transform.position, player.position);
		float currentRange = (playerFlashLight.intensity > 1f) ? lightDetectionRange : darkDetectionRange;

		// If player is out of range
		if (distanceToPlayer > currentRange) return false;

		Vector3 origin = transform.position + Vector3.up * 1.0f;
		Vector3 target = player.position + Vector3.up * 1.0f;
		Vector3 direction = (target - origin).normalized;

		// Raycast to see if there are obstacles in the way
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

	/// <summary>
	/// Initiates the chasing behavior, causing the entity to pursue the player.
	/// </summary>
	/// <remarks>
	/// This method sets the entity's chasing state to active, adjusts its movement speed to the chase
	/// speed,  and updates its destination to the player's current position.
	/// </remarks>
	void ChasePlayer()
	{
		isChasing = true;
		stuckTimer = 0f; // Reset stuck timer when chasing
		agent.speed = chaseSpeed;
		agent.SetDestination(player.position);
	}

	/// <summary>
	/// Initiates the patrol behavior, setting the agent to move at patrol speed and navigate to a new patrol point when
	/// the current one is reached.
	/// </summary>
	/// <remarks>
	/// This method sets the agent's speed to the patrol speed and ensures the agent moves to a new patrol
	/// point once it is close to the current destination.  The agent stops chasing any targets during patrol.
	/// </remarks>
	void Patrol()
	{
		isChasing = false;
		agent.speed = patrolSpeed;

		// 1. STANDARD CHECK: Are we there?
		if (!agent.pathPending && agent.remainingDistance < 0.5f)
		{
			SetNewPatrolPoint();
			stuckTimer = 0f;
		}

		// 2. STUCK CHECK: Are we standing still but haven't reached the destination?
		// If velocity is near zero, increase timer.
		if (agent.velocity.sqrMagnitude < 0.1f && agent.remainingDistance > 0.5f)
		{
			stuckTimer += Time.deltaTime;

			// If we have been stuck for 2 seconds, force a new point
			if (stuckTimer > 2.0f)
			{
				SetNewPatrolPoint();
				stuckTimer = 0f;
			}
		}
		else
		{
			// We are moving fine, reset timer
			stuckTimer = 0f;
		}
	}

	/// <summary>
	/// Sets a new patrol point for the agent by selecting a random position within a specified radius  and finding the
	/// nearest valid point on the NavMesh.
	/// </summary>
	/// <remarks>
	/// This method calculates a random point within a sphere of the specified patrol radius, centered  on
	/// the agent's current position. It then determines the closest valid point on the NavMesh to  use as the patrol
	/// destination. If a valid point is found, the agent's destination is updated  accordingly.
	/// </remarks>
	void SetNewPatrolPoint()
	{
		// Try up to 10 times to find a valid point.
		// This prevents the AI from picking a point on the roof or inside a wall and giving up.
		for (int i = 0; i < 10; i++)
		{
			Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
			randomDirection += transform.position;

			NavMeshHit hit;
			if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
			{
				NavMeshPath path = new NavMeshPath();
				agent.CalculatePath(hit.position, path);

				if (path.status == NavMeshPathStatus.PathComplete)
				{
					patrolDestination = hit.position;
					agent.SetDestination(patrolDestination);
					return;
				}
			}
		}
	}

	// DEBUG: Circles to visualize detection ranges
	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, darkDetectionRange);

		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, lightDetectionRange);

		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, patrolRadius);
	}
}