using UnityEngine;
using UnityEngine.InputSystem;

namespace Missile
{
	public class MissileInput : MonoBehaviour
	{
		private void OnThrottle(InputValue value)
		{
			Throttle = value.Get<float>();
		}

		public void OnPitch(InputValue value)
		{
			Pitch = value.Get<float>();
		}

		public void OnYaw(InputValue value)
		{
			Yaw = value.Get<float>();
		}

		public void OnRoll(InputValue value)
		{
			Roll = value.Get<float>();
		}

		public float Throttle { get; private set; }

		public float Pitch { get; private set; }

		public float Yaw { get; private set; }

		public float Roll { get; private set; }
	}
}