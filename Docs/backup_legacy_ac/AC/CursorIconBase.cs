using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class CursorIconBase
	{
		public Texture texture;

		public bool isAnimated;

		public int numFrames = 1;

		public int numRows = 1;

		public int numCols = 1;

		public float size = 0.015f;

		public float animSpeed = 4f;

		public bool endAnimOnLastFrame;

		public bool skipFirstFrameWhenLooping;

		public Vector2 clickOffset;

		public float[] frameSpeeds;

		public bool alwaysAnimate;

		private string uniqueIdentifier;

		private float frameIndex;

		private float frameWidth = -1f;

		private float frameHeight = -1f;

		private Sprite[] sprites;

		private Texture2D[] textures;

		private Texture2D texture2D;

		private Rect firstFrameRect = default(Rect);

		private Texture2D Texture2D
		{
			get
			{
				if (texture2D == null && texture != null && texture is Texture2D)
				{
					texture2D = (Texture2D)texture;
				}
				return texture2D;
			}
		}

		public CursorIconBase()
		{
			texture = null;
			isAnimated = false;
			frameSpeeds = new float[0];
			numFrames = (numRows = (numCols = 1));
			size = 0.015f;
			frameIndex = 0f;
			frameWidth = (frameHeight = -1f);
			animSpeed = 4f;
			endAnimOnLastFrame = false;
			skipFirstFrameWhenLooping = false;
			alwaysAnimate = false;
			clickOffset = Vector2.zero;
		}

		public void Copy(CursorIconBase _icon)
		{
			texture = _icon.texture;
			isAnimated = _icon.isAnimated;
			numFrames = _icon.numFrames;
			animSpeed = _icon.animSpeed;
			endAnimOnLastFrame = _icon.endAnimOnLastFrame;
			skipFirstFrameWhenLooping = _icon.skipFirstFrameWhenLooping;
			clickOffset = _icon.clickOffset;
			numRows = _icon.numRows;
			numCols = _icon.numCols;
			size = _icon.size;
			alwaysAnimate = _icon.alwaysAnimate;
			frameSpeeds = new float[0];
			if (_icon.frameSpeeds != null)
			{
				frameSpeeds = new float[_icon.frameSpeeds.Length];
				for (int i = 0; i < frameSpeeds.Length; i++)
				{
					frameSpeeds[i] = _icon.frameSpeeds[i];
				}
			}
			Reset();
		}

		public void DrawAsInteraction(Rect _rect, bool isActive)
		{
			if (texture == null)
			{
				return;
			}
			if (isAnimated && numFrames > 0)
			{
				if (Application.isPlaying)
				{
					if (isActive || alwaysAnimate)
					{
						GUI.DrawTextureWithTexCoords(_rect, texture, GetAnimatedRect());
						return;
					}
					GUI.DrawTextureWithTexCoords(_rect, texture, firstFrameRect);
					frameIndex = 0f;
				}
				else
				{
					Reset();
					GUI.DrawTextureWithTexCoords(_rect, texture, firstFrameRect);
					frameIndex = 0f;
				}
			}
			else
			{
				GUI.DrawTexture(_rect, texture, ScaleMode.StretchToFill, true, 0f);
			}
		}

		public Sprite GetSprite()
		{
			if (Texture2D == null)
			{
				return null;
			}
			if (sprites == null || sprites.Length < 1)
			{
				sprites = new Sprite[1];
			}
			if (sprites != null && sprites.Length > 0 && sprites[0] == null)
			{
				sprites[0] = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
				sprites[0].name = texture.name + "_0";
			}
			return sprites[0];
		}

		public Sprite GetAnimatedSprite(int _frameIndex)
		{
			if (Texture2D == null)
			{
				return null;
			}
			int num = _frameIndex + 1;
			int num2 = 1;
			while (num > numCols)
			{
				num -= numCols;
				num2++;
			}
			if (_frameIndex >= numFrames)
			{
				num = 1;
				num2 = 1;
			}
			if (sprites == null || sprites.Length <= _frameIndex)
			{
				sprites = new Sprite[_frameIndex + 1];
			}
			if (sprites[_frameIndex] == null)
			{
				Rect rect = new Rect(frameWidth * (float)(num - 1) * (float)texture.width, frameHeight * (float)(numRows - num2) * (float)texture.height, frameWidth * (float)texture.width, frameHeight * (float)texture.height);
				sprites[_frameIndex] = Sprite.Create(texture2D, rect, new Vector2(0.5f, 0.5f));
				sprites[_frameIndex].name = texture.name + "_" + _frameIndex;
			}
			return sprites[_frameIndex];
		}

		public Sprite GetAnimatedSprite(bool isActive)
		{
			if (Texture2D == null)
			{
				return null;
			}
			if (isAnimated && numFrames > 0)
			{
				if (Application.isPlaying)
				{
					if (sprites == null || sprites.Length != numFrames)
					{
						sprites = new Sprite[numFrames];
					}
					if (isActive || alwaysAnimate)
					{
						int num = Mathf.FloorToInt(frameIndex);
						Rect animatedRect = GetAnimatedRect();
						if (sprites[num] == null)
						{
							animatedRect = new Rect(animatedRect.x * (float)texture.width, animatedRect.y * (float)texture.height, animatedRect.width * (float)texture.width, animatedRect.height * (float)texture.height);
							sprites[num] = Sprite.Create(texture2D, animatedRect, new Vector2(0.5f, 0.5f));
							sprites[num].name = texture.name + "_" + num;
						}
						return sprites[num];
					}
					frameIndex = 0f;
					if (sprites[0] == null)
					{
						Rect rect = new Rect(firstFrameRect.x * (float)texture.width, firstFrameRect.y * (float)texture.height, firstFrameRect.width * (float)texture.width, firstFrameRect.height * (float)texture.height);
						sprites[0] = Sprite.Create(texture2D, rect, new Vector2(0.5f, 0.5f));
						sprites[0].name = texture.name + "_0";
					}
					return sprites[0];
				}
				return null;
			}
			if (sprites == null || sprites.Length < 1)
			{
				sprites = new Sprite[1];
			}
			if (sprites != null && sprites.Length > 0 && sprites[0] == null)
			{
				sprites[0] = Sprite.Create(texture2D, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
				sprites[0].name = texture.name + "_0";
			}
			return sprites[0];
		}

		public void ReplaceTexture(Texture newTexture)
		{
			if (texture != newTexture)
			{
				texture = newTexture;
				ClearSprites();
				ClearCache();
			}
		}

		public void ClearSprites()
		{
			sprites = null;
		}

		public Texture2D GetAnimatedTexture(bool canAnimate = true)
		{
			if (Texture2D == null)
			{
				return null;
			}
			if (isAnimated)
			{
				if (frameWidth < 0f && frameHeight < 0f)
				{
					Reset();
				}
				Rect animatedRect = GetAnimatedRect((canAnimate || alwaysAnimate) ? (-1) : 0);
				int x = Mathf.FloorToInt(animatedRect.x * (float)Texture2D.width);
				int y = Mathf.FloorToInt(animatedRect.y * (float)Texture2D.height);
				int blockWidth = Mathf.FloorToInt(animatedRect.width * (float)Texture2D.width);
				int blockHeight = Mathf.FloorToInt(animatedRect.height * (float)Texture2D.height);
				if (animatedRect.width >= 0f && animatedRect.height >= 0f)
				{
					try
					{
						Color[] pixels = Texture2D.GetPixels(x, y, blockWidth, blockHeight);
						Texture2D texture2D = new Texture2D((int)(frameWidth * (float)Texture2D.width), (int)(frameHeight * (float)Texture2D.height), TextureFormat.RGBA32, false);
						texture2D.SetPixels(pixels);
						texture2D.filterMode = texture.filterMode;
						texture2D.Apply();
						return texture2D;
					}
					catch
					{
						ACDebug.LogWarning("Cannot read texture '" + texture.name + "' - it may need to be set to type 'Advanced' and have 'Read/Write Enabled' checked.");
					}
				}
			}
			return this.texture2D;
		}

		public string GetName()
		{
			return uniqueIdentifier;
		}

		public void ClearCache()
		{
			textures = null;
			texture2D = null;
			sprites = null;
		}

		public Texture Draw(Vector2 centre, bool canAnimate = true)
		{
			float num = size;
			if (KickStarter.cursorManager.cursorRendering == CursorRendering.Hardware)
			{
				num = (float)texture.width / (float)ACScreen.width;
			}
			Rect position = AdvGame.GUIBox(centre, num);
			position.x -= clickOffset.x * position.width;
			position.y -= clickOffset.y * position.height;
			if (isAnimated && numFrames > 0 && Application.isPlaying)
			{
				if (!canAnimate && !alwaysAnimate)
				{
					frameIndex = 0f;
				}
				else if (skipFirstFrameWhenLooping && frameIndex < 1f)
				{
					frameIndex = 1f;
				}
				GetAnimatedRect();
				if (textures == null)
				{
					textures = new Texture2D[numFrames];
				}
				int num2 = Mathf.FloorToInt(frameIndex);
				if (textures.Length < num2 + 1)
				{
					textures = new Texture2D[num2 + 1];
				}
				if (textures[num2] == null)
				{
					if (Texture2D == null)
					{
						ACDebug.LogWarning(string.Concat("Cannot animate texture ", texture, " as it is not a Texture2D."));
					}
					else
					{
						textures[num2] = GetAnimatedTexture(canAnimate);
					}
				}
				GUI.DrawTexture(position, textures[num2]);
				return textures[num2];
			}
			GUI.DrawTexture(position, texture, ScaleMode.ScaleToFit, true, 0f);
			return texture;
		}

		public Rect GetAnimatedRect()
		{
			if (frameWidth < 0f)
			{
				Reset();
			}
			int num = 1;
			int num2 = 1;
			if (frameIndex < 0f)
			{
				frameIndex = 0f;
			}
			else if (frameIndex < (float)numFrames && (!endAnimOnLastFrame || !(frameIndex >= (float)(numFrames - 1))))
			{
				int num3 = Mathf.FloorToInt(frameIndex);
				float num4 = ((frameSpeeds == null || num3 >= frameSpeeds.Length) ? 1f : frameSpeeds[num3]);
				float num5 = ((!Mathf.Approximately(Time.deltaTime, 0f)) ? Time.deltaTime : 0.02f);
				frameIndex += num5 * animSpeed * num4;
			}
			num2 = Mathf.FloorToInt(frameIndex) + 1;
			while (num2 > numCols)
			{
				num2 -= numCols;
				num++;
			}
			if (frameIndex >= (float)numFrames)
			{
				if (endAnimOnLastFrame)
				{
					frameIndex = numFrames - 1;
					num2--;
				}
				else
				{
					if (skipFirstFrameWhenLooping && (float)numFrames > 1f)
					{
						frameIndex = 1f;
						num2 = 2;
					}
					else
					{
						frameIndex = 0f;
						num2 = 1;
					}
					num = 1;
				}
			}
			if (texture != null)
			{
				uniqueIdentifier = texture.name + num2 + num;
			}
			return new Rect(frameWidth * (float)(num2 - 1), frameHeight * (float)(numRows - num), frameWidth, frameHeight);
		}

		public Rect GetAnimatedRect(int _frameIndex)
		{
			if (frameWidth < 0f && frameHeight < 0f)
			{
				Reset();
			}
			if (_frameIndex < 0)
			{
				return GetAnimatedRect();
			}
			int num = _frameIndex + 1;
			int num2 = 1;
			while (num > numCols)
			{
				num -= numCols;
				num2++;
			}
			if (_frameIndex >= numFrames)
			{
				num = 1;
				num2 = 1;
			}
			return new Rect(frameWidth * (float)(num - 1), frameHeight * (float)(numRows - num2), frameWidth, frameHeight);
		}

		public void Reset()
		{
			if (isAnimated)
			{
				if (numFrames > 0)
				{
					frameWidth = 1f / (float)numCols;
					frameHeight = 1f / (float)numRows;
					frameIndex = 0f;
				}
				else
				{
					ACDebug.LogWarning("Cannot have an animated cursor with less than one frame!");
				}
				if (animSpeed < 0f)
				{
					animSpeed = 0f;
				}
				firstFrameRect = new Rect(0f, 1f - frameHeight, frameWidth, frameHeight);
			}
		}

		private void SyncFrameSpeeds()
		{
			if (frameSpeeds == null)
			{
				frameSpeeds = new float[0];
			}
			if (frameSpeeds.Length == numFrames)
			{
				return;
			}
			float[] array = new float[frameSpeeds.Length];
			for (int i = 0; i < frameSpeeds.Length; i++)
			{
				array[i] = frameSpeeds[i];
			}
			frameSpeeds = new float[numFrames];
			for (int j = 0; j < numFrames; j++)
			{
				if (j < array.Length)
				{
					frameSpeeds[j] = array[j];
				}
				else
				{
					frameSpeeds[j] = 1f;
				}
			}
		}
	}
}
