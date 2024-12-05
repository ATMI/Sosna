using UnityEngine;

public class Environment : MonoBehaviour
{
	public float radius;
	public Vector3 Center => transform.position;

	public bool IsOutside(Vector3 point)
	{
		return Vector3.Distance(Center, point) > radius;
	}
}