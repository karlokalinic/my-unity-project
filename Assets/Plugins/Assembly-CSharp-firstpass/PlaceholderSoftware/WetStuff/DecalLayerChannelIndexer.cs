using System;

namespace PlaceholderSoftware.WetStuff
{
	public struct DecalLayerChannelIndexer
	{
		private readonly DecalLayer _layer;

		public DecalLayerChannel this[int index]
		{
			get
			{
				switch (index)
				{
				case 1:
					return _layer.Channel1;
				case 2:
					return _layer.Channel2;
				case 3:
					return _layer.Channel3;
				case 4:
					return _layer.Channel4;
				default:
					throw new IndexOutOfRangeException("Channel index must be 1, 2, 3, or 4");
				}
			}
		}

		public DecalLayerChannelIndexer(DecalLayer layer)
		{
			_layer = layer;
		}
	}
}
