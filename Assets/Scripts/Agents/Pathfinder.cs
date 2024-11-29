using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Input = Missile.Input;
using Random = UnityEngine.Random;

namespace Agents
{
	[RequireComponent(typeof(Missile.Missile))]
	public class Pathfinder : Agent
	{
		public Transform target;
		public float safeRadius;

		private Environment _environment;
		private Input _input;
		private Missile.Missile _missile;
		private State _state;

		private enum Action
		{
			Throttle = 0,
			Pitch = 1,
			Yaw = 2,
			Roll = 3
		}

		private enum State
		{
			Running = 0,
			Reached = 1,
			Crashed = 2
		}

		protected override void Awake()
		{
			_environment = GetComponentInParent<Environment>();
			_missile = GetComponent<Missile.Missile>();
			_input = GetComponent<Input>();
		}

		private void LateUpdate()
		{
			Debug.DrawLine(transform.position, target.position, Color.blue);
		}

		private void OnCollisionEnter(Collision other) => Collision(other);
		private void OnCollisionStay(Collision other) => Collision(other);

		private void Collision(Collision other)
		{
			_state = other.gameObject.CompareTag("Target") ? State.Reached : State.Crashed;
		}

		public override void Heuristic(in ActionBuffers actionsOut)
		{
			if (_input == null) return;

			var continuousActions = actionsOut.ContinuousActions;
			continuousActions[(int)Action.Throttle] = _input.Throttle * 2f - 1f;
			continuousActions[(int)Action.Pitch] = _input.Pitch;
			continuousActions[(int)Action.Yaw] = _input.Yaw;
			continuousActions[(int)Action.Roll] = _input.Roll;
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
				case State.Crashed:
					AddReward(-100f);
					EndEpisode();
					return;
				case State.Reached:
					AddReward(+10f);
					EndEpisode();
					return;
				case State.Running:
				default:
					break;
			}

			var continuousActions = actions.ContinuousActions;
			_missile.Throttle = (continuousActions[(int)Action.Throttle] + 1f) / 2f;
			_missile.Pitch = continuousActions[(int)Action.Pitch];
			_missile.Yaw = continuousActions[(int)Action.Yaw];
			_missile.Roll = continuousActions[(int)Action.Roll];
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

				_state = State.Running;
				_missile.Place(position, rotation);
				_missile.Stop();
				return;
			}

			Debug.LogError("Failed to spawn at random position");
		}
	}
}