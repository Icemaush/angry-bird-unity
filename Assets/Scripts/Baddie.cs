using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Baddie : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private float damageThreshold = 0.2f;

    [Header("Particle Effects")]
    [SerializeField] private GameObject particleBaddieDeath;

    [Header("Sounds")]
    [SerializeField] private AudioClip baddiePopClip;

    private float currentHealth;

    private void Awake() {
        currentHealth = maxHealth;
    }

    public void DamageBaddie(float damageAmount) {
        currentHealth -= damageAmount;

        if (currentHealth <= 0f) {
            Die();
        }
    }

    private void Die() {
        GameManager.instance.RemoveBaddie(this);

        Instantiate(particleBaddieDeath, transform.position, Quaternion.identity);

        AudioSource.PlayClipAtPoint(baddiePopClip, transform.position);

        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        float impactVelocity = collision.relativeVelocity.magnitude;

        if (impactVelocity > damageThreshold) {
            DamageBaddie(impactVelocity);
        }
    }
}
