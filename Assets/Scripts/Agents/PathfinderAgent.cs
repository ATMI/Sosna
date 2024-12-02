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
			AddReward(-0.05f);
		}
		
		public override void OnEpisodeEnd()
		{
			switch (State)
			{
				case MissileState.Fly:
					break;
				case MissileState.Impact:
					AddReward(100f);
					break;
				case MissileState.Crash:
					AddReward(-1_000f);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			EndEpisode();
		}

		private void LateUpdate()
		{
			Debug.DrawLine(transform.position, target.position, Color.red);
		}
	}
}