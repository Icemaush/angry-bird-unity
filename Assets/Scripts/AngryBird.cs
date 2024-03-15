using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngryBird : MonoBehaviour
{
    [Header("Sounds")]
    [SerializeField] private AudioClip[] boxHitClips;
    
    private AudioSource audioSource;
    private Rigidbody2D rigidBody;
    private CapsuleCollider2D capsuleCollider;
    private bool hasBeenLaunched;
    private bool shouldFaceVelocityDirection;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
        rigidBody = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    private void Start() {
        rigidBody.isKinematic = true;
        capsuleCollider.enabled = false;
    }

    private void FixedUpdate() {
        if (hasBeenLaunched && shouldFaceVelocityDirection) {
            transform.right = rigidBody.velocity;
        }
    }

    public void LaunchBird(Vector2 direction, float force) {
        rigidBody.isKinematic = false;
        capsuleCollider.enabled = true;
        rigidBody.AddForce(direction * force, ForceMode2D.Impulse);
        hasBeenLaunched = true;
        shouldFaceVelocityDirection = true;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        shouldFaceVelocityDirection = false;

        if (collision.gameObject.CompareTag("Box")) {
            SoundManager.instance.PlayRandomClip(boxHitClips, audioSource);
        }
        
        Destroy(this);
    }
}
