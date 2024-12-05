using System;
using System.Linq;
using Missile;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Agents
{
	[RequireComponent(typeof(MissileBehaviour))]
	public class ColliderAgent : MissileAgent
	{
		private MissileAgent _target;

		private float _rendezvousT;
		private Vector3 _rendezvous;
		private Vector3 _rendezvousTarget;

		#region Initialization

		protected override void Awake()
		{
			base.Awake();
			_target = target.GetComponent<MissileAgent>();
		}

		#endregion

		#region Episode

		public override void OnEpisodeBegin()
		{
			base.OnEpisodeBegin();
			_rendezvousT = -1;
		}

		#endregion

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
			var reward = distance < 2
				? -distance / Environment.radius
				: _rendezvousT > 0
					? -Vector3.Distance(_rendezvous, _rendezvousTarget) / Environment.radius
					: -1f;

			AddReward(reward);
		}

		protected override void EndReward()
		{
			switch (Collision?.tag)
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

		#region Physics

		protected override void FixedUpdate()
		{
			base.FixedUpdate();

			var deltaP = _target.Position - Position;
			var deltaV = _target.LinearVelocity - LinearVelocity;
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
					.Min();
			}

			if (_rendezvousT < 0)
			{
				return;
			}

			_rendezvous = PredictedPosition(_rendezvousT);
			_rendezvousTarget = _target.PredictedPosition(_rendezvousT);

			if (Environment.IsOutside(_rendezvous) || Environment.IsOutside(_rendezvousTarget))
			{
				_rendezvousT = -1;
			}
		}

		#endregion

		#region Visualization

		protected void OnDrawGizmos()
		{
			if (_rendezvousT < 0) return;

			Gizmos.color = Color.green;
			Gizmos.DrawSphere(_rendezvous, 0.5f);
			Gizmos.DrawSphere(_rendezvousTarget, 0.5f);
		}

		#endregion
	}
}