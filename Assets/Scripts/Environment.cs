using System.Linq;
using Agents;
using UnityEngine;
using Random = UnityEngine.Random;

public class Environment : MonoBehaviour
{
	public float radius;
	public int warmup;

	private int _episodeNum;
	private bool _episode;

	private MissileAgent[] _agents;
	private MissileAgent[] _colliders;
	private MissileAgent[] _pathfinders;

	public void Awake()
	{
		_agents = GetComponentsInChildren<MissileAgent>();
		_pathfinders = _agents.Where(agent => agent is PathfinderAgent).ToArray();
		_colliders = _agents.Where(agent => agent is ColliderAgent).ToArray();
	}

	private void SpawnMissile(MissileAgent missile, Vector3 center, float spawnRadius)
	{
		var safeRadius = missile.safeRadius;
		var c = transform.position + center;
		var r = Mathf.Max(2 * safeRadius,
			Mathf.Min(spawnRadius, radius) - Vector3.Distance(transform.position, center));

		for (var i = 0; i < 100; i++)
		{
			var position = c + Random.insideUnitSphere * r;
			var rotation = Random.rotationUniform;

			var overlaps = Physics.CheckSphere(position, safeRadius);
			if (overlaps) continue;

			missile.Place(position, rotation);
			return;
		}

		Debug.LogError("Failed to spawn at a random position");
	}

	public void SpawnPathfinders()
	{
		foreach (var missile in _pathfinders)
		{
			SpawnMissile(missile, Vector3.zero, radius);
		}
	}

	public void SpawnColliders()
	{
		foreach (var missile in _colliders)
		{
			var i = Random.Range(0, _pathfinders.Length);
			var pathfinder = _pathfinders[i];

			var c = pathfinder.transform.position - transform.position;
			var r = _episodeNum < warmup ? radius * _episodeNum / warmup : radius;

			SpawnMissile(missile, c, r);
		}
	}
}