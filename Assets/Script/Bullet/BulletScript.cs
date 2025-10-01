using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private Rigidbody2D myBody;
    private float speed = 5f;

    void Awake()
    {
        myBody = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        myBody.linearVelocity = new Vector2(0, speed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Top") ||
            collision.gameObject.CompareTag("Ball") ||
            collision.gameObject.CompareTag("Platform"))
        {
            Destroy(gameObject);
        }
    }
}
