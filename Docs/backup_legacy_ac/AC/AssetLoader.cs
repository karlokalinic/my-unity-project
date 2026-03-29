using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Video;

namespace AC
{
	public static class AssetLoader
	{
		private static Object[] textureAssets;

		private static Object[] audioAssets;

		private static Object[] animationAssets;

		private static Object[] materialAssets;

		private static Object[] actionListAssets;

		private static Object[] runtimeAnimatorControllerAssets;

		private static Object[] timelineAssets;

		private static Object[] videoAssets;

		public static string GetAssetInstanceID<T>(T originalFile) where T : Object
		{
			if (originalFile != null)
			{
				string text = string.Concat(originalFile.GetType(), originalFile.name);
				return text.Replace(" (Instance)", string.Empty);
			}
			return string.Empty;
		}

		public static T RetrieveAsset<T>(T originalFile, string _name) where T : Object
		{
			if (string.IsNullOrEmpty(_name))
			{
				return originalFile;
			}
			if (originalFile == null)
			{
				return (T)null;
			}
			Object obj = null;
			if (originalFile is Texture)
			{
				obj = RetrieveTextures(_name);
			}
			else if (originalFile is AudioClip)
			{
				obj = RetrieveAudioClip(_name);
			}
			else if (originalFile is AnimationClip)
			{
				obj = RetrieveAnimClips(_name);
			}
			else if (originalFile is Material)
			{
				obj = RetrieveMaterials(_name);
			}
			else if (originalFile is ActionListAsset)
			{
				obj = RetrieveActionListAssets(_name);
			}
			else if (originalFile is TimelineAsset)
			{
				obj = RetrieveTimelines(_name);
			}
			else if (originalFile is VideoClip)
			{
				obj = RetrieveVideoClips(_name);
			}
			else if (originalFile is RuntimeAnimatorController)
			{
				obj = RetrieveRuntimeAnimatorControllerAssets(_name);
			}
			else
			{
				Object[] assetFiles = RetrieveAssetFiles<T>(null, string.Empty);
				obj = GetAssetFile<T>(assetFiles, _name);
			}
			return (!(obj != null)) ? originalFile : ((T)obj);
		}

		private static Texture RetrieveTextures(string _name)
		{
			textureAssets = RetrieveAssetFiles<Texture>(textureAssets, "Textures");
			return GetAssetFile<Texture>(textureAssets, _name);
		}

		public static AudioClip RetrieveAudioClip(string _name)
		{
			audioAssets = RetrieveAssetFiles<AudioClip>(audioAssets, "Audio");
			return GetAssetFile<AudioClip>(audioAssets, _name);
		}

		private static AnimationClip RetrieveAnimClips(string _name)
		{
			animationAssets = RetrieveAssetFiles<AnimationClip>(animationAssets, "Animations");
			return GetAssetFile<AnimationClip>(animationAssets, _name);
		}

		private static Material RetrieveMaterials(string _name)
		{
			materialAssets = RetrieveAssetFiles<Material>(materialAssets, "Materials");
			return GetAssetFile<Material>(materialAssets, _name);
		}

		private static ActionListAsset RetrieveActionListAssets(string _name)
		{
			actionListAssets = RetrieveAssetFiles<ActionListAsset>(actionListAssets, "ActionLists");
			return GetAssetFile<ActionListAsset>(actionListAssets, _name);
		}

		private static TimelineAsset RetrieveTimelines(string _name)
		{
			timelineAssets = RetrieveAssetFiles<TimelineAsset>(timelineAssets, "Timelines");
			return GetAssetFile<TimelineAsset>(timelineAssets, _name);
		}

		private static VideoClip RetrieveVideoClips(string _name)
		{
			videoAssets = RetrieveAssetFiles<VideoClip>(videoAssets, "VideoClips");
			return GetAssetFile<VideoClip>(videoAssets, _name);
		}

		private static RuntimeAnimatorController RetrieveRuntimeAnimatorControllerAssets(string _name)
		{
			runtimeAnimatorControllerAssets = RetrieveAssetFiles<RuntimeAnimatorController>(runtimeAnimatorControllerAssets, "Animators");
			return GetAssetFile<RuntimeAnimatorController>(runtimeAnimatorControllerAssets, _name);
		}

		private static T GetAssetFile<T>(Object[] assetFiles, string _name) where T : Object
		{
			if (assetFiles != null && _name != null)
			{
				_name = _name.Replace(" (Instance)", string.Empty);
				foreach (Object obj in assetFiles)
				{
					if (obj != null && (_name == string.Concat(obj.GetType(), obj.name) || _name == obj.name))
					{
						return (T)obj;
					}
				}
			}
			return (T)null;
		}

		private static Object[] RetrieveAssetFiles<T>(Object[] assetFiles, string saveableFolderName) where T : Object
		{
			if (assetFiles == null && !string.IsNullOrEmpty(saveableFolderName))
			{
				assetFiles = Resources.LoadAll("SaveableData/" + saveableFolderName, typeof(T));
			}
			if (assetFiles == null || assetFiles.Length == 0)
			{
				assetFiles = Resources.LoadAll(string.Empty, typeof(T));
			}
			return assetFiles;
		}

		public static void UnloadAssets()
		{
			textureAssets = null;
			audioAssets = null;
			animationAssets = null;
			materialAssets = null;
			actionListAssets = null;
			runtimeAnimatorControllerAssets = null;
			timelineAssets = null;
			Resources.UnloadUnusedAssets();
		}
	}
}
