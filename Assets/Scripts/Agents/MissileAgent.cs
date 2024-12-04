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
		protected MissileState State;
		protected Rigidbody Body;
		[CanBeNull] protected Collision Collision;

		private Vector3 _lastLinearVelocity;
		private Vector3 _lastAngularVelocity;

		#region Variables

		#region Orientation

		protected Vector3 Position => transform.position;
		protected Vector3 Forward => transform.forward;
		protected Vector3 Right => transform.right;
		protected Vector3 Up => transform.up;

		#endregion

		#region Linear velocity

		public Vector3 LinearVelocity => Body.linearVelocity;
		protected Vector3 LinearVelocityDirection => LinearVelocity.normalized;
		protected float LinearVelocityMagnitude => LinearVelocity.magnitude;

		#endregion

		#region Linear acceleration

		public Vector3 LinearAcceleration { get; private set; }
		protected Vector3 LinearAccelerationDirection => LinearAcceleration.normalized;
		protected float LinearAccelerationMagnitude => LinearAcceleration.magnitude;

		#endregion

		#region Angular acceleration

		public Vector3 AngularAcceleration { get; private set; }
		protected Vector3 AngularAccelerationDirection => AngularAcceleration.normalized;
		protected float AngularAccelerationMagnitude => AngularAcceleration.magnitude;

		#endregion

		#region Angular velocity

		protected Vector3 AngularVelocity => Body.angularVelocity;
		protected Vector3 AngularVelocityDirection => AngularVelocity.normalized;
		protected float AngularVelocityMagnitude => AngularVelocity.magnitude;

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
			State = MissileState.Fly;
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
			var c = Environment.transform.position;
			var r = Environment.radius;

			for (var i = 0; i < 100; i++)
			{
				var position = c + Random.insideUnitSphere * r;
				var rotation = Random.rotationUniform;

				var overlaps = Physics.CheckSphere(position, safeRadius);
				if (overlaps) continue;

				State = MissileState.Fly;
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
			if (State != MissileState.Fly)
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
			if (Collision != null) return;
			Collision = other;
			OnEpisodeEnd();
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

		public delegate Vector3 Trajectory(float t);

		public Trajectory GetTrajectory()
		{
			return Trajectory;
			Vector3 Trajectory(float t) => Body.position + LinearVelocity * t + LinearAcceleration * (t * t / 2.0f);
		}

		#endregion

		#region Visualization

		protected virtual void LateUpdate()
		{
			if (!_lineRenderer || !_lineRenderer.enabled) return;

			const int n = 15;
			const float timeStep = 0.1f;

			var points = new Vector3[n];
			var trajectory = GetTrajectory();

			var i = 0;
			for (float t = 0; i < n; ++i, t += timeStep)
			{
				points[i] = trajectory(t);
			}

			_lineRenderer.positionCount = n;
			_lineRenderer.SetPositions(points);
		}

		#endregion
	}
}