using System;
using JetBrains.Annotations;
using Missile;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Agents
{
	public class MissileAgent : Agent
	{
		public Transform target;
		public float safeRadius;

		private LineRenderer _lineRenderer;
		private MissileInput _input;

		protected Environment Environment;
		protected MissileBehaviour Missile;
		protected Rigidbody Body;
		[CanBeNull] protected GameObject Collision;

		private Vector3 _lastLinearVelocity;
		private Vector3 _lastAngularVelocity;

		#region Variables

		#region Orientation

		public Vector3 Position => transform.position;
		protected Vector3 Forward => transform.forward;

		#endregion

		#region Velocity

		public Vector3 LinearVelocity => Body.linearVelocity;
		public Vector3 AngularVelocity => Body.angularVelocity;

		#endregion

		#region Acceleration

		public Vector3 LinearAcceleration { get; private set; }
		public Vector3 AngularAcceleration { get; private set; }

		#endregion

		#region Target

		protected Vector3 TargetPosition => target.position;

		#endregion

		#endregion

		# region Initialization

		protected override void Awake()
		{
			_lineRenderer = GetComponent<LineRenderer>();
			_input = GetComponent<MissileInput>();

			Environment = GetComponentInParent<Environment>();
			Missile = GetComponent<MissileBehaviour>();
			Body = GetComponent<Rigidbody>();
		}

		#endregion

		#region Episode

		public override void OnEpisodeBegin()
		{
			Collision = null;
			BeginEpisode();
		}

		protected virtual void OnEpisodeEnd()
		{
			EndReward();
			EndEpisode();
		}

		private void BeginEpisode()
		{
			var c = Environment.Center;
			var r = Environment.radius - safeRadius;

			for (var i = 0; i < 100; i++)
			{
				var position = c + Random.insideUnitSphere * r;
				var rotation = Random.rotationUniform;

				var overlaps = Physics.CheckSphere(position, safeRadius);
				if (overlaps) continue;

				Missile.Place(position, rotation);
				Missile.Stop();
				return;
			}

			Debug.LogError("Failed to spawn at a random position");
		}

		#endregion

		#region Observation

		public override void CollectObservations(VectorSensor sensor)
		{
		}

		#endregion

		#region Action

		public override void Heuristic(in ActionBuffers actionsOut)
		{
			if (_input == null) return;

			var ca = actionsOut.ContinuousActions;
			ca[(int)MissileAction.Throttle] = _input.Throttle * 2f - 1f;
			ca[(int)MissileAction.Pitch] = _input.Pitch;
			ca[(int)MissileAction.Yaw] = _input.Yaw;
			ca[(int)MissileAction.Roll] = _input.Roll;
		}

		public override void OnActionReceived(ActionBuffers actions)
		{
			if (Collision != null)
			{
				OnEpisodeEnd();
				return;
			}

			var ca = actions.ContinuousActions;
			Missile.Throttle = (ca[(int)MissileAction.Throttle] + 1f) / 2f;
			Missile.Pitch = ca[(int)MissileAction.Pitch];
			Missile.Yaw = ca[(int)MissileAction.Yaw];
			Missile.Roll = ca[(int)MissileAction.Roll];

			StepReward();
		}

		#endregion

		#region Reward

		protected virtual void StepReward()
		{
			throw new NotImplementedException();
		}

		protected virtual void EndReward()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Collision

		protected void OnCollisionEnter(Collision other) => HandleCollision(other);

		protected void OnCollisionStay(Collision other) => HandleCollision(other);

		private void HandleCollision(Collision other)
		{
			Collision = other.gameObject;
		}

		#endregion

		#region Physics

		protected virtual void FixedUpdate()
		{
			var linearVelocity = LinearVelocity;
			LinearAcceleration = (linearVelocity - _lastLinearVelocity) / Time.fixedDeltaTime;
			_lastLinearVelocity = linearVelocity;

			var angularVelocity = AngularVelocity;
			AngularAcceleration = (angularVelocity - _lastAngularVelocity) / Time.fixedDeltaTime;
			_lastAngularVelocity = angularVelocity;
		}

		public Vector3 PredictedPosition(float t)
		{
			return Body.position + LinearVelocity * t + LinearAcceleration * (t * t / 2.0f);
		}

		#endregion

		#region Visualization

		protected virtual void LateUpdate()
		{
			if (!_lineRenderer || !_lineRenderer.enabled) return;

			const int n = 15;
			const float timeStep = 0.1f;

			var points = new Vector3[n];
			var i = 0;

			for (float t = 0; i < n; ++i, t += timeStep)
			{
				points[i] = PredictedPosition(t);
			}

			_lineRenderer.positionCount = n;
			_lineRenderer.SetPositions(points);
		}

		#endregion
	}
}