using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Rendering
{
	internal struct MaterialPermutation : IEquatable<MaterialPermutation>
	{
		public readonly WetDecalMode Mode;

		public readonly LayerMode LayerMode;

		public readonly ProjectionMode LayerProjectionMode;

		public readonly DecalShape Shape;

		public readonly bool EnableJitter;

		public int RenderOrder
		{
			get
			{
				return (Mode != WetDecalMode.Wet) ? 1 : 0;
			}
		}

		public MaterialPermutation(WetDecalMode mode, LayerMode layerMode, ProjectionMode layerProjectionMode, DecalShape shape, bool enableJitter)
		{
			Mode = mode;
			LayerMode = layerMode;
			LayerProjectionMode = layerProjectionMode;
			Shape = shape;
			EnableJitter = enableJitter;
		}

		public Shader SelectShader(Shader wet, Shader dry)
		{
			return (Mode != WetDecalMode.Wet) ? dry : wet;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is MaterialPermutation))
			{
				return false;
			}
			return Equals((MaterialPermutation)obj);
		}

		public bool Equals(MaterialPermutation other)
		{
			return EqualityComparer<WetDecalMode>.Default.Equals(Mode, other.Mode) && EqualityComparer<LayerMode>.Default.Equals(LayerMode, other.LayerMode) && EqualityComparer<ProjectionMode>.Default.Equals(LayerProjectionMode, other.LayerProjectionMode) && EqualityComparer<DecalShape>.Default.Equals(Shape, other.Shape) && EnableJitter == other.EnableJitter;
		}

		public override int GetHashCode()
		{
			int num = 409612459;
			num = num * -1521134295 + Mode.GetHashCode();
			num = num * -1521134295 + LayerMode.GetHashCode();
			num = num * -1521134295 + LayerProjectionMode.GetHashCode();
			num = num * -1521134295 + Shape.GetHashCode();
			return num * -1521134295 + EnableJitter.GetHashCode();
		}

		public static bool operator ==(MaterialPermutation permutation1, MaterialPermutation permutation2)
		{
			return permutation1.Equals(permutation2);
		}

		public static bool operator !=(MaterialPermutation permutation1, MaterialPermutation permutation2)
		{
			return !(permutation1 == permutation2);
		}
	}
}
