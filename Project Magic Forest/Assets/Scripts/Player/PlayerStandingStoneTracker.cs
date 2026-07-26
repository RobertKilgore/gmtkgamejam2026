using System;
using UnityEngine;

public sealed class PlayerStandingStoneTracker : MonoBehaviour
{
    public static PlayerStandingStoneTracker Instance { get; private set; }

    public const int MaxStandingStones = 5;

    public event Action<int> StoneCountChanged;

    [SerializeField] private int stoneCount;

    public int StoneCount => stoneCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public int AddStone()
    {
        if (stoneCount >= MaxStandingStones)
        {
            return stoneCount;
        }

        stoneCount = Mathf.Min(stoneCount + 1, MaxStandingStones);
        StoneCountChanged?.Invoke(stoneCount);
        return stoneCount;
    }

    public void ResetStones()
    {
        stoneCount = 0;
        StoneCountChanged?.Invoke(stoneCount);
    }
}
