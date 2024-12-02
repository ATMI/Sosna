using Missile;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Agents
{
	[RequireComponent(typeof(MissileBehaviour))]
	public class ColliderAgent : MissileAgent
	{
		private Rigidbody _missileBody;
		private Rigidbody _targetBody;

		protected override void Awake()
		{
			base.Awake();
			_missileBody = GetComponent<Rigidbody>();
			_targetBody = target.GetComponent<Rigidbody>();
		}

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

			sensor.AddObservation(_targetBody.linearVelocity.normalized); // 3
			sensor.AddObservation(_targetBody.linearVelocity.magnitude); // 1
			sensor.AddObservation(_targetBody.angularVelocity.normalized); // 3
			sensor.AddObservation(_targetBody.angularVelocity.magnitude); // 1
		}

		public override void OnEpisodeBegin()
		{
			base.OnEpisodeBegin();
			// some stuff that happens when episode begins
		}

		protected override void OnActionReward()
		{
			// calculating the reward after some action has happened
			var reward = -100;
			AddReward(reward);
		}

		protected override MissileState StateAfterCollision(Collision other)
		{
			return other.gameObject.CompareTag("Pathfinder")
				? MissileState.Impact
				: MissileState.Crash;
		}

		private void LateUpdate()
		{
			Debug.DrawLine(transform.position, target.position, Color.blue);
		}
	}
}