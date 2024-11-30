using Missile;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Agents
{
	[RequireComponent(typeof(MissileBehaviour))]
	public class ColliderAgent : MissileAgent
	{
		public override void CollectObservations(VectorSensor sensor)
		{
			var distance = target.position - transform.position;
			var rb = target.GetComponent<Rigidbody>();

			sensor.AddObservation(Missile.Forward); // 3
			sensor.AddObservation(Missile.Up); // 3
			sensor.AddObservation(Missile.Right); // 3
			sensor.AddObservation(Missile.LinearDirection); // 3
			sensor.AddObservation(Missile.LinearSpeed); // 1
			sensor.AddObservation(Missile.AngularDirection); // 3
			sensor.AddObservation(Missile.AngularSpeed); // 1
			
			sensor.AddObservation(distance.normalized); // 3
			sensor.AddObservation(distance.magnitude); // 1
			
			sensor.AddObservation(rb.linearVelocity.normalized); // 3
			sensor.AddObservation(rb.linearVelocity.magnitude); // 1
			sensor.AddObservation(rb.angularVelocity.normalized); // 3
			sensor.AddObservation(rb.angularVelocity.magnitude); // 1
		}

		protected override MissileState StateAfterCollision(Collision other)
		{
			return other.gameObject.CompareTag("Pathfinder")
				? MissileState.Impact
				: MissileState.Crash;
		}

		private void LateUpdate()
		{
			Debug.DrawLine(transform.position, target.position, Color.red);
		}
	}
}