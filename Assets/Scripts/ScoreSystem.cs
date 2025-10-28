using System.Collections.Generic;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    public static ScoreSystem Instance { get; private set; }

    [Header("Mode")]
    [SerializeField] private bool useHeightScoring = false;  // OFF by default

    [Header("Height Scoring Settings")]
    [SerializeField] private Transform floor;   // assign Floor transform
    [SerializeField] private float pointsPerUnit = 10f;
    [SerializeField] private float refreshInterval = 0.25f;

    private readonly List<Renderer> tracked = new();
    private float timer;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void RegisterRenderer(Renderer r)
    {
        if (r && !tracked.Contains(r)) tracked.Add(r);
    }

    void Update()
    {
        if (!useHeightScoring) return;

        timer += Time.unscaledDeltaTime;
        if (timer < refreshInterval) return;
        timer = 0f;

        if (tracked.Count == 0 || !floor) return;

        float maxY = float.NegativeInfinity;
        foreach (var r in tracked)
        {
            if (!r) continue;
            float y = r.bounds.max.y;
            if (y > maxY) maxY = y;
        }

        if (maxY > floor.position.y)
        {
            int score = Mathf.Max(0, Mathf.RoundToInt((maxY - floor.position.y) * pointsPerUnit));
            GameManager.Instance.SetScore(score);
        }
    }
}
