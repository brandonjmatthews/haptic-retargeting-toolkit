using UnityEngine;

namespace HRTK
{
    public class RetargetingHand : MonoBehaviour
    {

        [SerializeField]
        HandType _handType;   
        public HandType HandType {
            get => _handType;
            set { _handType = value; }
        }   

        [SerializeField]
        Transform _palm;
        public Transform Palm => _palm;

        [SerializeField]
        Transform _originContact;
        public Transform OriginContact => _originContact;

        [SerializeField]
        RetargetingShape _shape;
        public RetargetingShape Shape => _shape;

        public Transform indexTip;
        public Transform thumbTip;


        private void OnValidate() {
            if (_shape == null) {
                RetargetingShape s = GetComponent<RetargetingShape>();
                if (s != null) _shape = s;
            }
        }

        public virtual DistanceResult TargetDistance(TrackedTarget target)
        {
            if (target.Shape == null) throw new System.Exception("Target Shape must be provided");
            if (this.Shape == null) throw new System.Exception("Hand Shape must be provided");

            return Shape.ClosestPoints(target.Shape);
        }

        public virtual DistanceResult OriginDistance(RetargetingOrigin origin)
        {
            if (origin.Shape == null) throw new System.Exception("Origin Shape must be provided");
            if (this.Shape == null) throw new System.Exception("Hand Shape must be provided");

            return Shape.ClosestPoints(origin.Shape);
        }
    }    
}