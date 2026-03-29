using System;

namespace AC
{
	[Serializable]
	public class CursorIcon : CursorIconBase, ITranslatable
	{
		public bool dontCycle;

		public string label;

		public int lineID = -1;

		public int id;

		public CursorIcon()
		{
			dontCycle = false;
			texture = null;
			id = 0;
			lineID = -1;
			isAnimated = false;
			numFrames = 1;
			size = 0.04f;
			frameSpeeds = new float[0];
			label = "Icon " + (id + 1);
		}

		public CursorIcon(int[] idArray)
		{
			dontCycle = false;
			texture = null;
			id = 0;
			lineID = -1;
			isAnimated = false;
			numFrames = 1;
			frameSpeeds = new float[0];
			foreach (int num in idArray)
			{
				if (id == num)
				{
					id++;
				}
			}
			label = "Icon " + (id + 1);
		}

		public string GetButtonName()
		{
			if (label != string.Empty)
			{
				return "Icon_" + label.Replace(" ", string.Empty);
			}
			return "Icon_" + id;
		}

		public void Copy(CursorIcon _cursorIcon, bool includeTranslation)
		{
			label = _cursorIcon.label;
			if (includeTranslation)
			{
				lineID = _cursorIcon.lineID;
			}
			id = _cursorIcon.id;
			dontCycle = _cursorIcon.dontCycle;
			Copy(_cursorIcon);
		}

		public string GetTranslatableString(int index)
		{
			return label;
		}

		public int GetTranslationID(int index)
		{
			return lineID;
		}
	}
}
