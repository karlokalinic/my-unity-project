using System;
using UnityEngine;

namespace VLB
{
	public static class MeshGenerator
	{
		private const float kMinTruncatedRadius = 0.001f;

		public static Mesh GenerateConeZ_RadiusAndAngle(float lengthZ, float radiusStart, float coneAngle, int numSides, int numSegments, bool cap)
		{
			float radiusEnd = lengthZ * Mathf.Tan(coneAngle * ((float)Math.PI / 180f) * 0.5f);
			return GenerateConeZ_Radius(lengthZ, radiusStart, radiusEnd, numSides, numSegments, cap);
		}

		public static Mesh GenerateConeZ_Angle(float lengthZ, float coneAngle, int numSides, int numSegments, bool cap)
		{
			return GenerateConeZ_RadiusAndAngle(lengthZ, 0f, coneAngle, numSides, numSegments, cap);
		}

		public static Mesh GenerateConeZ_Radius(float lengthZ, float radiusStart, float radiusEnd, int numSides, int numSegments, bool cap)
		{
			Mesh mesh = new Mesh();
			bool flag = false;
			flag = cap && radiusStart > 0f;
			radiusStart = Mathf.Max(radiusStart, 0.001f);
			int num = numSides * (numSegments + 2);
			int num2 = num;
			if (flag)
			{
				num2 += numSides + 1;
			}
			Vector3[] array = new Vector3[num2];
			for (int i = 0; i < numSides; i++)
			{
				float f = (float)Math.PI * 2f * (float)i / (float)numSides;
				float num3 = Mathf.Cos(f);
				float num4 = Mathf.Sin(f);
				for (int j = 0; j < numSegments + 2; j++)
				{
					float num5 = (float)j / (float)(numSegments + 1);
					float num6 = Mathf.Lerp(radiusStart, radiusEnd, num5);
					array[i + j * numSides] = new Vector3(num6 * num3, num6 * num4, num5 * lengthZ);
				}
			}
			if (flag)
			{
				int num7 = num;
				array[num7] = Vector3.zero;
				num7++;
				for (int k = 0; k < numSides; k++)
				{
					float f2 = (float)Math.PI * 2f * (float)k / (float)numSides;
					float num8 = Mathf.Cos(f2);
					float num9 = Mathf.Sin(f2);
					array[num7] = new Vector3(radiusStart * num8, radiusStart * num9, 0f);
					num7++;
				}
			}
			mesh.vertices = array;
			Vector2[] array2 = new Vector2[num2];
			int num10 = 0;
			for (int l = 0; l < num; l++)
			{
				array2[num10++] = Vector2.zero;
			}
			if (flag)
			{
				for (int m = 0; m < numSides + 1; m++)
				{
					array2[num10++] = Vector2.one;
				}
			}
			mesh.uv = array2;
			int num11 = numSides * 2 * Mathf.Max(numSegments + 1, 1);
			int num12 = num11 * 3;
			int num13 = num12;
			if (flag)
			{
				num13 += numSides * 3;
			}
			int[] array3 = new int[num13];
			int num14 = 0;
			for (int n = 0; n < numSides; n++)
			{
				int num15 = n + 1;
				if (num15 == numSides)
				{
					num15 = 0;
				}
				for (int num16 = 0; num16 < numSegments + 1; num16++)
				{
					int num17 = num16 * numSides;
					array3[num14++] = num17 + n;
					array3[num14++] = num17 + num15;
					array3[num14++] = num17 + n + numSides;
					array3[num14++] = num17 + num15 + numSides;
					array3[num14++] = num17 + n + numSides;
					array3[num14++] = num17 + num15;
				}
			}
			if (flag)
			{
				for (int num18 = 0; num18 < numSides - 1; num18++)
				{
					array3[num14++] = num;
					array3[num14++] = num + num18 + 1;
					array3[num14++] = num + num18 + 2;
				}
				array3[num14++] = num;
				array3[num14++] = num + numSides;
				array3[num14++] = num + 1;
			}
			mesh.triangles = array3;
			return mesh;
		}
	}
}
