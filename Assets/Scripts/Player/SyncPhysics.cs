using UnityEngine;

public class SyncPhysics : MonoBehaviour
{
    private Rigidbody _rb;
    private ConfigurableJoint _joint;

    [SerializeField] private Rigidbody animateRigidbody;
    [SerializeField] private bool syncAnimation = false;

    private Quaternion _startLocalRotation;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _joint = GetComponent<ConfigurableJoint>();

        _startLocalRotation = _joint.transform.localRotation;
    }

    public void updateJointFromAnimation()
    {
        if(!syncAnimation) return;

        ConfigurableJointExtensions.SetTargetRotationLocal(_joint, animateRigidbody.transform.localRotation, _startLocalRotation);
    }
}
