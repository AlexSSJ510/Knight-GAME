using UnityEngine;

/// <summary>
/// Controlador de movimiento del Caballero: plataformas, dash, wall-jump, doble salto.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(PlayerAttack))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento Basico")]
    public float moveSpeed = 10f;
    public float jumpForce = 8f;

    [Header("Dash")]
    public float dashForce = 10f;
    public float dashCooldown = 1f;
    public float dashDuration = 0.2f;

    private bool _isDashing = false;
    private float _dashEndTime;


    [Header("Salto")]
    public int maxJumps = 1; // Cambia a 2 tras desbloquear doble salto
    private int _jumpsRemaining;
    private bool _canDoubleJump = false;

    [Header("Wall-Jump")]
    public float wallJumpForce = 12f;
    public LayerMask wallMask;
    public float wallCheckDistance = 0.4f;

    [Header("Referencias")]
    private Rigidbody2D _rb;
    private PlayerAttack _playerAttack;
    private bool _isFacingRight = true;
    private bool _isGrounded = false;
    private float _lastDashTime = -99f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = .18f;
    public LayerMask groundMask;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerAttack = GetComponent<PlayerAttack>();
        _jumpsRemaining = maxJumps;
    }

    private void Update()
    {
        if (GameManager.Instance.isPaused) return;

        if (_isDashing)
        {
            if (Time.time >= _dashEndTime)
            {
                _isDashing = false;
            }
            return;
        }

        float moveInput = InputManager.Instance.GetHorizontalAxis();
        Move(moveInput);

        if (InputManager.Instance.IsJumpPressed()) TryJump();

        if (InputManager.Instance.IsDashPressed()) TryDash();

        UpdateFacing(moveInput);

        // Grounded check
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);

        // Reset salto extra
        if (_isGrounded) _jumpsRemaining = maxJumps;
    }

    private void Move(float input)
    {
        _rb.linearVelocity = new Vector2(input * moveSpeed, _rb.linearVelocity.y);
    }

    private void UpdateFacing(float input)
    {
        if (input > 0 && !_isFacingRight) Flip();
        else if (input < 0 && _isFacingRight) Flip();
    }

    private void Flip()
    {
        _isFacingRight = !_isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    private void TryJump()
    {
        // �Wall-jump?
        RaycastHit2D wallCheck = Physics2D.Raycast(transform.position, Vector2.right * (_isFacingRight ? 1 : -1), wallCheckDistance, wallMask);

        if (wallCheck.collider != null && !_isGrounded)
        {
            _rb.linearVelocity = new Vector2(-(_isFacingRight ? 1 : -1) * moveSpeed, wallJumpForce);
            _jumpsRemaining = maxJumps - 1; // Resetea saltos a uno menos (post wall-jump)
            return;
        }

        // Salto normal/doble
        if (_isGrounded || _jumpsRemaining > 0)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
            _jumpsRemaining--;
        }
    }

    private void TryDash()
    {
        if (Time.time < _lastDashTime + dashCooldown) return;

        _isDashing = true;
        _lastDashTime = Time.time;
        _dashEndTime = Time.time + dashDuration;

        // Aplica velocidad fuerte en dirección actual
        _rb.linearVelocity = new Vector2((_isFacingRight ? 1 : -1) * dashForce, 0f);
    }


    /// <summary>
    /// Activa el doble salto tras evento de glitch/memoria
    /// </summary>
    public void UnlockDoubleJump()
    {
        maxJumps = 2;
        _canDoubleJump = true;
    }
}
