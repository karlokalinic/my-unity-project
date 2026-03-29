using UnityEngine;

public class HerdSimController : MonoBehaviour
{
	public Vector3 _roamingArea;

	public ParticleSystem _runPS;

	public ParticleSystem _deadPS;

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(base.transform.position, _roamingArea * 2f);
	}
}
