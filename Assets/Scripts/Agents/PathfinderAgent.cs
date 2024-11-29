using Missile;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Agents
{
	[RequireComponent(typeof(MissileAgent))]
	public class PathfinderAgent : Agent
	{
		public Transform target;
		public float safeRadius;

		private Environment _environment;
		private MissileInput _input;
		private MissileBehaviour _missile;
		private MissileState _state;

		protected override void Awake()
		{
			_environment = GetComponentInParent<Environment>();
			_missile = GetComponent<MissileBehaviour>();
			_input = GetComponent<MissileInput>();
		}

		private void LateUpdate()
		{
			Debug.DrawLine(transform.position, target.position, Color.blue);
		}

		private void OnCollisionEnter(Collision other) => Collision(other);
		private void OnCollisionStay(Collision other) => Collision(other);

		private void Collision(Collision other)
		{
			_state = other.gameObject.CompareTag("Target") ? MissileState.Impact : MissileState.Crash;
		}

		public override void Heuristic(in ActionBuffers actionsOut)
		{
			if (_input == null) return;

			var continuousActions = actionsOut.ContinuousActions;
			continuousActions[(int)MissileAction.Throttle] = _input.Throttle * 2f - 1f;
			continuousActions[(int)MissileAction.Pitch] = _input.Pitch;
			continuousActions[(int)MissileAction.Yaw] = _input.Yaw;
			continuousActions[(int)MissileAction.Roll] = _input.Roll;
		}

		public override void CollectObservations(VectorSensor sensor)
		{
			var distance = target.position - transform.position;
			sensor.AddObservation(_missile.Forward); // 3
			sensor.AddObservation(_missile.Up); // 3
			sensor.AddObservation(_missile.Right); // 3
			sensor.AddObservation(_missile.LinearDirection); // 3
			sensor.AddObservation(_missile.LinearSpeed); // 1
			sensor.AddObservation(_missile.AngularDirection); // 3
			sensor.AddObservation(_missile.AngularSpeed); // 1
			sensor.AddObservation(distance.normalized); // 3
			sensor.AddObservation(distance.magnitude); // 1
		}

		public override void OnActionReceived(ActionBuffers actions)
		{
			switch (_state)
			{
				case MissileState.Crash:
					AddReward(-100f);
					EndEpisode();
					return;
				case MissileState.Impact:
					AddReward(+10f);
					EndEpisode();
					return;
				case MissileState.Fly:
				default:
					break;
			}

			var continuousActions = actions.ContinuousActions;
			_missile.Throttle = (continuousActions[(int)MissileAction.Throttle] + 1f) / 2f;
			_missile.Pitch = continuousActions[(int)MissileAction.Pitch];
			_missile.Yaw = continuousActions[(int)MissileAction.Yaw];
			_missile.Roll = continuousActions[(int)MissileAction.Roll];
			AddReward(-0.05f);
		}

		public override void OnEpisodeBegin()
		{
			for (var i = 0; i < 100; i++)
			{
				var position = _environment.RandomPosition(safeRadius);
				var rotation = Random.rotationUniform;

				var overlaps = Physics.CheckSphere(position, safeRadius);
				if (overlaps) continue;

				_state = MissileState.Fly;
				_missile.Place(position, rotation);
				_missile.Stop();
				return;
			}

			Debug.LogError("Failed to spawn at random position");
		}
	}
}