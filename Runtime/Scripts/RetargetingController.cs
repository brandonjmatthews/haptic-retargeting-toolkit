using UnityEngine;
using UnityEngine.Events;
namespace HRTK
{
    // public enum RetargetingStatus {
    //     Disabled,
    //     HandAtOrigin,
    //     HandAtTarget,
    //     Retargeting,
    // }
    [System.Serializable]
    public struct RetargetingStatus {
        public bool Enabled;
        public bool AtOrigin;
        public bool AtTarget;
        public bool HasTargets;
        public bool BetweenOriginAndTargets;

        public override string ToString() {
            return "Enabled: " + Enabled + 
                 ", AtOrigin: " + AtOrigin + 
                 ", AtTarget: " + AtTarget + 
                 ", HasTargets: " + HasTargets + 
                 ", BetweenOriginAndTargets: " + BetweenOriginAndTargets; 
        }
    }

    public class RetargetingController : MonoBehaviour
    {
        public bool RetargetingEnabled;

        [Header("Modifications")]
        public bool UseOnTheFlyOrigin;


        [Header("Hand Setup")]
        public Chirality whichHand;
        public Chirality Hand
        {
            get
            {
                return whichHand;
            }
            set
            {
                whichHand = value;
            }
        }

        public RetargetingHand TrackedHand;
        public RetargetingHand VirtualHand;

        [Header("Origin and Reset")]
        public RetargetingOrigin Origin;
        public RetargetingReset Reset;
        public UnityEvent HandAtOrigin;

        [Header("Targets")]
        [SerializeField]
        VirtualTarget _currentVirtualTarget;
        [SerializeField]
        TrackedTarget _currentTrackedTarget;

        VirtualTarget _previousVirtualTarget;
        TrackedTarget _previousTrackedTarget;

        public UnityEvent HandAtTarget;

        public VirtualTarget VirtualTarget
        {
            get
            {
                return _currentVirtualTarget;
            }
        }

        public TrackedTarget TrackedTarget
        {
            get
            {
                return _currentTrackedTarget;
            }
        }

        public TrackedTarget PreviousTrackedTarget => _previousTrackedTarget;
        public VirtualTarget PreviousVirtualTarget => _previousVirtualTarget;

        [Header("Ratio Filter")]
        public FilterFunction ratioFilter;
        [Range(0.0f, 0.95f)]
        public float smoothingTrimMin = 0.0f;
        [Range(0.05f, 1.0f)]
        public float smoothingTrimMax = 1.0f;

        [SerializeField]
        protected RetargetingOffsetHandler offsetHandler;
        public RetargetingOffsetHandler OffsetHandler => offsetHandler; 

        [SerializeField] DistanceResult originDistance;
        [SerializeField] DistanceResult targetDistance;
        float ratio;
        public float Ratio => ratio;

        [SerializeField][ReadOnly("Status")]
        protected RetargetingStatus _status;
        public RetargetingStatus Status => _status;

        private void Start()
        {
            if (offsetHandler == null)
            {
                offsetHandler = GetComponent<RetargetingOffsetHandler>();
                if (offsetHandler == null)
                {
                    Debug.LogWarning("No RetargetingOffsetHandler found. Default handler will be created.");
                    offsetHandler = gameObject.AddComponent<RetargetingOffsetHandler>();
                }
            }

            offsetHandler.Initialize(TrackedHand, VirtualHand);

            if (Reset != null) Reset.Hand = this;

            _status = new RetargetingStatus() {
                Enabled = RetargetingEnabled,
                AtOrigin = false,
                AtTarget = false,
                HasTargets = false,
                BetweenOriginAndTargets = false
            };
        }

