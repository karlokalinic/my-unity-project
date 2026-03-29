using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace AC
{
	[Serializable]
	public class ActionMixerSnapshot : Action
	{
		public int numSnapshots = 1;

		public AudioMixer audioMixer;

		public AudioMixerSnapshot snapshot;

		public List<SnapshotMix> snapshotMixes = new List<SnapshotMix>();

		public float changeTime = 0.1f;

		public ActionMixerSnapshot()
		{
			isDisplayed = true;
			category = ActionCategory.Sound;
			title = "Set Mixer snapshot";
			description = "Transitions to a single or multiple Audio Mixer snapshots.";
		}

		public override float Run()
		{
			if (!isRunning)
			{
				isRunning = true;
				if (numSnapshots == 1)
				{
					if (snapshot != null)
					{
						snapshot.TransitionTo(changeTime);
						if (changeTime > 0f && willWait)
						{
							return changeTime;
						}
					}
					else
					{
						LogWarning("No Audio Mixer Snapshot assigned.");
					}
				}
				else if ((bool)audioMixer)
				{
					List<AudioMixerSnapshot> list = new List<AudioMixerSnapshot>();
					List<float> list2 = new List<float>();
					foreach (SnapshotMix snapshotMix in snapshotMixes)
					{
						list.Add(snapshotMix.snapshot);
						list2.Add(snapshotMix.weight);
					}
					audioMixer.TransitionToSnapshots(list.ToArray(), list2.ToArray(), changeTime);
					if (changeTime > 0f && willWait)
					{
						return changeTime;
					}
				}
				return 0f;
			}
			isRunning = false;
			return 0f;
		}

		public static ActionMixerSnapshot CreateNew_Single(AudioMixerSnapshot snapshot, float transitionTime = 0f, bool waitUntilFinish = false)
		{
			ActionMixerSnapshot actionMixerSnapshot = ScriptableObject.CreateInstance<ActionMixerSnapshot>();
			actionMixerSnapshot.numSnapshots = 1;
			actionMixerSnapshot.snapshot = snapshot;
			actionMixerSnapshot.changeTime = transitionTime;
			actionMixerSnapshot.willWait = waitUntilFinish;
			return actionMixerSnapshot;
		}

		public static ActionMixerSnapshot CreateNew_Mix(List<SnapshotMix> snapshotMixData, AudioMixer audioMixer, float transitionTime = 0f, bool waitUntilFinish = false)
		{
			ActionMixerSnapshot actionMixerSnapshot = ScriptableObject.CreateInstance<ActionMixerSnapshot>();
			actionMixerSnapshot.numSnapshots = 0;
			actionMixerSnapshot.audioMixer = audioMixer;
			actionMixerSnapshot.snapshotMixes = snapshotMixData;
			actionMixerSnapshot.changeTime = transitionTime;
			actionMixerSnapshot.willWait = waitUntilFinish;
			return actionMixerSnapshot;
		}
	}
}
