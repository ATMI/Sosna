using Missile;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Agents
{
	[RequireComponent(typeof(MissileBehaviour))]
	public class ColliderAgent : MissileAgent
	{
		private MissileAgent _target;

		protected override void Awake()
		{
			base.Awake();
			_target = target.GetComponent<MissileAgent>();
		}

		#region Observation

		public override void CollectObservations(VectorSensor sensor)
		{
			var observations = new[]
			{
				TargetPosition - Position,

				LinearVelocity,
				LinearAcceleration,

				AngularVelocity,
				AngularAcceleration,

				_target.LinearVelocity,
				_target.LinearAcceleration,

				_target.AngularVelocity,
				_target.AngularAcceleration
			};

			transform.InverseTransformDirections(observations);

			foreach (var observation in observations)
			{
				sensor.AddObservation(observation.normalized); // 3
				sensor.AddObservation(observation.magnitude); // 1
			}
		}

		#endregion

		#region Reward

		protected override void StepReward()
		{
			// var direction = TargetPosition - Position;
			// var delta = Position - _prevPosition;
			// _prevPosition = Position;

			// var distance = direction.magnitude;
			// var projection = Vector3.Dot(delta, direction) / distance;

			// AddReward(100 * projection - 0.1f);

			var distance = Vector3.Distance(Position, TargetPosition);
			var reward = -distance / Environment.radius;

			AddReward(reward);
		}

		protected override void EndReward()
		{
			switch (Collision?.gameObject?.tag)
			{
				case "Pathfinder":
					AddReward(+10_000);
					break;
				case "Boundary":
					AddReward(-10_000);
					break;
			}
		}

		#endregion
	}
}