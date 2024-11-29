using UnityEngine;
using UnityEngine.InputSystem;

namespace Missile
{
	[RequireComponent(typeof(Missile))]
	public class Controls : MonoBehaviour
	{
		private Missile _missile;

		private void Awake()
		{
			_missile = GetComponent<Missile>();
		}

		private void OnThrottle(InputValue value)
		{
			_missile.Throttle = value.Get<float>();
		}

		public void OnPitch(InputValue value)
		{
			_missile.Pitch = value.Get<float>();
		}

		public void OnYaw(InputValue value)
		{
			_missile.Yaw = value.Get<float>();
		}

		public void OnRoll(InputValue value)
		{
			_missile.Roll = value.Get<float>();
		}
	}
}