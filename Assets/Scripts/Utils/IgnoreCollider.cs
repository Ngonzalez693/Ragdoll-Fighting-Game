using UnityEngine;

public class IgnoreCollider : MonoBehaviour
{
    [SerializeField] private Collider _thisCollider;
    [SerializeField] private Collider[] _colliderToIgnore;

    private void Start()
    {
        foreach(Collider otherCollider in _colliderToIgnore)
        {
            Physics.IgnoreCollision(_thisCollider, otherCollider, true);
        }
    }
}
