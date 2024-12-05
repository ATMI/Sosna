using Missile;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Agents
{
	[RequireComponent(typeof(MissileBehaviour))]
	public class PathfinderAgent : MissileAgent
	{
		// private Vector3 _prevPosition;

		#region Observation

		public override void CollectObservations(VectorSensor sensor)
		{
			var localLinearVelocity = transform.InverseTransformDirection(LinearVelocity);
			var localAngularVelocity = transform.InverseTransformDirection(AngularVelocity);
			var localLinearAcceleration = transform.InverseTransformDirection(LinearAcceleration);
			var localAngularAcceleration = transform.InverseTransformDirection(AngularAcceleration);
			var localDirection = transform.InverseTransformDirection(TargetPosition - Position);

			// sensor.AddObservation(Forward); // 3
			// sensor.AddObservation(Up); // 3
			// sensor.AddObservation(Right); // 3
			sensor.AddObservation(localLinearVelocity.normalized); // 3
			sensor.AddObservation(localLinearVelocity.magnitude); // 1
			sensor.AddObservation(localLinearAcceleration.normalized); // 3
			sensor.AddObservation(localLinearAcceleration.magnitude); // 1
			sensor.AddObservation(localAngularVelocity.normalized); // 3
			sensor.AddObservation(localAngularVelocity.magnitude); // 1
			sensor.AddObservation(localAngularAcceleration.normalized); // 3
			sensor.AddObservation(localAngularVelocity.magnitude); // 1
			sensor.AddObservation(localDirection.normalized); // 3
			sensor.AddObservation(localDirection.magnitude); // 1
		}

		#endregion

		#region Reward

		// public override void OnEpisodeBegin()
		// {
			// base.OnEpisodeBegin();
			// _prevPosition = transform.position;
		// }

		protected override void StepReward()
		{
			// var direction = TargetPosition - Position;
			// var delta = Position - _prevPosition;
			// _prevPosition = Position;

			// var distance = direction.magnitude;
			// var projection = Vector3.Dot(delta, direction) / distance;

			// AddReward(100 * projection - 0.1f);

			var direction = TargetPosition - Position;
			var distance = direction.magnitude;
			
			var heading = Vector3.Dot(Forward, direction) / distance;
			var reward = -distance / Environment.radius + heading / 5.0f;

			AddReward(reward);
		}

		protected override void EndReward()
		{
			switch (Collision?.tag)
			{
				case "Target":
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