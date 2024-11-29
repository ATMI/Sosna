using UnityEngine;

public class ZoomObject : MonoBehaviour
{
	public Transform target;
	public float radiusFov;
	public float minFov;
	public float maxFov;

	private Camera _camera;

	private void Awake()
	{
		_camera = GetComponent<Camera>();
	}

	private void FixedUpdate()
	{
		var distance = target.position - _camera.transform.position;
		var angle = Mathf.Atan2(radiusFov, distance.magnitude) * Mathf.Rad2Deg;
		_camera.fieldOfView = Mathf.Clamp(angle, minFov, maxFov);
	}
}