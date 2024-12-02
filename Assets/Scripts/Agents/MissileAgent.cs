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

		protected MissileState State;
		protected MissileBehaviour Missile;

		protected override void Awake()
		{
			_environment = GetComponentInParent<Environment>();
			_input = GetComponent<MissileInput>();

			State = MissileState.Fly;
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

				State = MissileState.Fly;
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
			if (State != MissileState.Fly)
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

		public virtual void OnEpisodeEnd()
		{
			EndEpisode();
		}

		private void OnCollisionEnter(Collision other) => Collision(other);
		private void OnCollisionStay(Collision other) => Collision(other);

		private void Collision(Collision other)
		{
			State = StateAfterCollision(other);
		}

		protected virtual MissileState StateAfterCollision(Collision other)
		{
			return MissileState.Crash;
		}
	}
}