using UnityEngine;

namespace Invector
{
	public static class vExtensions
	{
		public static bool ContainsLayer(this LayerMask layermask, int layer)
		{
			return (int)layermask == ((int)layermask | (1 << layer));
		}
	}
}
