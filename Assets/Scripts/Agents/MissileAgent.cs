using Missile;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;

namespace Agents
{
	public class MissileAgent : Agent
	{
		public Transform target;
		public float safeRadius;

		public Vector3 LinearAcceleration { get; private set; }
		public Vector3 LinearVelocity { get; private set; }

		private LineRenderer _lineRenderer;
		private MissileInput _input;
		protected Rigidbody Rb;

		protected Environment Environment;
		protected MissileState State;
		public MissileBehaviour Missile { get; private set; }

		protected override void Awake()
		{
			_lineRenderer = GetComponent<LineRenderer>();
			_input = GetComponent<MissileInput>();
			Rb = GetComponent<Rigidbody>();

			State = MissileState.Fly;
			Environment = GetComponentInParent<Environment>();
			Missile = GetComponent<MissileBehaviour>();
		}

		protected virtual void FixedUpdate()
		{
			var velocity = Rb.linearVelocity;
			LinearAcceleration = (velocity - LinearVelocity) / Time.fixedDeltaTime;
			LinearVelocity = velocity;
		}

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

		public delegate Vector3 Trajectory(float t);

		public Trajectory GetTrajectory()
		{
			return Trajectory;
			Vector3 Trajectory(float t) => Rb.position + LinearVelocity * t + LinearAcceleration * (t * t / 2.0f);
		}

		public override void OnEpisodeBegin()
		{
			
		}

		public void Place(Vector3 position, Quaternion rotation)
		{
			State = MissileState.Fly;
			Missile.Place(position, rotation);
			Missile.Stop();
		}

		public override void Heuristic(in ActionBuffers actionsOut)
		{
			if (_input == null) return;

			var continuousActions = actionsOut.ContinuousActions;
			continuousActions[(int)MissileAction.Throttle] = _input.Throttle * 2f - 1f;
			continuousActions[(int)MissileAction.Pitch] = _input.Pitch;
			continuousActions[(int)MissileAction.Yaw] = _input.Yaw;
			continuousActions[(int)MissileAction.Roll] = _input.Roll;
		}

		public override void OnActionReceived(ActionBuffers actions)
		{
			if (State != MissileState.Fly)
			{
				OnEpisodeEnd();
				return;
			}

			var continuousActions = actions.ContinuousActions;
			Missile.Throttle = (continuousActions[(int)MissileAction.Throttle] + 1f) / 2f;
			Missile.Pitch = continuousActions[(int)MissileAction.Pitch];
			Missile.Yaw = continuousActions[(int)MissileAction.Yaw];
			Missile.Roll = continuousActions[(int)MissileAction.Roll];

			OnActionReward();
		}

		protected virtual void OnActionReward()
		{
		}

		public virtual void OnEpisodeEnd()
		{
			EndEpisode();
		}

		private void OnCollisionEnter(Collision other) => Collision(other);
		private void OnCollisionStay(Collision other) => Collision(other);

		private void Collision(Collision other)
		{
			State = StateAfterCollision(other);
		}

		protected virtual MissileState StateAfterCollision(Collision other)
		{
			return MissileState.Crash;
		}
	}
}