using UnityEngine;
using UnityEngine.Events;

namespace HRTK
{
    public class RetargetingOrigin : MonoBehaviour
    {
        [SerializeField]
        RetargetingShape _shape;
        public RetargetingShape Shape => _shape;
        
        public Vector3 PositionOffset = Vector3.zero;
        public Quaternion RotationOffset = Quaternion.identity;

        public Vector3 Position => transform.position;
        public Vector3 VirtualPosition => transform.position + PositionOffset;

        public Transform VirtualOrigin;

        public void UpdateOrigin(Vector3 origin, Vector3 positionOffset)
        {
            transform.position = origin;
            if (VirtualOrigin != null) VirtualOrigin.transform.position = origin + positionOffset;
            PositionOffset = positionOffset;
        }

        public void UpdateOrigin(Vector3 origin, Vector3 positionOffset, Quaternion rotationOffset)
        {
            transform.position = origin;
            if (VirtualOrigin != null) VirtualOrigin.transform.position = origin + positionOffset;
            PositionOffset = positionOffset;
        }
    }
}
