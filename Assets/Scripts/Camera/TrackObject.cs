using UnityEngine;

public class TrackObject : MonoBehaviour
{
	public Transform target;

	private void FixedUpdate()
	{
		transform.LookAt(target);
	}
}