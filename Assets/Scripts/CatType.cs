// CatType.cs (new file)
using UnityEngine;

[CreateAssetMenu(menuName = "Cats/Cat Type")]
public class CatType : ScriptableObject
{
    public string displayName = "Default";
    public Sprite sprite;
    public PhysicsMaterial2D physicsMaterial;   // friction/bounce
    public float mass = 1f;
    public float drag = 1.5f;
    public float angularDrag = 3f;
    public float rotationStep = 90f;
    public float dropImpulse = 8f;
    public bool freezeOnSettle = true;
    public int points = 1;
    public AudioClip thudClip;
    public float thudVelocity = 4.5f;
    public Vector3 localScale = Vector3.one;
}
