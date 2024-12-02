using Agents;
using UnityEngine;
using Random = UnityEngine.Random;

public class Environment : MonoBehaviour
{
	public float radius;

	private MissileAgent[] _agents;

	public Vector3 RandomPosition(float safeRadius) =>
		transform.position + Random.insideUnitSphere * (radius - safeRadius);

	public void Awake()
	{
		_agents = GetComponentsInChildren<MissileAgent>();
	}

	public void EndEpisode()
	{
		foreach (var agent in _agents)
		{
			agent.OnEpisodeEnd();
		}
	}
}