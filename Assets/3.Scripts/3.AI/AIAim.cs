using UnityEngine;

public class AIAim : MonoBehaviour
{
    [SerializeField] private Transform rotationTarget;

    private void Awake()
    {
        if (rotationTarget == null)
        {
            rotationTarget = transform;
        }
    }

    public void FaceTarget(Vector3 targetPosition)
    {
        Vector3 aimDirection = targetPosition - rotationTarget.position;

        aimDirection.y = 0f;

        if (aimDirection.sqrMagnitude <= 0.001f)
        {
            return;
        }

        rotationTarget.rotation = Quaternion.LookRotation(aimDirection);
    }
}