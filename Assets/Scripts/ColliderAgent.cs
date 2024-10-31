using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class ColliderAgent : Agent
{
	private Rigidbody _rb;

	protected override void Awake()
	{
		_rb = GetComponent<Rigidbody>();
	}

	public override void Heuristic(in ActionBuffers actionsOut)
	{
		var actions = actionsOut.DiscreteActions;
		actions[0] = 1;
	}

	public override void CollectObservations(VectorSensor sensor)
	{
		sensor.AddObservation(_rb.linearVelocity);
	}

	public override void OnActionReceived(ActionBuffers actions)
	{
		var f = Vector3.up * Time.fixedDeltaTime;
		var t = Vector3.forward * Time.fixedDeltaTime;

		_rb.AddForce(f, ForceMode.Impulse);
		_rb.AddTorque(t, ForceMode.Impulse);
	}

	public override void OnEpisodeBegin()
	{
	}
}