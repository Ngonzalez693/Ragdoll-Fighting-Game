using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

/// <summary>
/// Simple ragdoll controller
/// Force based movement
/// rotation by ConfigurableJoint
/// jumping only if its on the floor
/// atack: left & right
/// tap/press: punch
/// hold: raise hand & grab
/// </summary>
public class RagdollController : MonoBehaviour
{
    [Header("--- REFERENCES ---")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private ConfigurableJoint _joint;
    [SerializeField] private Animator _animator;

    [Header("--- SETTINGS ---")]
    [SerializeField] private float _movementSpeed = 20f;
    [SerializeField] private float _rotationSmoothTime = 250f;
    
    [Header("--- JUMP ---")]
    [SerializeField] private float _jumpForce = 25f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundCheckDistance = 0.7f;
    private bool _isJumping = false;

    // Left Punch
    // Right Punch
    // Grab

    // Conditions to grab something

    // Animator
    private readonly int _speedHash = Animator.StringToHash("Speed");

    // Sync Ragdoll
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

        Rotate();
        Move();
    }

    // Input callbacks
    public void OnMove(InputAction.CallbackContext ctx)
    {
        _direction = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            _isJumping = true;
        }
    }
}
