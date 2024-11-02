using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Missile))]
public class PathfinderAgent : Agent
{
	public Transform target;
	public float safeRadius;

	private Environment _environment;
	private MissileInput _input;
	private Missile _missile;

	private enum Action
	{
		Throttle = 0,
		Pitch = 1,
		Yaw = 2,
		Roll = 3
	}

	protected override void Awake()
	{
		_environment = GetComponentInParent<Environment>();
		_missile = GetComponent<Missile>();
		_input = GetComponent<MissileInput>();
	}

	private void LateUpdate()
	{
		Debug.DrawLine(transform.position, target.position, Color.blue);
	}

	private void OnCollisionEnter(Collision other)
	{
		var reward = other.gameObject.CompareTag("Target") ? 1f : -1f;
		AddReward(reward);
		EndEpisode();
	}

	public override void Heuristic(in ActionBuffers actionsOut)
	{
		if (_input == null) return;

		var discreteActions = actionsOut.DiscreteActions;
		discreteActions[(int)Action.Throttle] = _input.DiscreteThrottle;
		discreteActions[(int)Action.Pitch] = _input.DiscretePitch;
		discreteActions[(int)Action.Yaw] = _input.DiscreteYaw;
		discreteActions[(int)Action.Roll] = _input.DiscreteRoll;
	}

	public override void CollectObservations(VectorSensor sensor)
	{
		var direction = target.position - transform.position;
		sensor.AddObservation(_missile.Position / _environment.radius);
		sensor.AddObservation(_missile.Rotation.eulerAngles);
		sensor.AddObservation(_missile.LinearVelocity);
		sensor.AddObservation(_missile.AngularVelocity);
		sensor.AddObservation(direction.normalized);
		sensor.AddObservation(direction.magnitude);
	}

	public override void OnActionReceived(ActionBuffers actions)
	{
		var discreteActions = actions.DiscreteActions;
		_missile.Throttle = discreteActions[(int)Action.Throttle];
		_missile.Pitch = discreteActions[(int)Action.Pitch];
		_missile.Yaw = discreteActions[(int)Action.Yaw];
		_missile.Roll = discreteActions[(int)Action.Roll];
	}

	public override void OnEpisodeBegin()
	{
		for (var i = 0; i < 100; i++)
		{
			var position = _environment.RandomPosition(safeRadius);
			var rotation = Random.rotationUniform;

			var overlaps = Physics.CheckSphere(position, safeRadius);
			if (overlaps) continue;

			_missile.Place(position, rotation);
			_missile.Stop();
			return;
		}
		Debug.LogError("Failed to spawn at random position");
	}
}