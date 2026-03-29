using System;
using UnityEngine;

namespace AC
{
	public class DrawStraightLine
	{
		private static Texture2D _aaLineTex;

		private static Texture2D _lineTex;

		private static Texture2D adLineTex
		{
			get
			{
				if (!_aaLineTex)
				{
					_aaLineTex = new Texture2D(1, 3, TextureFormat.ARGB32, true);
					_aaLineTex.SetPixel(0, 0, new Color(1f, 1f, 1f, 0f));
					_aaLineTex.SetPixel(0, 1, Color.white);
					_aaLineTex.SetPixel(0, 2, new Color(1f, 1f, 1f, 0f));
					_aaLineTex.Apply();
				}
				return _aaLineTex;
			}
		}

		private static Texture2D lineTex
		{
			get
			{
				if (!_lineTex)
				{
					_lineTex = new Texture2D(1, 1, TextureFormat.ARGB32, true);
					_lineTex.SetPixel(0, 1, Color.white);
					_lineTex.Apply();
				}
				return _lineTex;
			}
		}

		private static void DrawLineMac(Vector2 pointA, Vector2 pointB, Color color, float width, bool antiAlias)
		{
			Color color2 = GUI.color;
			Matrix4x4 matrix = GUI.matrix;
			float num = width;
			if (antiAlias)
			{
				width *= 3f;
			}
			float num2 = Vector3.Angle(pointB - pointA, Vector2.right) * (float)((pointA.y <= pointB.y) ? 1 : (-1));
			float magnitude = (pointB - pointA).magnitude;
			if (magnitude > 0.01f)
			{
				Vector3 vector = new Vector3(pointA.x, pointA.y, 0f);
				Vector3 vector2 = new Vector3((pointB.x - pointA.x) * 0.5f, (pointB.y - pointA.y) * 0.5f, 0f);
				Vector3 zero = Vector3.zero;
				zero = ((!antiAlias) ? new Vector3((0f - num) * 0.5f * Mathf.Sin(num2 * ((float)Math.PI / 180f)), num * 0.5f * Mathf.Cos(num2 * ((float)Math.PI / 180f))) : new Vector3((0f - num) * 1.5f * Mathf.Sin(num2 * ((float)Math.PI / 180f)), num * 1.5f * Mathf.Cos(num2 * ((float)Math.PI / 180f))));
				GUI.color = color;
				GUI.matrix = translationMatrix(vector) * GUI.matrix;
				GUIUtility.ScaleAroundPivot(new Vector2(magnitude, width), new Vector2(-0.5f, 0f));
				GUI.matrix = translationMatrix(-vector) * GUI.matrix;
				GUIUtility.RotateAroundPivot(num2, Vector2.zero);
				GUI.matrix = translationMatrix(vector - zero - vector2) * GUI.matrix;
				GUI.DrawTexture(new Rect(0f, 0f, 1f, 1f), (!antiAlias) ? lineTex : adLineTex);
			}
			GUI.matrix = matrix;
			GUI.color = color2;
		}

		private static void DrawLineWindows(Vector2 pointA, Vector2 pointB, Color color, float width, bool antiAlias)
		{
			float magnitude = (pointB - pointA).magnitude;
			if (!Mathf.Approximately(magnitude, 0f))
			{
				Color color2 = GUI.color;
				Matrix4x4 matrix = GUI.matrix;
				if (antiAlias)
				{
					width *= 3f;
				}
				float num = Vector3.Angle(pointB - pointA, Vector2.right) * (float)((pointA.y <= pointB.y) ? 1 : (-1));
				Vector3 vector = new Vector3(pointA.x, pointA.y, 0f);
				GUI.color = color;
				GUI.matrix = translationMatrix(vector) * GUI.matrix;
				GUIUtility.ScaleAroundPivot(new Vector2(magnitude, width), new Vector2(-0.5f, 0f));
				GUI.matrix = translationMatrix(-vector) * GUI.matrix;
				GUIUtility.RotateAroundPivot(num, new Vector2(0f, 0f));
				GUI.matrix = translationMatrix(vector + new Vector3(width / 2f, (0f - magnitude) / 2f) * Mathf.Sin(num * ((float)Math.PI / 180f))) * GUI.matrix;
				GUI.DrawTexture(new Rect(0f, 0f, 1f, 1f), antiAlias ? adLineTex : lineTex);
				GUI.matrix = matrix;
				GUI.color = color2;
			}
		}

		public static void Draw(Vector2 pointA, Vector2 pointB, Color color, float width, bool antiAlias)
		{
			if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
			{
				DrawLineWindows(pointA, pointB, color, width, antiAlias);
			}
			else
			{
				DrawLineMac(pointA, pointB, color, width, antiAlias);
			}
		}

		public static void DrawBox(Rect rect, Color color, float width, bool antiAlias, int offset)
		{
			Draw(new Vector2(rect.x, rect.y - (float)offset), new Vector2(rect.x + rect.width, rect.y - (float)offset), color, width, false);
			Draw(new Vector2(rect.x - (float)offset, rect.y - (float)(2 * offset)), new Vector2(rect.x - (float)offset, rect.y + rect.height + (float)(2 * offset)), color, width, false);
			Draw(new Vector2(rect.x + rect.width + (float)offset, rect.y - (float)(2 * offset)), new Vector2(rect.x + rect.width + (float)offset, rect.y + rect.height + (float)(2 * offset)), color, width, false);
			Draw(new Vector2(rect.x, rect.y + rect.height + (float)offset), new Vector2(rect.x + rect.width, rect.y + rect.height + (float)offset), color, width, false);
		}

		private static Matrix4x4 translationMatrix(Vector3 v)
		{
			return Matrix4x4.TRS(v, Quaternion.identity, Vector3.one);
		}
	}
}
