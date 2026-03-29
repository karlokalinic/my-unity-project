using System;

namespace AC
{
	[Serializable]
	public abstract class Remember : ConstantID
	{
		protected bool savePrevented;

		public bool SavePrevented
		{
			get
			{
				return savePrevented;
			}
			set
			{
				savePrevented = value;
			}
		}
	}
}
