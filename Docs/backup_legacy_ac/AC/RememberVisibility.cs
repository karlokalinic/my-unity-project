using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Save system/Remember Visibility")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_visibility.html")]
	public class RememberVisibility : Remember
	{
		public AC_OnOff startState;

		public bool affectChildren;

		public bool saveColour;

		private LimitVisibility limitVisibility;

		private bool loadedData;

		private void Awake()
		{
			if (loadedData || !GameIsPlaying())
			{
				return;
			}
			bool flag = startState == AC_OnOff.On;
			if ((bool)GetComponent<LimitVisibility>())
			{
				limitVisibility = GetComponent<LimitVisibility>();
				limitVisibility.isLockedOff = !flag;
			}
			else if ((bool)GetComponent<Renderer>())
			{
				GetComponent<Renderer>().enabled = flag;
			}
			if (affectChildren)
			{
				Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
				foreach (Renderer renderer in componentsInChildren)
				{
					renderer.enabled = flag;
				}
			}
		}

		public override string SaveData()
		{
			VisibilityData visibilityData = new VisibilityData();
			visibilityData.objectID = constantID;
			visibilityData.savePrevented = savePrevented;
			if ((bool)GetComponent<SpriteFader>())
			{
				SpriteFader component = GetComponent<SpriteFader>();
				visibilityData.isFading = component.isFading;
				if (component.isFading)
				{
					if (component.fadeType == FadeType.fadeIn)
					{
						visibilityData.isFadingIn = true;
					}
					else
					{
						visibilityData.isFadingIn = false;
					}
					visibilityData.fadeTime = component.fadeTime;
					visibilityData.fadeStartTime = component.fadeStartTime;
				}
				visibilityData.fadeAlpha = GetComponent<SpriteRenderer>().color.a;
			}
			else if ((bool)GetComponent<SpriteRenderer>() && saveColour)
			{
				Color color = GetComponent<SpriteRenderer>().color;
				visibilityData.colourR = color.r;
				visibilityData.colourG = color.g;
				visibilityData.colourB = color.b;
				visibilityData.colourA = color.a;
			}
			if ((bool)GetComponent<FollowTintMap>())
			{
				visibilityData = GetComponent<FollowTintMap>().SaveData(visibilityData);
			}
			if ((bool)limitVisibility)
			{
				visibilityData.isOn = !limitVisibility.isLockedOff;
			}
			else if ((bool)GetComponent<Renderer>())
			{
				visibilityData.isOn = GetComponent<Renderer>().enabled;
			}
			else if (affectChildren)
			{
				Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
				int num = 0;
				if (num < componentsInChildren.Length)
				{
					Renderer renderer = componentsInChildren[num];
					visibilityData.isOn = renderer.enabled;
				}
			}
			return Serializer.SaveScriptData<VisibilityData>(visibilityData);
		}

		public override void LoadData(string stringData)
		{
			VisibilityData visibilityData = Serializer.LoadScriptData<VisibilityData>(stringData);
			if (visibilityData == null)
			{
				loadedData = false;
				return;
			}
			base.SavePrevented = visibilityData.savePrevented;
			if (savePrevented)
			{
				return;
			}
			if ((bool)GetComponent<SpriteFader>())
			{
				SpriteFader component = GetComponent<SpriteFader>();
				if (visibilityData.isFading)
				{
					if (visibilityData.isFadingIn)
					{
						component.Fade(FadeType.fadeIn, visibilityData.fadeTime, visibilityData.fadeAlpha);
					}
					else
					{
						component.Fade(FadeType.fadeOut, visibilityData.fadeTime, visibilityData.fadeAlpha);
					}
				}
				else
				{
					component.EndFade();
					component.SetAlpha(visibilityData.fadeAlpha);
				}
			}
			else if ((bool)GetComponent<SpriteRenderer>() && saveColour)
			{
				Color color = new Color(visibilityData.colourR, visibilityData.colourG, visibilityData.colourB, visibilityData.colourA);
				GetComponent<SpriteRenderer>().color = color;
			}
			if ((bool)GetComponent<FollowTintMap>())
			{
				GetComponent<FollowTintMap>().LoadData(visibilityData);
			}
			if ((bool)limitVisibility)
			{
				limitVisibility.isLockedOff = !visibilityData.isOn;
			}
			else if ((bool)GetComponent<Renderer>())
			{
				GetComponent<Renderer>().enabled = visibilityData.isOn;
			}
			if (affectChildren)
			{
				Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
				foreach (Renderer renderer in componentsInChildren)
				{
					renderer.enabled = visibilityData.isOn;
				}
			}
			loadedData = true;
		}
	}
}
