using UnityEngine;

public class Environment : MonoBehaviour
{
	public float radius;

	public Vector3 RandomPosition(float safeRadius) => Random.insideUnitSphere * (radius - safeRadius);
}