using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

/// <summary>
/// Controlador simple tipo "ragdoll" con:
/// - Movimiento por fuerza
/// - Rotación por ConfigurableJoint
/// - Salto (solo si está en el piso)
/// - Ataque izquierdo:
///     * Tap/Press -> Puñetazo
///     * Hold      -> Levanta mano e intenta agarrar
/// </summary>
public class RagdollController : MonoBehaviour
{
    [Header("----- REFERENCES -----")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private ConfigurableJoint _joint;
    [SerializeField] private Animator _animator;

    [Header("----- SETTINGS -----")]
    [SerializeField] private float _movementSpeed = 10f;
    [SerializeField] private float _rotationSmoothTime = 250f; // grados/seg aprox (depende de tu setup)

    // Jump
    [Header("----- JUMP -----")]
    [SerializeField] private float _jumpForce = 25f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundCheckDistance = 0.7f; // ajusta según tu personaje

    private bool _isJumping;

    // Punch (izquierdo)
    [Header("----- PUNCH (LEFT) -----")]
    [SerializeField] private Rigidbody _upperArmLeft;
    [SerializeField] private Rigidbody _lowerArmLeft;
    [SerializeField] private float _punchForce = 0.15f;
    [SerializeField] private float _punchDuration = 0.5f;

    private bool _isPunchingLeft;
    private Coroutine _punchCoroutineLeft;

    // Grab (izquierdo)
    [Header("----- GRAB (LEFT) -----")]
    [SerializeField] private Transform _grabPointLeft;
    [SerializeField] private float _grabRadius = 0.3f;
    [SerializeField] private float _grabDelay = 0.5f;

    private bool _isRaisingHandLeft;
    private bool _isTryingGrabLeft;
    private GameObject _grabbedObjectLeft;
    private Coroutine _grabCoroutineLeft;

    // Animator
    private readonly int _speedHash = Animator.StringToHash("Speed");

    // Sync ragdoll joints
    private SyncPhysics[] _syncPhysics;

    // Input
    private Vector2 _direction;

    private void Awake()
    {
        _syncPhysics = GetComponentsInChildren<SyncPhysics>();
    }

    private void FixedUpdate()
    {
        if (_rb == null)
        {
            Debug.LogWarning("Rigidbody is not assigned");
            return;
        }

        // Rotación (evita LookRotation con vector cero)
        Rotate();

        // Sincroniza joints con animación (si tu sistema lo requiere)
        UpdateJointRotation();

        // Movimiento por fuerza
        Move();

        // Salto (se dispara una sola vez cuando _isJumping es true)
        if (_isJumping)
            Jump();

        // Puñetazo aplica fuerzas mientras dura la ventana del golpe
        if (_isPunchingLeft)
            PerformPunchLeft();

        // Si estás en modo levantar mano (hold), intenta agarrar sin spamear corutinas
        if (_isRaisingHandLeft)
        {
            if (!_isTryingGrabLeft && _grabCoroutineLeft == null)
                _grabCoroutineLeft = StartCoroutine(TryGrabLeftRoutine());
        }
        else
        {
            // Si sueltas el hold: parar intento y soltar objeto
            if (_grabCoroutineLeft != null)
            {
                StopCoroutine(_grabCoroutineLeft);
                _grabCoroutineLeft = null;
            }

            _isTryingGrabLeft = false;
            ReleaseGrabLeft();
        }
    }

    // --------------------
    // INPUT CALLBACKS
    // --------------------

    public void OnMove(InputAction.CallbackContext ctx)
    {
        _direction = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        // Importante: ctx.performed + grounded para no saltar en el aire
        if (ctx.performed && IsGrounded())
            _isJumping = true;
    }

    /// <summary>
    /// AttackLeft:
    /// - HoldInteraction: levantar mano y agarrar mientras mantienes
    /// - Cualquier otra interacción (Tap/Press): puñetazo al presionar
    /// </summary>
    public void OnAttackLeft(InputAction.CallbackContext ctx)
    {
        // HOLD -> levantar mano + intentar agarrar
        if (ctx.interaction is HoldInteraction)
        {
            if (ctx.performed) _isRaisingHandLeft = true;
            if (ctx.canceled) _isRaisingHandLeft = false;
            return;
        }

        // TAP/PRESS -> puñetazo
        if (ctx.performed)
        {
            // Evita apilar corutinas de punch
            if (_punchCoroutineLeft != null) StopCoroutine(_punchCoroutineLeft);
            _punchCoroutineLeft = StartCoroutine(PunchLeftRoutine());
        }
    }

    // --------------------
    // MOVEMENT / ROTATION
    // --------------------

    private void Move()
    {
        Vector3 moveDirection = new Vector3(_direction.x, 0f, _direction.y);

        _rb.AddForce(moveDirection * _movementSpeed, ForceMode.Acceleration);

        if (_animator != null)
            _animator.SetFloat(_speedHash, moveDirection.magnitude);
    }

    private void Rotate()
    {
        if (_joint == null) return;

        // OJO: En tu código original invertías X para rotar; lo dejo igual.
        Vector3 inputDirection = new Vector3(-_direction.x, 0f, _direction.y).normalized;

        // Evita "Look rotation viewing vector is zero"
        if (inputDirection.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(inputDirection, Vector3.up);

        // RotateTowards: el 3er parámetro es "max degrees delta"
        float maxDegrees = _rotationSmoothTime * Time.fixedDeltaTime;
        _joint.targetRotation = Quaternion.RotateTowards(_joint.targetRotation, targetRotation, maxDegrees);
    }

    private void UpdateJointRotation()
    {
        if (_syncPhysics == null) return;

        for (int i = 0; i < _syncPhysics.Length; i++)
            _syncPhysics[i].UpdateJointFromAnimation();
    }

    private bool IsGrounded()
    {
        // Distancia configurable para que no falle por tamaño/pivot
        return Physics.Raycast(_rb.position, Vector3.down, _groundCheckDistance, _groundLayer);
    }

    // --------------------
    // JUMP
    // --------------------

    private void Jump()
    {
        _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        _isJumping = false;
    }

    // --------------------
    // PUNCH (LEFT)
    // --------------------

    private void PerformPunchLeft()
    {
        if (_lowerArmLeft == null || _upperArmLeft == null) return;

        // Golpe siempre hacia delante del personaje
        Vector3 punchDirection = transform.forward.normalized;

        // Fuerza pequeña en brazo superior + fuerza principal en antebrazo
        _upperArmLeft.AddForce(punchDirection * 0.05f, ForceMode.Impulse);
        _lowerArmLeft.AddForce(punchDirection * _punchForce, ForceMode.Impulse);
    }

    private IEnumerator PunchLeftRoutine()
    {
        _isPunchingLeft = true;

        yield return new WaitForSeconds(_punchDuration);

        _isPunchingLeft = false;
        _punchCoroutineLeft = null;
    }

    // --------------------
    // GRAB (LEFT)
    // --------------------

    private IEnumerator TryGrabLeftRoutine()
    {
        if (_grabPointLeft == null)
        {
            Debug.LogWarning("GrabPointLeft is not assigned");
            _grabCoroutineLeft = null;
            yield break;
        }

        _isTryingGrabLeft = true;

        // Sube el brazo antes de buscar
        RaiseArmLeft();

        // Espera para dar tiempo a que la mano llegue
        yield return new WaitForSeconds(_grabDelay);

        // Si en este lapso soltaste el botón, no intentes agarrar
        if (!_isRaisingHandLeft)
        {
            _isTryingGrabLeft = false;
            _grabCoroutineLeft = null;
            yield break;
        }

        // Si ya tienes algo agarrado, no vuelvas a crear joint
        if (_grabbedObjectLeft != null)
        {
            _isTryingGrabLeft = false;
            _grabCoroutineLeft = null;
            yield break;
        }

        Collider[] colliders = Physics.OverlapSphere(_grabPointLeft.position, _grabRadius);

        foreach (Collider col in colliders)
        {
            // Ignora tu propio cuerpo
            if (col.transform.IsChildOf(transform))
                continue;

            // Necesitamos un rigidbody para conectar un FixedJoint
            if (col.attachedRigidbody == null)
                continue;

            // Evita crear 2 joints:
            // Si existe, úsalo; si no, créalo.
            FixedJoint joint = _grabPointLeft.GetComponent<FixedJoint>();
            if (joint == null)
                joint = _grabPointLeft.gameObject.AddComponent<FixedJoint>();

            joint.connectedBody = col.attachedRigidbody;

            // Ajustes típicos
            joint.breakForce = 500f;
            joint.breakTorque = 500f;

            // Nota: Estos anchors dependen de tu setup. Esto es "simple" y suele bastar.
            joint.autoConfigureConnectedAnchor = true;

            _grabbedObjectLeft = col.gameObject;
            break;
        }

        _isTryingGrabLeft = false;
        _grabCoroutineLeft = null;
    }

    private void ReleaseGrabLeft()
    {
        if (_grabPointLeft == null) return;

        // Destruye el joint SIEMPRE que exista (aunque _grabbedObjectLeft sea null por un bug)
        FixedJoint joint = _grabPointLeft.GetComponent<FixedJoint>();
        if (joint != null)
            Destroy(joint);

        _grabbedObjectLeft = null;
    }

    private void RaiseArmLeft()
    {
        if (_lowerArmLeft == null) return;

        // Levanta mano hacia adelante y arriba
        Vector3 raiseDirection = transform.forward + Vector3.up * 0.6f;
        _lowerArmLeft.AddForce(raiseDirection.normalized * 0.05f, ForceMode.Impulse);
    }

#if UNITY_EDITOR
    // Gizmo para ver el radio de agarre en la escena
    private void OnDrawGizmosSelected()
    {
        if (_grabPointLeft == null) return;

        Gizmos.DrawWireSphere(_grabPointLeft.position, _grabRadius);
    }
#endif
}