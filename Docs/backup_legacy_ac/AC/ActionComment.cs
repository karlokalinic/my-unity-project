using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionComment : Action
	{
		protected enum ACLogType
		{
			No = 0,
			AsInfo = 1,
			AsWarning = 2,
			AsError = 3
		}

		public string commentText = string.Empty;

		[SerializeField]
		protected ACLogType acLogType = ACLogType.AsInfo;

		protected string convertedText;

		public ActionComment()
		{
			isDisplayed = true;
			category = ActionCategory.ActionList;
			title = "Comment";
			description = "Prints a comment for debug purposes.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			convertedText = AdvGame.ConvertTokens(commentText, 0, null, parameters);
		}

		public override float Run()
		{
			if (acLogType != ACLogType.No && !string.IsNullOrEmpty(convertedText))
			{
				if (acLogType == ACLogType.AsInfo)
				{
					Log(convertedText);
				}
				else if (acLogType == ACLogType.AsWarning)
				{
					LogWarning(convertedText);
				}
				else if (acLogType == ACLogType.AsError)
				{
					LogError(convertedText);
				}
			}
			return 0f;
		}

		public static ActionComment CreateNew(string text, bool displayAsWarning = false)
		{
			ActionComment actionComment = ScriptableObject.CreateInstance<ActionComment>();
			actionComment.commentText = text;
			actionComment.acLogType = ((!displayAsWarning) ? ACLogType.AsInfo : ACLogType.AsWarning);
			return actionComment;
		}
	}
}
