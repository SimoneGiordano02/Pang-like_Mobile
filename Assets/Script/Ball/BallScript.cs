using System;
using UnityEngine;

public enum BallSize
{   
        Large,
        Medium,
        Small
    }
public class BallScript : MonoBehaviour
{
    [SerializeField]
    public BallSize ballSize;

    private float forceX, forceY;

    private Rigidbody2D myBody;

    [SerializeField]
    private bool moveLeft, moveRight;

    [SerializeField]
    private GameObject originalBall;

    private GameObject ball1, ball2;
    private BallScript ballScript1, ballScript2;
    

    [SerializeField] private GameObject shieldPowerUp;
    [SerializeField] private GameObject rapidFirePowerUp;

    void TrySpawnPowerUp()
    {
        float dropChance = 0.3f; // 30% probabilità
        if (UnityEngine.Random.value < dropChance)
        {
            GameObject prefab = UnityEngine.Random.value < 0.5f ? shieldPowerUp : rapidFirePowerUp;
            Instantiate(prefab, transform.position, Quaternion.identity);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        
        myBody = GetComponent<Rigidbody2D>();
        SetBallSpeed();

        if (!moveLeft && !moveRight)
        {
            bool startLeft = UnityEngine.Random.value > 0.5f;
            SetMoveLeft(startLeft);
        }

    }

    // Update is called once per frame
    void Update()
    {
        MoveBall();
    }

    void InstantiateBalls()
    {
        ball1 = Instantiate(originalBall);
        ball2 = Instantiate(originalBall);

        ball1.name = originalBall.name;
        ball2.name = originalBall.name;

        ballScript1 = ball1.GetComponent<BallScript>();
        ballScript2 = ball2.GetComponent<BallScript>();

        // Imposta la nuova dimensione alle palle figlie
        BallSize nextSize = GetNextSmallerSize();
        ballScript1.ballSize = nextSize;
        ballScript2.ballSize = nextSize;

        // Ricalcola la velocità (serve se SetBallSpeed usa ballSize)
        ballScript1.SetBallSpeed();
        ballScript2.SetBallSpeed();
    }
    BallSize GetNextSmallerSize()
    {
        switch (ballSize)
        {
            
            case BallSize.Large:
                return BallSize.Medium;
            case BallSize.Medium:
                return BallSize.Small;
            default:
                return BallSize.Small; // Non si divide più
        }
    }

    void InitializeBallDestroyCurrentBall()
    {
        InstantiateBalls();

        Vector3 temp = transform.position;

        ball1.transform.position = temp;
        ballScript1.SetMoveLeft(true);

        ball2.transform.position = temp;
        ballScript2.SetMoveRight(true);

        ball1.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, 2f);
        ball2.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, 2f);

        gameObject.SetActive(false);
        TrySpawnPowerUp();
    }


    public void SetMoveLeft (bool canMoveLeft)
    {
        this.moveLeft = canMoveLeft;
        this.moveRight = !canMoveLeft;
    }

    public void SetMoveRight(bool canMoveRight)
    {
        this.moveRight = canMoveRight;
        this.moveLeft = !canMoveRight;
    }
    void MoveBall()
    {
        if (moveLeft)
        {
            Vector3  temp = transform.position;
            temp.x -= forceX * Time.deltaTime;
            transform.position = temp;  
        }

        if (moveRight)
        {
            Vector3 temp = transform.position;
            temp.x += forceX * Time.deltaTime;
            transform.position = temp;
        }
    }
    void SetBallSpeed()
    {
     
        forceX = 2.5f;

        switch (ballSize)
        {
            case BallSize.Large:
                forceY = 10.5f;
                break;
            case BallSize.Medium:
                forceY = 9f;
                break;
            case BallSize.Small:
                forceY = 7f;          
                break;
        }
       
    }
   

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Platform")
        {
            // Ottieni la direzione dell’impatto
            foreach (ContactPoint2D point in collision.contacts)
            {
                Vector2 normal = point.normal;

                // Se il contatto è con una superficie orizzontale (cioè da sotto la palla)
                if (normal.y > 0)
                {
                    float platformBounceForce = forceY * 0.7f; // 70% della forza originale
                    myBody.linearVelocity = new Vector2(myBody.linearVelocity.x, platformBounceForce);
                    break;
                }
            }
        }

        if (collision.gameObject.tag == "Ground")
        {
            
            myBody.linearVelocity = new Vector2(myBody.linearVelocity.x, forceY);
        }

        if (collision.gameObject.tag == "Left Wall")
        {
            SetMoveRight(true);
        }

        if (collision.gameObject.tag == "Right Wall")
        {
            SetMoveLeft(true);
        }

        if (collision.gameObject.tag == "Bullet")
        {
            // Controlla se è la più piccola
            if (gameObject.name.Contains("Small"))
            {
                gameObject.SetActive(false);
            }
            else
            {                              
                InitializeBallDestroyCurrentBall();
            }
        }
}

}
