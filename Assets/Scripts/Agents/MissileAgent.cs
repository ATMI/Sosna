using System;
using Missile;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Agents
{
	public class MissileAgent : Agent
	{
		public Transform target;
		public float safeRadius;

		// public float crashReward = -10000;
		// public float impactReward = +1000;
		// public float tickReward = -0.05f;

		private Environment _environment;
		private MissileInput _input;
		private MissileState _state;

		protected MissileBehaviour Missile;

		protected override void Awake()
		{
			_environment = GetComponentInParent<Environment>();
			_input = GetComponent<MissileInput>();
			_state = MissileState.Fly;

			Missile = GetComponent<MissileBehaviour>();
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
				Missile.Place(position, rotation);
				Missile.Stop();
				return;
			}

			Debug.LogError("Failed to spawn at a random position");
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

		public override void OnActionReceived(ActionBuffers actions)
		{
			if (_state != MissileState.Fly)
			{
				_environment.EndEpisode();
				return;
			}

			var continuousActions = actions.ContinuousActions;
			Missile.Throttle = (continuousActions[(int)MissileAction.Throttle] + 1f) / 2f;
			Missile.Pitch = continuousActions[(int)MissileAction.Pitch];
			Missile.Yaw = continuousActions[(int)MissileAction.Yaw];
			Missile.Roll = continuousActions[(int)MissileAction.Roll];

			OnActionReward();
		}

		protected virtual void OnActionReward()
		{
		}

		public void OnEpisodeEnd()
		{
			switch (_state)
			{
				case MissileState.Fly:
					AddReward(-1_000f);
					break;
				case MissileState.Crash:
					AddReward(-10_000f);
					break;
				case MissileState.Impact:
					AddReward(+1_000f);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			EndEpisode();
		}

		private void OnCollisionEnter(Collision other) => Collision(other);
		private void OnCollisionStay(Collision other) => Collision(other);

		private void Collision(Collision other)
		{
			_state = StateAfterCollision(other);
		}

		protected virtual MissileState StateAfterCollision(Collision other)
		{
			return MissileState.Crash;
		}
	}
}