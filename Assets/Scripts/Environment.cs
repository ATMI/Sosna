using UnityEngine;

public class Environment : MonoBehaviour
{
	public float radius;

	public Vector3 RandomPosition(float safeRadius) =>
		transform.position + Random.insideUnitSphere * (radius - safeRadius);
}