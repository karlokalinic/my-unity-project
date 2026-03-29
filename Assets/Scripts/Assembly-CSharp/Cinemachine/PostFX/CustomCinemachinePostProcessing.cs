#if CINEMACHINE_3_0_0_OR_NEWER
using Unity.Cinemachine;
#define CUSTOM_CM_AVAILABLE
#elif CINEMACHINE
using Cinemachine;
#define CUSTOM_CM_AVAILABLE
#endif
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Custom.CinemachinePostFX
{
#if CUSTOM_CM_AVAILABLE
    [ExecuteAlways]
    [AddComponentMenu("Cinemachine/Custom/Custom Cinemachine Post Processing")]
    [SaveDuringPlay]
    public class CustomCinemachinePostProcessing : CinemachineExtension
    {
        [Tooltip("If checked, then the Focus Distance will be set to the distance between the camera and the tracked target. Requires DepthOfField effect in the Profile.")]
        public bool m_FocusTracksTarget;

        [Tooltip("Offset from target distance, to be used with Focus Tracks Target. Offsets the sharpest point away from the tracked target.")]
        public float m_FocusOffset;

        [Tooltip("This Post-Processing profile will be applied whenever this Cinemachine camera is live.")]
        public PostProcessProfile m_Profile;

        [Range(0f, 1f)]
        [Tooltip("Additional weight multiplier for this post-processing profile.")]
        public float m_Weight = 1f;

        private bool mCachedProfileIsInvalid = true;
        private PostProcessProfile mProfileCopy;

        private const string VolumeOwnerName = "__CMCustomPPVolumes";

        private static readonly List<PostProcessVolume> sVolumes = new();
        private static readonly Dictionary<CinemachineBrain, PostProcessLayer> sBrainToLayer = new();
        private static readonly HashSet<CinemachineBrain> sSubscribedBrains = new();

        public PostProcessProfile Profile => mProfileCopy != null ? mProfileCopy : m_Profile;

        public bool IsValid => m_Profile != null && m_Profile.settings != null && m_Profile.settings.Count > 0;

        public void InvalidateCachedProfile()
        {
            mCachedProfileIsInvalid = true;
        }

        private void CreateProfileCopy()
        {
            DestroyProfileCopy();

            var profile = ScriptableObject.CreateInstance<PostProcessProfile>();
            if (m_Profile != null)
            {
                foreach (var setting in m_Profile.settings)
                {
                    var copy = UnityEngine.Object.Instantiate(setting);
                    profile.settings.Add(copy);
                }
            }

            mProfileCopy = profile;
            mCachedProfileIsInvalid = false;
        }

        private void DestroyProfileCopy()
        {
            if (mProfileCopy != null)
                RuntimeUtility.DestroyObject(mProfileCopy);

            mProfileCopy = null;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DestroyProfileCopy();
        }

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage,
            ref CameraState state,
            float deltaTime)
        {
            if (stage != CinemachineCore.Stage.Aim)
                return;

            if (!IsValid)
            {
                DestroyProfileCopy();
                return;
            }

            if (!m_FocusTracksTarget)
            {
                DestroyProfileCopy();
            }
            else
            {
                if (mProfileCopy == null || mCachedProfileIsInvalid)
                    CreateProfileCopy();

                if (mProfileCopy != null && mProfileCopy.TryGetSettings<DepthOfField>(out var dof))
                {
                    float focusDistance = m_FocusOffset;

                    if (state.HasLookAt())
                        focusDistance += Vector3.Distance(state.GetFinalPosition(), state.ReferenceLookAt);

                    dof.focusDistance.value = Mathf.Max(0f, focusDistance);
                }
            }

            state.AddCustomBlendable(new CameraState.CustomBlendableItems.Item
            {
                Custom = this,
                Weight = Mathf.Clamp01(m_Weight)
            });
        }

        private static void OnCameraCut(ICinemachineMixer mixer, ICinemachineCamera incomingCamera)
        {
            if (mixer is CinemachineBrain brain)
            {
                var ppLayer = GetPPLayer(brain);
                if (ppLayer != null)
                    ppLayer.ResetHistory();
            }
        }

        private static void ApplyPostFX(CinemachineBrain brain)
        {
            var ppLayer = GetPPLayer(brain);
            if (ppLayer == null || !ppLayer.enabled || ppLayer.volumeLayer.value == 0)
                return;

            var currentCameraState = brain.State;
            int numCustomBlendables = currentCameraState.GetNumCustomBlendables();
            var dynamicBrainVolumes = GetDynamicBrainVolumes(brain, ppLayer, numCustomBlendables);

            for (int i = 0; i < dynamicBrainVolumes.Count; i++)
            {
                dynamicBrainVolumes[i].weight = 0f;
                dynamicBrainVolumes[i].sharedProfile = null;
                dynamicBrainVolumes[i].profile = null;
            }

            PostProcessVolume firstVolume = null;
            int activeCount = 0;

            for (int j = 0; j < numCustomBlendables; j++)
            {
                var customBlendable = currentCameraState.GetCustomBlendable(j);

                if (customBlendable.Custom is not CustomCinemachinePostProcessing customPP)
                    continue;

                var volume = dynamicBrainVolumes[j];
                if (firstVolume == null)
                    firstVolume = volume;

                volume.sharedProfile = customPP.Profile;
                volume.isGlobal = true;
                volume.priority = float.MaxValue - (numCustomBlendables - j) - 1f;
                volume.weight = Mathf.Clamp01(customBlendable.Weight);
                activeCount++;

                if (activeCount > 1 && firstVolume != null)
                    firstVolume.weight = 1f;
            }
        }

        private static List<PostProcessVolume> GetDynamicBrainVolumes(
            CinemachineBrain brain,
            PostProcessLayer ppLayer,
            int minVolumes)
        {
            GameObject owner = null;
            var parent = brain.transform;
            int childCount = parent.childCount;

            sVolumes.Clear();

            for (int i = 0; owner == null && i < childCount; i++)
            {
                var child = parent.GetChild(i).gameObject;
                if (child.hideFlags == HideFlags.HideAndDontSave)
                {
                    child.GetComponents(sVolumes);
                    if (sVolumes.Count > 0)
                        owner = child;
                }
            }

            if (minVolumes > 0)
            {
                if (owner == null)
                {
                    owner = new GameObject(VolumeOwnerName);
                    owner.hideFlags = HideFlags.HideAndDontSave;
                    owner.transform.SetParent(parent, false);
                }

                int volumeLayerMask = ppLayer.volumeLayer.value;
                for (int i = 0; i < 32; i++)
                {
                    if ((volumeLayerMask & (1 << i)) != 0)
                    {
                        owner.layer = i;
                        break;
                    }
                }

                while (sVolumes.Count < minVolumes)
                    sVolumes.Add(owner.AddComponent<PostProcessVolume>());
            }

            return sVolumes;
        }

        private static PostProcessLayer GetPPLayer(CinemachineBrain brain)
        {
            if (brain == null)
                return null;

            if (!sBrainToLayer.TryGetValue(brain, out var layer) || layer == null)
            {
                layer = brain.GetComponent<PostProcessLayer>();
                sBrainToLayer[brain] = layer;
            }

            if (!sSubscribedBrains.Contains(brain))
            {
                brain.CameraCutEvent.AddListener(OnCameraCut);
                sSubscribedBrains.Add(brain);
            }

            return layer;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void InitializeModule()
        {
            CinemachineCore.CameraUpdatedEvent.RemoveListener(ApplyPostFX);
            CinemachineCore.CameraUpdatedEvent.AddListener(ApplyPostFX);

            sBrainToLayer.Clear();
            sSubscribedBrains.Clear();
        }
    }
#else
    [ExecuteAlways]
    [AddComponentMenu("Cinemachine/Custom/Custom Cinemachine Post Processing")]
    public class CustomCinemachinePostProcessing : MonoBehaviour
    {
        [Tooltip("If checked, then the Focus Distance will be set to the distance between the camera and the tracked target. Requires Cinemachine package.")]
        public bool m_FocusTracksTarget;

        [Tooltip("Offset from target distance, to be used with Focus Tracks Target.")]
        public float m_FocusOffset;

        [Tooltip("This Post-Processing profile will be applied whenever this camera is live.")]
        public PostProcessProfile m_Profile;

        [Range(0f, 1f)]
        [Tooltip("Additional weight multiplier for this post-processing profile.")]
        public float m_Weight = 1f;

        public PostProcessProfile Profile => m_Profile;

        public bool IsValid => m_Profile != null && m_Profile.settings != null && m_Profile.settings.Count > 0;

        public void InvalidateCachedProfile()
        {
        }
    }
#endif
}
