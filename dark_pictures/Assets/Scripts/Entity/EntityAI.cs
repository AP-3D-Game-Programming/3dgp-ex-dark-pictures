using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEditor.Rendering;

public class EntityAI : MonoBehaviour
{
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

	private NavMeshAgent agent;
	private bool isChasing = false;
	private Vector3 patrolDestination;
	private float stuckTimer = 0f;
	private bool isStunned = false;

	private Color originalColor;
	private Color stunColor = Color.yellow; 
	public float stunBrightness = 5f;

	[SerializeField] GameManager gameManager;	

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

		SetNewPatrolPoint();
	}

	void Update()
	{
		if (isStunned) return;

		if (CanSeePlayer())
		{
			ChasePlayer();
		}
		else
		{
			Patrol();
		}
	}

	public void StunEntity(float duration)
	{
		if (isStunned) return;
		StartCoroutine(StunRoutine(duration));
	}

	/// <summary>
	/// Routine to handle stunning the entity
	/// </summary>
	/// <param name="duration"></param>
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

	/// <summary>
	/// Update the eye color and glow color
	/// </summary>
	/// <param name="targetColor"></param>
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

	/// <summary>
	/// This method makes sure that the entity only chases if it sees the player, not just based on distance.
	/// </summary>
	/// <returns>
	/// True if the entity can see the player, false otherwise.
	/// </returns>
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

	/// <summary>
	/// This method handles the patrolling behavior of the entity.
	/// </summary>
	/// <remarks>
	/// This method has a stuck timer so it doesn't get stuck on obstacles. If the agent is not moving for more than 2 seconds,
	/// it will pick a new patrol point.
	/// </remarks>
	void Patrol()
	{
		isChasing = false;
		agent.speed = patrolSpeed;

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

	/// <summary>
	/// Sets a new patrol point for the agent within a specified radius.
	/// </summary>
	/// <remarks>
	/// This method attempts to find a random valid position on the NavMesh (where the ai can walk) within the patrol radius.  If a
	/// valid position is found and a complete path to it can be calculated, the entity's destination  is updated to the new
	/// patrol point. The method performs up to 10 attempts to find a valid patrol point. This prevents it being stuck
	/// </remarks>
	void SetNewPatrolPoint()
	{
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

	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, darkDetectionRange);
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, lightDetectionRange);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, patrolRadius);
	}
    void OnCollisionEnter(Collision collision)
    {
		if (collision.gameObject.CompareTag("Player") ) {
			gameManager.GameOver();
        }
    }
}