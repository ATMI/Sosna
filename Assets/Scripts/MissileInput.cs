using UnityEngine;
using UnityEngine.InputSystem;

public class MissileInput : MonoBehaviour
{
	private float _throttle;
	private float _pitch;
	private float _yaw;
	private float _roll;

	private void OnThrottle(InputValue value)
	{
		_throttle = value.Get<float>();
	}

	public void OnPitch(InputValue value)
	{
		_pitch = value.Get<float>();
	}

	public void OnYaw(InputValue value)
	{
		_yaw = value.Get<float>();
	}

	public void OnRoll(InputValue value)
	{
		_roll = value.Get<float>();
	}

	public float Throttle => _throttle;
	public float Pitch => _pitch;
	public float Yaw => _yaw;
	
	public int DiscreteThrottle => Mathf.RoundToInt(_throttle);
	public int DiscretePitch => Mathf.RoundToInt(_pitch);
	public int DiscreteYaw => Mathf.RoundToInt(_yaw);
	public int DiscreteRoll => Mathf.RoundToInt(_roll);
}