using UnityEngine;

public class HerdSimScary : MonoBehaviour
{
	public HerdSimCore _chase;

	public int[] _scareType;

	public bool _canChase;

	public float _scaryInterval = 0.25f;

	public LayerMask _herdLayerMask = -1;

	public void Start()
	{
		Init();
	}

	public void Init()
	{
		if (_scareType.Length > 0)
		{
			InvokeRepeating("BeScary", Random.value * _scaryInterval + 1f, _scaryInterval);
			InvokeRepeating("CheckChase", 2f, 2f);
		}
		else
		{
			Debug.Log(base.transform.name + " has nothing to scare; Please assigne ScareType");
		}
	}

	public void CheckChase()
	{
		_canChase = !_canChase;
		if (!_canChase)
		{
			_chase = null;
		}
	}

	public void BeScary()
	{
		Collider[] array = Physics.OverlapSphere(base.transform.position, 4f, _herdLayerMask);
		HerdSimCore herdSimCore = null;
		for (int i = 0; i < array.Length; i++)
		{
			Transform parent = array[i].transform.parent;
			if (parent != null)
			{
				herdSimCore = parent.GetComponent<HerdSimCore>();
			}
			if (!(herdSimCore != null))
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < _scareType.Length; j++)
			{
				if (herdSimCore._type == _scareType[j])
				{
					flag = true;
				}
			}
			if (flag)
			{
				herdSimCore.Scare(base.transform);
				if (_chase == null && _canChase)
				{
					_chase = herdSimCore;
				}
			}
		}
		if (_chase != null)
		{
			HerdSimCore component = GetComponent<HerdSimCore>();
			if (component != null)
			{
				component._waypoint = _chase.transform.position;
				component._mode = 2;
			}
		}
	}
}
