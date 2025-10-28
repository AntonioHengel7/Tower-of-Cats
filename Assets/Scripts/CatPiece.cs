using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
[DisallowMultipleComponent]
public class CatPiece : MonoBehaviour
{
    public enum State { PreDrop, Active, Settled }

    [Header("Movement (pre-drop)")]
    public float moveSpeed = 6f;
    public float rotationStep = 90f;

    [Header("Fast Drop")]
    public float dropImpulse = 8f;

    [Header("SFX")]
    public AudioSource audioSource;
    public AudioClip releaseClip;
    public AudioClip thudClip;
    public float thudVelocity = 4.5f;

    [Header("Scoring")]
    [Tooltip("Score this prefab grants when it settles.")]
    public int points = 1;

    [Header("Stability")]
    [Tooltip("If true, convert to Kinematic when settled to eliminate long jitter.")]
    public bool freezeOnSettle = true;
    [Tooltip("Extra damping once released, to help it come to rest.")]
    public float releasedLinearDrag = 1.5f;
    public float releasedAngularDrag = 3f;

    [Header("Settle Logic")]
    [Tooltip("Seconds rigidbody must be sleeping OR slow before we consider it settled.")]
    public float settleAfter = 0.5f;
    [Tooltip("Linear speed (m/s) below which we start counting as 'slow'.")]
    public float linearSpeedThreshold = 0.15f;
    [Tooltip("Angular speed (deg/s) below which we start counting as 'slow'.")]
    public float angularSpeedThreshold = 5f;
    [Tooltip("How long the body must stay below the speed thresholds to settle.")]
    public float slowTimeToSettle = 0.35f;
    [Tooltip("Safety: force-settle after this many seconds even if still bouncing.")]
    public float maxActiveSeconds = 8f;

    public State CurrentState { get; private set; } = State.PreDrop;
    public event Action<CatPiece> OnSettled;

    Rigidbody2D rb;
    Collider2D col;

    float sleepTimer;
    float slowTimer;
    float releasedAtTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // Pre-drop = Kinematic + no collisions
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        col.isTrigger = true;
    }

    void Update()
    {
        if (CurrentState != State.Active) return;

        // 1) Normal "sleep" settle path
        if (!rb.IsAwake())
        {
            sleepTimer += Time.deltaTime;
            if (sleepTimer >= settleAfter)
            {
                Settle();
                return;
            }
        }
        else
        {
            sleepTimer = 0f;
        }

        // 2) Velocity-threshold settle path (for bouncy materials that never sleep)
        bool slowLinear = rb.linearVelocity.sqrMagnitude <= (linearSpeedThreshold * linearSpeedThreshold);
        bool slowAngular = Mathf.Abs(rb.angularVelocity) <= angularSpeedThreshold;
        if (slowLinear && slowAngular)
        {
            slowTimer += Time.deltaTime;
            if (slowTimer >= slowTimeToSettle)
            {
                Settle();
                return;
            }
        }
        else
        {
            slowTimer = 0f;
        }

        // 3) Safety cap: hard-stop after N seconds
        if (Time.time - releasedAtTime >= maxActiveSeconds)
        {
            Settle();
            return;
        }
    }

    void Settle()
    {
        if (CurrentState == State.Settled) return;
        CurrentState = State.Settled;

        // Freeze to stop long-term tiny bounces
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (freezeOnSettle)
        {
            rb.bodyType = RigidbodyType2D.Kinematic; // remains a solid support
            // gravityScale irrelevant for Kinematic
        }
        else
        {
            rb.Sleep(); // stays Dynamic but goes to sleep
        }

        OnSettled?.Invoke(this);
    }

    public void MovePreDrop(float dx)
    {
        if (CurrentState != State.PreDrop) return;
        transform.position += new Vector3(dx * moveSpeed * Time.deltaTime, 0f, 0f);
    }

    public void RotatePreDrop()
    {
        if (CurrentState != State.PreDrop) return;
        transform.Rotate(0f, 0f, rotationStep);
    }

    public void Release()
    {
        if (CurrentState != State.PreDrop) return;

        audioSource.PlayOneShot(releaseClip);

        CurrentState = State.Active;
        col.isTrigger = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Damping to help settle
        rb.linearDamping = releasedLinearDrag;
        rb.angularDamping = releasedAngularDrag;

        releasedAtTime = Time.time;
        sleepTimer = 0f;
        slowTimer = 0f;
    }

    public void FastDrop()
    {
        if (CurrentState != State.Active) return;
        rb.AddForce(Vector2.down * dropImpulse, ForceMode2D.Impulse);
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (CurrentState == State.Active && audioSource && thudClip)
        {
            if (c.relativeVelocity.magnitude >= thudVelocity)
                audioSource.PlayOneShot(thudClip);
        }
    }
}
