using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace AC
{
	[Serializable]
	public class ActionMovie : Action
	{
		public MovieClipType movieClipType = MovieClipType.VideoPlayer;

		public MovieMaterialMethod movieMaterialMethod;

		public VideoPlayer videoPlayer;

		protected VideoPlayer runtimeVideoPlayer;

		public int videoPlayerParameterID = -1;

		public int videoPlayerConstantID;

		public bool prepareOnly;

		public bool pauseWithGame;

		public VideoClip newClip;

		protected bool isPaused;

		public string skipKey;

		public bool canSkip;

		public ActionMovie()
		{
			isDisplayed = true;
			title = "Play movie clip";
			category = ActionCategory.Engine;
			description = "Plays movie clips either on a Texture, or full-screen on mobile devices.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			movieClipType = MovieClipType.VideoPlayer;
			runtimeVideoPlayer = AssignFile(parameters, videoPlayerParameterID, videoPlayerConstantID, videoPlayer);
			isPaused = false;
		}

		public override float Run()
		{
			if (movieClipType == MovieClipType.VideoPlayer)
			{
				if (runtimeVideoPlayer != null)
				{
					if (!isRunning)
					{
						isRunning = true;
						if (movieMaterialMethod == MovieMaterialMethod.PlayMovie)
						{
							if (newClip != null)
							{
								runtimeVideoPlayer.clip = newClip;
							}
							if (prepareOnly)
							{
								runtimeVideoPlayer.Prepare();
								if (willWait)
								{
									return base.defaultPauseTime;
								}
							}
							else
							{
								KickStarter.playerInput.skipMovieKey = string.Empty;
								runtimeVideoPlayer.Play();
								if (runtimeVideoPlayer.isLooping)
								{
									LogWarning("Cannot wait for " + runtimeVideoPlayer.name + " to finish because it is looping!");
									return 0f;
								}
								if (canSkip && skipKey != string.Empty)
								{
									KickStarter.playerInput.skipMovieKey = skipKey;
								}
								if (willWait)
								{
									return base.defaultPauseTime;
								}
							}
						}
						else if (movieMaterialMethod == MovieMaterialMethod.PauseMovie)
						{
							runtimeVideoPlayer.Pause();
						}
						else if (movieMaterialMethod == MovieMaterialMethod.StopMovie)
						{
							runtimeVideoPlayer.Stop();
						}
						return 0f;
					}
					if (prepareOnly)
					{
						if (!runtimeVideoPlayer.isPrepared)
						{
							return base.defaultPauseTime;
						}
					}
					else
					{
						if (pauseWithGame)
						{
							if (KickStarter.stateHandler.gameState == GameState.Paused)
							{
								if (runtimeVideoPlayer.isPlaying && !isPaused)
								{
									runtimeVideoPlayer.Pause();
									isPaused = true;
								}
								return base.defaultPauseTime;
							}
							if (!runtimeVideoPlayer.isPlaying && isPaused)
							{
								isPaused = false;
								runtimeVideoPlayer.Play();
							}
						}
						if (canSkip && skipKey != string.Empty && KickStarter.playerInput.skipMovieKey == string.Empty)
						{
							runtimeVideoPlayer.Stop();
							isRunning = false;
							return 0f;
						}
						if (!runtimeVideoPlayer.isPrepared || runtimeVideoPlayer.isPlaying)
						{
							return base.defaultPauseTime;
						}
					}
					runtimeVideoPlayer.Stop();
					isRunning = false;
					return 0f;
				}
				LogWarning("Cannot play video - no Video Player found!");
				return 0f;
			}
			LogWarning("On non-mobile platforms, this Action requires use of the Video Player.");
			return 0f;
		}

		public override void Skip()
		{
			OnComplete();
		}

		protected void OnComplete()
		{
			if (movieClipType == MovieClipType.VideoPlayer)
			{
				if (runtimeVideoPlayer != null)
				{
					if (prepareOnly)
					{
						runtimeVideoPlayer.Prepare();
					}
					else
					{
						runtimeVideoPlayer.Stop();
					}
				}
			}
			else if (movieClipType != MovieClipType.FullScreen && (movieClipType != MovieClipType.OnMaterial || movieMaterialMethod != MovieMaterialMethod.PlayMovie) && movieClipType == MovieClipType.OnMaterial && movieMaterialMethod != MovieMaterialMethod.PlayMovie)
			{
				Run();
			}
		}

		public static ActionMovie CreateNew_Play(VideoPlayer videoPlayer, bool waitUntilFinish = true, bool pauseWhenGameDoes = true, string inputButtonToSkip = "")
		{
			ActionMovie actionMovie = ScriptableObject.CreateInstance<ActionMovie>();
			actionMovie.movieClipType = MovieClipType.VideoPlayer;
			actionMovie.movieMaterialMethod = MovieMaterialMethod.PlayMovie;
			actionMovie.videoPlayer = videoPlayer;
			actionMovie.willWait = waitUntilFinish;
			actionMovie.prepareOnly = false;
			actionMovie.pauseWithGame = pauseWhenGameDoes;
			actionMovie.canSkip = !string.IsNullOrEmpty(inputButtonToSkip);
			actionMovie.skipKey = inputButtonToSkip;
			return actionMovie;
		}

		public static ActionMovie CreateNew_Prepare(VideoPlayer videoPlayer)
		{
			ActionMovie actionMovie = ScriptableObject.CreateInstance<ActionMovie>();
			actionMovie.movieClipType = MovieClipType.VideoPlayer;
			actionMovie.movieMaterialMethod = MovieMaterialMethod.PlayMovie;
			actionMovie.videoPlayer = videoPlayer;
			actionMovie.prepareOnly = true;
			return actionMovie;
		}

		public static ActionMovie CreateNew_Stop(VideoPlayer videoPlayer, bool pauseOnly = false)
		{
			ActionMovie actionMovie = ScriptableObject.CreateInstance<ActionMovie>();
			actionMovie.movieClipType = MovieClipType.VideoPlayer;
			actionMovie.movieMaterialMethod = (pauseOnly ? MovieMaterialMethod.PauseMovie : MovieMaterialMethod.StopMovie);
			return actionMovie;
		}
	}
}
