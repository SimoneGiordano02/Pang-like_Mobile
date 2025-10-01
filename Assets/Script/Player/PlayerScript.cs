using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState { Idle, Walking, Shooting, Climbing, OnLadderTop, Dead }

    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject shieldVisual;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip defeatSound;
    [SerializeField] private int maxLives = 3;
    private int currentLives;

    private bool isInvincible = false;
    private float invincibilityDuration = 1.5f;

    private SpriteRenderer spriteRenderer;

    private Rigidbody2D myBody;
    private Animator anim;
    private PlayerState currentState;

    private float speed = 8f;
    private float maxVelocity = 4f;
    private float climbSpeed = 3f;

    private bool isOnLadder = false;
    private bool canShoot = true;

    private float normalShootCooldown = 0.5f;
    private float rapidShootCooldown = 0.25f;
    private float currentShootCooldown;
    private bool hasShield = false;

    private AudioSource audioSource;
    //PROVA INPUT MOBILE 
    private Vector2 GetInput()
    {
        float h = 0;
        float v = 0;


        if (MobileInput.MoveLeft) h = -1;
        if (MobileInput.MoveRight) h = 1;
        if (MobileInput.ClimbUp) v = 1;
        if (MobileInput.ClimbDown) v = -1;
        return new Vector2(h, v);
    }

    private bool GetShootInput()
    {

    bool result = MobileInput.Shoot;
    MobileInput.Shoot = false; // reset dopo la lettura
    return result;

    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
        myBody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        ChangeState(PlayerState.Idle);
        currentLives = maxLives;
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentShootCooldown = normalShootCooldown;
        if (shieldVisual != null)
            shieldVisual.SetActive(false);
    }

    void UpdateAnimationParameters()
    {
       
        float h = GetInput().x;
        anim.SetBool("IsWalking", h != 0);

    }

    void Update()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
                HandleIdle();
                break;
            case PlayerState.Walking:
                HandleWalking();
                break;
            case PlayerState.Shooting:
                HandleShooting();
                break;
            case PlayerState.Climbing:
                HandleClimbing();
                break;
        }
        UpdateAnimationParameters();
    }

    void FixedUpdate()
    {
        if (currentState == PlayerState.Walking || currentState == PlayerState.OnLadderTop)
            ApplyHorizontalMovement();
    }

    void HandleIdle()
    {


        Vector2 input = GetInput();

        if (input.x != 0)
            ChangeState(PlayerState.Walking);

        if (GetShootInput() && canShoot)
            StartCoroutine(HandleShooting());

        if (isOnLadder && input.y != 0)
            ChangeState(PlayerState.Climbing);
    }

    void HandleWalking()
    {

        Vector2 input = GetInput();

        if (input.x == 0)
            ChangeState(PlayerState.Idle);

        if (GetShootInput() && canShoot)
            StartCoroutine(HandleShooting());

        if (isOnLadder && input.y != 0)
            ChangeState(PlayerState.Climbing);
    }

    void ApplyHorizontalMovement()
    {


        float h = GetInput().x;
        float velocity = Mathf.Abs(myBody.linearVelocity.x);
        float force = 0f;

        if (h != 0 && velocity < maxVelocity)
        {
            force = speed * h;
            Vector3 scale = transform.localScale;
            scale.x = h > 0 ? -4 : 4;
            transform.localScale = scale;

            anim.SetBool("IsWalking", true);
        }
        else
        {
            anim.SetBool("IsWalking", false);
        }

        myBody.AddForce(new Vector2(force, 0));
    }

    IEnumerator HandleShooting()
    {
        ChangeState(PlayerState.Shooting);
        canShoot = false;
        anim.Play("Shoot");
        audioSource.PlayOneShot(shootSound);

        Instantiate(bullet, transform.position + Vector3.up * 1f, Quaternion.identity);

        yield return new WaitForSeconds(currentShootCooldown);
        ChangeState(PlayerState.Idle);
        canShoot = true;
    }

    void HandleClimbing()
    {


        float v = GetInput().y;

        myBody.linearVelocity = new Vector2(0, v * climbSpeed);
        anim.SetBool("isClimbing", v != 0);

        if (!isOnLadder || v == 0)
        {
            anim.SetBool("isClimbing", false);
            ChangeState(PlayerState.Idle);
        }
    }

    void ChangeState(PlayerState newState)
    {
        currentState = newState;

        if (newState != PlayerState.Climbing)
        {
            myBody.gravityScale = 3f;
        }
        else
        {
            myBody.gravityScale = 0f;
            myBody.linearVelocity = Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
            isOnLadder = true;



        if (collision.CompareTag("Ball") && !isInvincible)
        {
            
            if (currentLives <= 1) StartCoroutine(KillPlayer());
            else
            {
                StartCoroutine(HandleInvincibility());
                StartCoroutine(TemporaryIgnoreBallCollision());
            }
        }
        if (collision.CompareTag("PowerUp"))
        {
            PowerUp p = collision.GetComponent<PowerUp>();
            if (p != null)
            {
                if (p.type == PowerUpType.Shield)
                {
                    hasShield = true;
                    if (shieldVisual != null)
                        shieldVisual.SetActive(true);
                }

                else if (p.type == PowerUpType.RapidFire) StartCoroutine(ActivateRapidFire());
            }
            Destroy(collision.gameObject);
        }
    }
    IEnumerator TemporaryIgnoreBallCollision()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int ballLayer = LayerMask.NameToLayer("Ball");

        // Ignora collisioni fisiche
        Physics2D.IgnoreLayerCollision(playerLayer, ballLayer, true);

        // Aspetta 1.5 secondi
        yield return new WaitForSeconds(1.5f);

        // Riattiva collisioni
        Physics2D.IgnoreLayerCollision(playerLayer, ballLayer, false);
    }
    IEnumerator ActivateRapidFire()
    {
        currentShootCooldown = rapidShootCooldown;
        yield return new WaitForSeconds(7f);
        currentShootCooldown = normalShootCooldown;
    }

    IEnumerator HandleInvincibility()
    {
        if (hasShield) 
        {
            hasShield = false;
            if (shieldVisual != null)
                shieldVisual.SetActive(false);
            yield break; 
        }
        currentLives--;
        isInvincible = true;
        float blinkTime = 0.1f, elapsed = 0f;
        while (elapsed < invincibilityDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkTime);
            elapsed += blinkTime;
        }
        spriteRenderer.enabled = true;
        isInvincible = false;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
            isOnLadder = false;
    }


        IEnumerator KillPlayer()
        {
            ChangeState(PlayerState.Dead);
            transform.position = new Vector3(200, 200, 0);

        // Mostra testo Game Over
        if (gameOverUI != null)
        { 
        gameOverUI.SetActive(true);
        audioSource.PlayOneShot(defeatSound);
       } 
        yield return new WaitForSeconds(3f);
        if (gameOverUI != null)
            gameOverUI.SetActive(false);

        // Torna al menu principale
        SceneManager.LoadScene("MainMenu");
        }

    
}

