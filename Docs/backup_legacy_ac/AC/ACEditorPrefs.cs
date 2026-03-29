using UnityEngine;

namespace AC
{
	public class ACEditorPrefs : ScriptableObject
	{
		public static int HierarchyIconOffset
		{
			get
			{
				return DefaultHierarchyIconOffset;
			}
		}

		private static int DefaultHierarchyIconOffset
		{
			get
			{
				return 0;
			}
		}

		public static Color HotspotGizmoColor
		{
			get
			{
				return DefaultHotspotGizmoColor;
			}
		}

		private static Color DefaultHotspotGizmoColor
		{
			get
			{
				return new Color(1f, 1f, 0f, 0.6f);
			}
		}

		public static Color TriggerGizmoColor
		{
			get
			{
				return DefaultTriggerGizmoColor;
			}
		}

		private static Color DefaultTriggerGizmoColor
		{
			get
			{
				return new Color(1f, 0.3f, 0f, 0.8f);
			}
		}

		public static Color CollisionGizmoColor
		{
			get
			{
				return DefaultCollisionGizmoColor;
			}
		}

		private static Color DefaultCollisionGizmoColor
		{
			get
			{
				return new Color(0f, 1f, 1f, 0.8f);
			}
		}

		public static Color PathGizmoColor
		{
			get
			{
				return DefaultPathGizmoColor;
			}
		}

		private static Color DefaultPathGizmoColor
		{
			get
			{
				return Color.blue;
			}
		}

		public static CSVFormat CSVFormat
		{
			get
			{
				return CSVFormat.Legacy;
			}
		}

		private static CSVFormat DefaultCSVFormat
		{
			get
			{
				return CSVFormat.Standard;
			}
		}

		public static int MenuItemsBeforeScroll
		{
			get
			{
				return DefaultMenuItemsBeforeScroll;
			}
		}

		private static int DefaultMenuItemsBeforeScroll
		{
			get
			{
				return 15;
			}
		}
	}
}
