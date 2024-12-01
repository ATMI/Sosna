using Missile;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Agents
{
	[RequireComponent(typeof(MissileBehaviour))]
	public class PathfinderAgent : MissileAgent
	{
		public override void CollectObservations(VectorSensor sensor)
		{
			var distance = target.position - transform.position;
			sensor.AddObservation(Missile.Forward); // 3
			sensor.AddObservation(Missile.Up); // 3
			sensor.AddObservation(Missile.Right); // 3
			sensor.AddObservation(Missile.LinearDirection); // 3
			sensor.AddObservation(Missile.LinearSpeed); // 1
			sensor.AddObservation(Missile.AngularDirection); // 3
			sensor.AddObservation(Missile.AngularSpeed); // 1
			sensor.AddObservation(distance.normalized); // 3
			sensor.AddObservation(distance.magnitude); // 1
		}

		protected override MissileState StateAfterCollision(Collision other)
		{
			return other.gameObject.CompareTag("Target")
				? MissileState.Impact
				: MissileState.Crash;
		}

		protected override void OnActionReward()
		{
			AddReward(-0.05f);
		}

		private void LateUpdate()
		{
			Debug.DrawLine(transform.position, target.position, Color.red);
		}
	}
}