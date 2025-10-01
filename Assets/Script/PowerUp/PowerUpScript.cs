using UnityEngine;
using System.Collections;

public enum PowerUpType
{
    Shield,
    RapidFire
}
public class PowerUp : MonoBehaviour
{

    public PowerUpType type;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool hasLanded = false;

    [SerializeField] private float blinkStartTime = 2f;
    [SerializeField] private float destroyTime = 3f;
    [SerializeField] private float blinkInterval = 0.2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasLanded && !collision.collider.isTrigger)
        {
            hasLanded = true;

            // Ferma il movimento
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;

            // Avvia distruzione dopo 3 secondi
            StartCoroutine(BlinkAndDestroy());
        }
    }

    IEnumerator BlinkAndDestroy()
    {
        yield return new WaitForSeconds(blinkStartTime);

        float elapsed = 0f;
        while (elapsed < destroyTime - blinkStartTime)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        Destroy(gameObject);
    }
}
