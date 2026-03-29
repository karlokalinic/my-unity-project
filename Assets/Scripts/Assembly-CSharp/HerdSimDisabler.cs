using UnityEngine;

public class HerdSimDisabler : MonoBehaviour
{
	public int _distanceDisable = 1000;

	public Transform _distanceFrom;

	public bool _distanceFromMainCam;

	public float _checkDisableEverSeconds = 10f;

	public float _checkEnableEverSeconds = 1f;

	public bool _disableModel;

	public bool _disableCollider;

	public bool _disableOnStart;

	public void Start()
	{
		if (_distanceFromMainCam)
		{
			_distanceFrom = Camera.main.transform;
		}
		InvokeRepeating("CheckDisable", _checkDisableEverSeconds + Random.value * _checkDisableEverSeconds, _checkDisableEverSeconds);
		InvokeRepeating("CheckEnable", _checkEnableEverSeconds + Random.value * _checkEnableEverSeconds, _checkEnableEverSeconds);
		Invoke("DisableOnStart", 0.01f);
	}

	public void DisableOnStart()
	{
		if (_disableOnStart)
		{
			base.transform.GetComponent<HerdSimCore>().Disable(_disableModel, _disableCollider);
		}
	}

	public void CheckDisable()
	{
		if (_distanceFrom != null && base.transform.GetComponent<HerdSimCore>()._enabled && (base.transform.position - _distanceFrom.position).sqrMagnitude > (float)_distanceDisable)
		{
			base.transform.GetComponent<HerdSimCore>().Disable(_disableModel, _disableCollider);
		}
	}

	public void CheckEnable()
	{
		if (_distanceFrom != null && !base.transform.GetComponent<HerdSimCore>()._enabled && (base.transform.position - _distanceFrom.position).sqrMagnitude < (float)_distanceDisable)
		{
			base.transform.GetComponent<HerdSimCore>().Enable();
		}
	}
}
