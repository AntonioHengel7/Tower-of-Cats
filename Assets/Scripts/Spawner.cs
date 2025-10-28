using UnityEngine;
using UnityEngine.InputSystem;

public class Spawner : MonoBehaviour
{
    [Header("Refs")]
    public Camera cam;
    public Transform spawnPoint;

    [Header("Cat Prefab Variants")]
    [Tooltip("Assign multiple Cat prefab variants here (e.g., Sticky, Slippery, Heavy, Bouncy).")]
    public GameObject[] catPrefabs;

    [Header("Flow")]
    public float respawnDelay = 0.25f;

    [Header("Clamp")]
    public float sidePadding = 0.2f;       // keep cat within camera

    CatPiece current;
    float nextSpawnAt = -1f;

    void Start()
    {
        if (!cam) cam = Camera.main;
        SpawnNew();
    }

    void Update()
    {
        if (!current && Time.time >= nextSpawnAt && nextSpawnAt >= 0f)
        {
            SpawnNew();
        }

        if (!current) return;

        // Keyboard controls (pre-drop movement & actions)
        if (Keyboard.current != null)
        {
            float dir = 0f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) dir -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) dir += 1f;
            if (dir != 0f) current.MovePreDrop(dir);

            if (Keyboard.current.spaceKey.wasPressedThisFrame) current.RotatePreDrop();

            if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                bool wasPreDrop = current.CurrentState == CatPiece.State.PreDrop;
                current.Release();
                if (!wasPreDrop) current.FastDrop(); // second press = fast drop
            }
        }

        // Keep the pre-drop piece on-screen horizontally
        if (current && current.CurrentState == CatPiece.State.PreDrop)
        {
            Vector3 pos = current.transform.position;
            float halfWidth = GetHalfWidth(current);
            float xMin = cam.ViewportToWorldPoint(new Vector3(0f, 0f)).x + halfWidth + sidePadding;
            float xMax = cam.ViewportToWorldPoint(new Vector3(1f, 0f)).x - halfWidth - sidePadding;
            pos.x = Mathf.Clamp(pos.x, xMin, xMax);
            current.transform.position = pos;
        }
    }

    void SpawnNew()
    {
        if (catPrefabs == null || catPrefabs.Length == 0)
        {
            Debug.LogError("Spawner: No catPrefabs assigned.");
            return;
        }

        var prefab = catPrefabs[Random.Range(0, catPrefabs.Length)];
        GameObject go = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        current = go.GetComponent<CatPiece>();
        if (!current)
        {
            Debug.LogError("Spawned object has no CatPiece component.");
            return;
        }

        current.OnSettled += HandleSettled;
    }

    void HandleSettled(CatPiece piece)
    {
        piece.OnSettled -= HandleSettled;

        // Award score based on this prefab's points value
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(piece.points);

        current = null;
        nextSpawnAt = Time.time + respawnDelay;
    }

    float GetHalfWidth(CatPiece cp)
    {
        var r = cp.GetComponent<SpriteRenderer>();
        if (!r) return 0.5f;
        return r.bounds.extents.x;
    }
}
