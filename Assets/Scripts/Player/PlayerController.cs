using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;

    [Header("Jump")]
    public float jumpForce = 14f;                 // impulso inicial
    public float maxJumpHoldTime = 0.18f;         // cuánto tiempo máximo puede sostener el botón para 'boost'
    public float jumpHoldForce = 35f;             // fuerza adicional aplicada mientras se mantiene
    public float variableJumpMultiplier = 0.5f;   // si sueltas antes, multiplica Y para cortar salto

    [Header("Dash")]
    public float dashSpeed = 18f;
    public float dashDuration = 0.18f;
    public float dashEndDecel = 0.08f;            // tiempo para desacelerar al terminar dash
    public float dashCooldown = 0.25f;
    float dashCooldownTimer = 0f;

    [Header("Wall")]
    public float wallSlideSpeed = 1.5f;
    public float wallJumpForce = 14f;

    [Header("Wall Jump Settings")]
    public float wallJumpHorizontal = 10f;  // fuerza horizontal aplicada al wall jump
    public float wallJumpVertical = 14f;    // fuerza vertical aplicada
    public float wallJumpBufferTime = 0.12f; // tiempo para permitir wall jump después de soltar la pared
    public float coyoteTime = 0.08f;        // ventana tras despegar del suelo para permitir jump

    // internos
    private float lastLeftWallTime = -10f;
    private float lastRightWallTime = -10f;
    private float lastGroundedTime = -10f;

    [Header("Checks")]
    public Transform groundCheck;
    public Transform wallCheck;
    public LayerMask groundLayer;
    public float checkRadius = 0.18f;

    [Header("Buster (K)")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float shootCooldown = 0.25f;
    private float lastShootTime;

    [Header("Sword / Combo (O)")]
    public Transform attackPoint;
    public float attackRadius = 0.4f;
    public LayerMask enemyLayer;
    public float comboResetTime = 0.9f;
    private int comboStep = 0;

    [Header("Attack cancel options")]
    public bool allowAttackCancelByJump = true;
    public bool allowAttackCancelByDash = true;
    public bool allowAttackCancelByShoot = false;

    // internals
    Rigidbody2D rb;
    Animator anim;
    bool isGrounded;
    bool isWallSliding;
    bool isDashing;
    bool facingRight = true;

    // jump hold
    bool isJumping = false;
    float jumpHoldTimer = 0f;

    // combo flags
    bool isAttacking = false;
    bool comboQueued = false;
    float lastComboTime = 0f;

    // hit tracking
    HashSet<int> hitThisAttack = new HashSet<int>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // timers
        if (dashCooldownTimer > 0f) dashCooldownTimer -= Time.deltaTime;

        CheckSurroundings();
        HandleInput();
        UpdateAnimatorParams();

        // Jump hold handling: apply extra upward force while holding and under max time
        if (isJumping && Input.GetKey(KeyCode.L))
        {
            jumpHoldTimer += Time.deltaTime;
            if (jumpHoldTimer <= maxJumpHoldTime)
            {
                // apply small continuous upward force so holding increases height
                rb.AddForce(Vector2.up * jumpHoldForce * Time.deltaTime, ForceMode2D.Force);
            }
        }

        // Reset combo if too much time passes
        if (!isAttacking && Time.time - lastComboTime > comboResetTime)
            comboStep = 0;
    }

    void CheckSurroundings()
    {
        // Ground check (mantén tu lógica)
        isGrounded = (groundCheck != null) && Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        if (isGrounded) lastGroundedTime = Time.time;

        // Wall detection: boxcast a izquierda/derecha usando wallCheck como referencia
        // Ajusta size según tu sprite (height ~ 0.9f)
        Vector2 boxSize = new Vector2(0.1f, 0.9f);
        float castDist = 0.05f;

        bool leftHit = Physics2D.BoxCast(wallCheck.position, boxSize, 0f, Vector2.left, castDist, groundLayer);
        bool rightHit = Physics2D.BoxCast(wallCheck.position, boxSize, 0f, Vector2.right, castDist, groundLayer);

        // wall sliding si estamos en contacto con pared, no en suelo y cayendo
        isWallSliding = (leftHit || rightHit) && !isGrounded && rb.linearVelocity.y < 0.1f;

        // guarda último contacto para wall jump buffer
        if (leftHit) lastLeftWallTime = Time.time;
        if (rightHit) lastRightWallTime = Time.time;

        // opcional debug
        // Debug.Log($"leftHit:{leftHit} rightHit:{rightHit} isWallSliding:{isWallSliding} velY:{rb.velocity.y}");
    }

    void HandleInput()
    {
        float move = 0f;
        if (Input.GetKey(KeyCode.A)) move = -1f;
        if (Input.GetKey(KeyCode.D)) move = 1f;

        if (isWallSliding)
        {
            // si facingRight == true, la pared está a la derecha; si move > 0 (presionando derecha) entonces bloquea
            if ((facingRight && move > 0f) || (!facingRight && move < 0f))
            {
                move = 0f; // evita empujar contra la pared
            }
        }

        // Horizontal movement (permitir movimiento si no dashing; opcional bloquear mientras atacas)
        if (!isDashing && !isAttacking)
            rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        // Flip
        if (move > 0 && !facingRight) Flip();
        else if (move < 0 && facingRight) Flip();

        // JUMP (L)
        if (Input.GetKeyDown(KeyCode.L))
        {
            // If attacking and cancel allowed, cancel and jump
            if (isAttacking && allowAttackCancelByJump)
            {
                isAttacking = false;
                comboQueued = false;
                // reset any attack triggers if necessary
                // anim.ResetTrigger($"Attack{comboStep}");
                StartJump();
            }
            else
            {
                if (isGrounded)
                    StartJump();
                else if (isWallSliding)
                    WallJump();
                else if (isDashing && allowAttackCancelByJump)
                {
                    // cancel dash and jump
                    StopAllCoroutines(); // stop dash coroutine safely (we will rely on flags)
                    isDashing = false;
                    if (anim != null) anim.SetBool("Dashing", false);
                    StartJump();
                }
            }
        }

        // Jump release: short jump
        if (Input.GetKeyUp(KeyCode.L))
        {
            // stop jump hold
            isJumping = false;
            jumpHoldTimer = 0f;
            // apply short-cut if still going up (optional)
            if (rb.linearVelocity.y > 0f)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * variableJumpMultiplier);
        }

        // Dash (Ñ) - try several possible KeyCodes: semicolon, right bracket, backslash
        if ((Input.GetKeyDown(KeyCode.Semicolon) || Input.GetKeyDown(KeyCode.RightBracket) || Input.GetKeyDown(KeyCode.Backslash)) && dashCooldownTimer <= 0f)
        {
            if (isAttacking && allowAttackCancelByDash)
            {
                isAttacking = false;
                comboQueued = false;
                StartCoroutine(DoDash());
            }
            else
            {
                StartCoroutine(DoDash());
            }
        }

        // Shooting (K)
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (isAttacking && allowAttackCancelByShoot)
            {
                isAttacking = false;
                comboQueued = false;
            }
            HandleShooting();
        }

        // Sword (O)
        if (Input.GetKeyDown(KeyCode.O))
        {
            OnSwordButtonPressed();
        }
        else if (Input.GetKeyUp(KeyCode.O))
        {
            comboQueued = false;
        }

        // set animator speed (also set other animator params in UpdateAnimatorParams)
        if (anim != null)
            anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));

        // update grounded/wall/VSpeed in anim too
        if (anim != null)
        {
            anim.SetBool("Grounded", isGrounded);
            anim.SetBool("WallSliding", isWallSliding);
            anim.SetFloat("VSpeed", rb.linearVelocity.y);
        }
    }

    // Start jump helper: separate to allow dash-cancel->jump / attack-cancel->jump
    void StartJump()
    {
        // Reset vertical for consistent impulse, then add impulse
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        isJumping = true;
        jumpHoldTimer = 0f;
        if (anim != null) anim.SetTrigger("Jump");
    }

    void WallJump()
    {
        // permite wall jump si actualmente en wall slide o si tocaste pared recientemente (buffer)
        bool canWallJump = isWallSliding ||
            (Time.time - lastLeftWallTime <= wallJumpBufferTime) ||
            (Time.time - lastRightWallTime <= wallJumpBufferTime);

        if (!canWallJump) return;

        // decidir de qué lado saltar: si lastLeftWallTime reciente -> empujar a la derecha (dir=1)
        float dir = 0f;
        if (Time.time - lastLeftWallTime <= wallJumpBufferTime) dir = 1f;
        else if (Time.time - lastRightWallTime <= wallJumpBufferTime) dir = -1f;
        else dir = facingRight ? -1f : 1f;

        // aplicar impulso: primero resetear velocidad vertical/horizontal para consistencia
        rb.linearVelocity = new Vector2(0f, 0f);
        rb.AddForce(new Vector2(dir * wallJumpHorizontal, wallJumpVertical), ForceMode2D.Impulse);

        // mirar hacia fuera de la pared
        if (dir > 0 && !facingRight) Flip();
        else if (dir < 0 && facingRight) Flip();

        // desactivar wallSlide y permitir breve invulnerabilidad de re-attach si quieres (opcional)
        isWallSliding = false;

        if (anim != null) anim.SetTrigger("Jump");
    }

    IEnumerator DoDash()
    {
        if (isDashing) yield break;
        isDashing = true;
        if (anim != null) anim.SetBool("Dashing", true);

        float dir = facingRight ? 1f : -1f;
        float elapsed = 0f;
        float startVx = rb.linearVelocity.x;

        // dash main phase: maintain horizontal dash speed, preserve vertical
        while (elapsed < dashDuration)
        {
            rb.linearVelocity = new Vector2(dir * dashSpeed, rb.linearVelocity.y);
            elapsed += Time.deltaTime;

            // allow cancel into jump mid-dash
            if (Input.GetKeyDown(KeyCode.L) && allowAttackCancelByJump)
            {
                // cancel dash and start jump
                isDashing = false;
                if (anim != null) anim.SetBool("Dashing", false);
                StartJump();
                dashCooldownTimer = dashCooldown; // start cooldown
                yield break;
            }

            yield return null;
        }

        // end dash: decelerate smoothly for dashEndDecel seconds (so it doesn't snap)
        float decel = dashEndDecel;
        float t = 0f;
        float fromVx = rb.linearVelocity.x;
        while (t < decel)
        {
            float vx = Mathf.Lerp(fromVx, 0f, t / decel);
            rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
            t += Time.deltaTime;
            yield return null;
        }

        // ensure not left as isDashing
        isDashing = false;
        if (anim != null) anim.SetBool("Dashing", false);

        // start cooldown
        dashCooldownTimer = dashCooldown;
    }

    void HandleShooting()
    {
        if (Time.time - lastShootTime < shootCooldown) return;

        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogError("PlayerController: projectilePrefab o firePoint no asignado.");
            return;
        }

        lastShootTime = Time.time;
        if (anim != null) anim.SetTrigger("Shoot");

        GameObject p = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        Projectile projScript = p.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.SetDirection(facingRight ? Vector2.right : Vector2.left);
        }
        else
        {
            Rigidbody2D prb = p.GetComponent<Rigidbody2D>();
            if (prb != null)
            {
                prb.gravityScale = 0f;
                prb.linearVelocity = (facingRight ? Vector2.right : Vector2.left) * 12f;
            }
            else
                Debug.LogWarning("El proyectil instanciado no tiene ni Projectile ni Rigidbody2D.");
        }
    }

    // --------------------
    // COMBO SABER (O)
    // --------------------
    void OnSwordButtonPressed()
    {
        if (isAttacking)
        {
            comboQueued = true;
            return;
        }

        comboStep++;
        if (comboStep > 4) comboStep = 1;

        isAttacking = true;
        comboQueued = false;
        lastComboTime = Time.time;

        if (anim != null) anim.SetTrigger($"Attack{comboStep}");
    }

    // Animation Event -> se llama al final de cada Saber clip
    public void Finish_Ani()
    {
        isAttacking = false;

        if (comboQueued)
        {
            comboQueued = false;
            comboStep++;
            if (comboStep > 4) comboStep = 1;
            isAttacking = true;
            lastComboTime = Time.time;
            if (anim != null) anim.SetTrigger($"Attack{comboStep}");
        }
        else
        {
            comboStep = 0;
            hitThisAttack.Clear();
        }
    }

    // Animation Event -> frame de hit
    public void PerformMeleeHit(int step)
    {
        if (attackPoint == null)
        {
            Debug.LogWarning("PerformMeleeHit llamado pero attackPoint no asignado.");
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);
        foreach (var c in hits)
        {
            int id = c.gameObject.GetInstanceID();
            if (hitThisAttack.Contains(id)) continue;
            hitThisAttack.Add(id);

            Enemy e = c.GetComponent<Enemy>();
            if (e != null)
            {
                int dmg = 1;
                e.TakeDamage(dmg);

                // optional knockback
                Rigidbody2D erb = e.GetComponent<Rigidbody2D>();
                if (erb != null)
                {
                    Vector2 kbDir = (c.transform.position - transform.position).normalized;
                    erb.AddForce(kbDir * 200f);
                }
            }
        }
    }

    void UpdateAnimatorParams()
    {
        if (anim == null) return;
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("Grounded", isGrounded);
        anim.SetBool("WallSliding", isWallSliding);
        anim.SetFloat("VSpeed", rb.linearVelocity.y);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
        if (wallCheck != null)
        {
            Gizmos.color = Color.cyan;
            // box cast visualization
            Vector2 size = new Vector2(0.1f, 0.9f);
            Gizmos.DrawWireCube(wallCheck.position + Vector3.right * 0.05f, size); // right check
            Gizmos.DrawWireCube(wallCheck.position + Vector3.left * 0.05f, size); // left check
        }
    }
}