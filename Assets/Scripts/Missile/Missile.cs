using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Missile : MonoBehaviour
{
	public float maxThrust;
	public float pitchForce;
	public float yawForce;
	public float rollForce;

	private Rigidbody _rb;
	private float _throttle;
	private float _pitch;
	private float _roll;
	private float _yaw;

	public float Throttle
	{
		get => _throttle;
		set => _throttle = Mathf.Clamp(value, 0, 1);
	}

	public float Pitch
	{
		get => _pitch;
		set => _pitch = Mathf.Clamp(value, -1f, 1f);
	}

	public float Yaw
	{
		get => _yaw;
		set => _yaw = Mathf.Clamp(value, -1f, 1f);
	}

	public float Roll
	{
		get => _roll;
		set => _roll = Mathf.Clamp(value, -1f, 1f);
	}

	public Vector3 Forward => transform.forward;
	public Vector3 Right => transform.right;
	public Vector3 Up => transform.up;
	public Vector3 LinearDirection => _rb.linearVelocity.normalized;
	public float LinearSpeed => _rb.linearVelocity.magnitude;
	public Vector3 AngularDirection => _rb.angularVelocity.normalized;
	public float AngularSpeed => _rb.angularVelocity.magnitude;

	public void Stop()
	{
		_throttle = 0;
		_pitch = _yaw = _roll = 0;

		_rb.linearVelocity = Vector3.zero;
		_rb.angularVelocity = Vector3.zero;
	}

	public void Place(Vector3 position, Quaternion rotation)
	{
		_rb.position = position;
		_rb.rotation = rotation;
	}

	private void Awake()
	{
		_rb = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		var torqueDirection = new Vector3(_pitch, _yaw, _roll);
		var torqueMagnitude = new Vector3(pitchForce, yawForce, rollForce);
		var torqueImpulse = Vector3.Scale(torqueDirection, torqueMagnitude) * Time.fixedDeltaTime;
		_rb.AddRelativeTorque(torqueImpulse, ForceMode.Impulse);

		var thrustDirection = Vector3.forward;
		var thrustMagnitude = _throttle * maxThrust;
		var thrustImpulse = thrustDirection * (thrustMagnitude * Time.fixedDeltaTime);
		_rb.AddRelativeForce(thrustImpulse, ForceMode.Impulse);
	}
}