using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.UIElements;

public class RagdollController : MonoBehaviour
{
    [Header("--- References ---")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private ConfigurableJoint _mainJoint;
    [SerializeField] private Animator _animator;
    [SerializeField] private Rigidbody _upperArmLeft;
    [SerializeField] private Rigidbody _lowerArmLeft;
    [SerializeField] private Rigidbody _upperArmRight;
    [SerializeField] private Rigidbody _lowerArmRight;
    [SerializeField] private Transform _grabPointLeft;
    [SerializeField] private Transform _grabPointRight;
    [SerializeField] private SphereCollider _hitboxLeft;
    [SerializeField] private SphereCollider _hitboxRight;

    [Space]
    [SerializeField] private ParticleSystem walkingDust;

    [Header("--- Settings ---")]
    [SerializeField] private float _movementSpeed = 10f;
    [SerializeField] private float _rotationSmoothTime = 5f;
    [SerializeField] private float _jumpForce = 15f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _punchForce = .15f;
    [SerializeField] private float _punchDuration = .5f;

    private float _startSlerpPositionSpring = 0.0f;
    private bool _isActiveRagdoll = true;
    private Rigidbody[] _rigidbodies;
    private float[] _startRigidbodiesMass;
    // Animator parameters
    private int _speedAnimation = Animator.StringToHash("Speed");

    private SyncPhysics[] syncPhysics;

    private Vector2 direction;
    private bool _isJumping;
    private bool _isPunchingLeft;
    private bool _isPunchingRight;
    private bool _isRaisingArmLeft;
    private bool _isRaisingArmRight;

    private GameObject _grabbedObjectLeft;
    private GameObject _grabbedObjectRight;

    private void OnEnable()
    {
        syncPhysics = GetComponentsInChildren<SyncPhysics>();
        _rigidbodies = GetComponentsInChildren<Rigidbody>();
        _startRigidbodiesMass = new float[_rigidbodies.Length];

        for (int i = 0; i < _rigidbodies.Length; i++)
            _startRigidbodiesMass[i] = _rigidbodies[i].mass;

        if (_mainJoint != null)
            _startSlerpPositionSpring = _mainJoint.slerpDrive.positionSpring;
    }

    private void FixedUpdate()
    {
        if (!_isActiveRagdoll) { return; }

        Rotate();
        UpdateJointsRotation();

        if (_rb  != null )
        {
            Move();

            if (_isJumping)
            {
                Jump();
            }

            if ( _isPunchingLeft)
            {
                PerformPunchLeft();
            }
            if (_isPunchingRight)
            {
                PerformPunchRight();
            }

            if (_isRaisingArmLeft)
            {
                StartCoroutine(TryGrabLeftRoutine());
            }
            else if (!_isRaisingArmLeft)
            {
                ReleaseGrabLeft();
            }

            if (_isRaisingArmRight)
            {
                StartCoroutine(TryGrabRightRoutine());
            }
            else if (!_isRaisingArmRight)
            {
                ReleaseGrabRight();
            }
        }
        else
        {
            Debug.LogWarning("Rigidbody is not assigned!");
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        direction = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && IsGrounded())
        {
            _isJumping = true;
        }
    }

    public void OnAttackLeft(InputAction.CallbackContext ctx)
    {
        if (ctx.interaction is HoldInteraction)
        {
            if (ctx.performed)
            {
                _isRaisingArmLeft = true;
            }
            else if (ctx.canceled)
            {
                _isRaisingArmLeft = false;
            }

        }
        if (ctx.performed)
        {
            StartCoroutine(PunchLeftRoutine());
        }
    }

    public void OnAttackRight(InputAction.CallbackContext ctx)
    {
        if (ctx.interaction is HoldInteraction)
        {
            if (ctx.performed)
            {
                _isRaisingArmRight = true;
            }
            else if (ctx.canceled)
            {
                _isRaisingArmRight = false;
            }

        }
        if (ctx.performed)
        {
            StartCoroutine(PunchRightRoutine());
        }
    }

    private void Move()
    {
        Vector3 moveDirection = new Vector3(direction.x, 0, direction.y);
        float moveMagnitude = moveDirection.magnitude;
        _animator.SetFloat(_speedAnimation, moveMagnitude);

        if (IsGrounded())
        {
            _rb.AddForce(moveDirection * _movementSpeed, ForceMode.VelocityChange);

            if (moveMagnitude > 0.5 && !walkingDust.isPlaying)
                walkingDust.Play();
            else if (moveMagnitude <= 0.5 && walkingDust.isPlaying)
                walkingDust.Stop();
        }
        else
        {
            _rb.AddForce(moveDirection * (_movementSpeed / 2.5f), ForceMode.VelocityChange);

            if (walkingDust.isPlaying)
            walkingDust.Stop();
        }
    }

    private void Jump()
    {
        _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        _isJumping = false;
    }

    private void Rotate()
    {
        if (direction != Vector2.zero)
        {
            Vector3 inputDirection = new Vector3(-direction.x, 0, direction.y).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(inputDirection, Vector3.up);

            _mainJoint.targetRotation = Quaternion.RotateTowards(_mainJoint.targetRotation, targetRotation, _rotationSmoothTime * Time.fixedDeltaTime);
        }
    }

    private void UpdateJointsRotation()
    {
        for (int i = 0; i < syncPhysics.Length; i++)
        {
            syncPhysics[i].UpdateJointFromAnimation();
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(_rb.position, Vector3.down, .5f, _groundLayer);
    }

    private void PerformPunchLeft()
    {
        if (_lowerArmLeft != null)
        {
            Vector3 punchDirection = transform.forward.normalized;
            Vector3 uppArmDirection = punchDirection * -1;
            _upperArmLeft.AddForce(punchDirection * 0.05f, ForceMode.Impulse);
            _lowerArmLeft.AddForce(punchDirection * _punchForce, ForceMode.Impulse);

            _hitboxLeft.enabled = true;
            StartCoroutine(DisableHitboxAfterTime(_hitboxLeft, 0.25f));
        }
    }

    private void PerformPunchRight()
    {
        if (_lowerArmRight != null)
        {
            Vector3 punchDirection = transform.forward.normalized;
            Vector3 uppArmDirection = punchDirection * -1;
            _upperArmRight.AddForce(punchDirection * 0.05f, ForceMode.Impulse);
            _lowerArmRight.AddForce(punchDirection * _punchForce, ForceMode.Impulse);

            _hitboxRight.enabled = true;
            StartCoroutine(DisableHitboxAfterTime(_hitboxRight, 0.25f));
        }
    }

    private IEnumerator PunchLeftRoutine()
    {
        _isPunchingLeft = true;

        yield return new WaitForSeconds(_punchDuration);

        _isPunchingLeft = false;
    }

    private IEnumerator PunchRightRoutine()
    {
        _isPunchingRight = true;

        yield return new WaitForSeconds(_punchDuration);

        _isPunchingRight = false;
    }

    private IEnumerator DisableHitboxAfterTime(Collider hitbox, float time)
    {
        yield return new WaitForSeconds(time);
        hitbox.enabled = false;
    }

    private IEnumerator TryGrabLeftRoutine()
    {
        RaiseArmLeft();

        yield return new WaitForSeconds(0.5f);

        Collider[] colliders = Physics.OverlapSphere(_grabPointLeft.position, 0.2f);

        foreach (var collider in colliders)
        {
            if (collider.transform.IsChildOf(transform)) { continue; }

            if (collider.attachedRigidbody != null)
            {
                FixedJoint joint = _grabPointLeft.gameObject.GetComponent<FixedJoint>();

                if (joint == null)
                {
                    joint = _grabPointLeft.gameObject.AddComponent<FixedJoint>();
                }

                joint.connectedBody = collider.attachedRigidbody;

                joint.breakForce = 500f;
                joint.breakTorque = 500f;
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = collider.transform.InverseTransformPoint(collider.transform.position);

                _grabbedObjectLeft = collider.gameObject;
                break;
            }    
        }
    }

    private void ReleaseGrabLeft()
    {
        if (_grabbedObjectLeft != null)
        {
            var joint = _grabPointLeft.gameObject.GetComponent<FixedJoint>();
            if (joint != null)
            {
                Destroy(joint);
            }

            _grabbedObjectLeft = null;
        }
    }

    private void RaiseArmLeft()
    {
        if (_lowerArmLeft != null)
        {
            Vector3 raiseDirection = transform.forward + Vector3.up * 0.6f;

            _lowerArmLeft.AddForce(raiseDirection * 0.05f, ForceMode.Impulse);
        }
    }

    private IEnumerator TryGrabRightRoutine()
    {
        RaiseArmRight();

        yield return new WaitForSeconds(0.5f);

        Collider[] colliders = Physics.OverlapSphere(_grabPointRight.position, 0.2f);

        foreach (var collider in colliders)
        {
            if (collider.transform.IsChildOf(transform)) { continue; }

            if (collider.attachedRigidbody != null)
            {
                FixedJoint joint = _grabPointRight.gameObject.GetComponent<FixedJoint>();

                if (joint == null)
                {
                    joint = _grabPointRight.gameObject.AddComponent<FixedJoint>();
                }

                joint.connectedBody = collider.attachedRigidbody;

                joint.breakForce = 500f;
                joint.breakTorque = 500f;
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = collider.transform.InverseTransformPoint(collider.transform.position);

                _grabbedObjectRight = collider.gameObject;
                break;
            }
        }
    }

    private void ReleaseGrabRight()
    {
        if (_grabbedObjectRight != null)
        {
            var joint = _grabPointRight.gameObject.GetComponent<FixedJoint>();
            if (joint != null)
            {
                Destroy(joint);
            }

            _grabbedObjectRight = null;
        }
    }

    private void RaiseArmRight()
    {
        if (_lowerArmRight != null)
        {
            Vector3 raiseDirection = transform.forward + Vector3.up * 0.6f;

            _lowerArmRight.AddForce(raiseDirection * 0.05f, ForceMode.Impulse);
        }
    }

    public void MakeRagdoll()
    {
        JointDrive jointDrive = _mainJoint.slerpDrive;
        jointDrive.positionSpring = 0;
        _mainJoint.slerpDrive = jointDrive;

        for (int i = 0; i < syncPhysics.Length; i++)
        {
            syncPhysics[i].MakeRagdoll();
        }

        for (int i = 0; i < _rigidbodies.Length; i++)
        {
            _rigidbodies[i].mass = 0.001f;
        }

        _isActiveRagdoll = false;
    }

    public void MakeActiveRagdoll()
    {
        JointDrive jointDrive = _mainJoint.slerpDrive;
        jointDrive.positionSpring = _startSlerpPositionSpring;
        _mainJoint.slerpDrive = jointDrive;

        for (int i = 0; i < syncPhysics.Length; i++)
        {
            syncPhysics[i].MakeActiveRagdoll();
        }

        for (int i = 0; i < _rigidbodies.Length; i++)
        {
            _rigidbodies[i].mass = _startRigidbodiesMass[i];
        }

        _isActiveRagdoll = true;
    }
}
