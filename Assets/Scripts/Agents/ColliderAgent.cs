using System;
using System.Linq;
using Missile;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Agents
{
	[RequireComponent(typeof(MissileBehaviour))]
	public class ColliderAgent : MissileAgent
	{
		private Rigidbody _targetRb;
		private MissileAgent _target;

		private float _rendezvousT;
		private Vector3 _rendezvousMissile;
		private Vector3 _rendezvousTarget;

		protected override void Awake()
		{
			base.Awake();
			Rb = GetComponent<Rigidbody>();
			_targetRb = target.GetComponent<Rigidbody>();
			_target = target.GetComponent<MissileAgent>();
		}

		protected override void FixedUpdate()
		{
			base.FixedUpdate();

			var missileTrajectory = GetTrajectory();
			var targetTrajectory = _target.GetTrajectory();

			var missilePosition = Rb.position;
			var targetPosition = _targetRb.position;

			var deltaP = targetPosition - missilePosition;
			var deltaV = _target.LinearVelocity - LinearAcceleration;
			var deltaA = _target.LinearAcceleration - LinearAcceleration;

			var a = Vector3.Dot(deltaA, deltaA) / 2.0f;
			var b = Vector3.Dot(deltaA, deltaV) * 3.0f / 2.0f;
			var c = Vector3.Dot(deltaA, deltaP) + Vector3.Dot(deltaV, deltaV);
			var d = Vector3.Dot(deltaP, deltaV);

			var d2 = b / a;
			var d1 = c / a;
			var d0 = d / a;

			var q = d1 / 3 - d2 * d2 / 9;
			var p = (d1 * d2 - 3 * d0) / 6.0f - d2 * d2 * d2 / 27;

			var cc = p * p + q * q * q;
			if (cc > 0)
			{
				var aa = Math.Cbrt(Mathf.Abs(p) + Mathf.Sqrt(p * p + q * q * q));
				var t1 = p < 0 ? q / aa - aa : aa - q / aa;

				_rendezvousT = (float)(t1 - d2 / 3);
			}
			else
			{
				var h = Mathf.Abs(q) < 1e-5 ? 0.0 : Math.Acos(p / Math.Cbrt(-q * q * q));
				var h1 = (float)h / 3;
				var h2 = h1 - 2 * Mathf.PI / 3;
				var h3 = h1 + 2 * Mathf.PI / 3;

				var n = 2 * Mathf.Sqrt(-q);
				var m = d2 / 3;

				var x = new[]
				{
					n * Mathf.Cos(h1) - m,
					n * Mathf.Cos(h2) - m,
					n * Mathf.Cos(h3) - m
				};

				_rendezvousT = x
					.Where(t => t > 0)
					.DefaultIfEmpty(-1)
					.Min(t => Vector3.Distance(GetTrajectory()(t), _target.GetTrajectory()(t)));
			}

			if (_rendezvousT < 1e-5)
			{
				_rendezvousT = -1;
				return;
			}

			_rendezvousMissile = missileTrajectory(_rendezvousT);
			_rendezvousTarget = targetTrajectory(_rendezvousT);

			var center = Environment.transform.position;
			if (Vector3.Distance(_rendezvousMissile, center) > Environment.radius ||
			    Vector3.Distance(_rendezvousTarget, center) > Environment.radius)
			{
				_rendezvousT = -1;
			}
		}

		public override void CollectObservations(VectorSensor sensor)
		{
			var distance = target.position - transform.position;

			sensor.AddObservation(Missile.Forward); // 3
			sensor.AddObservation(Missile.Up); // 3
			sensor.AddObservation(Missile.Right); // 3
			sensor.AddObservation(Missile.LinearDirection); // 3
			sensor.AddObservation(Missile.LinearSpeed); // 1
			sensor.AddObservation(LinearAcceleration.normalized); // 3
			sensor.AddObservation(LinearAcceleration.magnitude); // 1
			sensor.AddObservation(Missile.AngularDirection); // 3
			sensor.AddObservation(Missile.AngularSpeed); // 1

			sensor.AddObservation(distance.normalized); // 3
			sensor.AddObservation(distance.magnitude); // 1

			var tm = _target.Missile;
			sensor.AddObservation(tm.Forward); // 3
			sensor.AddObservation(tm.Up); // 3
			sensor.AddObservation(tm.Right); // 3
			sensor.AddObservation(tm.LinearDirection); // 3
			sensor.AddObservation(tm.LinearSpeed); // 1
			sensor.AddObservation(_target.LinearAcceleration.normalized); // 3
			sensor.AddObservation(_target.LinearAcceleration.magnitude); // 1
			sensor.AddObservation(tm.AngularDirection); // 3
			sensor.AddObservation(tm.AngularSpeed); // 1
		}

		protected override void OnActionReward()
		{
			var distance = Vector3.Distance(_targetRb.position, Rb.position);
			var reward = _rendezvousT < 0
				? -distance
				: 1 / Vector3.Distance(_rendezvousMissile, _rendezvousTarget);
			AddReward(reward);
		}

		public override void OnEpisodeBegin()
		{
			base.OnEpisodeBegin();
			_rendezvousT = -1;
		}

		public override void OnEpisodeEnd()
		{
			switch (State)
			{
				case MissileState.Fly:
					var distance = Vector3.Distance(Rb.position, target.position);
					AddReward(-distance);
					break;
				case MissileState.Crash:
					AddReward(-10_000f);
					break;
				case MissileState.Impact:
					Debug.Log("Kaboom");
					AddReward(+10_000f);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			EndEpisode();
			Environment.SpawnColliders();
		}

		protected override MissileState StateAfterCollision(Collision other)
		{
			return other.gameObject.CompareTag("Pathfinder")
				? MissileState.Impact
				: MissileState.Crash;
		}

		protected void OnDrawGizmos()
		{
			if (_rendezvousT < 1e-5) return;
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(_rendezvousTarget, 1f);
			Gizmos.DrawSphere(_rendezvousMissile, 1f);
		}
	}
}