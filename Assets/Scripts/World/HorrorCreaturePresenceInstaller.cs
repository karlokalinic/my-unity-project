using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Makes the real Rake and Snowman meshes physically present in their encounter rooms
/// before the charge begins. It never activates a procedural stand-in: the monster root
/// is only exposed when a real SkinnedMeshRenderer exists beneath it.
/// </summary>
public sealed class HorrorCreaturePresenceInstaller : MonoBehaviour
{
    private static readonly string[] SupportedScenes =
    {
        "Scena",
        "SampleScene",
        "INTERAKCIJA",
        "VerticalSlice_Consolidated"
    };

    private static readonly string[] MonsterRootNames =
    {
        "RakeEncounter_Monster",
        "AbominableSnowmanEncounter_Monster"
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || !IsSupportedScene(activeScene.name))
        {
            return;
        }

        if (FindAnyObjectByType<HorrorCreaturePresenceInstaller>() != null)
        {
            return;
        }

        GameObject host = new GameObject("__HorrorCreaturePresenceInstaller");
        host.hideFlags = HideFlags.DontSave;
        host.AddComponent<HorrorCreaturePresenceInstaller>();
    }

    private IEnumerator Start()
    {
        // Encounter controllers construct their room/monster hierarchy in Start.
        yield return null;
        yield return null;

        int readyCount = 0;
        for (int i = 0; i < MonsterRootNames.Length; i++)
        {
            if (ExposeRealMonster(MonsterRootNames[i]))
            {
                readyCount++;
            }
        }

        if (readyCount == MonsterRootNames.Length)
        {
            Debug.Log("[HorrorCreaturePresenceInstaller] REAL_MONSTERS_VISIBLE rake=true snowman=true");
        }
        else
        {
            Debug.LogError(
                $"[HorrorCreaturePresenceInstaller] REAL_MONSTERS_INCOMPLETE ready={readyCount}/{MonsterRootNames.Length}. " +
                "No procedural monster visual was accepted as a substitute.");
        }

        Destroy(gameObject);
    }

    private static bool ExposeRealMonster(string rootName)
    {
        Transform[] transforms = Object.FindObjectsByType<Transform>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        Transform monsterRoot = null;
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform candidate = transforms[i];
            if (candidate == null || candidate.name != rootName)
            {
                continue;
            }

            if (!candidate.gameObject.scene.IsValid())
            {
                continue;
            }

            monsterRoot = candidate;
            break;
        }

        if (monsterRoot == null)
        {
            Debug.LogError($"[HorrorCreaturePresenceInstaller] Missing encounter monster root '{rootName}'.");
            return false;
        }

        SkinnedMeshRenderer[] skins = monsterRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        if (skins == null || skins.Length == 0)
        {
            monsterRoot.gameObject.SetActive(false);
            Debug.LogError(
                $"[HorrorCreaturePresenceInstaller] '{rootName}' has no real skinned creature mesh; keeping it hidden.");
            return false;
        }

        bool hasRenderableSkin = false;
        for (int i = 0; i < skins.Length; i++)
        {
            SkinnedMeshRenderer skin = skins[i];
            if (skin == null || skin.sharedMesh == null)
            {
                continue;
            }

            skin.enabled = true;
            hasRenderableSkin = true;
        }

        if (!hasRenderableSkin)
        {
            monsterRoot.gameObject.SetActive(false);
            Debug.LogError(
                $"[HorrorCreaturePresenceInstaller] '{rootName}' skinned renderers have no usable meshes; keeping it hidden.");
            return false;
        }

        monsterRoot.gameObject.SetActive(true);
        return true;
    }

    private static bool IsSupportedScene(string sceneName)
    {
        for (int i = 0; i < SupportedScenes.Length; i++)
        {
            if (string.Equals(sceneName, SupportedScenes[i], System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
