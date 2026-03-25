using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Difficulty system that affects world population, item placement, and enemy tuning.
/// Set once at game start; room populators read from it.
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    public enum Difficulty { Easy, Normal, Hard, Nightmare }

    [SerializeField] private Difficulty currentDifficulty = Difficulty.Normal;

    public static DifficultyManager Instance { get; private set; }
    public Difficulty CurrentDifficulty => currentDifficulty;

    public event Action<Difficulty> DifficultyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // DontDestroyOnLoad works only on scene roots. If this component was placed on a child,
        // skip persistence instead of throwing a runtime warning.
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void SetDifficulty(Difficulty diff)
    {
        currentDifficulty = diff;
        DifficultyChanged?.Invoke(diff);
    }

    // --- Tuning queries used by room populators and combat ---

    public float EnemyHealthMultiplier => currentDifficulty switch
    {
        Difficulty.Easy => 0.6f,
        Difficulty.Normal => 1f,
        Difficulty.Hard => 1.5f,
        Difficulty.Nightmare => 2.2f,
        _ => 1f
    };

    public float EnemyDamageMultiplier => currentDifficulty switch
    {
        Difficulty.Easy => 0.5f,
        Difficulty.Normal => 1f,
        Difficulty.Hard => 1.4f,
        Difficulty.Nightmare => 2f,
        _ => 1f
    };

    public float PlayerDamageMultiplier => currentDifficulty switch
    {
        Difficulty.Easy => 1.5f,
        Difficulty.Normal => 1f,
        Difficulty.Hard => 0.85f,
        Difficulty.Nightmare => 0.7f,
        _ => 1f
    };

    public bool ShouldSpawnItem(string difficultyTag)
    {
        if (string.IsNullOrWhiteSpace(difficultyTag)) return true;

        return difficultyTag switch
        {
            "easy_only" => currentDifficulty == Difficulty.Easy,
            "normal_and_below" => currentDifficulty <= Difficulty.Normal,
            "hard_and_above" => currentDifficulty >= Difficulty.Hard,
            "nightmare_only" => currentDifficulty == Difficulty.Nightmare,
            _ => true
        };
    }

    public float ShopPriceMultiplier => currentDifficulty switch
    {
        Difficulty.Easy => 0.8f,
        Difficulty.Normal => 1f,
        Difficulty.Hard => 1.3f,
        Difficulty.Nightmare => 1.6f,
        _ => 1f
    };

    public int StartingCurrency => currentDifficulty switch
    {
        Difficulty.Easy => 200,
        Difficulty.Normal => 100,
        Difficulty.Hard => 50,
        Difficulty.Nightmare => 25,
        _ => 100
    };
}

/// <summary>
/// Attach to a room root. On Awake, enables/disables child objects based on difficulty tags.
/// Children with a DifficultySpawnTag component are evaluated against DifficultyManager.
/// </summary>
public class RoomPopulator : MonoBehaviour
{
    private void Start()
    {
        Populate();
    }

    public void Populate()
    {
        var tags = GetComponentsInChildren<DifficultySpawnTag>(true);
        for (int i = 0; i < tags.Length; i++)
        {
            bool shouldExist = true;
            if (DifficultyManager.Instance != null)
                shouldExist = DifficultyManager.Instance.ShouldSpawnItem(tags[i].Tag);

            tags[i].gameObject.SetActive(shouldExist);
        }
    }
}

/// <summary>
/// Tag a GameObject so RoomPopulator can enable/disable it based on difficulty.
/// e.g. tag="normal_and_below" means this object only exists on Easy and Normal.
/// </summary>
public class DifficultySpawnTag : MonoBehaviour
{
    [SerializeField] private string difficultyTag = "";
    public string Tag => difficultyTag;

    public void Configure(string tag) { difficultyTag = tag; }
}
