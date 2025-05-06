using UnityEngine;
namespace HRTK.MaskedRetargeting
{
    public class IKHand : MonoBehaviour
    {
        [SerializeField] private Transform hand;
        [SerializeField] private Vector3 positionOffset, rotationOffset;

        void LateUpdate()
        {
            transform.position = hand.TransformPoint(positionOffset);
            transform.rotation = hand.rotation * Quaternion.Euler(rotationOffset);
        }
    }
}