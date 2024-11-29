using UnityEngine;

namespace Camera
{
	public class TrackObject : MonoBehaviour
	{
		public Transform target;

		private void FixedUpdate()
		{
			transform.LookAt(target);
		}
	}
}