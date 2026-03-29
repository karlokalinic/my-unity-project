using UnityEngine;

public class Wormhole : MonoBehaviour
{
	public float tunnelTextureTwist = -0.1f;

	public float tunnelTextureSpeed = -1f;

	public float tunnelMeshAnimSpeed = 1f;

	public GameObject tunnel;

	private Renderer _myRenderer;

	private bool scroll = true;

	private void Start()
	{
		_myRenderer = GetComponent<Renderer>();
		if (_myRenderer == null)
		{
			base.enabled = false;
		}
		Animator component = tunnel.GetComponent<Animator>();
		component.speed = tunnelMeshAnimSpeed;
	}

	public void FixedUpdate()
	{
		if (scroll)
		{
			float y = Time.time * tunnelTextureSpeed;
			float x = Time.time * tunnelTextureTwist;
			_myRenderer.material.mainTextureOffset = new Vector2(x, y);
		}
	}

	public void DoActivateTrigger()
	{
		scroll = !scroll;
	}
}
