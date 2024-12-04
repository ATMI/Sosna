using System;
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
			var direction = target.position - transform.position;
			var distance = direction.magnitude;
			var projection = Vector3.Dot(Rb.linearVelocity, direction) / distance;
			AddReward(projection + 1 / distance);
		}

		public override void OnEpisodeEnd()
		{
			switch (State)
			{
				case MissileState.Fly:
					var distance = Vector3.Distance(transform.position, target.position);
					AddReward(-distance);
					break;
				case MissileState.Crash:
					AddReward(-1_000f);
					break;
				case MissileState.Impact:
					AddReward(1_000f);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			EndEpisode();
			Environment.SpawnPathfinders();
		}

		protected override void LateUpdate()
		{
			base.LateUpdate();
			Debug.DrawLine(transform.position, target.position, Color.red);
		}
	}
}