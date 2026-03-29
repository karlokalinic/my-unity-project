using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionCharPortrait : Action
	{
		public int parameterID = -1;

		public int constantID;

		public bool isPlayer;

		public Char _char;

		protected Char runtimeChar;

		public Texture newPortraitGraphic;

		public ActionCharPortrait()
		{
			isDisplayed = true;
			category = ActionCategory.Character;
			title = "Switch Portrait";
			description = "Changes the 'speaking' graphic used by Characters. To display this graphic in a Menu, place a Graphic element of type Dialogue Portrait in a Menu of Appear type: When Speech Plays. If the new graphic is placed in a Resources folder, it will be stored in saved game files.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeChar = AssignFile(parameters, parameterID, constantID, _char);
			if (isPlayer)
			{
				runtimeChar = KickStarter.player;
			}
		}

		public override float Run()
		{
			if ((bool)runtimeChar)
			{
				runtimeChar.portraitIcon.ReplaceTexture(newPortraitGraphic);
			}
			return 0f;
		}

		public static ActionCharPortrait CreateNew(Char characterToUpdate, Texture newPortraitGraphic)
		{
			ActionCharPortrait actionCharPortrait = ScriptableObject.CreateInstance<ActionCharPortrait>();
			actionCharPortrait._char = characterToUpdate;
			actionCharPortrait.newPortraitGraphic = newPortraitGraphic;
			return actionCharPortrait;
		}
	}
}
