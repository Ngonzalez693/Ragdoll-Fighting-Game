using UnityEngine;

public class SyncPhysics : MonoBehaviour
{
    private Rigidbody _rb;
    private ConfigurableJoint joint;

    [SerializeField] private Rigidbody animatedRigidbody;
    [SerializeField] private bool syncAnimation = false;

    private float _startSlerpPositionSpring = 0.0f;

    private Quaternion startLocalRotation;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        joint = GetComponent<ConfigurableJoint>();
        startLocalRotation = transform.localRotation;
        _startSlerpPositionSpring = joint.slerpDrive.positionSpring;
    }

    public void UpdateJointFromAnimation()
    {
        if (!syncAnimation) { return; }

        ConfigurableJointExtensions.SetTargetRotationLocal(joint, animatedRigidbody.transform.localRotation, startLocalRotation);
    }

    public void MakeRagdoll()
    {
        JointDrive jointDrive = joint.slerpDrive;
        jointDrive.positionSpring = 0.001f;
        joint.slerpDrive = jointDrive;
    }

    public void MakeActiveRagdoll()
    {
        JointDrive jointDrive = joint.slerpDrive;
        jointDrive.positionSpring = _startSlerpPositionSpring;
        joint.slerpDrive = jointDrive;
    }
}