        private void Update()
        {
            if (TrackedHand == null || VirtualHand == null || offsetHandler == null) return;


            // Compute origin distance
            if (Origin != null)
            {
                // Distance between the given position and the warp origin/region
                originDistance = TrackedHand.OriginDistance(Origin);
                if (originDistance.intersecting == 1) originDistance.distance = 0.0f;
                if (originDistance.distance <= 0.0f) {
                    _status.AtOrigin = true;
                    HandAtOrigin.Invoke();
                } else {
                    _status.AtOrigin = false;
                }
            }

            // Compute Target Distance
            if (TrackedTarget != null && TrackedHand != null)
            {
                // Distance between the given position and the current physical target
                targetDistance = TrackedHand.TargetDistance(TrackedTarget);
                if (targetDistance.intersecting == 1) targetDistance.distance = 0.0f;
                if (targetDistance.distance <= 0.0f){
                    _status.AtTarget = true;
                    HandAtTarget.Invoke();
                 } else {
                    _status.AtTarget = false;
                }
            }


            if (TrackedTarget != null && VirtualTarget != null)
            {
                _status.HasTargets = true;
                
                // Calculate the ratio
                if (targetDistance.distance <= 0.0f) {
                    ratio = 1.0f;
                }
                else if (originDistance.distance <= 0.0f) {
                    ratio = 0.0f;
                }
                else {
                    ratio = originDistance.distance / (originDistance.distance + targetDistance.distance);
                    _status.BetweenOriginAndTargets = true;
                }

                // Clamp to ensure ratio stays between 0 and 1
                ratio = Mathf.Clamp(ratio, 0.0f, 1.0f);

                // Apply the filter function to the ratio to smooth
                ratio = FilterRatio(ratio);

                if (RetargetingEnabled)
                {
                    _status.Enabled = true;
                    offsetHandler.ApplyRetargetingOffset(ratio, Origin, TrackedTarget, VirtualTarget, TrackedHand, VirtualHand);
                }
                else
                {
                    _status.Enabled = false;
                    ApplyZeroRetargeting();
                }
            }
        }


        void ApplyZeroRetargeting()
        {
            ratio = 0;
            offsetHandler.ApplyRetargetingOffset(ratio, Origin, TrackedTarget, VirtualTarget, TrackedHand, VirtualHand);
        }
        // void ApplyRetargetingOffset() {
          
                    // Vector3 worldScaleDifference = VirtualTarget.transform.lossyScale - TrackedTarget.transform.lossyScale;
        //     // if (HandleSizeDifference) {
        //     //     Vector3 scaleDiff = TrackedTarget.transform.lossyScale - VirtualTarget.transform.localScale;
        //     //     Vector3 toNearestPoint = targetDistance.pointB - TrackedTarget.position;
        //     // }
        // }

        float FilterRatio(float ratio)
        {
            float adjustedRatio = Util.MapValue(0.0f, 1.0f, smoothingTrimMin, smoothingTrimMax, ratio);
            float from = 0.0f;
            float to = 1.0f;
            float min = 0.0f;
            float max = 1.0f;
            switch (ratioFilter)
            {
                case FilterFunction.SmoothStep:
                    min = Util.SmoothStep(from, to, smoothingTrimMin);
                    max = Util.SmoothStep(from, to, smoothingTrimMax);
                    ratio = Util.SmoothStep(from, to, adjustedRatio);
                    ratio = Util.MapValue(min, max, 0.0f, 1.0f, ratio);
                    break;
                case FilterFunction.SmootherStep:
                    min = Util.SmootherStep(from, to, smoothingTrimMin);
                    max = Util.SmootherStep(from, to, smoothingTrimMax);
                    ratio = Util.SmootherStep(from, to, adjustedRatio);
                    ratio = Util.MapValue(min, max, 0.0f, 1.0f, ratio);
                    break;
                case FilterFunction.SmoothestStep:
                    min = Util.SmoothestStep(from, to, smoothingTrimMin);
                    max = Util.SmoothestStep(from, to, smoothingTrimMax);
                    ratio = Util.SmoothestStep(from, to, adjustedRatio);
                    ratio = Util.MapValue(min, max, 0.0f, 1.0f, ratio);
                    break;
            }

            return ratio;
        }

        public void SetTargets(TrackedTarget trackedTarget, VirtualTarget virtualTarget)
        {
            _previousTrackedTarget = _currentTrackedTarget;
            _previousVirtualTarget = _currentVirtualTarget;

            if (_previousTrackedTarget != null) _previousTrackedTarget.Deselect();
            if (_previousVirtualTarget != null) _previousVirtualTarget.Deselect();

            _currentTrackedTarget = trackedTarget;
            _currentVirtualTarget = virtualTarget;

            if (_currentTrackedTarget != null) _currentTrackedTarget.Select();
            if (_currentVirtualTarget != null) _currentVirtualTarget.Select();

            ConfigureOriginAndReset();
        }

        public void SetTarget(TrackedTarget trackedTarget)
        {
            SetTargets(trackedTarget, VirtualTarget);
        }

        public void SetTarget(VirtualTarget virtualTarget)
        {
            SetTargets(TrackedTarget, virtualTarget);
        }

        void ConfigureOriginAndReset()
        {
            if (UseOnTheFlyOrigin) 
            {
                Debug.Log("Updated origin");
                Origin.UpdateOrigin(TrackedHand.OriginContact.position, offsetHandler.PositionOffset, offsetHandler.RotationOffset);
            }

            if (Reset != null && TrackedTarget != null && VirtualTarget != null) Reset.ConfigureReset(TrackedTarget, VirtualTarget, PreviousTrackedTarget, PreviousVirtualTarget);
        }
    }
}