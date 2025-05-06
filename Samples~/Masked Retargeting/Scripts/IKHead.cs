using UnityEngine;
namespace HRTK.MaskedRetargeting
{
    public class IKHead : MonoBehaviour
    {
        [SerializeField] private Transform bodyRoot, hmd;
        [SerializeField] private Vector3 positionOffset, rotationOffset, headBodyOffset;

        void LateUpdate()
        {
            bodyRoot.position = transform.position + headBodyOffset;
            Vector3 forward = Vector3.ProjectOnPlane(hmd.forward, Vector3.up).normalized;
            if (forward.magnitude != 0.0f) bodyRoot.forward = forward;

            transform.position = hmd.TransformPoint(positionOffset);
            transform.rotation = hmd.rotation * Quaternion.Euler(rotationOffset);
        }
    }
}