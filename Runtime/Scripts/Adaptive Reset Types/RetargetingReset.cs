using UnityEngine;
using UnityEngine.Events;

namespace HRTK
{

    public enum ResetType
    {
        Static,
        Adaptive
    }

    [System.Serializable]
    public class ResetEvent : UnityEvent {}

    public abstract class RetargetingReset : MonoBehaviour
    {
        public ResetType ResetType;
        public RetargetingController Hand;
        public Transform Head;

        protected bool awaitingReset;
        public bool AwaitingReset => awaitingReset;

        [Header("Static Reset Distance")]
        public float StaticDistance;

        [Header("Adaptive Reset Configuration")]
        public float AdaptiveMaxAngle;
        public float AdaptiveMaxTranslation;
        public float AdaptiveTolerance;
        public float AdaptiveMaxDistance;
        [Header("Reset Events")]
        public ResetEvent OnResetComplete;

        void Start() {
            SetVisible(awaitingReset);
        }

        void SetVisible(bool visible) {
            gameObject.SetActive(visible);
        }

        public void AwaitReset() {
            SetVisible(true);
            awaitingReset = true;
        }

        public void StopAwaitingReset() {
            SetVisible(false);
            awaitingReset = false;
        }
        
        protected virtual void Update() {
            if (awaitingReset) {
                if (CheckResetComplete()) {
                    OnResetComplete.Invoke();
                    StopAwaitingReset();
                }
            }
        }

        protected abstract bool CheckResetComplete();

        public abstract void ConfigureInitialReset();

        public virtual void ConfigureReset(TrackedTarget currentTracked, VirtualTarget currentVirtual, TrackedTarget nextTracked = null, VirtualTarget nextVirtual = null) {
            if (currentTracked == null || currentVirtual == null) return;

            if (ResetType == ResetType.Static) {
                ConfigureStaticReset(currentTracked, currentVirtual, nextTracked, nextVirtual);
            } else {
                ConfigureAdaptiveReset(currentTracked, currentVirtual, nextTracked, nextVirtual);
            }
        }

        public abstract void ConfigureStaticReset(TrackedTarget currentTracked, VirtualTarget currentVirtual, TrackedTarget nextTracked = null, VirtualTarget nextVirtual = null);

        public abstract void ConfigureAdaptiveReset(TrackedTarget currentTracked, VirtualTarget currentVirtual, TrackedTarget nextTracked, VirtualTarget nextVirtual);

    }
}