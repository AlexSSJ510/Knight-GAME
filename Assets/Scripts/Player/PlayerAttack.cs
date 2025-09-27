using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    public Animator ani;
    [Tooltip("Cantidad actual de combo (0..n). No modificar en tiempo de ejecución a menos que sepas lo que haces.")]
    public int combo;
    public bool atacando;

    [Header("Z-Buster (Shoot)")]
    public GameObject busterProjectilePrefab;
    public Transform shootPoint;

    [Header("Carga")]
    public float chargeMax = 2.0f;          // segundos para carga máxima
    public float chargeCooldown = 1.2f;     // (no usado actualmente, dejado para futuras mejoras)
    public float busterRechargeRate = 0.5f; // recarga por segundo

    // Estado interno
    private float _currentCharge;
    private bool _isCharging = false;
    private float _chargeStartTime = 0f;

    [Header("Daño")]
    public float baseBusterDamage = 4f;    // daño base
    public float maxBusterDamage = 20f;    // daño al cargar full

    // Animator hashes (más eficiente que usar strings concatenados)
    private int hashShoot;
    private int hashCharging;

    void Start()
    {
        // inicializa referencias seguras
        if (ani == null) ani = GetComponent<Animator>();

        // init charge properly to full chargeMax (antes estaba con valor fijo)
        _currentCharge = Mathf.Max(0f, chargeMax);

        // caches de hashes
        hashShoot = Animator.StringToHash("Shoot");
        hashCharging = Animator.StringToHash("Charging");
    }

    // Llamado por eventos de animación o código para permitir encadenar el siguiente golpe
    public void Start_Combo()
    {
        // permitir que el jugador vuelva a dar input (encadenar)
        atacando = false;

        // Limitar el número de pasos del combo si hace falta
        if (combo < 4)
        {
            combo++;
        }
    }

    // Llamado al finalizar la animación/combos para resetear
    public void Finish_Ani()
    {
        atacando = false;
        combo = 0;
    }

    // Manejo básico de inputs de combo (se puede exponer la KeyCode si quieres)
    public void Combos_()
    {
        if (Input.GetKeyDown(KeyCode.I) && !atacando)
        {
            atacando = true;
            // utilizar el trigger correspondiente al paso del combo
            // en el Animator se espera triggers llamados "0", "1", "2", etc.
            ani?.SetTrigger(combo.ToString());
        }
    }

    private void HandleBusterInput()
    {
        // seguridad: evitar divide by zero si chargeMax es 0
        if (chargeMax <= Mathf.Epsilon) return;

        // start charging
        if (InputManager.Instance != null && InputManager.Instance.IsShootPressed())
        {
            if (!_isCharging && _currentCharge > 0f)
            {
                _isCharging = true;
                _chargeStartTime = Time.time;
                if (ani) ani.SetBool(hashCharging, true);
            }
        }
        else
        {
            // Release: si estaba cargando, disparar con potencia proporcional al tiempo sostenido y carga disponible
            if (_isCharging)
            {
                float held = Time.time - _chargeStartTime;
                float usedCharge = Mathf.Min(_currentCharge, held);
                float t = Mathf.Clamp01(usedCharge / chargeMax);
                float damage = Mathf.Lerp(baseBusterDamage, maxBusterDamage, t);

                ShootBuster(damage, t);

                _currentCharge -= usedCharge;
                _isCharging = false;
                if (ani) ani.SetBool(hashCharging, false);

                // prevenir valores negativos por seguridad
                if (_currentCharge < 0f) _currentCharge = 0f;
            }
        }
    }

    private void ShootBuster(float damage, float chargeRatio)
    {
        if (busterProjectilePrefab == null || shootPoint == null) return;

        // instanciar y ajustar dirección/rotación del proyectil según la escala local X del jugador
        int dir = transform.localScale.x > 0f ? 1 : -1;
        Quaternion rot = Quaternion.identity;

        // si el prefab debe rotarse según la dirección, rotar 180 en Y cuando dir==-1
        if (dir < 0) rot = Quaternion.Euler(0f, 180f, 0f);

        GameObject p = Instantiate(busterProjectilePrefab, shootPoint.position, rot);
        var proj = p.GetComponent<Projectile>();
        if (proj != null)
        {
            // velocidad y knockback (o cualquier otro parámetro) se ajustan con chargeRatio
            float speed = 10f + 20f * chargeRatio;
            float knockback = 1f + chargeRatio * 1.5f;

            // pasar daño redondeado y dirección
            proj.Init(Mathf.Clamp(Mathf.RoundToInt(damage), 0, 99999), dir, speed, knockback);
        }
        if (ani) ani.SetTrigger(hashShoot);
    }

    // Para UI - expose charge percent 0..1
    public float GetBusterChargePercent() => (chargeMax <= Mathf.Epsilon) ? 0f : Mathf.Clamp01(_currentCharge / chargeMax);

    // Debug draw (dejado vacío pero preparado para ser usado)
    private void OnDrawGizmosSelected()
    {
        /* ejemplo:
        if (saberPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(saberPoint.position, saberRange);
        }*/
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isPaused) return;

        Combos_();
        HandleBusterInput();

        // recarga pasiva de buster
        if (!_isCharging && _currentCharge < chargeMax)
        {
            _currentCharge = Mathf.Min(chargeMax, _currentCharge + busterRechargeRate * Time.deltaTime);
        }
    }
}